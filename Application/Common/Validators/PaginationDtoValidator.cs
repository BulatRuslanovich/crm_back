using CrmBack.Application.Common.Dto;
using FluentValidation;

namespace CrmBack.Application.Common.Validators;

public class PaginationDtoValidator : AbstractValidator<PaginationDto>
{
    public PaginationDtoValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(1000)
            .WithMessage("PageSize must be between 1 and 1000");
    }
}
