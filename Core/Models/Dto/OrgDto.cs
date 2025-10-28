using System.ComponentModel.DataAnnotations;
using CrmBack.Core.Models.Entities;

namespace CrmBack.Core.Models.Dto;

public class CreateOrgDto
{
    public string Name { get; set; } = string.Empty;
    public string INN { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Address { get; set; } = string.Empty;
}

public class ReadOrgDto
{
    public int OrgId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string INN { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Address { get; set; } = string.Empty;
}

public class UpdateOrgDto
{
    public string? Name { get; set; }
    public string? INN { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Address { get; set; }
}

public static class OrgMapper
{
    public static ReadOrgDto ToReadDto(this OrgEntity entity) => new()
    {
        OrgId = entity.OrgId,
        Name = entity.Name,
        INN = entity.Inn ?? "-",
        Latitude = entity.Latitude,
        Longitude = entity.Longitude,
        Address = entity.Address
    };

    public static OrgEntity ToEntity(this CreateOrgDto dto) => new()
    {
        Name = dto.Name,
        Inn = dto.INN,
        Latitude = dto.Latitude,
        Longitude = dto.Longitude,
        Address = dto.Address,
    };
}
