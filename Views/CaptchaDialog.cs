using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CadastroProducaoCRE.Views
{
    public partial class CaptchaDialog : Form
    {
        private byte[] _captchaImage;
        private Func<Task<byte[]>> _refreshCaptchaFunc;
        
        public string CaptchaTexto { get; private set; } = "";
        
        public CaptchaDialog(byte[] captchaImage, Func<Task<byte[]>> refreshCaptchaFunc)
        {
            InitializeComponent();
            _captchaImage = captchaImage;
            _refreshCaptchaFunc = refreshCaptchaFunc;
            CarregarImagem();
        }
        
        private void InitializeComponent()
        {
            this.picCaptcha = new System.Windows.Forms.PictureBox();
            this.txtCaptcha = new System.Windows.Forms.TextBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.lblInstrucao = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picCaptcha)).BeginInit();
            this.SuspendLayout();
            
            // Form
            this.Text = "🔐 Resolução de CAPTCHA";
            this.Size = new System.Drawing.Size(450, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = System.Drawing.Color.White;
            
            // lblInstrucao
            this.lblInstrucao.Text = "Digite o código CAPTCHA exatamente como aparece na imagem:";
            this.lblInstrucao.Location = new System.Drawing.Point(20, 20);
            this.lblInstrucao.Size = new System.Drawing.Size(400, 30);
            this.lblInstrucao.Font = new System.Drawing.Font("Segoe UI", 10);
            
            // picCaptcha
            this.picCaptcha.Location = new System.Drawing.Point(20, 60);
            this.picCaptcha.Size = new System.Drawing.Size(400, 150);
            this.picCaptcha.BorderStyle = BorderStyle.FixedSingle;
            this.picCaptcha.BackColor = System.Drawing.Color.LightGray;
            this.picCaptcha.SizeMode = PictureBoxSizeMode.Zoom;
            
            // txtCaptcha
            this.txtCaptcha.Location = new System.Drawing.Point(20, 230);
            this.txtCaptcha.Size = new System.Drawing.Size(400, 27);
            this.txtCaptcha.Font = new System.Drawing.Font("Segoe UI", 12);
            this.txtCaptcha.TextAlign = HorizontalAlignment.Center;
            this.txtCaptcha.KeyPress += TxtCaptcha_KeyPress;
            
            // btnOk
            this.btnOk.Text = "✅ OK - Continuar";
            this.btnOk.Location = new System.Drawing.Point(20, 275);
            this.btnOk.Size = new System.Drawing.Size(190, 40);
            this.btnOk.BackColor = System.Drawing.Color.LightGreen;
            this.btnOk.FlatStyle = FlatStyle.Flat;
            this.btnOk.Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold);
            this.btnOk.Click += BtnOk_Click;
            
            // btnRefresh
            this.btnRefresh.Text = "🔄 Refrescar CAPTCHA";
            this.btnRefresh.Location = new System.Drawing.Point(230, 275);
            this.btnRefresh.Size = new System.Drawing.Size(190, 40);
            this.btnRefresh.BackColor = System.Drawing.Color.LightBlue;
            this.btnRefresh.FlatStyle = FlatStyle.Flat;
            this.btnRefresh.Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold);
            this.btnRefresh.Click += BtnRefresh_Click;
            
            // Adicionar controles
            this.Controls.AddRange(new Control[] {
                lblInstrucao, picCaptcha, txtCaptcha, btnOk, btnRefresh
            });
            
            ((System.ComponentModel.ISupportInitialize)(this.picCaptcha)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        
        private void CarregarImagem()
        {
            if (_captchaImage != null)
            {
                using (var ms = new MemoryStream(_captchaImage))
                {
                    var img = Image.FromStream(ms);
                    picCaptcha.Image = new Bitmap(img);
                }
            }
        }
        
        private async void BtnRefresh_Click(object? sender, EventArgs e)
        {
            if (_refreshCaptchaFunc != null)
            {
                btnRefresh.Enabled = false;
                btnRefresh.Text = "⏳ Atualizando...";
                
                _captchaImage = await _refreshCaptchaFunc();
                CarregarImagem();
                txtCaptcha.Clear();
                
                btnRefresh.Enabled = true;
                btnRefresh.Text = "🔄 Refrescar CAPTCHA";
            }
        }
        
        private void BtnOk_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCaptcha.Text))
            {
                MessageBox.Show("Por favor, digite o código CAPTCHA.", "Atenção", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            CaptchaTexto = txtCaptcha.Text.Trim();
            DialogResult = DialogResult.OK;
            Close();
        }
        
        private void TxtCaptcha_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                BtnOk_Click(sender, e);
            }
        }
        
        private PictureBox picCaptcha;
        private TextBox txtCaptcha;
        private Button btnOk;
        private Button btnRefresh;
        private Label lblInstrucao;
    }
}