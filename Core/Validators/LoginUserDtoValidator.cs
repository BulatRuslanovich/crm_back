using CrmBack.Core.Models.Dto;
using FluentValidation;

namespace CrmBack.Core.Validators;

public class LoginUserDtoValidator : AbstractValidator<LoginUserDto>
{
    public LoginUserDtoValidator()
    {
        RuleFor(x => x.Login)
            .NotEmpty().WithMessage("Логин обязателен")
            .Length(4, 40).WithMessage("Логин должен содержать от 4 до 40 символов");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен")
            .Length(4, 40).WithMessage("Пароль должен содержать от 4 до 40 символов");
    }
}
