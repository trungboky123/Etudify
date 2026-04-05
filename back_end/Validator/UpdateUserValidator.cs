using back_end.Dto.Request;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace back_end.Validator;

public class UpdateUserValidator : AbstractValidator<UpdateUserRequest>
{
    private readonly IStringLocalizer<Messages> _localizer;
    
    public UpdateUserValidator(IStringLocalizer<Messages> localizer)
    {
        _localizer = localizer;
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.FullName)
            .MaximumLength(50).WithMessage(_localizer["FullNameMaximum"])
            .When(x => x.FullName != null);
        RuleFor(x => x.UserName)
            .Matches(@"^\S*$").WithMessage(_localizer["UsernameNotContainSpace"])
            .MaximumLength(50).WithMessage(_localizer["UsernameMaximum"])
            .When(x => x.UserName != null);
        RuleFor(x => x.NewPassword)
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z0-9]).{8,}$")
            .WithMessage(_localizer["PasswordInvalid"])
            .When(x => x.NewPassword != null);
    }
}