using Backend_Exam.DTOs;
using FluentValidation;

namespace Backend_Exam.Validator
{
    public class CommentsValidator: AbstractValidator<CommentsDTO>
    {
        public CommentsValidator()
        {
            RuleFor(c => c.Comment).NotEmpty().WithMessage("Comment is required.")
                .MinimumLength(5).WithMessage("Comment must be atleast 5 characters.");

        }
    }
}
