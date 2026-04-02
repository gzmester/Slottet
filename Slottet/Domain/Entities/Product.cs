//  Domain entity er egentlig bare vores database-model, som den ser ud i koden. Den afspejler præcis hvad der ligger i tabellen i databasen.
//  Den bør IKKE sendes direkte til frontend her bruger vi DTOs til at "skjule" den rå entity og kun sende det nødvendige data ud.
// ============================================================

namespace Domain.Entities;

public class Product
{
    public int ProductID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAt { get; set; }
}
