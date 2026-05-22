using Microsoft.Playwright;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CadastroProducaoCRE.Services
{
    public class PlaywrightService
    {
        private IPlaywright? _playwright;
        private IBrowser? _browser;
        private IBrowserContext? _context;
        private IPage? _page;
        private bool _loginRealizado = false;
        private bool _headless;
        
        public event Action<string>? OnLog;
        
        private static readonly string SESSION_DIR = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
            ".meu_app", "session");
        private static readonly string SESSION_FILE = Path.Combine(SESSION_DIR, "auth.json");
        
        public PlaywrightService(bool headless = false)
        {
            _headless = headless;
            Directory.CreateDirectory(SESSION_DIR);
            Log($"📁 Sessão será salva em: {SESSION_FILE}");
        }
        
        private void Log(string message)
        {
            OnLog?.Invoke(message);
        }
        
        public async Task<bool> SalvarSessao()
        {
            try
            {
                if (_context == null)
                {
                    Log("⚠️ Context não disponível para salvar sessão");
                    return false;
                }
                
                var storageState = await _context.StorageStateAsync();
                var json = storageState;
                
                if (string.IsNullOrEmpty(json))
                {
                    Log("⚠️ Nenhum dado de sessão encontrado!");
                    return false;
                }
                
                Directory.CreateDirectory(SESSION_DIR);
                await File.WriteAllTextAsync(SESSION_FILE, json);
                
                Log($"✅ Sessão salva com sucesso!");
                return true;
            }
            catch (Exception ex)
            {
                Log($"❌ Erro ao salvar sessão: {ex.Message}");
                return false;
            }
        }
        
        public async Task<string?> CarregarSessao()
        {
            try
            {
                if (!File.Exists(SESSION_FILE))
                {
                    Log("ℹ️ Nenhum arquivo de sessão encontrado");
                    return null;
                }
                
                var json = await File.ReadAllTextAsync(SESSION_FILE);
                Log($"✅ Sessão carregada com sucesso!");
                return json;
            }
            catch (Exception ex)
            {
                Log($"⚠️ Erro ao carregar sessão: {ex.Message}");
                return null;
            }
        }
        
        public async Task<bool> IniciarNavegador()
        {
            try
            {
                Log("🌐 Iniciando navegador...");
                
                _playwright = await Playwright.CreateAsync();
                
                var args = new List<string>
                {
                    "--disable-blink-features=AutomationControlled",
                    "--no-sandbox",
                    "--disable-dev-shm-usage"
                };
                
                if (_headless)
                {
                    args.Add("--disable-gpu");
                    args.Add("--disable-software-rasterizer");
                }
                
                _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = _headless,
                    Args = args
                });
                
                // Carregar sessão salva
                var storageState = await CarregarSessao();
                
                var contextoConfig = new BrowserNewContextOptions
                {
                    ViewportSize = new ViewportSize { Width = 800, Height = 600 },
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
                    IgnoreHTTPSErrors = true
                };
                
                if (!string.IsNullOrEmpty(storageState))
                {
                    contextoConfig.StorageState = storageState;
                    _context = await _browser.NewContextAsync(contextoConfig);
                    Log("✅ Sessão anterior carregada (login persistente)");
                }
                else
                {
                    _context = await _browser.NewContextAsync(contextoConfig);
                    Log("✅ Novo contexto criado");
                }
                
                _page = await _context.NewPageAsync();
                Log("✅ Navegador iniciado com sucesso");
                return true;
            }
            catch (Exception ex)
            {
                Log($"❌ Erro ao iniciar navegador: {ex.Message}");
                return false;
            }
        }
        
        public async Task FecharNavegador()
        {
            try
            {
                if (_context != null && _loginRealizado)
                {
                    Log("💾 Salvando sessão antes de fechar...");
                    await SalvarSessao();
                }
                
                if (_page != null) await _page.CloseAsync();
                if (_context != null) await _context.CloseAsync();
                if (_browser != null) await _browser.CloseAsync();
                _playwright?.Dispose();
                
                Log("🔒 Navegador fechado");
            }
            catch (Exception ex)
            {
                Log($"⚠️ Erro ao fechar navegador: {ex.Message}");
            }
        }
        
        public async Task<bool> AcessarPagina(string url)
        {
            try
            {
                if (_page == null) return false;
                
                Log($"🌍 Acessando: {url}");
                await _page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
                await Task.Delay(3000);
                Log("✅ Página carregada com sucesso");
                
                return true;
            }
            catch (Exception ex)
            {
                Log($"❌ Erro ao acessar página: {ex.Message}");
                return false;
            }
        }
        
        public async Task<bool> PreencherCamposLogin(string username, string password, string otp)
        {
            try
            {
                if (_page == null) return false;
                
                var senhaCompleta = password + otp;
                Log("=".PadRight(50, '='));
                Log("🔐 PREENCHENDO CAMPOS DE LOGIN");
                Log("=".PadRight(50, '='));
                Log($"👤 Usuário: {username}");
                Log($"🔑 Senha + OTP: {'*' * senhaCompleta.Length}");
                
                // Aguarda a página estabilizar
                await Task.Delay(2000);
                
                // === CAMPO DE USUÁRIO ===
                Log("🔍 Procurando campo de usuário...");
                var userField = await _page.QuerySelectorAsync("#username, #user, input[type=\"text\"]");
                if (userField == null)
                    userField = await _page.QuerySelectorAsync("input[name=\"user\"], input[name=\"username\"]");
                
                if (userField != null)
                {
                    await userField.ClickAsync();
                    await Task.Delay(300);
                    await userField.PressAsync("Control+A");
                    await Task.Delay(300);
                    await userField.PressAsync("Delete");
                    await Task.Delay(300);
                    await userField.FillAsync(username);
                    Log($"✅ Usuário preenchido: {username}");
                }
                else
                {
                    Log("❌ Campo de usuário não encontrado!");
                    return false;
                }
                
                await Task.Delay(500);
                
                // === CAMPO DE SENHA ===
                Log("🔍 Procurando campo de senha...");
                var passwordField = await _page.QuerySelectorAsync("#password, #senha, input[type=\"password\"]");
                if (passwordField != null)
                {
                    await passwordField.ClickAsync();
                    await Task.Delay(300);
                    await passwordField.PressAsync("Control+A");
                    await Task.Delay(300);
                    await passwordField.PressAsync("Delete");
                    await Task.Delay(300);
                    await passwordField.FillAsync(senhaCompleta);
                    Log($"✅ Senha + OTP preenchida");
                }
                else
                {
                    Log("❌ Campo de senha não encontrado!");
                    return false;
                }
                
                // NÃO PRESSIONA ENTER - NÃO SUBMETE O FORMULÁRIO
                
                
                // Injeta mensagem visual na página
                await _page.EvaluateAsync(@"
                    function() {
                        var oldDiv = document.getElementById('automacao-status');
                        if (oldDiv) oldDiv.remove();
                        
                        var div = document.createElement('div');
                        div.id = 'automacao-status';
                        div.style.position = 'fixed';
                        div.style.top = '10px';
                        div.style.right = '10px';
                        div.style.backgroundColor = '#4CAF50';
                        div.style.color = 'white';
                        div.style.padding = '15px';
                        div.style.zIndex = '999999';
                        div.style.borderRadius = '5px';
                        div.style.fontFamily = 'Arial, sans-serif';
                        div.style.fontSize = '14px';
                        div.style.fontWeight = 'bold';
                        div.style.boxShadow = '0 4px 8px rgba(0,0,0,0.2)';
                        div.style.border = '2px solid #388E3C';
                        div.innerHTML = 'CAMPOS PREENCHIDOS AUTOMATICAMENTE!<br><br>' +
                                    'Usuario e Senha+OTP ja estao preenchidos.<br><br>' +
                                    'AGORA:<br>' +
                                    '1. Resolva o CAPTCHA manualmente<br>' +
                                    '2. Clique no botao Entrar<br><br>' +
                                    'O sistema vai detectar o login automaticamente...';
                        document.body.appendChild(div);
                    }
                ");
                
                Log("");
                Log("=".PadRight(50, '='));
                Log("✅ PREENCHIMENTO AUTOMÁTICO CONCLUÍDO!");
                Log("=".PadRight(50, '='));
                Log("🔓 AGORA VOCÊ DEVE:");
                Log("   1. Resolver o CAPTCHA manualmente");
                Log("   2. Clicar no botão 'Entrar' ou 'Login'");
                Log("");
                Log("⏳ O programa vai aguardar você fazer o login...");
                Log("=".PadRight(50, '='));
                
                return true;
            }
            catch (Exception ex)
            {
                Log($"❌ Erro ao preencher campos: {ex.Message}");
                return false;
            }
        }
        
        public async Task<bool> AguardarLoginManual(int timeoutSegundos = 120)
        {
            Log($"⏳ Aguardando login manual (máximo {timeoutSegundos}s)...");
            Log("🔍 Procurando elemento do usuário logado...");
            
            var timeoutInicial = DateTime.Now;
            var ultimoLog = DateTime.Now;
            
            while ((DateTime.Now - timeoutInicial).TotalSeconds < timeoutSegundos)
            {
                try
                {
                    if (_page == null) continue;
                    
                    // Procura pelo elemento do usuário logado
                    var userLabel = await _page.QuerySelectorAsync("label:has-text(\"Usuário:\")");
                    if (userLabel != null && await userLabel.IsVisibleAsync())
                    {
                        var userText = await userLabel.TextContentAsync();
                        if (!string.IsNullOrEmpty(userText) && userText.Contains("TR"))
                        {
                            // Atualiza mensagem visual para sucesso
                            await _page.EvaluateAsync(@"
                                () => {
                                    const div = document.getElementById('automacao-status');
                                    if (div) {
                                        div.style.backgroundColor = '#2196F3';
                                        div.style.border = '2px solid #1976D2';
                                        div.innerHTML = '✅ LOGIN DETECTADO COM SUCESSO!<br><br>' +
                                                    '🔓 Redirecionando para o sistema...<br><br>' +
                                                    '🔄 Aguarde...';
                                    }
                                }
                            ");
                            
                            Log($"✅ LOGIN DETECTADO COM SUCESSO!");
                            Log($"👤 {userText.Trim()}");
                            _loginRealizado = true;
                            return true;
                        }
                    }
                    
                    // Log a cada 30 segundos
                    if ((DateTime.Now - ultimoLog).TotalSeconds >= 30)
                    {
                        ultimoLog = DateTime.Now;
                        var tempoDecorrido = (DateTime.Now - timeoutInicial).TotalSeconds;
                        Log($"⏳ Aguardando login... ({tempoDecorrido:F0}s / {timeoutSegundos}s)");
                    }
                }
                catch (Exception ex)
                {
                    // Ignora erros temporários
                }
                
                await Task.Delay(1000);
            }
            
            Log($"❌ Timeout de {timeoutSegundos} segundos atingido. Login não detectado.");
            return false;
        }
        
        public async Task<bool> RealizarLoginCompleto(string username, string password, string otp, string urlLogin, string urlIndex)
        {
            Log("🚀 Iniciando fluxo de login...");
            
            var tentativas = 0;
            var maxTentativas = 3;
            var otpAtual = otp;
            
            while (tentativas < maxTentativas)
            {
                tentativas++;
                Log($"📌 Tentativa {tentativas} de {maxTentativas}");
                
                // Recarrega a página de login
                await _page!.GotoAsync(urlLogin, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
                await Task.Delay(3000);
                
                // Preenche os campos
                if (!await PreencherCamposLogin(username, password, otpAtual))
                {
                    Log("❌ Falha ao preencher campos");
                    continue;
                }
                
                // Aguarda a resposta
                await Task.Delay(5000);
                
                // Verifica erro
                var erroLogin = await VerificarErroLogin();
                
                if (erroLogin)
                {
                    Log("⚠️ Erro no login! Usuário ou senha inválidos.");
                    
                    if (tentativas < maxTentativas && OnRequisitarNovoOTP != null)
                    {
                        var novoOtp = await OnRequisitarNovoOTP.Invoke(tentativas);
                        if (!string.IsNullOrEmpty(novoOtp) && novoOtp.Length == 6)
                        {
                            otpAtual = novoOtp;
                            Log($"🔐 Novo OTP: {'*' * otpAtual.Length}");
                            continue;
                        }
                    }
                    return false;
                }
                
                // Aguarda o redirecionamento
                await Task.Delay(3000);
                
                // Verifica se o login foi bem sucedido
                if (await AguardarLoginManual(30))
                {
                    Log("✅ Login detectado!");
                    
                    // Salva a sessão
                    await SalvarSessao();
                    
                    // Navega para a página de cadastro
                    Log($"🌍 Navegando para página de cadastro: {urlIndex}");
                    await _page.GotoAsync(urlIndex, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
                    await Task.Delay(3000);
                    
                    return true;
                }
            }
            
            Log($"❌ Número máximo de tentativas ({maxTentativas}) atingido!");
            return false;
        }

        // Evento para requisitar novo OTP
        public event Func<int, Task<string>>? OnRequisitarNovoOTP;
        
        public async Task<bool> VerificarSessaoValida()
        {
            try
            {
                if (_page == null) return false;
                
                // Aguarda um pouco para a página estabilizar
                await Task.Delay(2000);
                
                // Método 1: Procura pelo label do usuário específico
                try
                {
                    // Procura pelo elemento que contém "Usuário:" seguido de TR (matrícula)
                    var userLabel = await _page.QuerySelectorAsync("label:has-text(\"Usuário:\")");
                    if (userLabel != null && await userLabel.IsVisibleAsync())
                    {
                        var userText = await userLabel.TextContentAsync();
                        if (!string.IsNullOrEmpty(userText) && userText.Contains("TR"))
                        {
                            Log($"✅ Login detectado! {userText.Trim()}");
                            _loginRealizado = true;
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log($"⚠️ Erro ao buscar label do usuário: {ex.Message}");
                }
                
                // Método 2: Procura diretamente pela matrícula TR + números
                try
                {
                    var trElement = await _page.QuerySelectorAsync("text=/TR\\d+/");
                    if (trElement != null)
                    {
                        var trText = await trElement.TextContentAsync();
                        Log($"✅ Login detectado! Matrícula encontrada: {trText}");
                        _loginRealizado = true;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Log($"⚠️ Erro ao buscar matrícula: {ex.Message}");
                }
                
                // Método 3: Verifica URL (fallback)
                var currentUrl = _page.Url;
                if (currentUrl.Contains("cre.oi.net.br") && !currentUrl.ToLower().Contains("login"))
                {
                    Log($"✅ Sessão válida pela URL: {currentUrl}");
                    _loginRealizado = true;
                    return true;
                }
                
                // Método 4: Verifica elementos da página do CRE
                var elementosCre = new[] { "#Projeto", "#Sequencial", "#btnBusca" };
                foreach (var selector in elementosCre)
                {
                    try
                    {
                        var elemento = await _page.QuerySelectorAsync(selector);
                        if (elemento != null && await elemento.IsVisibleAsync())
                        {
                            Log($"✅ Sessão válida - elemento encontrado: {selector}");
                            _loginRealizado = true;
                            return true;
                        }
                    }
                    catch { }
                }
                
                // Método 5: Verifica se ainda está na página de login
                if (currentUrl.Contains("oilogin.oi.net.br") || currentUrl.ToLower().Contains("login"))
                {
                    Log("⚠️ Ainda na página de login - aguardando...");
                    return false;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Log($"⚠️ Erro ao verificar sessão: {ex.Message}");
                return false;
            }
        }
        
        public async Task<bool> VerificarSessaoOculta(string urlLogin)
        {
            IPlaywright? tempPlaywright = null;
            IBrowser? tempBrowser = null;
            IBrowserContext? tempContext = null;
            IPage? tempPage = null;
            
            try
            {
                Log("🔍 Verificando sessão existente (modo oculto)...");
                
                if (!File.Exists(SESSION_FILE))
                {
                    Log("ℹ️ Nenhum arquivo de sessão encontrado");
                    return false;
                }
                
                var fileInfo = new FileInfo(SESSION_FILE);
                if (fileInfo.Length < 100)
                {
                    Log("⚠️ Arquivo muito pequeno");
                    return false;
                }
                
                var storageState = await File.ReadAllTextAsync(SESSION_FILE);
                
                tempPlaywright = await Playwright.CreateAsync();
                tempBrowser = await tempPlaywright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--disable-blink-features=AutomationControlled" }
                });
                
                tempContext = await tempBrowser.NewContextAsync(new BrowserNewContextOptions
                {
                    ViewportSize = new ViewportSize { Width = 800, Height = 600 },
                    StorageState = storageState,
                    IgnoreHTTPSErrors = true
                });
                
                tempPage = await tempContext.NewPageAsync();
                
                var urlCre = "https://cre.oi.net.br/CRE_NEW/CadastroProducao/Index";
                Log("🌍 Testando acesso direto ao CRE...");
                
                try
                {
                    await tempPage.GotoAsync(urlCre, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
                }
                catch
                {
                    Log("🌍 Tentando via página de login...");
                    await tempPage.GotoAsync(urlLogin, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
                }
                
                await Task.Delay(5000);
                
                var currentUrl = tempPage.Url;
                Log($"📍 URL: {currentUrl}");
                
                await tempPage.ScreenshotAsync(new PageScreenshotOptions { Path = "debug_headless_check.png" });
                Log("📸 Screenshot salvo: debug_headless_check.png");
                
                if (currentUrl.Contains("cre.oi.net.br"))
                {
                    Log("✅ SESSÃO VÁLIDA! (Redirecionado para CRE)");
                    return true;
                }
                
                if (currentUrl.Contains("oilogin.oi.net.br") || currentUrl.Contains("login"))
                {
                    Log("❌ Sessão inválida (página de login)");
                    return false;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Log($"⚠️ Erro: {ex.Message}");
                return false;
            }
            finally
            {
                try
                {
                    if (tempPage != null) await tempPage.CloseAsync();
                    if (tempContext != null) await tempContext.CloseAsync();
                    if (tempBrowser != null) await tempBrowser.CloseAsync();
                    tempPlaywright?.Dispose();
                }
                catch { }
            }
        }
        
        public async Task<bool> IniciarNavegadorComLoginInteligente(string urlLogin, string username, string password, string otp = "", string urlIndex = "")
        {
            try
            {
                if (!_headless)
                {
                    Log("🪟 Modo normal (visível)");
                    if (!await IniciarNavegador())
                        return false;
                    if (!await AcessarPagina(urlLogin))
                        return false;
                    if (!await VerificarSessaoValida())
                    {
                        if (string.IsNullOrEmpty(otp))
                            return false;
                        if (!await RealizarLoginCompleto(username, password, otp, urlLogin, urlIndex))
                            return false;
                    }
                    return true;
                }
                
                Log("🕶️ Modo headless ativado");
                
                if (await VerificarSessaoOculta(urlLogin))
                {
                    Log("✅ Sessão válida encontrada! Iniciando headless...");
                    
                    if (!await IniciarNavegador())
                        return false;
                    
                    var urlCre = "https://cre.oi.net.br/CRE_NEW/CadastroProducao/Index";
                    try
                    {
                        await _page!.GotoAsync(urlCre, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
                    }
                    catch
                    {
                        await _page!.GotoAsync(urlLogin, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
                    }
                    
                    await Task.Delay(5000);
                    
                    if (await VerificarSessaoValida())
                    {
                        Log("✅ Headless funcionando com sessão existente!");
                        return true;
                    }
                    else
                    {
                        Log("⚠️ Sessão carregada mas não validada");
                        await _page!.ReloadAsync();
                        await Task.Delay(3000);
                        if (await VerificarSessaoValida())
                        {
                            Log("✅ Sessão validada após reload!");
                            return true;
                        }
                        else
                        {
                            Log("❌ Falha na validação");
                            await FecharNavegador();
                        }
                    }
                }
                else
                {
                    Log("ℹ️ Nenhuma sessão encontrada");
                }
                
                Log("🔄 Abrindo navegador visível para login...");
                var headlessOriginal = _headless;
                _headless = false;
                
                if (!await IniciarNavegador())
                {
                    _headless = headlessOriginal;
                    return false;
                }
                
                if (!await AcessarPagina(urlLogin))
                {
                    _headless = headlessOriginal;
                    await FecharNavegador();
                    return false;
                }
                
                if (!await VerificarSessaoValida())
                {
                    if (string.IsNullOrEmpty(otp))
                    {
                        _headless = headlessOriginal;
                        return false;
                    }
                    if (!await RealizarLoginCompleto(username, password, otp, urlLogin, urlIndex))
                    {
                        _headless = headlessOriginal;
                        await FecharNavegador();
                        return false;
                    }
                }
                
                Log("✅ Login OK! Salvando sessão...");
                await SalvarSessao();
                await FecharNavegador();
                
                _headless = headlessOriginal;
                Log("🕶️ Voltando para headless...");
                
                if (!await IniciarNavegador())
                    return false;
                
                var urlCreFinal = "https://cre.oi.net.br/CRE_NEW/CadastroProducao/Index";
                try
                {
                    await _page!.GotoAsync(urlCreFinal, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
                }
                catch
                {
                    await _page!.GotoAsync(urlLogin, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
                }
                
                await Task.Delay(5000);
                
                if (await VerificarSessaoValida())
                {
                    Log("✅ Headless com sessão válida!");
                    return true;
                }
                else
                {
                    Log("❌ Falha ao estabelecer sessão headless");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log($"❌ Erro: {ex.Message}");
                return false;
            }
        }
        
        public async Task<bool> VerificarErroLogin()
        {
            try
            {
                if (_page == null) return false;
                
                // Seletores para mensagem de erro
                var errorSelectors = new[]
                {
                    ".p-error",
                    "p.p-error",
                    ".alert-danger",
                    ".error-message",
                    "div:has-text(\"não encontramos seu usuário ou senha\")",
                    "div:has-text(\"usuário ou senha inválidos\")",
                    "div:has-text(\"senha incorreta\")",
                    "div[class*=\"error\"]"
                };
                
                foreach (var selector in errorSelectors)
                {
                    try
                    {
                        var errorElement = await _page.QuerySelectorAsync(selector);
                        if (errorElement != null && await errorElement.IsVisibleAsync())
                        {
                            var errorText = await errorElement.TextContentAsync();
                            if (!string.IsNullOrEmpty(errorText))
                            {
                                Log($"❌ Mensagem de erro detectada: {errorText.Trim()}");
                                return true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Ignora erros de seletor
                    }
                }
                
                // Verifica se a URL ainda está na página de login (indicativo de erro)
                var currentUrl = _page.Url;
                if (currentUrl.Contains("oilogin.oi.net.br") && currentUrl.Contains("error"))
                {
                    Log("⚠️ URL contém indicativo de erro");
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Log($"⚠️ Erro ao verificar erro de login: {ex.Message}");
                return false;
            }
        }

        public async Task<string?> ObterUsuarioLogado()
        {
            try
            {
                if (_page == null) return null;
                
                // Procura pelo label do usuário
                var userLabel = await _page.QuerySelectorAsync("label:has-text(\"Usuário:\")");
                if (userLabel != null && await userLabel.IsVisibleAsync())
                {
                    var userText = await userLabel.TextContentAsync();
                    return userText?.Trim();
                }
                
                // Procura diretamente pela matrícula TR
                var trElement = await _page.QuerySelectorAsync("text=/TR\\d+/");
                if (trElement != null)
                {
                    var trText = await trElement.TextContentAsync();
                    return trText?.Trim();
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Log($"⚠️ Erro ao obter usuário logado: {ex.Message}");
                return null;
            }
        }
    
        public async Task<bool> IniciarNavegadorComSessao(string urlLogin, string urlIndex = "")
        {
            try
            {
                if (!File.Exists(SESSION_FILE))
                {
                    Log("ℹ️ Nenhuma sessão salva encontrada");
                    return false;
                }
                
                var fileInfo = new FileInfo(SESSION_FILE);
                if (fileInfo.Length < 100)
                {
                    Log("⚠️ Arquivo de sessão corrompido ou inválido");
                    return false;
                }
                
                Log("🔄 Tentando login com sessão salva...");
                
                if (!await IniciarNavegador())
                {
                    Log("❌ Falha ao iniciar navegador");
                    return false;
                }
                
                // Acessa a página de login primeiro (para validar a sessão)
                Log($"🌍 Acessando página de login para validar sessão: {urlLogin}");
                if (!await AcessarPagina(urlLogin))
                {
                    Log("❌ Falha ao acessar página de login");
                    await FecharNavegador();
                    return false;
                }
                
                await Task.Delay(3000);
                
                // Verifica se a sessão é válida (se já estiver logado, vai redirecionar)
                if (await VerificarSessaoValida())
                {
                    Log("✅ Sessão válida! Login automático realizado.");
                    _loginRealizado = true;
                    
                    // Se tiver urlIndex, navega para ela
                    if (!string.IsNullOrEmpty(urlIndex))
                    {
                        Log($"🌍 Navegando para página de cadastro: {urlIndex}");
                        await _page!.GotoAsync(urlIndex, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
                        await Task.Delay(3000);
                        Log("✅ Página de cadastro carregada com sucesso!");
                    }
                    return true;
                }
                
                Log("⚠️ Sessão inválida ou expirada");
                await FecharNavegador();
                return false;
            }
            catch (Exception ex)
            {
                Log($"❌ Erro ao tentar login com sessão: {ex.Message}");
                await FecharNavegador();
                return false;
            }
        }

        public async Task<bool> IniciarNavegadorComLogin(string urlLogin, string urlIndex, string username, string password, string otp)
        {
            try
            {
                // Inicia navegador
                if (!await IniciarNavegador())
                {
                    Log("❌ Falha ao iniciar navegador");
                    return false;
                }
                
                // Tenta acessar diretamente a página do CRE
                Log($"🌍 Tentando acessar diretamente: {urlIndex}");
                await _page!.GotoAsync(urlIndex, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
                await Task.Delay(5000);
                
                // Verifica se já está logado (sessão válida)
                var currentUrl = _page.Url;
                Log($"📍 URL atual: {currentUrl}");
                
                if (currentUrl.Contains("cre.oi.net.br") && !currentUrl.Contains("login"))
                {
                    Log("✅ Sessão válida! Acessou diretamente a página de cadastro.");
                    return true;
                }
                
                // Se não está logado, faz o login
                Log("⚠️ Sessão não encontrada ou expirada. Realizando login...");
                
                // Vai para página de login
                Log($"🌍 Acessando página de login: {urlLogin}");
                await _page.GotoAsync(urlLogin, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
                await Task.Delay(3000);
                
                // Realiza o login
                var sucessoLogin = await RealizarLoginCompleto(username, password, otp, urlLogin, urlIndex);
                
                if (sucessoLogin)
                {
                    Log("✅ Login realizado com sucesso!");
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Log($"❌ Erro: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> IniciarNavegadorEFazerLogin(string urlIndex, string urlLogin, string username, string password, string otp)
        {
            try
            {
                // 1. Inicia navegador
                if (!await IniciarNavegador())
                {
                    Log("❌ Falha ao iniciar navegador");
                    return false;
                }
                
                // 2. Tenta acessar diretamente a página do CRE (se estiver logado, vai direto)
                Log($"🌍 Tentando acessar: {urlIndex}");
                await _page!.GotoAsync(urlIndex, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
                await Task.Delay(5000);
                
                var currentUrl = _page.Url;
                Log($"📍 URL atual: {currentUrl}");
                
                // 3. Verifica se já está na página do CRE (logado)
                if (currentUrl.Contains("cre.oi.net.br/CRE_NEW/CadastroProducao"))
                {
                    Log("✅ Já está logado! Página de cadastro acessada diretamente.");
                    return true;
                }
                
                // 4. Se não está logado, vai para página de login
                Log("⚠️ Não está logado. Redirecionando para página de login...");
                await _page.GotoAsync(urlLogin, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
                await Task.Delay(3000);
                
                // 5. Preenche os campos (NÃO clica no botão)
                if (!await PreencherCamposLogin(username, password, otp))
                {
                    Log("❌ Falha ao preencher campos");
                    return false;
                }
                
                // 6. Aguarda usuário resolver CAPTCHA e clicar manualmente
                Log("⏳ Aguardando usuário resolver CAPTCHA e clicar em Entrar...");
                
                if (await AguardarLoginManual(120))
                {
                    Log("✅ Login detectado!");
                    await SalvarSessao();
                    
                    // 7. Verifica se está na página correta
                    if (!_page.Url.Contains("cre.oi.net.br/CRE_NEW/CadastroProducao"))
                    {
                        Log($"🌍 Navegando para página de cadastro: {urlIndex}");
                        await _page.GotoAsync(urlIndex, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
                        await Task.Delay(3000);
                    }
                    
                    return true;
                }
                
                // 8. Se chegou aqui, perguntar se quer tentar novamente com novo OTP
                if (OnRequisitarNovoOTP != null)
                {
                    var novoOtp = await OnRequisitarNovoOTP.Invoke(1);
                    if (!string.IsNullOrEmpty(novoOtp) && novoOtp.Length == 6)
                    {
                        Log($"🔄 Tentando novamente com novo OTP...");
                        return await IniciarNavegadorEFazerLogin(urlIndex, urlLogin, username, password, novoOtp);
                    }
                }
                
                Log("❌ Login não detectado dentro do tempo limite!");
                return false;
            }
            catch (Exception ex)
            {
                Log($"❌ Erro: {ex.Message}");
                return false;
            }
        }
        public IPage? GetPage() => _page;
    }
}