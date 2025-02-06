using FluentValidation;
using MessageApp.API.Models.Messages;

namespace MessageApp.API.ModelValidators.Messages
{
    public class SendMessageRequestModelValidator : AbstractValidator<SendMessageRequestModel>
    {
        public SendMessageRequestModelValidator()
        {
            RuleFor(p => p.Text)
                .NotEmpty()
                .NotNull()
                .MaximumLength(128);
        }
    }
}