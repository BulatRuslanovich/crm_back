using CrmBack.Core.Models.Dto;
using FluentValidation;

namespace CrmBack.Core.Validators;

public class LoginUserDtoValidator : AbstractValidator<LoginUserDto>
{
    public LoginUserDtoValidator()
    {
        RuleFor(x => x.Login)
            .NotEmpty()
            .WithMessage("Логин обязателен")
            .MinimumLength(3)
            .WithMessage("Логин должен содержать минимум 3 символа")
            .MaximumLength(50)
            .WithMessage("Логин не должен превышать 50 символов")
            .Matches(@"^[a-zA-Z0-9_]+$")
            .WithMessage("Логин может содержать только буквы, цифры и подчеркивания");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Пароль обязателен")
            .MinimumLength(8)
            .WithMessage("Пароль должен содержать минимум 8 символов")
            .MaximumLength(100)
            .WithMessage("Пароль не должен превышать 100 символов");
    }
}
