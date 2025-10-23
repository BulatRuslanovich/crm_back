using CrmBack.Core.Models.Dto;
using FluentValidation;

namespace CrmBack.Core.Validators;

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .MinimumLength(2)
            .WithMessage("Имя должно содержать минимум 2 символа")
            .MaximumLength(50)
            .WithMessage("Имя не должно превышать 50 символов")
            .Matches(@"^[а-яА-Яa-zA-Z\s\-']+$")
            .WithMessage("Имя может содержать только буквы, пробелы, дефисы и апострофы")
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .MinimumLength(2)
            .WithMessage("Фамилия должна содержать минимум 2 символа")
            .MaximumLength(50)
            .WithMessage("Фамилия не должна превышать 50 символов")
            .Matches(@"^[а-яА-Яa-zA-Z\s\-']+$")
            .WithMessage("Фамилия может содержать только буквы, пробелы, дефисы и апострофы")
            .When(x => !string.IsNullOrEmpty(x.LastName));

        RuleFor(x => x.MiddleName)
            .MaximumLength(50)
            .WithMessage("Отчество не должно превышать 50 символов")
            .Matches(@"^[а-яА-Яa-zA-Z\s\-']*$")
            .WithMessage("Отчество может содержать только буквы, пробелы, дефисы и апострофы")
            .When(x => !string.IsNullOrEmpty(x.MiddleName));

        RuleFor(x => x.Login)
            .MinimumLength(3)
            .WithMessage("Логин должен содержать минимум 3 символа")
            .MaximumLength(50)
            .WithMessage("Логин не должен превышать 50 символов")
            .Matches(@"^[a-zA-Z0-9_]+$")
            .WithMessage("Логин может содержать только буквы, цифры и подчеркивания")
            .When(x => !string.IsNullOrEmpty(x.Login));

        RuleFor(x => x.Password)
            .MinimumLength(8)
            .WithMessage("Пароль должен содержать минимум 8 символов")
            .MaximumLength(100)
            .WithMessage("Пароль не должен превышать 100 символов")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)")
            .WithMessage("Пароль должен содержать минимум одну строчную букву, одну заглавную букву и одну цифру")
            .When(x => !string.IsNullOrEmpty(x.Password));

        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage("Текущий пароль обязателен при изменении пароля")
            .When(x => !string.IsNullOrEmpty(x.Password));
    }
}
