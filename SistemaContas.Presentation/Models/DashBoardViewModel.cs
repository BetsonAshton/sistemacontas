﻿namespace SistemaContas.Presentation.Models
{
    public class DashBoardViewModel
    {
        /// <summary>
        /// Modelo de dados para as informações exibidas na página dashboard
        /// </summary>

        public string? DataIni { get; set; }
        public string? DataFim { get; set; }
        public decimal? TotalReceber { get; set; }
        public decimal? TotalPagar { get; set; }
        public decimal? Saldo { get; set; }
        public string? Situacao { get; set; }
    }
}
