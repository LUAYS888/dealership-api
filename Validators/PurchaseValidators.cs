using Dealership.Api.DTOs;            //Input validation for purchaseing. 
using FluentValidation;

namespace Dealership.Api.Validators;

public class PurchaseRequestStartDtoValidator : AbstractValidator<PurchaseRequestStartDto>
{
    public PurchaseRequestStartDtoValidator()
    {
        RuleFor(x => x.VehicleId).GreaterThan(0);
        RuleFor(x => x.Notes).MaximumLength(500).When(x => x.Notes != null);
    }
}

public class PurchaseRequestConfirmDtoValidator : AbstractValidator<PurchaseRequestConfirmDto>
{
    public PurchaseRequestConfirmDtoValidator()
    {
        RuleFor(x => x.VehicleId).GreaterThan(0);
        RuleFor(x => x.OtpCode).NotEmpty().Length(6);
    }
}
