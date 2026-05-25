using Microsoft.Playwright;
using CadastroProducaoCRE.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace CadastroProducaoCRE.Services
{
    public class ScraperService
    {
        private readonly IPage _page;
        private readonly Action<string> _log;
        
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
                await _page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
                await Task.Delay(3000);
                Log("✅ Página de cadastro carregada");
                return true;
            }
            catch (Exception ex)
            {
                Log($"❌ Erro ao acessar página: {ex.Message}");
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
                    Log($"✅ DC preenchida: {dc}");
                }
                
                var seqField = await _page.QuerySelectorAsync("#Sequencial");
                if (seqField != null)
                {
                    await seqField.ClickAsync();
                    await seqField.PressAsync("Control+A");
                    await seqField.PressAsync("Delete");
                    await seqField.FillAsync(sequencial);
                    Log($"✅ Sequencial preenchido: {sequencial}");
                }
                
                var btnBuscar = await _page.QuerySelectorAsync("#btnBusca");
                if (btnBuscar != null)
                {
                    await btnBuscar.ClickAsync();
                    Log("🔍 Botão Pesquisar clicado");
                    await Task.Delay(3000);
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Log($"❌ Erro ao pesquisar: {ex.Message}");
                return false;
            }
        }
        
        // 3. Preencher recursos
        public async Task<bool> PreencherRecursos(string recurso, string fornecimento, string contrato)
        {
            try
            {
                Log($"📝 Preenchendo recursos - Recurso: {recurso}, Fornecimento: {fornecimento}, Contrato: {contrato}");
                
                // Recurso
                var recursoValue = recurso == "Material" ? "MT" : "SR";
                await _page.SelectOptionAsync("#ddlTipoRecurso", recursoValue);
                
                // Fornecimento
                var fornecimentoValue = fornecimento == "Oi" ? "P" : "T";
                await _page.SelectOptionAsync("#ddlOrigemFornecimento", fornecimentoValue);
                
                // Contrato
                await _page.SelectOptionAsync("#ddlModeloRecurso", contrato);
                
                // Confirmar recursos
                var btnConfirmar = await _page.QuerySelectorAsync("button:has-text('Confirmar Recursos')");
                if (btnConfirmar != null)
                {
                    await btnConfirmar.ClickAsync();
                    Log("✅ Recursos confirmados");
                    await Task.Delay(2000);
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Log($"❌ Erro ao preencher recursos: {ex.Message}");
                return false;
            }
        }
        
        // 4. Verificar se código existe na tabela
        public async Task<bool> VerificarCodigoExiste(string codigo)
        {
            try
            {
                var codigoBusca = codigo.Trim();
                Log($"🔍 Verificando se código {codigoBusca} existe...");
                
                await Task.Delay(1000);
                
                var celulas = await _page.QuerySelectorAllAsync("td");
                foreach (var celula in celulas)
                {
                    var texto = await celula.TextContentAsync();
                    if (!string.IsNullOrEmpty(texto) && texto.Contains(codigoBusca))
                    {
                        Log($"✅ Código {codigoBusca} encontrado!");
                        return true;
                    }
                }
                
                Log($"❌ Código {codigoBusca} não encontrado");
                return false;
            }
            catch (Exception ex)
            {
                Log($"⚠️ Erro ao verificar código: {ex.Message}");
                return false;
            }
        }
        
        // 5. Clicar no botão Novo para o código
        public async Task<bool> ClicarNovo(string codigo)
        {
            try
            {
                Log($"🔘 Clicando em Novo para o código: {codigo}");
                
                var botoesNovo = await _page.QuerySelectorAllAsync("input[value='Novo']");
                foreach (var btn in botoesNovo)
                {
                    var onclick = await btn.GetAttributeAsync("onclick");
                    if (!string.IsNullOrEmpty(onclick) && onclick.Contains(codigo))
                    {
                        await btn.ClickAsync();
                        Log($"✅ Botão Novo clicado para {codigo}");
                        await Task.Delay(2000);
                        return true;
                    }
                }
                
                Log($"❌ Botão Novo para {codigo} não encontrado");
                return false;
            }
            catch (Exception ex)
            {
                Log($"❌ Erro ao clicar em Novo: {ex.Message}");
                return false;
            }
        }
        
        // 6. Inserir quantidade
        public async Task<bool> InserirQuantidade(decimal quantidade)
        {
            try
            {
                Log($"📝 Inserindo quantidade: {quantidade}");
                
                var campo = await _page.QuerySelectorAsync("#Quantidade");
                if (campo != null)
                {
                    await campo.ClickAsync();
                    await campo.PressAsync("Control+A");
                    await campo.PressAsync("Delete");
                    await campo.FillAsync(quantidade.ToString().Replace(",", "."));
                    Log($"✅ Quantidade {quantidade} inserida");
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
                Log($"📝 Selecionando equipe: {equipe}");
                await _page.SelectOptionAsync("#IdEquipeContratada", equipe);
                Log($"✅ Equipe {equipe} selecionada");
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
                Log($"📝 Selecionando natureza: {natureza}");
                
                var naturezaMap = new Dictionary<string, string>
                {
                    { "Retirada", "RE" },
                    { "Instalação", "IN" },
                    { "Furto", "FU" }
                };
                
                var valor = naturezaMap.ContainsKey(natureza) ? naturezaMap[natureza] : natureza;
                await _page.SelectOptionAsync("#NaturezaProducao", valor);
                Log($"✅ Natureza {natureza} selecionada");
                return true;
            }
            catch (Exception ex)
            {
                Log($"❌ Erro ao selecionar natureza: {ex.Message}");
                return false;
            }
        }
        
        // 9. Clicar em Salvar
        public async Task<(bool sucesso, string mensagem)> ClicarSalvar(string codigo)
        {
            try
            {
                Log($"💾 Clicando em Salvar para {codigo}");
                
                var btnSalvar = await _page.QuerySelectorAsync("#btnSalvar");
                if (btnSalvar != null)
                {
                    await btnSalvar.ClickAsync();
                    Log("✅ Botão Salvar clicado");
                    await Task.Delay(3000);
                    
                    // Verificar mensagem de sucesso
                    var alertaSucesso = await _page.QuerySelectorAsync(".alert-success");
                    if (alertaSucesso != null && await alertaSucesso.IsVisibleAsync())
                    {
                        var mensagem = await alertaSucesso.TextContentAsync();
                        Log($"✅ Sucesso: {mensagem?.Trim()}");
                        return (true, mensagem?.Trim() ?? "Salvo com sucesso");
                    }
                    
                    return (true, "Salvo com sucesso");
                }
                
                return (false, "Botão Salvar não encontrado");
            }
            catch (Exception ex)
            {
                Log($"❌ Erro ao salvar: {ex.Message}");
                return (false, ex.Message);
            }
        }
        
        // 10. Clicar em Sair
        public async Task<bool> ClicarSair()
        {
            try
            {
                var btnSair = await _page.QuerySelectorAsync("#btnVoltar");
                if (btnSair != null)
                {
                    await btnSair.ClickAsync();
                    Log("🔒 Botão Sair clicado");
                    await Task.Delay(2000);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log($"⚠️ Erro ao clicar em Sair: {ex.Message}");
                return false;
            }
        }
        
        // 11. Inserir Item Não Orçado
        public async Task<bool> InserirItemNaoOrcado(string codigo)
        {
            try
            {
                Log($"📌 Inserindo Item Não Orçado para: {codigo}");
                
                var btnInserir = await _page.QuerySelectorAsync("#btnInserirRecurso");
                if (btnInserir != null)
                {
                    await btnInserir.ClickAsync();
                    Log("✅ Botão Inserir Item Não Orçado clicado");
                    await Task.Delay(2000);
                    
                    // Inserir código
                    var campoParam2 = await _page.QuerySelectorAsync("#param2");
                    if (campoParam2 != null)
                    {
                        await campoParam2.FillAsync(codigo);
                        Log($"✅ Código {codigo} inserido");
                    }
                    
                    // Filtrar
                    var btnFiltrar = await _page.QuerySelectorAsync("#btnFiltrar");
                    if (btnFiltrar != null)
                    {
                        await btnFiltrar.ClickAsync();
                        Log("🔍 Botão Filtrar clicado");
                        await Task.Delay(2000);
                    }
                    
                    // Selecionar radio
                    var radio = await _page.QuerySelectorAsync($"input[value='{codigo}']");
                    if (radio != null)
                    {
                        await radio.ClickAsync();
                        Log($"✅ Radio selecionado para {codigo}");
                    }
                    
                    // Confirmar
                    var btnConfirmar = await _page.QuerySelectorAsync("#btnConfirmar");
                    if (btnConfirmar != null)
                    {
                        await btnConfirmar.ClickAsync();
                        Log("✅ Confirmar clicado");
                        await Task.Delay(2000);
                    }
                    
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Log($"❌ Erro ao inserir Item Não Orçado: {ex.Message}");
                return false;
            }
        }
        
        // 12. Limpar formulário para próximo registro
        public async Task<bool> LimparFormulario()
        {
            try
            {
                var btnLimpar = await _page.QuerySelectorAsync("#btnLimpa");
                if (btnLimpar != null && await btnLimpar.IsEnabledAsync())
                {
                    await btnLimpar.ClickAsync();
                    Log("🧹 Formulário limpo");
                    await Task.Delay(2000);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log($"⚠️ Erro ao limpar formulário: {ex.Message}");
                return true; // Continua mesmo se não conseguir limpar
            }
        }
        
        // Método principal para processar um registro
        public async Task<RegistroProducao> ProcessarRegistro(RegistroProducao registro)
        {
            var inicio = DateTime.Now;
            registro.DataProcessamento = inicio;
            
            Log($"🔵 INICIANDO PROCESSAMENTO DO REGISTRO: DC={registro.DC}, Sequencial={registro.Sequencial}");
            try
            {
                Log("");
                Log("=".PadRight(50, '='));
                Log($"📌 Processando: DC={registro.DC}, Sequencial={registro.Sequencial}, Código={registro.Codigo}");
                Log("=".PadRight(50, '='));
                
                // 1. Pesquisar DC e Sequencial
                if (!await PesquisarDCSequencial(registro.DC, registro.Sequencial))
                {
                    registro.Status = "Erro";
                    registro.Mensagem = "Falha ao pesquisar DC/Sequencial";
                    return registro;
                }
                
                // 2. Preencher recursos
                if (!await PreencherRecursos(registro.Recurso, registro.Fornecimento, registro.Contrato))
                {
                    registro.Status = "Erro";
                    registro.Mensagem = "Falha ao preencher recursos";
                    return registro;
                }
                
                // 3. Verificar se código existe
                var codigoExiste = await VerificarCodigoExiste(registro.Codigo);
                
                if (codigoExiste)
                {
                    // Código existe - seguir fluxo normal
                    Log("✅ Código encontrado, seguindo fluxo normal...");
                    
                    if (!await ClicarNovo(registro.Codigo))
                    {
                        registro.Status = "Erro";
                        registro.Mensagem = "Falha ao clicar em Novo";
                        return registro;
                    }
                    
                    await InserirQuantidade(registro.Quantidade);
                    await SelecionarEquipe(registro.Equipe);
                    await SelecionarNatureza(registro.Natureza);
                    
                    var (sucesso, mensagem) = await ClicarSalvar(registro.Codigo);
                    if (sucesso)
                    {
                        registro.Status = "Sucesso";
                        registro.Mensagem = mensagem;
                    }
                    else
                    {
                        registro.Status = "Erro";
                        registro.Mensagem = mensagem;
                    }
                    
                    await ClicarSair();
                }
                else
                {
                    // Código não existe - inserir Item Não Orçado
                    Log("⚠️ Código não encontrado, inserindo Item Não Orçado...");
                    
                    if (!await InserirItemNaoOrcado(registro.Codigo))
                    {
                        registro.Status = "Erro";
                        registro.Mensagem = "Falha ao inserir Item Não Orçado";
                        return registro;
                    }
                    
                    await ClicarSair();
                    
                    // Aguarda e tenta novamente
                    await Task.Delay(2000);
                    
                    // Verifica se o código foi inserido
                    if (await VerificarCodigoExiste(registro.Codigo))
                    {
                        Log("✅ Código inserido com sucesso, continuando cadastro...");
                        
                        if (!await ClicarNovo(registro.Codigo))
                        {
                            registro.Status = "Erro";
                            registro.Mensagem = "Falha ao clicar em Novo após inserir item";
                            return registro;
                        }
                        
                        await InserirQuantidade(registro.Quantidade);
                        await SelecionarEquipe(registro.Equipe);
                        await SelecionarNatureza(registro.Natureza);
                        
                        var (sucesso, mensagem) = await ClicarSalvar(registro.Codigo);
                        if (sucesso)
                        {
                            registro.Status = "Sucesso";
                            registro.Mensagem = mensagem;
                        }
                        else
                        {
                            registro.Status = "Erro";
                            registro.Mensagem = mensagem;
                        }
                        
                        await ClicarSair();
                    }
                    else
                    {
                        registro.Status = "Erro";
                        registro.Mensagem = "Item Não Orçado não foi inserido corretamente";
                    }
                }
                
                // Limpar formulário para próximo registro
                await LimparFormulario();
                
                var fim = DateTime.Now;
                registro.DataProcessamento = fim;
                Log($"⏱️ Tempo de processamento: {(fim - inicio).TotalSeconds:F1} segundos");
                
                return registro;
            }
            catch (Exception ex)
            {
                registro.Status = "Erro";
                registro.Mensagem = ex.Message;
                registro.DataProcessamento = DateTime.Now;
                Log($"❌ Erro ao processar registro: {ex.Message}");
                return registro;
            }
        }
    }
}