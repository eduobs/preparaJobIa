using System.ComponentModel.DataAnnotations;

namespace PreparaJobAI.Api.Models
{
    public class VagaModel
    {
        [Required(ErrorMessage = "A descrição da vaga é obrigatória.")]
        [MinLength(30, ErrorMessage = "A descrição da vaga deve ter pelo menos 30 caracteres.")]
        [MaxLength(5000, ErrorMessage = "A descrição da vaga não pode exceder 5000 caracteres.")]
        public required string Descricao { get; set; }

        // Campo opcional para o link da vaga
        [Url(ErrorMessage = "O link da vaga fornecido não é uma URL válida.")]
        public string? Link { get; set; }
    }
}