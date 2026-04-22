using back_end.Dto.Request;
using back_end.Repository.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace back_end.Validator;

public class RegisterValidator : AbstractValidator<RegisterRequest>
{
    private readonly IStringLocalizer<Messages> _localizer;
    private readonly IUserRepository _userRepository;
    public RegisterValidator(IStringLocalizer<Messages> localizer, IUserRepository userRepository)
    {
        _localizer = localizer;
        _userRepository = userRepository;
        ClassLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage(_localizer["FullNameRequired"])
            .MaximumLength(50).WithMessage(_localizer["FullNameMaximum"]);
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(_localizer["UsernameRequired"])
            .Matches(@"^\S*$").WithMessage(_localizer["UsernameNotContainSpace"])
            .MaximumLength(50).WithMessage(_localizer["UsernameMaximum"])
            .MustAsync(async (username, cancellation) =>
            {
                return !await _userRepository.UsernameExists(username);
            }).WithMessage(_localizer["UsernameExisted"]);
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(_localizer["EmailRequired"])
            .MaximumLength(255).WithMessage(_localizer["EmailMaximum"])
            .EmailAddress().WithMessage(_localizer["EmailInvalid"])
            .MustAsync(async (email, cancellation) =>
            {
                return !await _userRepository.EmailExists(email);
            }).WithMessage(_localizer["EmailExisted"]);
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(_localizer["PasswordRequired"])
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z0-9]).{8,}$")
            .WithMessage(_localizer["PasswordInvalid"]);
    }
}