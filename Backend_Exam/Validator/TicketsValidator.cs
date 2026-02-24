using Backend_Exam.DTOs;
using FluentValidation;


namespace Backend_Exam.Validator
{
    public class TicketsValidator: AbstractValidator<TicketsDTO>
    {
        public TicketsValidator()
        {
            RuleFor(t => t.Title).NotEmpty().WithMessage("Title is required.")
                .MinimumLength(5).WithMessage("Title must be atleast 5 characters.");
            RuleFor(t => t.Description).NotEmpty().WithMessage("Description is required.")
                .MinimumLength(10).WithMessage("Description must be atleast 10 characters.");
            RuleFor(t => t.Priority).NotEmpty().WithMessage("Priority is required.")
                .Must(p => p == "Low" || p == "Medium" || p == "High")
                .WithMessage("Priority must be 'Low', 'Medium', or 'High'.");
            RuleFor(t => t.Status).NotEmpty().WithMessage("Status is required.")
                .Must(s => s == "Open" || s == "In Progress" || s == "Closed")
                .WithMessage("Status must be either 'Open', 'In Progress', or 'Closed'.");

        }

    }
}
