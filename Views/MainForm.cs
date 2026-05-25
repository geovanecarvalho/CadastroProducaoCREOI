using System;
using System.Windows.Forms;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using CadastroProducaoCRE.Services;

namespace CadastroProducaoCRE.Views
{
    public partial class MainForm : Form
    {
        private string _caminhoArquivo = "";
        private ConfigManager _configManager;
        private ToolTip _toolTip;
        private PlaywrightService? _playwright;
        
        public MainForm()
        {
            InitializeComponent();
            _configManager = new ConfigManager(debug: true);
            LimparValidacaoCampos();
            CarregarConfiguracoes();
            AdicionarLog("✅ Aplicação iniciada");
            
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
            btnSalvarConfig.Enabled = habilitado;
            btnLimparConfig.Enabled = habilitado;
            btnSelecionarArquivo.Enabled = habilitado;
            btnLimparArquivo.Enabled = habilitado && !string.IsNullOrEmpty(_caminhoArquivo);
            btnLimparLog.Enabled = habilitado;
            btnExportarLog.Enabled = habilitado;
            btnMostrarSenha.Enabled = habilitado;
            txtUsuario.Enabled = habilitado;
            txtSenha.Enabled = habilitado;
            chkHeadless.Enabled = habilitado;
            btnIniciar.Enabled = habilitado;
            btnParar.Enabled = !habilitado;
            
            Color corDesabilitado = System.Drawing.Color.FromArgb(200, 200, 200);
            Color corTextoDesabilitado = System.Drawing.Color.DimGray;
            Color corHabilitado = System.Drawing.Color.LightBlue;
            Color corTextoHabilitado = System.Drawing.Color.Black;
            
            if (!habilitado)
            {
                btnSalvarConfig.BackColor = corDesabilitado;
                btnLimparConfig.BackColor = corDesabilitado;
                btnSalvarConfig.ForeColor = corTextoDesabilitado;
                btnLimparConfig.ForeColor = corTextoDesabilitado;
                btnSelecionarArquivo.BackColor = corDesabilitado;
                btnLimparArquivo.BackColor = corDesabilitado;
                btnSelecionarArquivo.ForeColor = corTextoDesabilitado;
                btnLimparArquivo.ForeColor = corTextoDesabilitado;
                btnLimparLog.BackColor = corDesabilitado;
                btnExportarLog.BackColor = corDesabilitado;
                btnLimparLog.ForeColor = corTextoDesabilitado;
                btnExportarLog.ForeColor = corTextoDesabilitado;
                btnMostrarSenha.BackColor = corDesabilitado;
                btnMostrarSenha.ForeColor = corTextoDesabilitado;
                btnIniciar.BackColor = corDesabilitado;
                btnIniciar.ForeColor = corTextoDesabilitado;
                btnParar.BackColor = System.Drawing.Color.LightCoral;
                btnParar.ForeColor = System.Drawing.Color.Black;
                btnParar.Text = "⏹ PARAR";
                txtUsuario.BackColor = corDesabilitado;
                txtSenha.BackColor = corDesabilitado;
                txtUsuario.ForeColor = corTextoDesabilitado;
                txtSenha.ForeColor = corTextoDesabilitado;
                chkHeadless.BackColor = corDesabilitado;
                chkHeadless.ForeColor = corTextoDesabilitado;
            }
            else
            {
                btnSalvarConfig.BackColor = corHabilitado;
                btnLimparConfig.BackColor = corHabilitado;
                btnSalvarConfig.ForeColor = corTextoHabilitado;
                btnLimparConfig.ForeColor = corTextoHabilitado;
                btnSelecionarArquivo.BackColor = corHabilitado;
                btnLimparArquivo.BackColor = corHabilitado;
                btnSelecionarArquivo.ForeColor = corTextoHabilitado;
                btnLimparArquivo.ForeColor = corTextoHabilitado;
                btnLimparLog.BackColor = corHabilitado;
                btnExportarLog.BackColor = corHabilitado;
                btnLimparLog.ForeColor = corTextoHabilitado;
                btnExportarLog.ForeColor = corTextoHabilitado;
                btnMostrarSenha.BackColor = corHabilitado;
                btnMostrarSenha.ForeColor = corTextoHabilitado;
                btnParar.BackColor = System.Drawing.Color.FromArgb(200, 150, 150);
                btnParar.ForeColor = System.Drawing.Color.DimGray;
                btnParar.Text = "⏹ PARAR";
                txtUsuario.BackColor = System.Drawing.Color.White;
                txtSenha.BackColor = System.Drawing.Color.White;
                txtUsuario.ForeColor = txtUsuario.Text == "Digite sua matrícula" ? System.Drawing.Color.Gray : System.Drawing.Color.Black;
                txtSenha.ForeColor = txtSenha.Text == "Digite sua senha" ? System.Drawing.Color.Gray : System.Drawing.Color.Black;
                chkHeadless.BackColor = System.Drawing.Color.Transparent;
                chkHeadless.ForeColor = System.Drawing.Color.Black;
                VerificarProntoParaIniciar();
            }
        }
        
        // ========== VALIDAÇÃO DE CAMPOS ==========
        
        private void LimparValidacaoCampos()
        {
            txtUsuario.Text = "Digite sua matrícula";
            txtUsuario.ForeColor = System.Drawing.Color.Gray;
            txtUsuario.BackColor = System.Drawing.Color.White;
            txtSenha.PasswordChar = '\0';
            txtSenha.Text = "Digite sua senha";
            txtSenha.ForeColor = System.Drawing.Color.Gray;
            txtSenha.BackColor = System.Drawing.Color.White;
        }
        
        private void ValidarCampoObrigatorio(TextBox textBox, string placeholder)
        {
            if (textBox.Text == placeholder || string.IsNullOrWhiteSpace(textBox.Text))
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
        
        // ========== EVENTOS DOS CAMPOS ==========
        
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
                ValidarCampoObrigatorio(txtUsuario, "Digite sua matrícula");
            VerificarProntoParaIniciar();
        }
        
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
                ValidarCampoObrigatorio(txtSenha, "Digite sua senha");
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
            
            SalvarConfiguracoes();
        }
        
        private void BtnLimparConfig_Click(object? sender, EventArgs e)
        {
            var resultado = MessageBox.Show("Tem certeza que deseja limpar todas as configurações salvas?\n\nIsso removerá usuário, senha e preferências.", 
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (resultado == DialogResult.Yes)
            {
                _configManager.ClearConfig();
                LimparValidacaoCampos();
                chkHeadless.Checked = false;
                txtUsuario.Focus();
                AdicionarLog("🗑️ Configurações limpas");
                VerificarProntoParaIniciar();
            }
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
                AdicionarLog($"📂 Arquivo selecionado: {Path.GetFileName(_caminhoArquivo)}");
                VerificarProntoParaIniciar();
                SalvarConfiguracoes();
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
            SalvarConfiguracoes();
        }
        
        private async void BtnIniciar_Click(object? sender, EventArgs e)
        {
            // Validação dos campos
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
            
            var urlLogin = "https://oilogin.oi.net.br/nidp/idff/sso?id=OiPasswordClassCorporativoId&sid=0&option=credential&sid=0&target=https%3A%2F%2Fcre.oi.net.br%2FCRE_NEW%2F";
            var urlIndex = "https://cre.oi.net.br/CRE_NEW/CadastroProducao/Index";
            var username = txtUsuario.Text;
            var password = txtSenha.Text;
            var headless = chkHeadless.Checked;
            
            AdicionarLog("🚀 Iniciando automação...");
            AdicionarLog($"👤 Usuário: {username}");
            AdicionarLog($"🖥️ Modo Headless: {(headless ? "Sim" : "Não")}");
            
            btnIniciar.Text = "⏳ PROCESSANDO...";
            SetBotoesHabilitados(false);
            ResetarProgresso();
            
            // Solicitar OTP
            AdicionarLog("🔐 Por favor, insira o token OTP...");
            var otpDialog = new OTPDialog();
            if (otpDialog.ShowDialog() != DialogResult.OK)
            {
                AdicionarLog("❌ Login cancelado - OTP não fornecido");
                FinalizarExecucao(false);
                return;
            }
            
            var otp = otpDialog.GetOTP();
            if (string.IsNullOrEmpty(otp) || otp.Length != 6)
            {
                AdicionarLog("❌ OTP inválido! Digite 6 dígitos.");
                FinalizarExecucao(false);
                return;
            }
            
            AdicionarLog($"🔐 OTP recebido: {'*' * otp.Length}");
            
            // Criar PlaywrightService
            _playwright = new PlaywrightService(headless);
            _playwright.OnLog += AdicionarLog;
            
            // Evento para novo OTP em caso de erro
            _playwright.OnRequisitarNovoOTP += async (tentativa) =>
            {
                string novoOtp = "";
                this.Invoke(new Action(() =>
                {
                    AdicionarLog($"🔐 Tentativa {tentativa} - Login falhou! Por favor, digite um novo OTP.");
                    var otpDialog = new OTPDialog();
                    if (otpDialog.ShowDialog() == DialogResult.OK)
                    {
                        novoOtp = otpDialog.GetOTP();
                        AdicionarLog($"🔐 Novo OTP recebido: {'*' * novoOtp.Length}");
                    }
                }));
                return await Task.FromResult(novoOtp);
            };
            
            // Inicia navegador e faz o fluxo completo
            var sucesso = await _playwright.IniciarNavegadorEFazerLogin(urlIndex, urlLogin, username, password, otp);
            
            if (sucesso)
            {
                AdicionarLog("🌐 Navegador logado e pronto!");
                AdicionarLog("✅ Página de cadastro carregada!");
                AdicionarLog("⏸️ Aguardando - Pressione Parar para encerrar");
            }
            else
            {
                AdicionarLog("❌ Falha ao realizar login!");
                FinalizarExecucao(false);
            }
        }
        
        private async void BtnParar_Click(object? sender, EventArgs e)
        {
            btnParar.Text = "⏸ PARANDO...";
            btnParar.BackColor = System.Drawing.Color.Orange;
            btnParar.ForeColor = System.Drawing.Color.Black;
            btnParar.Enabled = false;
            
            AdicionarLog("⚠️ Solicitando parada da automação...");
            
            if (_playwright != null)
            {
                AdicionarLog("🔒 Fechando navegador...");
                await _playwright.FecharNavegador();
                _playwright = null;
                AdicionarLog("✅ Navegador fechado com sucesso!");
            }
            
            await Task.Delay(500);
            SetBotoesHabilitados(true);
            btnIniciar.Text = "▶ INICIAR";
            AdicionarLog("✅ Automação interrompida pelo usuário");
            ResetarProgresso();
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
                File.WriteAllText(dialog.FileName, txtLog.Text);
                AdicionarLog($"💾 Log exportado: {Path.GetFileName(dialog.FileName)}");
                MessageBox.Show($"Log exportado com sucesso!\n\n{dialog.FileName}", "Sucesso", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        
        // ========== VALIDAÇÃO E UTILITÁRIOS ==========
        
        private void VerificarProntoParaIniciar()
        {
            if (txtUsuario == null || txtSenha == null || btnIniciar == null) return;
            
            bool temArquivo = !string.IsNullOrEmpty(_caminhoArquivo) && File.Exists(_caminhoArquivo);
            bool temUsuario = !string.IsNullOrWhiteSpace(txtUsuario.Text) && txtUsuario.Text != "Digite sua matrícula";
            bool temSenha = !string.IsNullOrWhiteSpace(txtSenha.Text) && txtSenha.Text != "Digite sua senha";
            bool pronto = temArquivo && temUsuario && temSenha;
            
            btnIniciar.Enabled = pronto;
            btnLimparConfig.Enabled = (txtUsuario.Text != "Digite sua matrícula" && !string.IsNullOrWhiteSpace(txtUsuario.Text)) ||
                                      (txtSenha.Text != "Digite sua senha" && !string.IsNullOrWhiteSpace(txtSenha.Text));
            
            if (pronto)
            {
                btnIniciar.Text = "▶ INICIAR";
                btnIniciar.BackColor = System.Drawing.Color.LightGreen;
                btnIniciar.ForeColor = System.Drawing.Color.Black;
                _toolTip?.SetToolTip(btnIniciar, "▶ Clique para iniciar a automação");
            }
            else
            {
                btnIniciar.BackColor = System.Drawing.Color.FromArgb(200, 200, 200);
                btnIniciar.ForeColor = System.Drawing.Color.DimGray;
                
                if ((!temUsuario || !temSenha) && !temArquivo)
                    btnIniciar.Text = "⚠️ AGUARDANDO ARQUIVO E CREDENCIAIS";
                else if (!temArquivo)
                    btnIniciar.Text = "⚠️ AGUARDANDO ARQUIVO EXCEL";
                else if (!temUsuario || !temSenha)
                    btnIniciar.Text = "⚠️ AGUARDANDO CREDENCIAIS";
                else
                    btnIniciar.Text = "▶ INICIAR";
            }
        }
        
        private void FinalizarExecucao(bool sucesso)
        {
            SetBotoesHabilitados(true);
            btnIniciar.Text = "▶ INICIAR";
            AdicionarLog(sucesso ? "✅ Execução concluída com sucesso!" : "❌ Execução finalizada com erros!");
            VerificarProntoParaIniciar();
        }
        
        public void OnAutomacaoConcluida(bool sucesso)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnAutomacaoConcluida(sucesso)));
                return;
            }
            FinalizarExecucao(sucesso);
        }
        
        private void BtnSuporte_Click(object? sender, EventArgs e)
        {
            string mensagem = 
                "🛠️ SUPORTE TÉCNICO\n\n📧 E-mail: geovanehacker.io@gmail.com\n\n📋 PARA ENVIAR O SUPORTE:\n• Clique em 'Baixar Log' para salvar o arquivo de log\n• Tire um print da tela do erro\n• Envie um e-mail para o endereço acima anexando o log e o print\n• Descreva o problema no e-mail\n\nℹ️ Informações do sistema serão incluídas automaticamente no log.\n\nClique em OK para copiar o e-mail automaticamente.";
            
            if (MessageBox.Show(mensagem, "Suporte Técnico", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
            {
                Clipboard.SetText("geovanehacker.io@gmail.com");
                MessageBox.Show("✅ E-mail copiado para a área de transferência!", "Copiado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        
        private void CarregarConfiguracoes()
        {
            try
            {
                var config = _configManager.LoadConfig();
                AdicionarLog("📂 Carregando configurações salvas...");
                
                // Carregar usuário
                if (config.ContainsKey("username") && config["username"] != null)
                {
                    var usuario = config["username"].ToString();
                    if (!string.IsNullOrEmpty(usuario))
                    {
                        txtUsuario.Text = usuario;
                        txtUsuario.ForeColor = System.Drawing.Color.Black;
                        AdicionarLog($"   ✅ Usuário carregado: {usuario}");
                    }
                }
                
                // Carregar senha (já vem descriptografada)
                if (config.ContainsKey("password") && config["password"] != null)
                {
                    var senha = config["password"].ToString();
                    if (!string.IsNullOrEmpty(senha))
                    {
                        txtSenha.Text = senha;
                        txtSenha.PasswordChar = '*';
                        txtSenha.ForeColor = System.Drawing.Color.Black;
                        AdicionarLog($"   ✅ Senha carregada");
                    }
                }
                
                // Carregar modo headless - CORRIGIDO
                if (config.ContainsKey("headless"))
                {
                    try
                    {
                        var headlessValue = config["headless"];
                        
                        if (headlessValue is bool boolValue)
                        {
                            chkHeadless.Checked = boolValue;
                        }
                        else if (headlessValue is JsonElement jsonElement)
                        {
                            chkHeadless.Checked = jsonElement.GetBoolean();
                        }
                        else
                        {
                            chkHeadless.Checked = Convert.ToBoolean(headlessValue);
                        }
                        AdicionarLog($"   ✅ Headless: {chkHeadless.Checked}");
                    }
                    catch (Exception ex)
                    {
                        AdicionarLog($"   ⚠️ Erro ao carregar headless: {ex.Message}");
                        chkHeadless.Checked = false;
                    }
                }
                
                // Carregar último arquivo usado
                if (config.ContainsKey("last_file") && config["last_file"] != null)
                {
                    string lastFile = config["last_file"].ToString();
                    AdicionarLog($"   📁 Último arquivo do config: '{lastFile}'");
                    
                    if (!string.IsNullOrEmpty(lastFile) && File.Exists(lastFile))
                    {
                        _caminhoArquivo = lastFile;
                        lblArquivo.Text = _caminhoArquivo;
                        lblArquivo.ForeColor = System.Drawing.Color.Green;
                        btnLimparArquivo.Enabled = true;
                        AdicionarLog($"   ✅ Arquivo carregado: {Path.GetFileName(lastFile)}");
                    }
                    else if (!string.IsNullOrEmpty(lastFile))
                    {
                        AdicionarLog($"   ⚠️ Arquivo não encontrado: {lastFile}");
                    }
                }
                
                // Carregar geometria da janela - CORRIGIDO
                if (config.ContainsKey("window_geometry") && config["window_geometry"] != null)
                {
                    try
                    {
                        var geometryDict = new Dictionary<string, int>();
                        var geometryObj = config["window_geometry"];
                        
                        if (geometryObj is JsonElement jsonGeom)
                        {
                            foreach (var property in jsonGeom.EnumerateObject())
                            {
                                geometryDict[property.Name] = property.Value.GetInt32();
                            }
                        }
                        else if (geometryObj is Dictionary<string, object> dict)
                        {
                            foreach (var kvp in dict)
                            {
                                if (kvp.Value != null)
                                    geometryDict[kvp.Key] = Convert.ToInt32(kvp.Value);
                            }
                        }
                        
                        if (geometryDict.ContainsKey("x"))
                            this.Location = new System.Drawing.Point(geometryDict["x"], this.Location.Y);
                        if (geometryDict.ContainsKey("y"))
                            this.Location = new System.Drawing.Point(this.Location.X, geometryDict["y"]);
                        if (geometryDict.ContainsKey("width"))
                            this.Size = new System.Drawing.Size(geometryDict["width"], this.Size.Height);
                        if (geometryDict.ContainsKey("height"))
                            this.Size = new System.Drawing.Size(this.Size.Width, geometryDict["height"]);
                        
                        AdicionarLog($"   ✅ Geometria da janela carregada");
                    }
                    catch (Exception ex)
                    {
                        AdicionarLog($"   ⚠️ Erro ao carregar geometria: {ex.Message}");
                    }
                }
                
                VerificarProntoParaIniciar();
                AdicionarLog("✅ Configurações carregadas com sucesso");
            }
            catch (Exception ex)
            {
                AdicionarLog($"⚠️ Erro ao carregar configurações: {ex.Message}");
            }
        }
        
        private void SalvarConfiguracoes()
        {
            var config = new Dictionary<string, object>
            {
                ["username"] = txtUsuario.Text == "Digite sua matrícula" ? "" : txtUsuario.Text,
                ["password"] = txtSenha.Text == "Digite sua senha" ? "" : txtSenha.Text,
                ["headless"] = chkHeadless.Checked,
                ["last_file"] = _caminhoArquivo,
                ["window_geometry"] = new Dictionary<string, int>
                {
                    ["x"] = Location.X,
                    ["y"] = Location.Y,
                    ["width"] = Size.Width,
                    ["height"] = Size.Height
                }
            };
            
            if (_configManager.SaveConfig(config))
                AdicionarLog("💾 Configurações salvas com sucesso!");
            else
                AdicionarLog("❌ Erro ao salvar configurações");
        }
        
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            SalvarConfiguracoes();
        }
    }
}