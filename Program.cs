using CadastroProducaoCRE.Views;

namespace CadastroProducaoCRE;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}