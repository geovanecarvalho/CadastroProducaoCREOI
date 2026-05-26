using System;
using System.IO;
using System.Windows.Forms;

namespace CadastroProducaoCRE
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            
            // Apenas verifica se o Playwright está instalado (sem tentar instalar)
            VerificarPlaywright();
            
            Application.Run(new Views.MainForm());
        }
        
        private static void VerificarPlaywright()
        {
            var playwrightPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ms-playwright");
            
            var chromiumInstalled = false;
            
            if (Directory.Exists(playwrightPath))
            {
                var dirs = Directory.GetDirectories(playwrightPath, "chromium-*");
                chromiumInstalled = dirs.Length > 0;
            }
            
            if (!chromiumInstalled)
            {
                MessageBox.Show(
                    "⚠️ Playwright não encontrado!\n\n" +
                    "O programa precisa do Playwright para funcionar.\n\n" +
                    "Por favor, entre em contato com o suporte para instalação.\n\n" +
                    "E-mail: geovanehacker.io@gmail.com",
                    "Componente Ausente",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                
                // Opcional: não encerra o programa, apenas avisa
                // Environment.Exit(0);
            }
        }
    }
}