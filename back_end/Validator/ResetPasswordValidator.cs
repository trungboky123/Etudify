using back_end.Dto.Request;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace back_end.Validator;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordRequest>
{
    public readonly IStringLocalizer<Messages> _localizer;

    public ResetPasswordValidator(IStringLocalizer<Messages> localizer)
    {
        _localizer = localizer;
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage(_localizer["PasswordRequired"])
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z0-9]).{8,}$")
            .WithMessage(_localizer["PasswordInvalid"]);
    }
}