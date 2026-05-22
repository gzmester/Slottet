using System;
using Domain.Entities;

namespace Application.DTOs.Resident;

public class ResidentResponseDto
{
    public int ResidentID { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty;
    public string Mood { get; set; } = "Neutral";
    public string ShoppingDay { get; set; } = string.Empty;
    public string Payment { get; set; } = string.Empty;

    public int LocationID { get; set; }
    public string Location { get; set; } = string.Empty;

    public List<MedicinDto> Medicins { get; set; } = new();
    public List<PNMedicinResponseDto> PNMedicins { get; set; } = new();
    public List<StatusDto> Statuses { get; set; } = new();
}
