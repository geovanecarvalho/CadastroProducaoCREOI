using System;
using System.Windows.Forms;

namespace CadastroProducaoCRE.Views
{
    public partial class MainForm : Form
    {
        private string _caminhoArquivo = "";
        
        public MainForm()
        {
            InitializeComponent();
            LimparValidacaoCampos();
            AdicionarLog("✅ Aplicação iniciada");
        }
        
        public void AdicionarLog(string mensagem)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => AdicionarLog(mensagem)));
                return;
            }
            
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            txtLog.AppendText($"[{timestamp}] {mensagem}\n");
            txtLog.ScrollToCaret();
        }
        
        public void AtualizarProgresso(int current, int total)
        {
            if (progressBar.InvokeRequired)
            {
                progressBar.Invoke(new Action(() => AtualizarProgresso(current, total)));
                return;
            }
            
            if (total > 0)
            {
                var percent = (int)((double)current / total * 100);
                progressBar.Value = percent;
            }
        }
        
        public void ResetarProgresso()
        {
            if (progressBar.InvokeRequired)
            {
                progressBar.Invoke(new Action(ResetarProgresso));
                return;
            }
            
            progressBar.Value = 0;
        }
        
        // ========== VALIDAÇÃO DE CAMPOS ==========
        
        private void LimparValidacaoCampos()
        {
            // Resetar txtUsuario
            txtUsuario.Text = "Digite sua matrícula";
            txtUsuario.ForeColor = System.Drawing.Color.Gray;
            txtUsuario.BackColor = System.Drawing.Color.White;
            
            // Resetar txtSenha
            txtSenha.PasswordChar = '\0';
            txtSenha.Text = "Digite sua senha";
            txtSenha.ForeColor = System.Drawing.Color.Gray;
            txtSenha.BackColor = System.Drawing.Color.White;
        }
        
        private void ValidarCampoObrigatorio(TextBox textBox, string placeholder)
        {
            // Se o texto for o placeholder, considerar como vazio
            if (textBox.Text == placeholder)
            {
                textBox.BackColor = System.Drawing.Color.LightCoral;
                textBox.ForeColor = System.Drawing.Color.White;
                return;
            }
            
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.BackColor = System.Drawing.Color.LightCoral;
                textBox.ForeColor = System.Drawing.Color.White;
            }
            else
            {
                textBox.BackColor = System.Drawing.Color.White;
                textBox.ForeColor = System.Drawing.Color.Black;
            }
        }
        
        // ========== EVENTOS DO CAMPO MATRÍCULA ==========
        
        private void TxtUsuario_Enter(object? sender, EventArgs e)
        {
            if (txtUsuario.Text == "Digite sua matrícula")
            {
                txtUsuario.Text = "";
                txtUsuario.ForeColor = System.Drawing.Color.Black;
                txtUsuario.BackColor = System.Drawing.Color.White;
            }
        }
        
        private void TxtUsuario_Leave(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsuario.Text))
            {
                txtUsuario.Text = "Digite sua matrícula";
                txtUsuario.ForeColor = System.Drawing.Color.Gray;
                txtUsuario.BackColor = System.Drawing.Color.White;
            }
            else
            {
                ValidarCampoObrigatorio(txtUsuario, "Digite sua matrícula");
            }
        }
        
        private void TxtUsuario_TextChanged(object? sender, EventArgs e)
        {
            if (txtUsuario.Text != "Digite sua matrícula")
            {
                ValidarCampoObrigatorio(txtUsuario, "Digite sua matrícula");
            }
            VerificarProntoParaIniciar();
        }
        
        // ========== EVENTOS DO CAMPO SENHA ==========
        
        private void TxtSenha_Enter(object? sender, EventArgs e)
        {
            if (txtSenha.Text == "Digite sua senha")
            {
                txtSenha.Text = "";
                txtSenha.ForeColor = System.Drawing.Color.Black;
                txtSenha.BackColor = System.Drawing.Color.White;
                txtSenha.PasswordChar = '*';
            }
        }
        
        private void TxtSenha_Leave(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSenha.Text))
            {
                txtSenha.PasswordChar = '\0';
                txtSenha.Text = "Digite sua senha";
                txtSenha.ForeColor = System.Drawing.Color.Gray;
                txtSenha.BackColor = System.Drawing.Color.White;
            }
            else
            {
                ValidarCampoObrigatorio(txtSenha, "Digite sua senha");
            }
        }
        
        private void TxtSenha_TextChanged(object? sender, EventArgs e)
        {
            if (txtSenha.Text != "Digite sua senha")
            {
                ValidarCampoObrigatorio(txtSenha, "Digite sua senha");
            }
            VerificarProntoParaIniciar();
        }
        
        // ========== BOTÕES ==========
        
        private void BtnMostrarSenha_Click(object? sender, EventArgs e)
        {
            if (txtSenha.PasswordChar == '*')
            {
                txtSenha.PasswordChar = '\0';
                btnMostrarSenha.Text = "🙈 Ocultar";
            }
            else
            {
                txtSenha.PasswordChar = '*';
                btnMostrarSenha.Text = "👁️ Mostrar";
            }
        }
        
        private void BtnSalvarConfig_Click(object? sender, EventArgs e)
        {
            // Validar campos antes de salvar
            if (txtUsuario.Text == "Digite sua matrícula" || string.IsNullOrWhiteSpace(txtUsuario.Text))
            {
                AdicionarLog("❌ Não foi possível salvar: Matrícula é obrigatória!");
                txtUsuario.BackColor = System.Drawing.Color.LightCoral;
                txtUsuario.Text = "";
                txtUsuario.Focus();
                return;
            }
            
            if (txtSenha.Text == "Digite sua senha" || string.IsNullOrWhiteSpace(txtSenha.Text))
            {
                AdicionarLog("❌ Não foi possível salvar: Senha é obrigatória!");
                txtSenha.BackColor = System.Drawing.Color.LightCoral;
                txtSenha.Text = "";
                txtSenha.Focus();
                return;
            }
            
            AdicionarLog("💾 Configurações salvas");
        }
        
        private void BtnLimparConfig_Click(object? sender, EventArgs e)
        {
            LimparValidacaoCampos();
            chkHeadless.Checked = false;
            txtUsuario.Focus();
            AdicionarLog("🗑️ Configurações limpas");
            VerificarProntoParaIniciar();
        }
        
        private void BtnSelecionarArquivo_Click(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog();
            dialog.Title = "Selecionar Arquivo Excel";
            dialog.Filter = "Arquivos Excel|*.xlsx;*.xls|Todos os arquivos|*.*";
            
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _caminhoArquivo = dialog.FileName;
                lblArquivo.Text = _caminhoArquivo;
                lblArquivo.ForeColor = System.Drawing.Color.Green;
                btnLimparArquivo.Enabled = true;
                AdicionarLog($"📂 Arquivo selecionado: {System.IO.Path.GetFileName(_caminhoArquivo)}");
                VerificarProntoParaIniciar();
            }
        }
        
        private void BtnLimparArquivo_Click(object? sender, EventArgs e)
        {
            _caminhoArquivo = "";
            lblArquivo.Text = "Nenhum arquivo selecionado";
            lblArquivo.ForeColor = System.Drawing.Color.Gray;
            btnLimparArquivo.Enabled = false;
            AdicionarLog("🗑️ Arquivo removido");
            VerificarProntoParaIniciar();
        }
        
        private void BtnIniciar_Click(object? sender, EventArgs e)
        {
            // Validação final antes de iniciar
            if (txtUsuario.Text == "Digite sua matrícula" || string.IsNullOrWhiteSpace(txtUsuario.Text))
            {
                AdicionarLog("❌ Matrícula é obrigatória!");
                txtUsuario.BackColor = System.Drawing.Color.LightCoral;
                txtUsuario.Text = "";
                txtUsuario.Focus();
                return;
            }
            
            if (txtSenha.Text == "Digite sua senha" || string.IsNullOrWhiteSpace(txtSenha.Text))
            {
                AdicionarLog("❌ Senha é obrigatória!");
                txtSenha.BackColor = System.Drawing.Color.LightCoral;
                txtSenha.Text = "";
                txtSenha.Focus();
                return;
            }
            
            AdicionarLog("🚀 Iniciando automação...");
            btnIniciar.Enabled = false;
            btnParar.Enabled = true;
            ResetarProgresso();
        }
        
        private void BtnParar_Click(object? sender, EventArgs e)
        {
            AdicionarLog("⚠️ Parando automação...");
            btnParar.Enabled = false;
        }
        
        private void BtnLimparLog_Click(object? sender, EventArgs e)
        {
            txtLog.Clear();
            AdicionarLog("📝 Log limpo");
        }
        
        private void BtnExportarLog_Click(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog();
            dialog.Title = "Salvar Log";
            dialog.Filter = "Arquivos de texto|*.txt|Todos os arquivos|*.*";
            dialog.FileName = $"log_automacao_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                System.IO.File.WriteAllText(dialog.FileName, txtLog.Text);
                AdicionarLog($"💾 Log exportado: {System.IO.Path.GetFileName(dialog.FileName)}");
                MessageBox.Show($"Log exportado com sucesso!\n\n{dialog.FileName}", "Sucesso", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        
        // ========== VALIDAÇÃO E UTILITÁRIOS ==========
        
        private void VerificarProntoParaIniciar()
        {
            bool temArquivo = !string.IsNullOrEmpty(_caminhoArquivo) && System.IO.File.Exists(_caminhoArquivo);
            
            // Verificar usuário (ignorando placeholder)
            bool temUsuario = !string.IsNullOrWhiteSpace(txtUsuario.Text) && 
                              txtUsuario.Text != "Digite sua matrícula";
            
            // Verificar senha (ignorando placeholder)
            bool temSenha = !string.IsNullOrWhiteSpace(txtSenha.Text) && 
                            txtSenha.Text != "Digite sua senha";
            
            btnIniciar.Enabled = temArquivo && temUsuario && temSenha;
        }
        
        private void FinalizarExecucao(bool sucesso)
        {
            btnIniciar.Enabled = true;
            btnParar.Enabled = false;
            
            if (sucesso)
            {
                AdicionarLog("✅ Execução concluída com sucesso!");
            }
            else
            {
                AdicionarLog("❌ Execução finalizada com erros!");
            }
        }
        
        public void OnAutomacaoConcluida(bool sucesso)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => OnAutomacaoConcluida(sucesso)));
                return;
            }
            
            FinalizarExecucao(sucesso);
        }
    }
}