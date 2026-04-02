
//  DTO eksempel data transfer object 
//  DTOs er det du SENDER og MODTAGER over API'et.
//  De skjuler interne felter (fx CreatedAt, PinCode osv.) og giver dig kontrol over hvad der går ind/ud.

//  Typisk har du:
//    ResponseDto  → hvad du sender TIL klienten (GET)
//    CreateDto    → hvad du modtager FRA klienten (POST)
//    UpdateDto    → hvad du modtager FRA klienten (PUT)
//  der er sikkert flere eksempler online at finde

namespace API.DTOs;

// Bruges til GET – det klienten modtager
public class ProductResponseDto
{
    public int ProductID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

// Bruges til POST – det klienten sender når de opretter
public class ProductCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

// Bruges til PUT – det klienten sender når de opdaterer
public class ProductUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}
