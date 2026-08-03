using System;
using System.Collections.Generic;

namespace TechStore.Models;

public partial class CompraProveedor
{
    public decimal IdCompra { get; set; }

    public DateTime FechaCompra { get; set; }

    public decimal? Total { get; set; }

    public string Estado { get; set; } = null!;

    public decimal IdProveedor { get; set; }

    public virtual ICollection<DetalleCompra> DetalleCompras { get; set; } = new List<DetalleCompra>();

    public virtual Proveedor IdProveedorNavigation { get; set; } = null!;
}
