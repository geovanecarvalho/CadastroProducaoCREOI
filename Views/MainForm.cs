using System;
using System.Windows.Forms;

namespace CadastroProducaoCRE.Views
{
    public partial class MainForm : Form
    {
        private string _caminhoArquivo = "";
        private ToolTip _toolTip;
        
        public MainForm()
        {
            InitializeComponent();
            LimparValidacaoCampos();
            AdicionarLog("✅ Aplicação iniciada");
            
            // Configurar ToolTip
            _toolTip = new ToolTip();
            _toolTip.InitialDelay = 100;
            _toolTip.ReshowDelay = 50;
            _toolTip.AutoPopDelay = 10000;
            _toolTip.ShowAlways = true;
            
            _toolTip.SetToolTip(btnIniciar, "⚠️ Aguardando dados para iniciar...");
            
            VerificarProntoParaIniciar();
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
        
        // ========== MÉTODOS PARA CONTROLAR ESTADO DOS BOTÕES ==========
        
        private void SetBotoesHabilitados(bool habilitado)
        {
            // Botões de configuração
            btnSalvarConfig.Enabled = habilitado;
            btnLimparConfig.Enabled = habilitado;
            
            // Botões de arquivo
            btnSelecionarArquivo.Enabled = habilitado;
            btnLimparArquivo.Enabled = habilitado && !string.IsNullOrEmpty(_caminhoArquivo);
            
            // Botões de log
            btnLimparLog.Enabled = habilitado;
            btnExportarLog.Enabled = habilitado;
            
            // Botão mostrar senha
            btnMostrarSenha.Enabled = habilitado;
            
            // CAMPOS DE TEXTO - Bloquear durante processamento
            txtUsuario.Enabled = habilitado;
            txtSenha.Enabled = habilitado;
            
            // Checkbox
            chkHeadless.Enabled = habilitado;
            
            // Botão iniciar (oposto ao estado de processamento)
            btnIniciar.Enabled = habilitado;
            
            // Botão parar (ativo apenas durante processamento)
            btnParar.Enabled = !habilitado;
            
            // Ajustar cores
            Color corDesabilitado = System.Drawing.Color.FromArgb(200, 200, 200);
            Color corTextoDesabilitado = System.Drawing.Color.DimGray;
            Color corHabilitado = System.Drawing.Color.LightBlue;
            Color corTextoHabilitado = System.Drawing.Color.Black;
            
            if (!habilitado) // Durante processamento
            {
                // Botões de configuração
                btnSalvarConfig.BackColor = corDesabilitado;
                btnLimparConfig.BackColor = corDesabilitado;
                btnSalvarConfig.ForeColor = corTextoDesabilitado;
                btnLimparConfig.ForeColor = corTextoDesabilitado;
                
                // Botões de arquivo
                btnSelecionarArquivo.BackColor = corDesabilitado;
                btnLimparArquivo.BackColor = corDesabilitado;
                btnSelecionarArquivo.ForeColor = corTextoDesabilitado;
                btnLimparArquivo.ForeColor = corTextoDesabilitado;
                
                // Botões de log
                btnLimparLog.BackColor = corDesabilitado;
                btnExportarLog.BackColor = corDesabilitado;
                btnLimparLog.ForeColor = corTextoDesabilitado;
                btnExportarLog.ForeColor = corTextoDesabilitado;
                
                // Botão mostrar senha
                btnMostrarSenha.BackColor = corDesabilitado;
                btnMostrarSenha.ForeColor = corTextoDesabilitado;
                
                // Botão iniciar
                btnIniciar.BackColor = corDesabilitado;
                btnIniciar.ForeColor = corTextoDesabilitado;
                
                // Botão parar (ativo, cor vermelha)
                btnParar.BackColor = System.Drawing.Color.LightCoral;
                btnParar.ForeColor = System.Drawing.Color.Black;
                btnParar.Text = "⏹ PARAR";
                
                // Campos de texto - opacos
                txtUsuario.BackColor = corDesabilitado;
                txtSenha.BackColor = corDesabilitado;
                txtUsuario.ForeColor = corTextoDesabilitado;
                txtSenha.ForeColor = corTextoDesabilitado;
                
                // Checkbox
                chkHeadless.BackColor = corDesabilitado;
                chkHeadless.ForeColor = corTextoDesabilitado;
            }
            else // Em espera
            {
                // Botões de configuração
                btnSalvarConfig.BackColor = corHabilitado;
                btnLimparConfig.BackColor = corHabilitado;
                btnSalvarConfig.ForeColor = corTextoHabilitado;
                btnLimparConfig.ForeColor = corTextoHabilitado;
                
                // Botões de arquivo
                btnSelecionarArquivo.BackColor = corHabilitado;
                btnLimparArquivo.BackColor = corHabilitado;
                btnSelecionarArquivo.ForeColor = corTextoHabilitado;
                btnLimparArquivo.ForeColor = corTextoHabilitado;
                
                // Botões de log
                btnLimparLog.BackColor = corHabilitado;
                btnExportarLog.BackColor = corHabilitado;
                btnLimparLog.ForeColor = corTextoHabilitado;
                btnExportarLog.ForeColor = corTextoHabilitado;
                
                // Botão mostrar senha - restaurar cor
                btnMostrarSenha.BackColor = corHabilitado;
                btnMostrarSenha.ForeColor = corTextoHabilitado;
                
                // Botão parar (desabilitado, cor opaca)
                btnParar.BackColor = System.Drawing.Color.FromArgb(200, 150, 150); // Vermelho opaco
                btnParar.ForeColor = System.Drawing.Color.DimGray;
                btnParar.Text = "⏹ PARAR";
                
                // Restaurar campos de texto
                txtUsuario.BackColor = System.Drawing.Color.White;
                txtSenha.BackColor = System.Drawing.Color.White;
                
                if (txtUsuario.Text == "Digite sua matrícula")
                {
                    txtUsuario.ForeColor = System.Drawing.Color.Gray;
                }
                else
                {
                    txtUsuario.ForeColor = System.Drawing.Color.Black;
                }
                
                if (txtSenha.Text == "Digite sua senha")
                {
                    txtSenha.ForeColor = System.Drawing.Color.Gray;
                }
                else
                {
                    txtSenha.ForeColor = System.Drawing.Color.Black;
                }
                
                // Restaurar checkbox
                chkHeadless.BackColor = System.Drawing.Color.Transparent;
                chkHeadless.ForeColor = System.Drawing.Color.Black;
                
                // O botão iniciar terá sua cor definida pelo VerificarProntoParaIniciar
                VerificarProntoParaIniciar();
            }
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
            VerificarProntoParaIniciar();
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
            VerificarProntoParaIniciar();
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
            
            // Após limpar, desabilitar o próprio botão
            btnLimparConfig.Enabled = false;
            btnLimparConfig.BackColor = System.Drawing.Color.FromArgb(200, 200, 200);
            btnLimparConfig.ForeColor = System.Drawing.Color.DimGray;
            
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
            
            // Mudar texto do botão iniciar
            btnIniciar.Text = "⏳ PROCESSANDO...";
            
            // Desabilitar botões e campos durante o processamento
            SetBotoesHabilitados(false);
            
            ResetarProgresso();
            
            // TODO: Chamar o controller para iniciar a automação
        }
                
        private void BtnParar_Click(object? sender, EventArgs e)
        {
            // Mudar texto e cor do botão parar durante o cancelamento
            btnParar.Text = "⏸ PARANDO...";
            btnParar.BackColor = System.Drawing.Color.Orange;
            btnParar.ForeColor = System.Drawing.Color.Black;
            btnParar.Enabled = false; // Desabilitar temporariamente para evitar múltiplos cliques
            
            AdicionarLog("⚠️ Parando automação...");
            
            // TODO: Chamar o controller para cancelar a automação
            
            // Simular um pequeno delay para o usuário ver a mudança
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 500;
            timer.Tick += (s, args) =>
            {
                timer.Stop();
                
                // Reabilitar botões e campos
                SetBotoesHabilitados(true);
                
                // Restaurar texto do botão iniciar
                btnIniciar.Text = "▶ INICIAR";
                
                AdicionarLog("✅ Automação interrompida pelo usuário");
                
                // Liberar o timer
                timer.Dispose();
            };
            timer.Start();
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
            // Verificar se o arquivo foi selecionado e existe
            bool temArquivo = !string.IsNullOrEmpty(_caminhoArquivo) && System.IO.File.Exists(_caminhoArquivo);
            
            // Verificar matrícula (ignorando placeholder)
            bool temUsuario = !string.IsNullOrWhiteSpace(txtUsuario.Text) && 
                            txtUsuario.Text != "Digite sua matrícula";
            
            // Verificar senha (ignorando placeholder)
            bool temSenha = !string.IsNullOrWhiteSpace(txtSenha.Text) && 
                            txtSenha.Text != "Digite sua senha";
            
            // Verificar se os campos estão vazios (com placeholders)
            bool camposVazios = (txtUsuario.Text == "Digite sua matrícula" || string.IsNullOrWhiteSpace(txtUsuario.Text)) &&
                                (txtSenha.Text == "Digite sua senha" || string.IsNullOrWhiteSpace(txtSenha.Text));
            
            bool pronto = temArquivo && temUsuario && temSenha;
            
            btnIniciar.Enabled = pronto;
            
            // Botão Limpar Configurações - desabilitado se os campos já estão vazios
            btnLimparConfig.Enabled = !camposVazios;
            
            // Ajustar cor do btnLimparConfig baseado no estado
            if (btnLimparConfig.Enabled)
            {
                btnLimparConfig.BackColor = System.Drawing.Color.LightBlue;
                btnLimparConfig.ForeColor = System.Drawing.Color.Black;
            }
            else
            {
                btnLimparConfig.BackColor = System.Drawing.Color.FromArgb(200, 200, 200);
                btnLimparConfig.ForeColor = System.Drawing.Color.DimGray;
            }
            
            // Atualizar cor do btnLimparArquivo baseado se há arquivo
            btnLimparArquivo.Enabled = temArquivo;
            if (temArquivo)
            {
                btnLimparArquivo.BackColor = System.Drawing.Color.LightBlue;
                btnLimparArquivo.ForeColor = System.Drawing.Color.Black;
            }
            else
            {
                btnLimparArquivo.BackColor = System.Drawing.Color.FromArgb(200, 200, 200);
                btnLimparArquivo.ForeColor = System.Drawing.Color.DimGray;
            }
            
            if (pronto)
            {
                btnIniciar.Text = "▶ INICIAR";
                btnIniciar.BackColor = System.Drawing.Color.LightGreen;
                btnIniciar.ForeColor = System.Drawing.Color.Black;
                _toolTip.SetToolTip(btnIniciar, "▶ Clique para iniciar a automação");
            }
            else
            {
                btnIniciar.BackColor = System.Drawing.Color.FromArgb(200, 200, 200);
                btnIniciar.ForeColor = System.Drawing.Color.DimGray;
                
                bool faltaCredenciais = !temUsuario || !temSenha;
                bool faltaArquivo = !temArquivo;
                
                if (faltaCredenciais && faltaArquivo)
                {
                    btnIniciar.Text = "⚠️ AGUARDANDO ARQUIVO E CREDENCIAIS";
                    _toolTip.SetToolTip(btnIniciar, "⚠️ Selecione um arquivo Excel e preencha matrícula e senha");
                }
                else if (faltaArquivo)
                {
                    btnIniciar.Text = "⚠️ AGUARDANDO ARQUIVO EXCEL";
                    _toolTip.SetToolTip(btnIniciar, "⚠️ Clique em 'Selecionar Arquivo Excel' para escolher uma planilha");
                }
                else if (faltaCredenciais)
                {
                    btnIniciar.Text = "⚠️ AGUARDANDO CREDENCIAIS";
                    _toolTip.SetToolTip(btnIniciar, "⚠️ Preencha sua matrícula e senha nos campos acima");
                }
                else
                {
                    btnIniciar.Text = "▶ INICIAR";
                    _toolTip.SetToolTip(btnIniciar, "⚠️ Verifique os campos obrigatórios");
                }
            }
        }
        
        private void FinalizarExecucao(bool sucesso)
        {
            // Reabilitar botões e campos
            SetBotoesHabilitados(true);
            
            // Restaurar texto do botão iniciar
            btnIniciar.Text = "▶ INICIAR";
            
            if (sucesso)
            {
                AdicionarLog("✅ Execução concluída com sucesso!");
            }
            else
            {
                AdicionarLog("❌ Execução finalizada com erros!");
            }
            VerificarProntoParaIniciar();
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

        private void BtnSuporte_Click(object? sender, EventArgs e)
        {
            string mensagem = 
                "🛠️ SUPORTE TÉCNICO\n\n" +
                "📧 E-mail: geovanehacker.io@gmail.com\n\n" +
                "📋 PARA ENVIAR O SUPORTE:\n" +
                "• Clique em 'Baixar Log' para salvar o arquivo de log\n" +
                "• Tire um print da tela do erro (Print Screen)\n" +
                "• Envie um e-mail para o endereço acima anexando o log e o print\n" +
                "• Descreva o problema no e-mail\n\n" +
                "ℹ️ Informações do sistema serão incluídas automaticamente no log.\n\n" +
                "Clique em OK para copiar o e-mail automaticamente.";
            
            if (MessageBox.Show(mensagem, "Suporte Técnico", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
            {
                Clipboard.SetText("geovanehacker.io@gmail.com");
                MessageBox.Show("✅ E-mail copiado para a área de transferência!", "Copiado", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
