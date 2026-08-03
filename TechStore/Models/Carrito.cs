using System;
using System.Collections.Generic;

namespace TechStore.Models;

public partial class Carrito
{
    public decimal IdCarrito { get; set; }

    public DateTime FechaCreacion { get; set; }

    public string Estado { get; set; } = null!;

    public decimal IdUsuario { get; set; }

    public virtual ICollection<DetalleCarrito> DetalleCarritos { get; set; } = new List<DetalleCarrito>();

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
