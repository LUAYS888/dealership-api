using Dealership.Api.DTOs;          //Input validatior for info. 
using FluentValidation;

namespace Dealership.Api.Validators;

public class RegisterStartDtoValidator : AbstractValidator<RegisterStartDto>
{
    public RegisterStartDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Must contain a digit.")
            .Matches(@"[\W_]").WithMessage("Must contain a symbol.");
    }
}

public class RegisterVerifyDtoValidator : AbstractValidator<RegisterVerifyDto>
{
    public RegisterVerifyDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.OtpCode).NotEmpty().Length(6);
    }
}

public class LoginStartDtoValidator : AbstractValidator<LoginStartDto>
{
    public LoginStartDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginVerifyDtoValidator : AbstractValidator<LoginVerifyDto>
{
    public LoginVerifyDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.OtpCode).NotEmpty().Length(6);
    }
}
