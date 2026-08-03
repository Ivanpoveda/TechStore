using System;
using System.Collections.Generic;

namespace TechStore.Models;

public partial class Proveedor
{
    public decimal IdProveedor { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Empresa { get; set; }

    public string? Telefono { get; set; }

    public string? Correo { get; set; }

    public string? SitioWeb { get; set; }

    public virtual ICollection<CompraProveedor> CompraProveedors { get; set; } = new List<CompraProveedor>();
}
