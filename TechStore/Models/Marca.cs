using System;
using System.Collections.Generic;

namespace TechStore.Models;

public partial class Marca
{
    public int IdMarca { get; set; }

    public string Nombre { get; set; } = null!;

    public string? PaisOrigen { get; set; }

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
