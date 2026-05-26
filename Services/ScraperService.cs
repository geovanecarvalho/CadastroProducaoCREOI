using Microsoft.Playwright;
using CadastroProducaoCRE.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Globalization;

namespace CadastroProducaoCRE.Services
{
    public class ScraperService
    {
        private readonly IPage _page;
        private readonly Action<string> _log;
        
        // Mapeamento de natureza - definido uma vez no início da classe
        private static readonly Dictionary<string, string> NaturezaMap = new Dictionary<string, string>
        {
            { "Retirada", "RE" },
            { "Instalação", "IN" },
            { "Furto", "FU" }
        };
        
        public ScraperService(IPage page, Action<string> logCallback)
        {
            _page = page;
            _log = logCallback;
        }
        
        private void Log(string message)
        {
            _log?.Invoke(message);
        }
        
        // 1. Acessar página de cadastro
        public async Task<bool> AcessarPaginaCadastro()
        {
            try
            {
                var url = "https://cre.oi.net.br/CRE_NEW/CadastroProducao/Index";
                Log($"🌍 Acessando: {url}");
                await _page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
                await Task.Delay(500);
                Log("✅ Página carregada");
                return true;
            }
            catch (Exception ex)
            {
                Log($"❌ Erro: {ex.Message}");
                return false;
            }
        }
        
        // 2. Pesquisar DC e Sequencial
        public async Task<bool> PesquisarDCSequencial(string dc, string sequencial)
        {
            try
            {
                Log($"📝 Pesquisando DC: {dc}, Sequencial: {sequencial}");
                
                var dcField = await _page.QuerySelectorAsync("#Projeto");
                if (dcField != null)
                {
                    await dcField.ClickAsync();
                    await dcField.PressAsync("Control+A");
                    await dcField.PressAsync("Delete");
                    await dcField.FillAsync(dc);
                }
                
                var seqField = await _page.QuerySelectorAsync("#Sequencial");
                if (seqField != null)
                {
                    await seqField.ClickAsync();
                    await seqField.PressAsync("Control+A");
                    await seqField.PressAsync("Delete");
                    await seqField.FillAsync(sequencial);
                }
                
                var btnBuscar = await _page.QuerySelectorAsync("#btnBusca");
                if (btnBuscar != null)
                {
                    await btnBuscar.ClickAsync();
                    await Task.Delay(500);
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
        
        // 3. Preencher recursos
        public async Task<bool> PreencherRecursos(string recurso, string fornecimento, string contrato)
        {
            try
            {
                var recursoValue = recurso == "Material" ? "MT" : "SR";
                await _page.SelectOptionAsync("#ddlTipoRecurso", recursoValue);
                
                var fornecimentoValue = fornecimento == "Oi" ? "P" : "T";
                await _page.SelectOptionAsync("#ddlOrigemFornecimento", fornecimentoValue);
                
                await _page.SelectOptionAsync("#ddlModeloRecurso", contrato);
                
                var btnConfirmar = await _page.QuerySelectorAsync("button:has-text('Confirmar Recursos')");
                if (btnConfirmar != null)
                {
                    await btnConfirmar.ClickAsync();
                    await Task.Delay(300);
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
        
        // 4. Verificar código existe
        public async Task<bool> VerificarCodigoExiste(string codigo)
        {
            try
            {
                var codigoBusca = codigo.Trim();
                
                var existe = await _page.EvaluateAsync<bool>(@"
                    (codigo) => {
                        const cells = document.querySelectorAll('td');
                        for (let cell of cells) {
                            if (cell.textContent && cell.textContent.includes(codigo)) {
                                return true;
                            }
                        }
                        return false;
                    }
                ", codigoBusca);
                
                if (existe)
                    Log($"✅ Código {codigoBusca} encontrado!");
                else
                    Log($"❌ Código {codigoBusca} não encontrado");
                
                return existe;
            }
            catch (Exception ex)
            {
                Log($"⚠️ Erro: {ex.Message}");
                return false;
            }
        }
        
        // 5. Clicar em Novo
        public async Task<bool> ClicarNovo(string codigo)
        {
            try
            {
                var botoesNovo = await _page.QuerySelectorAllAsync("input[value='Novo']");
                foreach (var btn in botoesNovo)
                {
                    var onclick = await btn.GetAttributeAsync("onclick");
                    if (!string.IsNullOrEmpty(onclick) && onclick.Contains(codigo))
                    {
                        await btn.ClickAsync();
                        await Task.Delay(300);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Log($"❌ Erro: {ex.Message}");
                return false;
            }
        }
        
        // 6. Inserir quantidade
        public async Task<bool> InserirQuantidade(decimal quantidade)
        {
            try
            {
                // Usa CultureInfo.InvariantCulture para garantir o ponto decimal
                var quantidadeStr = quantidade.ToString(CultureInfo.InvariantCulture);
                
                Log($"📝 Inserindo quantidade: {quantidade} -> '{quantidadeStr}'");
                
                var campo = await _page.QuerySelectorAsync("#Quantidade");
                if (campo != null)
                {
                    await campo.ClickAsync();
                    await campo.PressAsync("Control+A");
                    await campo.PressAsync("Delete");
                    await campo.FillAsync(quantidadeStr);
                    
                    // Verifica o valor após preenchimento
                    var valorPreenchido = await campo.InputValueAsync();
                    Log($"📝 Valor após preenchimento: {valorPreenchido}");
                    
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log($"❌ Erro ao inserir quantidade: {ex.Message}");
                return false;
            }
        }
        
        // 7. Selecionar equipe
        public async Task<bool> SelecionarEquipe(string equipe)
        {
            try
            {
                await _page.SelectOptionAsync("#IdEquipeContratada", equipe);
                return true;
            }
            catch (Exception ex)
            {
                Log($"❌ Erro ao selecionar equipe: {ex.Message}");
                return false;
            }
        }
        
        // 8. Selecionar natureza
        public async Task<bool> SelecionarNatureza(string natureza)
        {
            try
            {
                var valor = NaturezaMap.ContainsKey(natureza) ? NaturezaMap[natureza] : natureza;
                await _page.SelectOptionAsync("#NaturezaProducao", valor);
                return true;
            }
            catch (Exception ex)
            {
                Log($"❌ Erro ao selecionar natureza: {ex.Message}");
                return false;
            }
        }
        
        // 9. Clicar em Salvar
        public async Task<bool> ClicarSalvar()
        {
            try
            {
                var btnSalvar = await _page.QuerySelectorAsync("#btnSalvar");
                if (btnSalvar != null)
                {
                    await btnSalvar.ClickAsync();
                    await Task.Delay(500);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log($"❌ Erro ao salvar: {ex.Message}");
                return false;
            }
        }
        
        // 10. Inserir Item Não Orçado
        public async Task<bool> InserirItemNaoOrcado(string codigo)
        {
            try
            {
                var btnInserir = await _page.QuerySelectorAsync("#btnInserirRecurso");
                if (btnInserir != null)
                {
                    await btnInserir.ClickAsync();
                    await Task.Delay(500);
                    
                    var campoParam2 = await _page.QuerySelectorAsync("#param2");
                    if (campoParam2 != null)
                    {
                        await campoParam2.FillAsync(codigo);
                        
                        var btnFiltrar = await _page.QuerySelectorAsync("#btnFiltrar");
                        if (btnFiltrar != null)
                        {
                            await btnFiltrar.ClickAsync();
                            await Task.Delay(500);
                            
                            var radio = await _page.QuerySelectorAsync($"input[value='{codigo}']");
                            if (radio != null)
                            {
                                await radio.ClickAsync();
                                
                                var btnConfirmar = await _page.QuerySelectorAsync("#btnConfirmar");
                                if (btnConfirmar != null)
                                {
                                    await btnConfirmar.ClickAsync();
                                    await Task.Delay(500);
                                    return true;
                                }
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Log($"❌ Erro: {ex.Message}");
                return false;
            }
        }
        
        // 11. Clicar em Sair
        public async Task<bool> ClicarSair()
        {
            try
            {
                await _page.ClickAsync("#btnVoltar");
                await Task.Delay(300);
                return true;
            }
            catch (Exception ex)
            {
                Log($"⚠️ Erro ao sair: {ex.Message}");
                return false;
            }
        }
        
        // 12. Limpar formulário
        public async Task<bool> LimparFormulario()
        {
            try
            {
                var btnLimpar = await _page.QuerySelectorAsync("#btnLimpa");
                if (btnLimpar != null && await btnLimpar.IsEnabledAsync())
                {
                    await btnLimpar.ClickAsync();
                    await Task.Delay(300);
                }
                return true;
            }
            catch (Exception ex)
            {
                return true;
            }
        }
        
        // Método principal
        public async Task<RegistroProducao> ProcessarRegistro(RegistroProducao registro)
        {
            var inicio = DateTime.Now;
            registro.DataProcessamento = inicio;
            
            try
            {
                Log($"📌 Processando: DC={registro.DC}, Sequencial={registro.Sequencial}, Código={registro.Codigo}");
                
                // 1. Pesquisar
                if (!await PesquisarDCSequencial(registro.DC, registro.Sequencial))
                {
                    registro.Status = "Erro";
                    registro.Mensagem = "Falha ao pesquisar";
                    return registro;
                }
                
                // 2. Recursos
                if (!await PreencherRecursos(registro.Recurso, registro.Fornecimento, registro.Contrato))
                {
                    registro.Status = "Erro";
                    registro.Mensagem = "Falha ao preencher recursos";
                    return registro;
                }
                
                // 3. Verificar código
                var codigoExiste = await VerificarCodigoExiste(registro.Codigo);
                
                if (codigoExiste)
                {
                    // Fluxo normal - código já existe
                    if (!await ClicarNovo(registro.Codigo))
                    {
                        registro.Status = "Erro";
                        registro.Mensagem = "Falha ao clicar em Novo";
                        return registro;
                    }
                    
                    await InserirQuantidade(registro.Quantidade);
                    await SelecionarEquipe(registro.Equipe);
                    await SelecionarNatureza(registro.Natureza);
                    
                    if (await ClicarSalvar())
                    {
                        registro.Status = "Sucesso";
                        registro.Mensagem = "Cadastrado com sucesso";
                    }
                    else
                    {
                        registro.Status = "Erro";
                        registro.Mensagem = "Falha ao salvar";
                    }
                    
                    await ClicarSair();
                }
                else
                {
                    // Fluxo - código não existe, inserir Item Não Orçado
                    Log("⚠️ Código não encontrado, inserindo Item Não Orçado...");
                    
                    if (!await InserirItemNaoOrcado(registro.Codigo))
                    {
                        registro.Status = "Erro";
                        registro.Mensagem = "Falha ao inserir Item Não Orçado";
                        return registro;
                    }
                    
                    await ClicarSair();
                    await Task.Delay(500);
                    
                    // Verificar se o código foi inserido
                    if (await VerificarCodigoExiste(registro.Codigo))
                    {
                        if (!await ClicarNovo(registro.Codigo))
                        {
                            registro.Status = "Erro";
                            registro.Mensagem = "Falha ao clicar em Novo após inserir item";
                            return registro;
                        }
                        
                        await InserirQuantidade(registro.Quantidade);
                        await SelecionarEquipe(registro.Equipe);
                        await SelecionarNatureza(registro.Natureza);
                        
                        if (await ClicarSalvar())
                        {
                            registro.Status = "Sucesso";
                            registro.Mensagem = "Item Não Orçado cadastrado com sucesso";
                        }
                        else
                        {
                            registro.Status = "Erro";
                            registro.Mensagem = "Falha ao salvar após inserir item";
                        }
                        
                        await ClicarSair();
                    }
                    else
                    {
                        registro.Status = "Erro";
                        registro.Mensagem = "Item Não Orçado não foi inserido corretamente";
                    }
                }
                
                await LimparFormulario();
                
                var fim = DateTime.Now;
                registro.DataProcessamento = fim;
                Log($"⏱️ Tempo: {(fim - inicio).TotalSeconds:F1}s - Status: {registro.Status}");
                
                return registro;
            }
            catch (Exception ex)
            {
                registro.Status = "Erro";
                registro.Mensagem = ex.Message;
                Log($"❌ Erro: {ex.Message}");
                return registro;
            }
        }

        public async Task<bool> ClicarBotaoLogin()
        {
            try
            {
                Log("🔍 Procurando botão de login...");
                
                var loginSelectors = new[]
                {
                    "button:has-text('efetuar login')",
                    "button:has-text('Efetuar Login')",
                    "button:has-text('Entrar')",
                    "button:has-text('Login')",
                    "button[type='submit']",
                    "input[type='submit']",
                    ".btn-oi-new",
                    "button.btn-block",
                    "#btnLogin"
                };
                
                foreach (var selector in loginSelectors)
                {
                    try
                    {
                        var button = await _page.QuerySelectorAsync(selector);
                        if (button != null && await button.IsVisibleAsync())
                        {
                            Log($"✅ Botão encontrado: {selector}");
                            await button.ClickAsync();
                            Log("🖱️ Botão clicado automaticamente!");
                            await Task.Delay(3000);
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"⚠️ Erro com seletor {selector}: {ex.Message}");
                    }
                }
                
                Log("⚠️ Botão não encontrado, tentando Enter...");
                await _page.Keyboard.PressAsync("Enter");
                await Task.Delay(3000);
                return true;
            }
            catch (Exception ex)
            {
                Log($"❌ Erro ao clicar no botão: {ex.Message}");
                return false;
            }
        }
    }
}