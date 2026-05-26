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
            ApplicationConfiguration.Initialize();
            
            // Verificar Playwright antes de abrir a janela principal
            if (!VerificarPlaywright())
            {
                return;
            }
            
            Application.Run(new Views.MainForm());
        }
        
        private static bool VerificarPlaywright()
        {
            var playwrightPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ms-playwright");
            
            // Verificar se o Chromium já está instalado
            var chromiumPath = Path.Combine(playwrightPath, "chromium-*");
            var chromiumInstalled = false;
            
            if (Directory.Exists(playwrightPath))
            {
                var dirs = Directory.GetDirectories(playwrightPath, "chromium-*");
                if (dirs.Length > 0)
                {
                    chromiumInstalled = true;
                }
            }
            
            // Se já estiver instalado, prossegue
            if (chromiumInstalled)
            {
                return true;
            }
            
            // Mostrar formulário de instalação
            using (var installForm = new PlaywrightInstallForm())
            {
                installForm.ShowDialog();
                return installForm.InstalacaoSucesso;
            }
        }
    }
    
    public class PlaywrightInstallForm : Form
    {
        private Label lblStatus;
        private Label lblDetail;
        private ProgressBar progressBar;
        private RichTextBox txtLog;
        private Button btnCancel;
        private bool _isInstalling = true;
        private bool _success = false;
        
        public bool InstalacaoSucesso => _success;
        
        public PlaywrightInstallForm()
        {
            InitializeComponent();
            StartInstallation();
        }
        
        private void InitializeComponent()
        {
            this.Text = "Configuração do Playwright - Cadastro Producao CRE";
            this.Size = new System.Drawing.Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = System.Drawing.Color.White;
            
            // Título
            var lblTitle = new Label();
            lblTitle.Text = "⚙️ Configuração Inicial do Sistema";
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold);
            lblTitle.Location = new System.Drawing.Point(20, 20);
            lblTitle.Size = new System.Drawing.Size(560, 30);
            lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            
            // Status
            lblStatus = new Label();
            lblStatus.Text = "Preparando instalação...";
            lblStatus.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            lblStatus.Location = new System.Drawing.Point(20, 65);
            lblStatus.Size = new System.Drawing.Size(560, 25);
            lblStatus.ForeColor = System.Drawing.Color.Blue;
            
            // Progress Bar
            progressBar = new ProgressBar();
            progressBar.Location = new System.Drawing.Point(20, 100);
            progressBar.Size = new System.Drawing.Size(560, 30);
            progressBar.Style = ProgressBarStyle.Marquee;
            
            // Label detalhe
            lblDetail = new Label();
            lblDetail.Text = "Por favor, aguarde...";
            lblDetail.Location = new System.Drawing.Point(20, 140);
            lblDetail.Size = new System.Drawing.Size(560, 20);
            lblDetail.ForeColor = System.Drawing.Color.Gray;
            
            // Log
            var lblLog = new Label();
            lblLog.Text = "📋 Log da instalação:";
            lblLog.Location = new System.Drawing.Point(20, 170);
            lblLog.Size = new System.Drawing.Size(560, 20);
            lblLog.Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold);
            
            txtLog = new RichTextBox();
            txtLog.Location = new System.Drawing.Point(20, 195);
            txtLog.Size = new System.Drawing.Size(560, 230);
            txtLog.ReadOnly = true;
            txtLog.BackColor = System.Drawing.Color.Black;
            txtLog.ForeColor = System.Drawing.Color.LightGreen;
            txtLog.Font = new System.Drawing.Font("Consolas", 9);
            txtLog.BorderStyle = BorderStyle.FixedSingle;
            
            // Botão Cancelar
            btnCancel = new Button();
            btnCancel.Text = "Cancelar";
            btnCancel.Size = new System.Drawing.Size(100, 35);
            btnCancel.Location = new System.Drawing.Point(480, 440);
            btnCancel.BackColor = System.Drawing.Color.LightCoral;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Click += (s, e) => 
            {
                _isInstalling = false;
                this.Close();
            };
            
            this.Controls.AddRange(new Control[] {
                lblTitle, lblStatus, progressBar, lblDetail, lblLog, txtLog, btnCancel
            });
        }
        
        private async void StartInstallation()
        {
            try
            {
                await InstallPlaywrightOnlyChromium();
            }
            catch (Exception ex)
            {
                AddLog($"❌ ERRO: {ex.Message}");
                lblStatus.Text = "❌ Falha na instalação!";
                lblStatus.ForeColor = System.Drawing.Color.Red;
                progressBar.Style = ProgressBarStyle.Blocks;
                progressBar.Value = 100;
                
                var result = MessageBox.Show(
                    $"Erro na instalação do Playwright:\n\n{ex.Message}\n\n" +
                    "Deseja tentar instalar manualmente?",
                    "Erro de Instalação",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error);
                
                if (result == DialogResult.Yes)
                {
                    InstallManually();
                }
                
                _success = false;
                btnCancel.Text = "Fechar";
            }
        }
        
        private async Task InstallPlaywrightOnlyChromium()
        {
            AddLog("🔧 Iniciando instalação do Playwright...");
            AddLog("📦 Instalando apenas Chromium (navegador mais rápido)...");
            
            lblStatus.Text = "Baixando Chromium... (aproximadamente 300MB)";
            lblStatus.ForeColor = System.Drawing.Color.Blue;
            lblDetail.Text = "Isso pode levar alguns minutos. Por favor, aguarde...";
            
            progressBar.Style = ProgressBarStyle.Marquee;
            
            // Criar script PowerShell otimizado apenas para Chromium
            var scriptContent = @"
$ErrorActionPreference = 'Stop'
Write-Host 'Iniciando instalacao do Playwright...'
Write-Host 'Instalando apenas Chromium...'

# Definir caminho do Playwright
$playwrightPath = Join-Path $env:USERPROFILE '.playwright'

# Instalar apenas Chromium
& playwright.ps1 install chromium

if ($LASTEXITCODE -eq 0) {
    Write-Host 'Instalacao concluida com sucesso!'
    exit 0
} else {
    Write-Host 'Erro na instalacao!'
    exit 1
}
";
            
            var scriptPath = Path.Combine(Path.GetTempPath(), $"playwright_install_{DateTime.Now.Ticks}.ps1");
            await File.WriteAllTextAsync(scriptPath, scriptContent);
            
            AddLog($"📝 Script criado: {scriptPath}");
            
            var processInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            
            using (var process = new Process())
            {
                process.StartInfo = processInfo;
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        AddLog(e.Data);
                    }
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        AddLog($"⚠️ {e.Data}");
                    }
                };
                
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                
                // Aguardar conclusão com timeout de 10 minutos
                var timeout = TimeSpan.FromMinutes(10);
                var completed = await Task.Run(() => process.WaitForExit((int)timeout.TotalMilliseconds));
                
                if (!completed)
                {
                    process.Kill();
                    throw new Exception("Timeout de 10 minutos excedido. Verifique sua conexão com a internet.");
                }
                
                if (process.ExitCode != 0)
                {
                    throw new Exception($"Falha na instalação. Código de erro: {process.ExitCode}");
                }
            }
            
            // Limpar script temporário
            try { File.Delete(scriptPath); } catch { }
            
            AddLog("✅ Instalação concluída com sucesso!");
            lblStatus.Text = "✅ Instalação concluída!";
            lblStatus.ForeColor = System.Drawing.Color.Green;
            progressBar.Style = ProgressBarStyle.Blocks;
            progressBar.Value = 100;
            
            await Task.Delay(1000);
            _success = true;
            
            MessageBox.Show(
                "✅ Playwright configurado com sucesso!\n\n" +
                "O Chromium foi instalado e está pronto para uso.\n\n" +
                "O programa será iniciado.",
                "Instalação Concluída",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            
            this.Close();
        }
        
        private void AddLog(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => AddLog(message)));
                return;
            }
            
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            txtLog.AppendText($"[{timestamp}] {message}\n");
            txtLog.ScrollToCaret();
        }
        
        private void InstallManually()
        {
            var result = MessageBox.Show(
                "Deseja abrir o PowerShell para instalar manualmente?\n\n" +
                "Digite o comando: playwright.ps1 install chromium",
                "Instalação Manual",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
        }
    }
}