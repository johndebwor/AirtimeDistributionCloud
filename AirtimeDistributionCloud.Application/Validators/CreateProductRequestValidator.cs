using AirtimeDistributionCloud.Application.DTOs;
using FluentValidation;

namespace AirtimeDistributionCloud.Application.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");

        RuleFor(x => x.BonusPercentage)
            .GreaterThanOrEqualTo(0).WithMessage("Bonus percentage must be non-negative")
            .LessThanOrEqualTo(100).WithMessage("Bonus percentage cannot exceed 100");

        RuleFor(x => x.AirtimeAccountBalance)
            .GreaterThanOrEqualTo(0).WithMessage("Initial balance must be non-negative");
    }
}
