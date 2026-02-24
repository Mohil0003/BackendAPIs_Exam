using Microsoft.EntityFrameworkCore;
using Backend_Exam.DTOs;
using FluentValidation;


namespace Backend_Exam.Validator
{
    public class UserValidator: AbstractValidator<UserDTO>
    {
        public UserValidator()
        {
            RuleFor(u => u.Name).NotEmpty().WithMessage("Username is required.")
                .MaximumLength(50).WithMessage("Username cannot exceed 50 characters.");

            RuleFor(u => u.Email).NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters.");

        }
    }
}
