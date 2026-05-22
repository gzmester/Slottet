namespace Application.DTOs.Resident;

public class PNMedicinResponseDto
{
    //Dto som returnere en PN medicin, bruges når residentcard skal hente dagens pn medicin for en beboer
    public int PNMedicinID { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime Time { get; set; }
}

