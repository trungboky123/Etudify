using back_end.Dto.Response;
using back_end.Repository.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace back_end.Validator;

public class AddAccountValidator : AbstractValidator<AddAccountRequest>
{
    private readonly IStringLocalizer<Messages>  _localizer;
    private readonly IUserRepository _userRepository;

    public AddAccountValidator(IStringLocalizer<Messages> localizer, IUserRepository userRepository)
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
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage(_localizer["RoleRequired"]);
    }
}