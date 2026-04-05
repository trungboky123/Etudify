using back_end.Dto.Request;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace back_end.Validator;

public class RegisterValidator : AbstractValidator<RegisterRequest>
{
    private readonly IStringLocalizer<Messages> _localizer;
    public RegisterValidator(IStringLocalizer<Messages> localizer)
    {
        _localizer = localizer;
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage(_localizer["FullNameRequired"])
            .MaximumLength(50).WithMessage(_localizer["FullNameMaximum"]);
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(_localizer["UsernameRequired"])
            .Matches(@"^\S*$").WithMessage(_localizer["UsernameNotContainSpace"])
            .MaximumLength(50).WithMessage(_localizer["UsernameMaximum"]);
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(_localizer["EmailRequired"])
            .MaximumLength(255).WithMessage(_localizer["EmailMaximum"])
            .EmailAddress().WithMessage(_localizer["EmailInvalid"]);
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(_localizer["PasswordRequired"])
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z0-9]).{8,}$")
            .WithMessage(_localizer["PasswordInvalid"]);
    }
}