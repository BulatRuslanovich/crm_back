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

		RuleFor(x => x.Login)
			.NotEmpty().WithMessage("Логин обязателен")
			.Length(3, 50).WithMessage("Логин должен содержать от 3 до 50 символов");

		RuleFor(x => x.Password)
			.NotEmpty().WithMessage("Пароль обязателен")
			.Length(4, 40).WithMessage("Пароль должен содержать от 4 до 40 символов");
	}
}
