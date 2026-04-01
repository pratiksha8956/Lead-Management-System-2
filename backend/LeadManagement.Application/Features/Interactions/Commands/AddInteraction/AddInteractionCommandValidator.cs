using FluentValidation;

namespace LeadManagement.Application.Features.Interactions.Commands.AddInteraction;

public class AddInteractionCommandValidator : AbstractValidator<AddInteractionCommand>
{
    public AddInteractionCommandValidator()
    {
        RuleFor(x => x.LeadId).GreaterThan(0);
        RuleFor(x => x.Notes).MaximumLength(4000);
    }
}
