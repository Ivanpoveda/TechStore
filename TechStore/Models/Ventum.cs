using System;
using System.Collections.Generic;

namespace TechStore.Models;

public partial class Ventum
{
    public int IdVenta { get; set; }

    public DateTime Fecha { get; set; }

    public decimal? Impuesto { get; set; }

    public decimal? Descuento { get; set; }

    public decimal? Total { get; set; }

    public string Estado { get; set; } = null!;

    public int IdUsuario { get; set; }

    public virtual ICollection<DetalleVentum> DetalleVenta { get; set; } = new List<DetalleVentum>();

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
