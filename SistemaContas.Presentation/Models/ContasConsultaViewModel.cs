using System.ComponentModel.DataAnnotations;

namespace SistemaContas.Presentation.Models
{
    public class ContasConsultaViewModel
    {
        [Required(ErrorMessage ="Por favor, informe a data de início.")]
        public string? DataIni { get; set; }

        [Required(ErrorMessage ="Por favor, informe a data de término.")]
        public string? DataFim { get; set; }

        /// <summary>
        /// Lista para exibir os registros dentro do grid da página
        /// </summary>
        public List<ContasConsultaResultadoViewModel>? Resultado { get; set; }
    }
}
