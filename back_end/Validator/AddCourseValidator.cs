using back_end.Dto.Request;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace back_end.Validator;

public class AddCourseValidator : AbstractValidator<AddCourseRequest>
{
    private readonly IStringLocalizer<Messages> _localizer;

    public AddCourseValidator(IStringLocalizer<Messages> localizer)
    {
        _localizer = localizer;
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(_localizer["CourseNameRequired"]);
        RuleFor(x => x.ListedPrice)
            .NotNull()
            .WithMessage(_localizer["ListedPriceRequired"]);
        RuleFor(x => x.ListedPrice)
            .GreaterThanOrEqualTo(0)    
            .WithMessage(_localizer["ListedPriceInvalid"]);
        RuleFor(x => x.SalePrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.SalePrice.HasValue)
            .WithMessage(_localizer["SalePriceInvalid"]);
        RuleFor(x => x.SalePrice)
            .LessThanOrEqualTo(x => x.ListedPrice)
            .When(x => x.SalePrice.HasValue && x.ListedPrice.HasValue)
            .WithMessage(_localizer["PriceInvalid"]);
        RuleFor(x => x.InstructorId)
            .NotEmpty().WithMessage(_localizer["InstructorRequired"]);
        RuleFor(x => x.CategoryIds)
            .NotEmpty().WithMessage(_localizer["CategoryRequired"]);
    }
}