using CrmBack.Core.Models.Dto;
using FluentValidation;

namespace CrmBack.Core.Validators;

public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("Имя обязательно")
            .MinimumLength(2)
            .WithMessage("Имя должно содержать минимум 2 символа")
            .MaximumLength(50)
            .WithMessage("Имя не должно превышать 50 символов")
            .Matches(@"^[а-яА-Яa-zA-Z\s\-']+$")
            .WithMessage("Имя может содержать только буквы, пробелы, дефисы и апострофы");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Фамилия обязательна")
            .MinimumLength(2)
            .WithMessage("Фамилия должна содержать минимум 2 символа")
            .MaximumLength(50)
            .WithMessage("Фамилия не должна превышать 50 символов")
            .Matches(@"^[а-яА-Яa-zA-Z\s\-']+$")
            .WithMessage("Фамилия может содержать только буквы, пробелы, дефисы и апострофы");

        RuleFor(x => x.MiddleName)
            .MaximumLength(50)
            .WithMessage("Отчество не должно превышать 50 символов")
            .Matches(@"^[а-яА-Яa-zA-Z\s\-']*$")
            .WithMessage("Отчество может содержать только буквы, пробелы, дефисы и апострофы")
            .When(x => !string.IsNullOrEmpty(x.MiddleName));

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
            .WithMessage("Пароль не должен превышать 100 символов")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)")
            .WithMessage("Пароль должен содержать минимум одну строчную букву, одну заглавную букву и одну цифру");
    }
}
