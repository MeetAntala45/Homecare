using FluentValidation;
using Homecare.Application.DTOs.Offers;
public class CreateCouponValidator : AbstractValidator<CreateOfferDto>
{
    public CreateCouponValidator()
    {
        RuleFor(x => x.OfferCode)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(x => x.Description)
            .MaximumLength(255);

        RuleFor(x => x.DiscountPct)
            .NotEmpty()
            .GreaterThan(0)
            .LessThanOrEqualTo(100);
    }
}

