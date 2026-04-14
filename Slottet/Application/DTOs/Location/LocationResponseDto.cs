namespace Application.DTOs.Location;

public class LocationResponseDto
{
    public int LocationID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int ZipCode { get; set; }
}
