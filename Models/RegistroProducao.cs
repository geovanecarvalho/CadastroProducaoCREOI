namespace CadastroProducaoCRE.Models
{
    public class RegistroProducao
    {
        public string Centro { get; set; } = "";
        public string DC { get; set; } = "";
        public string Sequencial { get; set; } = "";
        public string Recurso { get; set; } = "";
        public string Fornecimento { get; set; } = "";
        public string Contrato { get; set; } = "";
        public string Natureza { get; set; } = "";
        public string Codigo { get; set; } = "";
        public decimal Quantidade { get; set; }
        public string Equipe { get; set; } = "";
        
        // Colunas do relatório
        public string Status { get; set; } = "Pendente";
        public string Mensagem { get; set; } = "";
        public DateTime? DataProcessamento { get; set; }
    }
}