using System;
using System.Windows.Forms;

namespace CadastroProducaoCRE.Views
{
    public partial class OTPDialog : Form
    {
        public OTPDialog()
        {
            InitializeComponent();
        }
        
        public string GetOTP()
        {
            return txtOTP.Text.Trim();
        }
        
        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (txtOTP.Text.Trim().Length == 6)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Por favor, digite um OTP válido de 6 dígitos.", "OTP Inválido", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        
        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
        
        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblInfo = new System.Windows.Forms.Label();
            this.txtOTP = new System.Windows.Forms.TextBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            
            // Form
            this.Text = "Autenticação de Dois Fatores (OTP)";
            this.Size = new System.Drawing.Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = System.Drawing.Color.White;
            
            // lblTitle
            this.lblTitle.Text = "🔐 Autenticação de Dois Fatores";
            this.lblTitle.Location = new System.Drawing.Point(20, 20);
            this.lblTitle.Size = new System.Drawing.Size(350, 30);
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold);
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            
            // lblInfo
            this.lblInfo.Text = "Digite o código OTP gerado pelo seu aplicativo\nde autenticação (Google Authenticator, etc.)\n\nO código será concatenado com sua senha para realizar o login.";
            this.lblInfo.Location = new System.Drawing.Point(20, 60);
            this.lblInfo.Size = new System.Drawing.Size(350, 80);
            this.lblInfo.Font = new System.Drawing.Font("Segoe UI", 9);
            this.lblInfo.ForeColor = System.Drawing.Color.Gray;
            
            // txtOTP
            this.txtOTP.Location = new System.Drawing.Point(20, 150);
            this.txtOTP.Size = new System.Drawing.Size(350, 27);
            this.txtOTP.Font = new System.Drawing.Font("Segoe UI", 12);
            this.txtOTP.TextAlign = HorizontalAlignment.Center;
            this.txtOTP.MaxLength = 6;
            this.txtOTP.PlaceholderText = "Digite os 6 dígitos do OTP";
            
            // btnOk
            this.btnOk.Text = "OK";
            this.btnOk.Location = new System.Drawing.Point(180, 195);
            this.btnOk.Size = new System.Drawing.Size(90, 30);
            this.btnOk.BackColor = System.Drawing.Color.LightGreen;
            this.btnOk.FlatStyle = FlatStyle.Flat;
            this.btnOk.Click += new System.EventHandler(this.BtnOk_Click);
            
            // btnCancel
            this.btnCancel.Text = "Cancelar";
            this.btnCancel.Location = new System.Drawing.Point(280, 195);
            this.btnCancel.Size = new System.Drawing.Size(90, 30);
            this.btnCancel.BackColor = System.Drawing.Color.LightCoral;
            this.btnCancel.FlatStyle = FlatStyle.Flat;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            
            // Adicionar controles
            this.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.lblTitle, this.lblInfo, this.txtOTP, this.btnOk, this.btnCancel
            });
            
            this.ResumeLayout(false);
        }
        
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.TextBox txtOTP;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
    }
}