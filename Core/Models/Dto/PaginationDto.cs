namespace CrmBack.Core.Models.Dto;

public record PaginationDto(
    int Page = 1,
    int PageSize = 10,
    string? SearchTerm = null
);
