using System.ComponentModel.DataAnnotations;

namespace ProductGrpc.Server.Data;

/// <summary>
/// EF Core entity. Kept separate from the generated <see cref="ProductModel"/>
/// proto message so the database schema and the wire contract can evolve apart.
/// </summary>
public class Product
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Stock { get; set; }
}
