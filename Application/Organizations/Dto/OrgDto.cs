using CrmBack.Domain.Organizations;

namespace CrmBack.Application.Organizations.Dto;

public record CreateOrgDto(
	string Name,
	string INN,
	double Latitude,
	double Longitude,
	string Address
);

public record ReadOrgDto(
	int OrgId,
	string Name,
	string INN,
	double Latitude,
	double Longitude,
	string Address
);

public record UpdateOrgDto(
	string? Name = null,
	string? INN = null,
	double? Latitude = null,
	double? Longitude = null,
	string? Address = null
);

public static class OrgMapper
{
	public static ReadOrgDto ToReadDto(this OrgEntity entity) =>
		new(entity.OrgId, entity.Name, entity.Inn ?? "-", entity.Latitude, entity.Longitude, entity.Address);

	public static OrgEntity ToEntity(this CreateOrgDto dto) => new()
	{
		Name = dto.Name,
		Inn = dto.INN,
		Latitude = dto.Latitude,
		Longitude = dto.Longitude,
		Address = dto.Address,
	};
}
