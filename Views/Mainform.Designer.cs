namespace CadastroProducaoCRE.Views
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            // Cabeçalho
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblSubtitulo = new System.Windows.Forms.Label();
            
            this.grpConfig = new System.Windows.Forms.GroupBox();
            this.lblUsuario = new System.Windows.Forms.Label();
            this.txtUsuario = new System.Windows.Forms.TextBox();
            this.lblSenha = new System.Windows.Forms.Label();
            this.txtSenha = new System.Windows.Forms.TextBox();
            this.btnMostrarSenha = new System.Windows.Forms.Button();
            this.chkHeadless = new System.Windows.Forms.CheckBox();
            this.btnSalvarConfig = new System.Windows.Forms.Button();
            this.btnLimparConfig = new System.Windows.Forms.Button();
            
            this.grpExcel = new System.Windows.Forms.GroupBox();
            this.lblArquivo = new System.Windows.Forms.Label();
            this.btnSelecionarArquivo = new System.Windows.Forms.Button();
            this.btnLimparArquivo = new System.Windows.Forms.Button();
            
            this.grpLogs = new System.Windows.Forms.GroupBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.txtLog = new System.Windows.Forms.RichTextBox();
            this.btnLimparLog = new System.Windows.Forms.Button();
            this.btnExportarLog = new System.Windows.Forms.Button();
            
            this.grpExecucao = new System.Windows.Forms.GroupBox();
            this.btnIniciar = new System.Windows.Forms.Button();
            this.btnParar = new System.Windows.Forms.Button();
            
            this.lblVersao = new System.Windows.Forms.Label();
            
            this.SuspendLayout();
            
            // ========== CABEÇALHO ==========
            // pnlHeader
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(52, 73, 94);
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Size = new System.Drawing.Size(950, 145);
            this.pnlHeader.TabIndex = 0;
            
            // picLogo
            try
            {
                string pngPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "icone.png");
                if (System.IO.File.Exists(pngPath))
                {
                    this.picLogo.Image = System.Drawing.Image.FromFile(pngPath);
                    this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
                }
            }
            catch { }
            this.picLogo.Location = new System.Drawing.Point(20, 15);
            this.picLogo.Size = new System.Drawing.Size(100, 100);
            
            // lblTitulo
            this.lblTitulo.Text = "Cadastro de Produção CREOI";
            this.lblTitulo.Location = new System.Drawing.Point(120, 20);
            this.lblTitulo.Size = new System.Drawing.Size(450, 50);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 18, System.Drawing.FontStyle.Bold);
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            
            // lblSubtitulo
            this.lblSubtitulo.Text = "Automação Web - Cadastro de Serviços e Materiais";
            this.lblSubtitulo.Location = new System.Drawing.Point(140, 65);
            this.lblSubtitulo.Size = new System.Drawing.Size(500, 22);
            this.lblSubtitulo.ForeColor = System.Drawing.Color.LightGray;
            this.lblSubtitulo.Font = new System.Drawing.Font("Segoe UI", 11, System.Drawing.FontStyle.Italic);
            this.lblSubtitulo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            
            // Adicionar controles ao cabeçalho
            this.pnlHeader.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.picLogo, this.lblTitulo, this.lblSubtitulo
            });
            
            // ========== MAINFORM ==========
            this.Text = "Cadastro de Produção CREOI - Automação Web";
            this.Size = new System.Drawing.Size(970, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            
            // Carregar ícone da janela
            try
            {
                string icoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "icone.ico");
                if (System.IO.File.Exists(icoPath))
                {
                    this.Icon = new System.Drawing.Icon(icoPath);
                }
            }
            catch { }
            
            // ========== VERSÃO ==========
            this.lblVersao.Text = "Versão: v1.0.0";
            this.lblVersao.Location = new System.Drawing.Point(820, 150);
            this.lblVersao.Size = new System.Drawing.Size(110, 20);
            this.lblVersao.ForeColor = System.Drawing.Color.Gray;
            this.lblVersao.Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Italic);
            this.lblVersao.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            
            // ========== GRUPO CONFIGURAÇÕES ==========
            this.grpConfig.Text = "Configurações";
            this.grpConfig.Location = new System.Drawing.Point(15, 165);
            this.grpConfig.Size = new System.Drawing.Size(920, 150);
            this.grpConfig.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            
            this.lblUsuario.Text = "Matrícula:";
            this.lblUsuario.Location = new System.Drawing.Point(15, 32);
            this.lblUsuario.Size = new System.Drawing.Size(85, 25);
            this.lblUsuario.Font = new System.Drawing.Font("Segoe UI", 9);
            
            this.txtUsuario.Location = new System.Drawing.Point(105, 30);
            this.txtUsuario.Size = new System.Drawing.Size(300, 27);
            this.txtUsuario.Font = new System.Drawing.Font("Segoe UI", 9);
            
            this.lblSenha.Text = "Senha:";
            this.lblSenha.Location = new System.Drawing.Point(15, 68);
            this.lblSenha.Size = new System.Drawing.Size(60, 25);
            this.lblSenha.Font = new System.Drawing.Font("Segoe UI", 9);
            
            this.txtSenha.Location = new System.Drawing.Point(105, 66);
            this.txtSenha.Size = new System.Drawing.Size(300, 27);
            this.txtSenha.PasswordChar = '*';
            this.txtSenha.Font = new System.Drawing.Font("Segoe UI", 9);
            
            // txtUsuario
            this.txtUsuario.Text = "";
            this.txtUsuario.ForeColor = System.Drawing.Color.Gray;
            this.txtUsuario.Text = "Digite sua matrícula";
            this.txtUsuario.BackColor = System.Drawing.Color.White;
            this.txtUsuario.Enter += new System.EventHandler(this.TxtUsuario_Enter);
            this.txtUsuario.Leave += new System.EventHandler(this.TxtUsuario_Leave);
            this.txtUsuario.TextChanged += new System.EventHandler(this.TxtUsuario_TextChanged);

            // txtSenha
            this.txtSenha.Text = "";
            this.txtSenha.ForeColor = System.Drawing.Color.Gray;
            this.txtSenha.Text = "Digite sua senha";
            this.txtSenha.BackColor = System.Drawing.Color.White;
            this.txtSenha.PasswordChar = '\0'; // Inicialmente sem máscara para mostrar o placeholder
            this.txtSenha.Enter += new System.EventHandler(this.TxtSenha_Enter);
            this.txtSenha.Leave += new System.EventHandler(this.TxtSenha_Leave);
            this.txtSenha.TextChanged += new System.EventHandler(this.TxtSenha_TextChanged);

            this.btnMostrarSenha.Text = "👁️";
            this.btnMostrarSenha.Location = new System.Drawing.Point(410, 65);
            this.btnMostrarSenha.Size = new System.Drawing.Size(40, 30);
            this.btnMostrarSenha.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMostrarSenha.BackColor = System.Drawing.Color.LightGray;
            this.btnMostrarSenha.Click += new System.EventHandler(this.BtnMostrarSenha_Click);
            
            this.chkHeadless.Text = "Modo janela web oculta (Headless)";
            this.chkHeadless.Location = new System.Drawing.Point(15, 105);
            this.chkHeadless.Size = new System.Drawing.Size(300, 25);
            this.chkHeadless.Font = new System.Drawing.Font("Segoe UI", 9);
            
            this.btnSalvarConfig.Text = "💾 Salvar Configurações";
            this.btnSalvarConfig.Location = new System.Drawing.Point(560, 30);
            this.btnSalvarConfig.Size = new System.Drawing.Size(160, 35);
            this.btnSalvarConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSalvarConfig.BackColor = System.Drawing.Color.LightGray;
            this.btnSalvarConfig.Click += new System.EventHandler(this.BtnSalvarConfig_Click);
            
            this.btnLimparConfig.Text = "🗑️ Limpar Configurações";
            this.btnLimparConfig.Location = new System.Drawing.Point(730, 30);
            this.btnLimparConfig.Size = new System.Drawing.Size(160, 35);
            this.btnLimparConfig.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLimparConfig.BackColor = System.Drawing.Color.LightGray;
            this.btnLimparConfig.Click += new System.EventHandler(this.BtnLimparConfig_Click);
            
            this.grpConfig.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblUsuario, this.txtUsuario, this.lblSenha, this.txtSenha,
                this.btnMostrarSenha, this.chkHeadless, this.btnSalvarConfig, this.btnLimparConfig
            });
            
            // ========== GRUPO EXCEL ==========
            this.grpExcel.Text = "Carregar Dados (Excel)";
            this.grpExcel.Location = new System.Drawing.Point(15, 320);
            this.grpExcel.Size = new System.Drawing.Size(920, 110);
            this.grpExcel.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            
            this.lblArquivo.Text = "Nenhum arquivo selecionado";
            this.lblArquivo.Location = new System.Drawing.Point(15, 25);
            this.lblArquivo.Size = new System.Drawing.Size(700, 25);
            this.lblArquivo.ForeColor = System.Drawing.Color.Gray;
            this.lblArquivo.Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Italic);
            
            this.btnSelecionarArquivo.Text = "📂 Selecionar Arquivo Excel";
            this.btnSelecionarArquivo.Location = new System.Drawing.Point(15, 60);
            this.btnSelecionarArquivo.Size = new System.Drawing.Size(170, 35);
            this.btnSelecionarArquivo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSelecionarArquivo.BackColor = System.Drawing.Color.LightGray;
            this.btnSelecionarArquivo.Click += new System.EventHandler(this.BtnSelecionarArquivo_Click);
            
            this.btnLimparArquivo.Text = "🗑️ Limpar Seleção";
            this.btnLimparArquivo.Location = new System.Drawing.Point(195, 60);
            this.btnLimparArquivo.Size = new System.Drawing.Size(130, 35);
            this.btnLimparArquivo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLimparArquivo.BackColor = System.Drawing.Color.LightGray;
            this.btnLimparArquivo.Enabled = false;
            this.btnLimparArquivo.Click += new System.EventHandler(this.BtnLimparArquivo_Click);
            
            this.grpExcel.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblArquivo, this.btnSelecionarArquivo, this.btnLimparArquivo
            });
            
            // ========== GRUPO LOGS ==========
            this.grpLogs.Text = "Logs e Progresso";
            this.grpLogs.Location = new System.Drawing.Point(15, 435);
            this.grpLogs.Size = new System.Drawing.Size(920, 300);
            this.grpLogs.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            
            this.progressBar.Location = new System.Drawing.Point(15, 28);
            this.progressBar.Size = new System.Drawing.Size(890, 30);
            this.progressBar.Minimum = 0;
            this.progressBar.Maximum = 100;
            this.progressBar.Value = 0;
            
            this.txtLog.Location = new System.Drawing.Point(15, 65);
            this.txtLog.Size = new System.Drawing.Size(890, 180);
            this.txtLog.ReadOnly = true;
            this.txtLog.BackColor = System.Drawing.Color.Black;
            this.txtLog.ForeColor = System.Drawing.Color.LightGreen;
            this.txtLog.Font = new System.Drawing.Font("Consolas", 9);
            
            this.btnLimparLog.Text = "🧹 Limpar Log";
            this.btnLimparLog.Location = new System.Drawing.Point(15, 255);
            this.btnLimparLog.Size = new System.Drawing.Size(130, 35);
            this.btnLimparLog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLimparLog.BackColor = System.Drawing.Color.LightGray;
            this.btnLimparLog.Click += new System.EventHandler(this.BtnLimparLog_Click);
            
            this.btnExportarLog.Text = "💾 Baixar Log";
            this.btnExportarLog.Location = new System.Drawing.Point(155, 255);
            this.btnExportarLog.Size = new System.Drawing.Size(130, 35);
            this.btnExportarLog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportarLog.BackColor = System.Drawing.Color.LightGray;
            this.btnExportarLog.Click += new System.EventHandler(this.BtnExportarLog_Click);
            
            this.grpLogs.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.progressBar, this.txtLog, this.btnLimparLog, this.btnExportarLog
            });
            
            // ========== GRUPO EXECUÇÃO ==========
            this.grpExecucao.Text = "Controle de Execução";
            this.grpExecucao.Location = new System.Drawing.Point(15, 745);
            this.grpExecucao.Size = new System.Drawing.Size(920, 90);
            this.grpExecucao.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            
            this.btnIniciar.Text = "▶ INICIAR";
            this.btnIniciar.Location = new System.Drawing.Point(15, 28);
            this.btnIniciar.Size = new System.Drawing.Size(440, 45);
            this.btnIniciar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIniciar.BackColor = System.Drawing.Color.LightGreen;
            this.btnIniciar.Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);
            this.btnIniciar.Enabled = false;
            this.btnIniciar.Click += new System.EventHandler(this.BtnIniciar_Click);
            
            this.btnParar.Text = "⏹ PARAR";
            this.btnParar.Location = new System.Drawing.Point(465, 28);
            this.btnParar.Size = new System.Drawing.Size(440, 45);
            this.btnParar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnParar.BackColor = System.Drawing.Color.LightCoral;
            this.btnParar.Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);
            this.btnParar.Enabled = false;
            this.btnParar.Click += new System.EventHandler(this.BtnParar_Click);
            
            this.grpExecucao.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.btnIniciar, this.btnParar
            });
            
            // ========== ADICIONAR CONTROLES AO FORMULÁRIO ==========
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.pnlHeader, this.lblVersao, this.grpConfig, 
                this.grpExcel, this.grpLogs, this.grpExecucao
            });
            
            this.ResumeLayout(false);
        }
        
        // ========== DECLARAÇÃO DOS CONTROLES ==========
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.PictureBox picLogo;
        private System.Windows.Forms.Label lblTitulo;
        private System.Windows.Forms.Label lblSubtitulo;
        
        private System.Windows.Forms.GroupBox grpConfig;
        private System.Windows.Forms.Label lblUsuario;
        private System.Windows.Forms.TextBox txtUsuario;
        private System.Windows.Forms.Label lblSenha;
        private System.Windows.Forms.TextBox txtSenha;
        private System.Windows.Forms.Button btnMostrarSenha;
        private System.Windows.Forms.CheckBox chkHeadless;
        private System.Windows.Forms.Button btnSalvarConfig;
        private System.Windows.Forms.Button btnLimparConfig;
        
        private System.Windows.Forms.GroupBox grpExcel;
        private System.Windows.Forms.Label lblArquivo;
        private System.Windows.Forms.Button btnSelecionarArquivo;
        private System.Windows.Forms.Button btnLimparArquivo;
        
        private System.Windows.Forms.GroupBox grpLogs;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.RichTextBox txtLog;
        private System.Windows.Forms.Button btnLimparLog;
        private System.Windows.Forms.Button btnExportarLog;
        
        private System.Windows.Forms.GroupBox grpExecucao;
        private System.Windows.Forms.Button btnIniciar;
        private System.Windows.Forms.Button btnParar;
        
        private System.Windows.Forms.Label lblVersao;
    }
}