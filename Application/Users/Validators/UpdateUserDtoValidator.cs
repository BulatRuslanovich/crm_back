using CrmBack.Application.Users.Dto;
using FluentValidation;

namespace CrmBack.Application.Users.Validators;

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

		RuleFor(x => x.Login)
			.Length(3, 50).WithMessage("Логин должен содержать от 3 до 50 символов");

		RuleFor(x => x.Password)
			.Length(4, 40).WithMessage("Пароль должен содержать от 4 до 40 символов")
			.When(x => !string.IsNullOrEmpty(x.Password));

		RuleFor(x => x.CurrentPassword)
			.NotEmpty().WithMessage("Текущий пароль обязателен при изменении пароля")
			.When(x => !string.IsNullOrEmpty(x.Password));
	}
}
