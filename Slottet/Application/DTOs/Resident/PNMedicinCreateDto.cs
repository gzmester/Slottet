namespace Application.DTOs.Resident;

public class PNMedicinCreateDto
{
    //dto til at oprette PN medicin, bruges når man opretter en PN medicin for en beboer
    public string Type { get; set; } = string.Empty;
    public DateTime Time { get; set; }
    public int ResidentId { get; set; }
}

