using CrmBack.Application.Users.Dto;
using FluentValidation;

namespace CrmBack.Application.Users.Validators;

public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Имя обязательно")
            .Length(2, 50).WithMessage("Имя должно содержать от 2 до 50 символов");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Фамилия обязательна")
            .Length(2, 50).WithMessage("Фамилия должна содержать от 2 до 50 символов");

        RuleFor(x => x.MiddleName)
            .MaximumLength(50).WithMessage("Отчество не должно превышать 50 символов");

        RuleFor(x => x.Login)
            .NotEmpty().WithMessage("Логин обязателен")
            .Length(3, 50).WithMessage("Логин должен содержать от 3 до 50 символов");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен")
            .Length(8, 100).WithMessage("Пароль должен содержать от 8 до 100 символов")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)")
            .WithMessage("Пароль должен содержать минимум одну строчную букву, одну заглавную букву и одну цифру");
    }
}
