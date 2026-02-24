using Backend_Exam.DTOs;
using Backend_Exam.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend_Exam.Controllers
{
    [Route("/")]
    [ApiController]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly DBContext _context;
        private readonly IValidator<CommentsDTO> _validator;

        public CommentsController(DBContext context, IValidator<CommentsDTO> validator)
        {
            _context = context;
            _validator = validator;
        }

        
        [HttpPost("tickets/{ticketId}/comments")]
        public async Task<ActionResult> AddComment(int ticketId, [FromBody] CommentsDTO commentDto)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null)
                return NotFound(new { status = false, message = "Ticket not found" });

           
            if (!CanAccessTicket(ticket, currentUserId))
                return Forbid();

            var validation = _validator.Validate(commentDto);
            if (!validation.IsValid)
                return BadRequest(new { status = false, message = "Validation failed", errors = validation.Errors.Select(e => e.ErrorMessage) });

            var comment = new TicketComment
            {
                TicketId = ticketId,
                UserId = currentUserId,
                Comment = commentDto.Comment,
                CreatedAt = DateTime.Now
            };

            _context.TicketComments.Add(comment);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                status = true,
                message = "Comment added successfully",
                data = new { comment.Id, comment.Comment, comment.UserId, comment.TicketId, comment.CreatedAt }
            });
        }

        
        [HttpGet("tickets/{ticketId}/comments")]
        public async Task<ActionResult> GetComments(int ticketId)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null)
                return NotFound(new { status = false, message = "Ticket not found" });

          
            if (!CanAccessTicket(ticket, currentUserId))
                return Forbid();

            var comments = await _context.TicketComments
                .Include(c => c.User)
                .Where(c => c.TicketId == ticketId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new
                {
                    c.Id,
                    c.Comment,
                    c.UserId,
                    AuthorName = c.User.Name,
                    c.TicketId,
                    c.CreatedAt
                })
                .ToListAsync();

            return Ok(new { status = true, message = "Comments fetched successfully", data = comments });
        }

        [HttpPatch("comments/{id}")]
        public async Task<ActionResult> UpdateComment(int id, [FromBody] CommentsDTO commentDto)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var comment = await _context.TicketComments.FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null)
                return NotFound(new { status = false, message = "Comment not found" });

            if (!User.IsInRole("MANAGER") && comment.UserId != currentUserId)
                return Forbid();

            comment.Comment = commentDto.Comment;
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Comment updated successfully", data = new { comment.Id, comment.Comment, comment.UserId, comment.TicketId, comment.CreatedAt } });
        }

        [HttpDelete("comments/{id}")]
        public async Task<ActionResult> DeleteComment(int id)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var comment = await _context.TicketComments.FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null)
                return NotFound(new { status = false, message = "Comment not found" });

            if (!User.IsInRole("MANAGER") && comment.UserId != currentUserId)
                return Forbid();

            _context.TicketComments.Remove(comment);
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Comment deleted successfully" });
        }

        private bool CanAccessTicket(Ticket ticket, int currentUserId)
        {
            if (User.IsInRole("MANAGER")) return true;
            if (User.IsInRole("SUPPORT")) return ticket.AssignedTo == currentUserId;
            return ticket.CreatedBy == currentUserId;
        }
    }
}
