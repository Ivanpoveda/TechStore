using System;
using System.Collections.Generic;

namespace TechStore.Models;

public partial class Garantium
{
    public int IdGarantia { get; set; }

    public DateTime FechaSolicitud { get; set; }

    public string? Motivo { get; set; }

    public string? Descripcion { get; set; }

    public string Estado { get; set; } = null!;

    public DateTime? FechaResolucion { get; set; }

    public int IdDetalleVenta { get; set; }

    public virtual DetalleVentum IdDetalleVentaNavigation { get; set; } = null!;
}
