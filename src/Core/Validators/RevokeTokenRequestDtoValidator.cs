using CrmBack.Core.Models.Dto;
using FluentValidation;

namespace CrmBack.Core.Validators;

public class RevokeTokenRequestDtoValidator : AbstractValidator<RevokeTokenRequestDto>
{
    public RevokeTokenRequestDtoValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh токен обязателен");
    }
}
