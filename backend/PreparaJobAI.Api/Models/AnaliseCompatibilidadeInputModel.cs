using System.ComponentModel.DataAnnotations;

namespace PreparaJobAI.Api.Models
{
    public class AnaliseCompatibilidadeInputModel
    {
        [Required(ErrorMessage = "O texto do currículo é obrigatório para a análise de compatibilidade.")]
        [MinLength(100, ErrorMessage = "O texto do currículo parece muito curto para uma análise eficaz. Deve ter pelo menos 100 caracteres.")]
        public string TextoCurriculo { get; set; }

        [Required(ErrorMessage = "O texto da vaga é obrigatório para a análise de compatibilidade.")]
        [MinLength(100, ErrorMessage = "O texto da vaga parece muito curto para uma análise eficaz. Deve ter pelo menos 100 caracteres.")]
        public string TextoVaga { get; set; }
    }
}