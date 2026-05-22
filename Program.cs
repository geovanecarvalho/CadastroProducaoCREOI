using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CadastroProducaoCRE
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Verificar e instalar Playwright antes de iniciar a aplicação
            VerificarEInstalarPlaywright().GetAwaiter().GetResult();
            
            ApplicationConfiguration.Initialize();
            Application.Run(new Views.MainForm());
        }
        
        private static async Task VerificarEInstalarPlaywright()
        {
            var playwrightPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ms-playwright");
            
            // Verifica se os navegadores já estão instalados
            if (!Directory.Exists(playwrightPath) || Directory.GetDirectories(playwrightPath).Length == 0)
            {
                var result = MessageBox.Show(
                    "⚙️ Configuração Inicial Necessária\n\n" +
                    "O Playwright precisa ser configurado para funcionar corretamente.\n" +
                    "Isso irá baixar os navegadores necessários (aproximadamente 500MB).\n\n" +
                    "O download pode levar alguns minutos.\n\n" +
                    "Deseja continuar?",
                    "Configuração do Playwright",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    await InstallPlaywrightBrowsers();
                }
                else
                {
                    MessageBox.Show(
                        "A aplicação será encerrada. É necessário instalar o Playwright para continuar.",
                        "Instalação Necessária",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    Environment.Exit(0);
                }
            }
        }
        
        private static async Task InstallPlaywrightBrowsers()
        {
            try
            {
                // Criar um formulário de progresso
                var progressForm = new Form
                {
                    Text = "Instalando Playwright...",
                    Size = new System.Drawing.Size(400, 100),
                    StartPosition = FormStartPosition.CenterScreen,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false
                };
                
                var lblProgress = new Label
                {
                    Text = "Baixando navegadores... Isso pode levar alguns minutos.",
                    Location = new System.Drawing.Point(20, 20),
                    Size = new System.Drawing.Size(360, 25),
                    TextAlign = System.Drawing.ContentAlignment.MiddleCenter
                };
                
                var progressBar = new ProgressBar
                {
                    Location = new System.Drawing.Point(20, 55),
                    Size = new System.Drawing.Size(360, 25),
                    Style = ProgressBarStyle.Marquee
                };
                
                progressForm.Controls.Add(lblProgress);
                progressForm.Controls.Add(progressBar);
                progressForm.Show();
                
                // Criar script PowerShell
                var scriptPath = Path.GetTempFileName() + ".ps1";
                var scriptContent = @"
$ProgressPreference = 'SilentlyContinue'
$output = & playwright.ps1 install chromium 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error 'Falha na instalação'
    exit 1
}
Write-Host 'Instalação concluída com sucesso!'
";
                await File.WriteAllTextAsync(scriptPath, scriptContent);
                
                // Executar PowerShell
                var processInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                
                using var process = Process.Start(processInfo);
                if (process != null)
                {
                    await process.WaitForExitAsync();
                    
                    if (process.ExitCode != 0)
                    {
                        throw new Exception("Falha na instalação do Playwright");
                    }
                }
                
                progressForm.Close();
                
                MessageBox.Show(
                    "✅ Playwright configurado com sucesso!\n\nOs navegadores foram instalados e estão prontos para uso.",
                    "Instalação Concluída",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ Erro ao instalar Playwright: {ex.Message}\n\n" +
                    "Tente executar o programa como Administrador ou instale manualmente:\n" +
                    "1. Abra o PowerShell como Administrador\n" +
                    "2. Execute: playwright.ps1 install",
                    "Erro de Instalação",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }
    }
}