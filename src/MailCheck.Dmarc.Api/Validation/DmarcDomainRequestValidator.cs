using FluentValidation;
using MailCheck.Common.Util;
using MailCheck.Dmarc.Api.Domain;

namespace MailCheck.Dmarc.Api.Validation
{
    public class DmarcDomainRequestValidator : AbstractValidator<DmarcDomainRequest>
    {
        public DmarcDomainRequestValidator(IDomainValidator domainValidator)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(_ => _.Domain)
                .NotNull()
                .WithMessage("A \"domain\" field is required.")
                .NotEmpty()
                .WithMessage("The \"domain\" field should not be empty.")
                .Must(domainValidator.IsValidDomain)
                .WithMessage("The domains must be be a valid domain");
        }
    }
}
