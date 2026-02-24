using Backend_Exam.DTOs;

using FluentValidation;

namespace Backend_Exam.Validator
{
    public class TicketStatusLogValidator: AbstractValidator<TicketStatusLogDTO>
    {
        public TicketStatusLogValidator()
        {
            RuleFor(t => t.TicketId).NotEmpty().WithMessage("TicketId is required.");
        }
    }
}
