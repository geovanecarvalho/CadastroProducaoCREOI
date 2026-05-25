using Microsoft.Playwright;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using CadastroProducaoCRE.Views;
using Microsoft.Playwright;

namespace CadastroProducaoCRE.Services
{
    public class PlaywrightService
    {
        private IPlaywright? _playwright;
        public IPage? GetPage() => _page;
        private IBrowser? _browser;
        private IBrowserContext? _context;
        private IPage? _page;
        private bool _loginRealizado = false;
        private bool _headless;
        
        public event Action<string>? OnLog;
        
        private static readonly string SESSION_DIR = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
            ".cadastro_producao_cre", "session");
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
                Log("🌐 Iniciando navegador (modo anti-detecção)...");
                
                _playwright = await Playwright.CreateAsync();
                
                // Argumentos avançados para evitar detecção
                var args = new List<string>
                {
                    "--disable-blink-features=AutomationControlled",
                    "--disable-features=IsolateOrigins,site-per-process",
                    "--disable-site-isolation-trials",
                    "--disable-web-security",
                    "--disable-features=BlockInsecurePrivateNetworkRequests",
                    "--disable-automation",
                    "--disable-default-apps",
                    "--disable-extensions",
                    "--disable-component-extensions-with-background-pages",
                    "--disable-sync",
                    "--metrics-recording-only",
                    "--no-first-run",
                    "--no-default-browser-check",
                    "--disable-background-networking",
                    "--disable-background-timer-throttling",
                    "--disable-backgrounding-occluded-windows",
                    "--disable-breakpad",
                    "--disable-client-side-phishing-detection",
                    "--disable-crash-reporter",
                    "--disable-domain-reliability",
                    "--disable-ipc-flooding-protection",
                    "--disable-popup-blocking",
                    "--disable-prompt-on-repost",
                    "--disable-renderer-backgrounding",
                    "--force-fieldtrials=*BackgroundTracing/default/",
                    "--test-type=webdriver",
                    "--mute-audio",
                    "--disable-dev-shm-usage",
                    "--no-sandbox"
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
                
                // Criar contexto com configurações realistas
                var contextoConfig = new BrowserNewContextOptions
                {
                    ViewportSize = new ViewportSize { Width = 1366, Height = 768 },
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                    IgnoreHTTPSErrors = true,
                    Locale = "pt-BR",
                    TimezoneId = "America/Sao_Paulo",
                    Permissions = new[] { "geolocation" },
                    DeviceScaleFactor = 1,
                    HasTouch = false,
                    IsMobile = false
                };

                // CARREGAR SESSÃO SALVA SE EXISTIR
                var storageState = await CarregarSessao();
                if (!string.IsNullOrEmpty(storageState))
                {
                    contextoConfig.StorageState = storageState;
                    Log("📂 Sessão anterior carregada");
                }

                _context = await _browser.NewContextAsync(contextoConfig);
                
                // ========== CRIA A PÁGINA ==========
                _page = await _context.NewPageAsync();
                
                // Scripts anti-detecção
                await _page.AddInitScriptAsync(@"
                    () => {
                        // Remover propriedade webdriver
                        Object.defineProperty(navigator, 'webdriver', {
                            get: () => undefined
                        });
                        
                        // Remover plugins vazios
                        Object.defineProperty(navigator, 'plugins', {
                            get: () => [1, 2, 3, 4, 5]
                        });
                        
                        // Adicionar languages
                        Object.defineProperty(navigator, 'languages', {
                            get: () => ['pt-BR', 'pt', 'en-US', 'en']
                        });
                        
                        // Adicionar platform
                        Object.defineProperty(navigator, 'platform', {
                            get: () => 'Win32'
                        });
                        
                        // Simular chrome
                        window.chrome = { runtime: {} };
                        
                        // Adicionar permissões
                        const originalQuery = window.navigator.permissions.query;
                        window.navigator.permissions.query = (parameters) => (
                            parameters.name === 'notifications' ?
                                Promise.resolve({ state: Notification.permission }) :
                                originalQuery(parameters)
                        );
                    }
                ");
                
                Log("✅ Navegador iniciado com sucesso (anti-detecção ativada)");
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
                
                // === 1. PRIMEIRO: CAPTCHA ===
                Log("🔍 Capturando CAPTCHA...");
                
                byte[] captchaImage = null;
                string captchaTexto = "";
                bool captchaResolvido = false;
                
                while (!captchaResolvido)
                {
                    captchaImage = await CapturarCaptcha();
                    
                    if (captchaImage == null)
                    {
                        Log("⚠️ Não foi possível capturar o CAPTCHA.");
                        return false;
                    }
                    
                    var captchaDialog = new CaptchaDialog(captchaImage, async () =>
                    {
                        await RecarregarCaptcha();
                        return await CapturarCaptcha();
                    });
                    
                    if (captchaDialog.ShowDialog() == DialogResult.OK)
                    {
                        captchaTexto = captchaDialog.CaptchaTexto;
                        Log($"🔐 CAPTCHA informado: {captchaTexto}");
                        
                        var captchaField = await _page.QuerySelectorAsync("#textVerificacao");
                        if (captchaField != null)
                        {
                            await captchaField.ClickAsync();
                            await captchaField.PressAsync("Control+A");
                            await captchaField.PressAsync("Delete");
                            await captchaField.FillAsync(captchaTexto);
                            Log("✅ CAPTCHA preenchido!");
                        }
                        else
                        {
                            Log("❌ Campo do CAPTCHA não encontrado!");
                            return false;
                        }
                        
                        captchaResolvido = true;
                    }
                    else
                    {
                        Log("❌ Usuário cancelou a resolução do CAPTCHA");
                        return false;
                    }
                }
                
                await Task.Delay(500);
                
                // === 2. SEGUNDO: SENHA ===
                Log("🔍 Preenchendo campo de senha...");
                var passwordField = await _page.QuerySelectorAsync("#password, #senha, input[type=\"password\"]");
                if (passwordField != null)
                {
                    await passwordField.ClickAsync();
                    await passwordField.PressAsync("Control+A");
                    await passwordField.PressAsync("Delete");
                    await passwordField.FillAsync(senhaCompleta);
                    Log($"✅ Senha + OTP preenchida");
                }
                else
                {
                    Log("❌ Campo de senha não encontrado!");
                }
                
                await Task.Delay(500);
                
                // === 3. TERCEIRO: USUÁRIO ===
                Log("🔍 Preenchendo campo de usuário...");
                var userField = await _page.QuerySelectorAsync("#username, #user, input[type=\"text\"]");
                if (userField == null)
                    userField = await _page.QuerySelectorAsync("input[name=\"user\"], input[name=\"username\"]");
                
                if (userField != null)
                {
                    await userField.ClickAsync();
                    await userField.PressAsync("Control+A");
                    await userField.PressAsync("Delete");
                    await userField.FillAsync(username);
                    Log($"✅ Usuário preenchido: {username}");
                }
                else
                {
                    Log("❌ Campo de usuário não encontrado!");
                }
                
                await Task.Delay(500);
                
                // === 4. CLICAR NO BOTÃO DE LOGIN ===
                Log("🔍 Procurando botão 'efetuar login'...");
                
                var loginSelectors = new[]
                {
                    "button:has-text('efetuar login')",
                    "button:has-text('Efetuar Login')",
                    "button:has-text('Entrar')",
                    "button:has-text('Login')",
                    "button[type='submit']",
                    ".btn-oi-new",
                    "button.btn-block"
                };
                
                bool clicou = false;
                foreach (var selector in loginSelectors)
                {
                    try
                    {
                        var loginButton = await _page.QuerySelectorAsync(selector);
                        if (loginButton != null && await loginButton.IsVisibleAsync())
                        {
                            Log($"✅ Botão de login encontrado: {selector}");
                            await loginButton.ClickAsync();
                            Log("🖱️ Botão 'efetuar login' clicado!");
                            clicou = true;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"⚠️ Erro com seletor {selector}: {ex.Message}");
                    }
                }
                
                if (!clicou)
                {
                    Log("⚠️ Botão de login não encontrado, tentando submeter com Enter...");
                    await _page.Keyboard.PressAsync("Enter");
                    Log("⏎ Tecla Enter pressionada");
                }
                
                Log("");
                Log("=".PadRight(50, '='));
                Log("✅ TODOS OS CAMPOS PREENCHIDOS E LOGIN SUBMETIDO!");
                Log("=".PadRight(50, '='));
                Log("📋 RESUMO DO PREENCHIMENTO:");
                Log($"   - CAPTCHA: {captchaTexto}");
                Log($"   - Senha+OTP: {'*' * senhaCompleta.Length}");
                Log($"   - Usuário: {username}");
                Log("");
                Log("⏳ Aguardando resposta do servidor...");
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
                // 1. Verificar se já existe sessão salva
                Log("🔍 Verificando sessão salva...");
                
                if (File.Exists(SESSION_FILE))
                {
                    var fileInfo = new FileInfo(SESSION_FILE);
                    if (fileInfo.Length > 100)
                    {
                        Log("📂 Sessão encontrada. Tentando carregar...");
                        
                        // NÃO CHAMA IniciarNavegadorComSessao - usa diretamente o IniciarNavegador com sessão
                        if (!await IniciarNavegador())
                        {
                            Log("❌ Falha ao iniciar navegador");
                            return false;
                        }
                        
                        // Tenta acessar diretamente a página do CRE
                        Log($"🌍 Tentando acessar: {urlIndex}");
                        await _page!.GotoAsync(urlIndex, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
                        await Task.Delay(5000);
                        
                        if (await VerificarSessaoValida())
                        {
                            Log("✅ Sessão válida! Login automático realizado!");
                            return true;
                        }
                        else
                        {
                            Log("⚠️ Sessão inválida ou expirada. Será necessário novo login.");
                            await FecharNavegador();
                        }
                    }
                }
                
                // 2. Se não tem sessão ou sessão inválida, faz login completo
                Log("🔄 Realizando login completo...");
                
                // Inicia navegador (sem sessão)
                if (!await IniciarNavegador())
                {
                    Log("❌ Falha ao iniciar navegador");
                    return false;
                }
                
                // Vai para página de login
                Log($"🌍 Acessando página de login: {urlLogin}");
                await _page!.GotoAsync(urlLogin, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
                await Task.Delay(3000);
                
                // Loop de tentativas de login
                var maxTentativas = 3;
                var tentativaAtual = 0;
                var otpAtual = otp;
                bool loginSucesso = false;
                
                while (tentativaAtual < maxTentativas && !loginSucesso)
                {
                    tentativaAtual++;
                    Log($"📌 Tentativa {tentativaAtual} de {maxTentativas}");
                    
                    if (tentativaAtual > 1)
                    {
                        Log("🔄 Recarregando página para nova tentativa...");
                        await _page.ReloadAsync();
                        await Task.Delay(3000);
                        
                        if (OnRequisitarNovoOTP != null)
                        {
                            var novoOtp = await OnRequisitarNovoOTP.Invoke(tentativaAtual);
                            if (!string.IsNullOrEmpty(novoOtp) && novoOtp.Length == 6)
                            {
                                otpAtual = novoOtp;
                                Log($"🔐 Novo OTP recebido: {'*' * otpAtual.Length}");
                            }
                            else
                            {
                                Log("❌ Novo OTP não fornecido!");
                                return false;
                            }
                        }
                    }
                    
                    if (!await PreencherCamposLogin(username, password, otpAtual))
                    {
                        Log("❌ Falha ao preencher campos");
                        continue;
                    }
                    
                    Log("⏳ Aguardando usuário clicar em Entrar...");
                    
                    var resultado = await AguardarLoginOuErro(60);
                    
                    if (resultado == LoginResult.Sucesso)
                    {
                        Log("✅ Login detectado com sucesso!");
                        loginSucesso = true;
                        await SalvarSessao();
                        Log("💾 Sessão salva com sucesso!");
                        
                        Log($"🌍 Navegando para página de cadastro: {urlIndex}");
                        await _page.GotoAsync(urlIndex, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
                        await Task.Delay(3000);
                    }
                    else if (resultado == LoginResult.Erro)
                    {
                        Log("⚠️ Erro no login! Verifique suas credenciais.");
                        if (tentativaAtual >= maxTentativas)
                        {
                            Log($"❌ Número máximo de tentativas ({maxTentativas}) atingido!");
                            return false;
                        }
                        continue;
                    }
                    else
                    {
                        Log("❌ Timeout - Login não detectado");
                        return false;
                    }
                }
                
                return loginSucesso;
            }
            catch (Exception ex)
            {
                Log($"❌ Erro: {ex.Message}");
                return false;
            }
        }

        private enum LoginResult
        {
            Aguardando,
            Sucesso,
            Erro,
            Timeout
        }

        private async Task<LoginResult> AguardarLoginOuErro(int timeoutSegundos = 60)
        {
            var timeoutInicial = DateTime.Now;
            
            while ((DateTime.Now - timeoutInicial).TotalSeconds < timeoutSegundos)
            {
                try
                {
                    if (_page == null) continue;
                    
                    // Verifica se apareceu mensagem de erro
                    var errorElement = await _page.QuerySelectorAsync(".p-error, .alert-danger, div:has-text('Ops, não encontramos seu usuário ou senha')");
                    if (errorElement != null && await errorElement.IsVisibleAsync())
                    {
                        var errorText = await errorElement.TextContentAsync();
                        Log($"❌ Erro detectado: {errorText?.Trim()}");
                        return LoginResult.Erro;
                    }
                    
                    // Verifica se o login foi bem sucedido (elemento do usuário)
                    var userLabel = await _page.QuerySelectorAsync("label:has-text(\"Usuário:\")");
                    if (userLabel != null && await userLabel.IsVisibleAsync())
                    {
                        var userText = await userLabel.TextContentAsync();
                        if (!string.IsNullOrEmpty(userText) && userText.Contains("TR"))
                        {
                            Log($"✅ Login detectado! {userText.Trim()}");
                            return LoginResult.Sucesso;
                        }
                    }
                    
                    // Verifica se a URL mudou para o CRE
                    var currentUrl = _page.Url;
                    if (currentUrl.Contains("cre.oi.net.br") && !currentUrl.Contains("login"))
                    {
                        Log($"✅ Redirecionamento detectado: {currentUrl}");
                        return LoginResult.Sucesso;
                    }
                }
                catch { }
                
                await Task.Delay(1000);
            }
            
            return LoginResult.Timeout;
        }

        public async Task<bool> IniciarNavegadorComSessao(string urlLogin, string urlIndex)
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
                    Log("⚠️ Arquivo de sessão corrompido");
                    return false;
                }
                
                Log("🔄 Carregando sessão salva...");
                
                // Carrega a sessão
                var storageState = await File.ReadAllTextAsync(SESSION_FILE);
                
                // INICIA O NAVEGADOR PRIMEIRO
                if (!await IniciarNavegador())
                {
                    Log("❌ Falha ao iniciar navegador");
                    return false;
                }
                
                // Fecha o contexto atual e cria um novo com a sessão
                if (_context != null)
                {
                    await _context.CloseAsync();
                }
                
                var contextoConfig = new BrowserNewContextOptions
                {
                    ViewportSize = new ViewportSize { Width = 1366, Height = 768 },
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                    IgnoreHTTPSErrors = true,
                    StorageState = storageState
                };
                
                if (_browser == null)
                {
                    Log("❌ Browser não disponível");
                    return false;
                }
                
                _context = await _browser.NewContextAsync(contextoConfig);
                _page = await _context.NewPageAsync();
                
                // Tenta acessar diretamente a página do CRE
                Log($"🌍 Tentando acessar: {urlIndex}");
                await _page.GotoAsync(urlIndex, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
                await Task.Delay(5000);
                
                var currentUrl = _page.Url;
                Log($"📍 URL atual: {currentUrl}");
                
                // Verifica se está logado
                if (currentUrl.Contains("cre.oi.net.br") && !currentUrl.Contains("login"))
                {
                    Log("✅ Sessão válida! Acessou diretamente.");
                    return true;
                }
                
                // Verifica se ainda está na página de login (sessão inválida)
                if (currentUrl.Contains("oilogin.oi.net.br") || currentUrl.Contains("login"))
                {
                    Log("⚠️ Sessão inválida ou expirada");
                    return false;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Log($"❌ Erro ao carregar sessão: {ex.Message}");
                return false;
            }
        }
            
        public async Task<byte[]> CapturarCaptcha()
        {
            try
            {
                if (_page == null) return null;
                
                // Procura pela imagem do CAPTCHA (geralmente perto do campo)
                var captchaSelectors = new[]
                {
                    "img[src*='captcha']",
                    "img[src*='Captcha']",
                    "img[src*='verificacao']",
                    "img[alt*='captcha']",
                    "#captchaImg",
                    ".captcha-img",
                    "div[id*='captcha'] img",
                    "div[class*='captcha'] img"
                };
                
                foreach (var selector in captchaSelectors)
                {
                    try
                    {
                        var elemento = await _page.QuerySelectorAsync(selector);
                        if (elemento != null && await elemento.IsVisibleAsync())
                        {
                            Log($"✅ Imagem CAPTCHA encontrada com seletor: {selector}");
                            var screenshot = await elemento.ScreenshotAsync();
                            return screenshot;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"⚠️ Erro com seletor {selector}: {ex.Message}");
                    }
                }
                
                // Se não encontrou imagem específica, tenta capturar a área ao redor do campo
                var captchaField = await _page.QuerySelectorAsync("#textVerificacao");
                if (captchaField != null)
                {
                    // Tenta encontrar a imagem do CAPTCHA próxima ao campo
                    var parentDiv = await captchaField.EvaluateAsync<string>("el => el.closest('div')?.id || ''");
                    if (!string.IsNullOrEmpty(parentDiv))
                    {
                        var parentElement = await _page.QuerySelectorAsync($"#{parentDiv}");
                        if (parentElement != null)
                        {
                            var screenshot = await parentElement.ScreenshotAsync();
                            return screenshot;
                        }
                    }
                }
                
                // Fallback: captura a tela inteira
                Log("⚠️ Imagem CAPTCHA não encontrada, capturando tela inteira...");
                var screenshotFull = await _page.ScreenshotAsync();
                return screenshotFull;
            }
            catch (Exception ex)
            {
                Log($"❌ Erro ao capturar CAPTCHA: {ex.Message}");
                return null;
            }
        }
        
        public async Task RecarregarCaptcha()
        {
            try
            {
                if (_page == null) return;
                
                // Tenta encontrar o botão de refresh do CAPTCHA
                var refreshSelectors = new[]
                {
                    "button:has-text('refresh')",
                    "button:has-text('Refresh')",
                    "button:has-text('Atualizar')",
                    "button:has-text('Novo')",
                    "img[alt*='refresh']",
                    "a[onclick*='refresh']",
                    "a[href*='refresh']"
                };
                
                foreach (var selector in refreshSelectors)
                {
                    try
                    {
                        var btnRefresh = await _page.QuerySelectorAsync(selector);
                        if (btnRefresh != null && await btnRefresh.IsVisibleAsync())
                        {
                            await btnRefresh.ClickAsync();
                            Log("🔄 CAPTCHA atualizado via botão de refresh!");
                            await Task.Delay(1500);
                            return;
                        }
                    }
                    catch { }
                }
                
                // Se não encontrou botão, recarrega a página
                Log("🔄 Recarregando página para novo CAPTCHA...");
                await _page.ReloadAsync();
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                Log($"⚠️ Erro ao recarregar CAPTCHA: {ex.Message}");
            }
        }
    }
}