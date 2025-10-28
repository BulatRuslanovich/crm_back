using CrmBack.Core.Models.Dto;
using FluentValidation;

namespace CrmBack.Core.Validators;

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .Length(2, 50).WithMessage("Имя должно содержать от 2 до 50 символов")
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .Length(2, 50).WithMessage("Фамилия должна содержать от 2 до 50 символов")
            .When(x => !string.IsNullOrEmpty(x.LastName));

        RuleFor(x => x.MiddleName)
            .MaximumLength(50).WithMessage("Отчество не должно превышать 50 символов")
            .When(x => !string.IsNullOrEmpty(x.MiddleName));

        RuleFor(x => x.Login)
            .Length(3, 50).WithMessage("Логин должен содержать от 3 до 50 символов");

        RuleFor(x => x.Password)
            .Length(8, 100).WithMessage("Пароль должен содержать от 8 до 100 символов")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)")
            .WithMessage("Пароль должен содержать минимум одну строчную букву, одну заглавную букву и одну цифру")
            .When(x => !string.IsNullOrEmpty(x.Password));

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Текущий пароль обязателен при изменении пароля")
            .When(x => !string.IsNullOrEmpty(x.Password));
    }
}
