using ClosedXML.Excel;
using CadastroProducaoCRE.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CadastroProducaoCRE.Services
{
    public class ExcelService
    {
        private readonly Action<string> _log;
        
        public ExcelService(Action<string> logCallback)
        {
            _log = logCallback;
        }
        
        private void Log(string message)
        {
            _log?.Invoke(message);
        }
        
        public async Task<List<RegistroProducao>> LerPlanilha(string caminhoArquivo)
        {
            return await Task.Run(() =>
            {
                var registros = new List<RegistroProducao>();
                
                try
                {
                    Log($"📂 Lendo arquivo: {caminhoArquivo}");
                    
                    using (var workbook = new XLWorkbook(caminhoArquivo))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var rows = worksheet.RowsUsed().Skip(1); // Pular cabeçalho
                        
                        foreach (var row in rows)
                        {
                            var registro = new RegistroProducao
                            {
                                Centro = row.Cell(1).GetString(),
                                DC = row.Cell(2).GetString(),
                                Sequencial = row.Cell(3).GetString(),
                                Recurso = row.Cell(4).GetString(),
                                Fornecimento = row.Cell(5).GetString(),
                                Contrato = row.Cell(6).GetString(),
                                Natureza = row.Cell(7).GetString(),
                                Codigo = row.Cell(8).GetString(),
                                Quantidade = decimal.TryParse(row.Cell(9).GetString(), out var qtd) ? qtd : 0,
                                Equipe = row.Cell(10).GetString()
                            };
                            
                            registros.Add(registro);
                        }
                    }
                    
                    Log($"✅ {registros.Count} registros carregados");
                    return registros;
                }
                catch (Exception ex)
                {
                    Log($"❌ Erro ao ler planilha: {ex.Message}");
                    return registros;
                }
            });
        }
        
        public async Task<string> GerarRelatorio(List<RegistroProducao> registros)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var downloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                    var dataHora = DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss");
                    var nomeArquivo = $"relatorio_cadastro_{dataHora}.xlsx";
                    var caminhoCompleto = Path.Combine(downloadPath, nomeArquivo);
                    
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Cadastros");
                        
                        // Cabeçalho
                        worksheet.Cell(1, 1).Value = "CENTRO";
                        worksheet.Cell(1, 2).Value = "DC";
                        worksheet.Cell(1, 3).Value = "SEQUENCIAL";
                        worksheet.Cell(1, 4).Value = "RECURSO";
                        worksheet.Cell(1, 5).Value = "FORNECIMENTO";
                        worksheet.Cell(1, 6).Value = "CONTRATO";
                        worksheet.Cell(1, 7).Value = "NATUREZA";
                        worksheet.Cell(1, 8).Value = "CODIGO";
                        worksheet.Cell(1, 9).Value = "QUANTIDADE";
                        worksheet.Cell(1, 10).Value = "EQUIPE";
                        worksheet.Cell(1, 11).Value = "STATUS";
                        worksheet.Cell(1, 12).Value = "MENSAGEM";
                        worksheet.Cell(1, 13).Value = "DATA_PROCESSAMENTO";
                        
                        // Dados
                        for (int i = 0; i < registros.Count; i++)
                        {
                            var row = i + 2;
                            var r = registros[i];
                            worksheet.Cell(row, 1).Value = r.Centro;
                            worksheet.Cell(row, 2).Value = r.DC;
                            worksheet.Cell(row, 3).Value = r.Sequencial;
                            worksheet.Cell(row, 4).Value = r.Recurso;
                            worksheet.Cell(row, 5).Value = r.Fornecimento;
                            worksheet.Cell(row, 6).Value = r.Contrato;
                            worksheet.Cell(row, 7).Value = r.Natureza;
                            worksheet.Cell(row, 8).Value = r.Codigo;
                            worksheet.Cell(row, 9).Value = r.Quantidade;
                            worksheet.Cell(row, 10).Value = r.Equipe;
                            worksheet.Cell(row, 11).Value = r.Status;
                            worksheet.Cell(row, 12).Value = r.Mensagem;
                            worksheet.Cell(row, 13).Value = r.DataProcessamento?.ToString("dd/MM/yyyy HH:mm:ss") ?? "";
                        }
                        
                        worksheet.Columns().AdjustToContents();
                        workbook.SaveAs(caminhoCompleto);
                    }
                    
                    Log($"✅ Relatório salvo: {caminhoCompleto}");
                    return caminhoCompleto;
                }
                catch (Exception ex)
                {
                    Log($"❌ Erro ao gerar relatório: {ex.Message}");
                    return "";
                }
            });
        }
    }
}