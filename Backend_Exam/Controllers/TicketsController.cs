using Backend_Exam.DTOs;
using Backend_Exam.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend_Exam.Controllers
{
    [Route("/tickets")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly DBContext _context;
        private readonly IValidator<TicketsDTO> _validator;

        public TicketsController(DBContext context, IValidator<TicketsDTO> validator)
        {
            _context = context;
            _validator = validator;
        }

       
        [HttpPost]
        [Authorize(Roles = "USER,MANAGER")]
        public async Task<ActionResult> CreateTicket([FromBody] TicketsDTO ticketDto)
        {
            var validation = _validator.Validate(ticketDto);
            if (!validation.IsValid)
                return BadRequest(new { status = false, message = "Validation failed", errors = validation.Errors.Select(e => e.ErrorMessage) });

            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var ticket = new Ticket
            {
                Title = ticketDto.Title,
                Description = ticketDto.Description,
                Status = ticketDto.Status ?? "Open",
                Priority = ticketDto.Priority,
                CreatedBy = currentUserId,
                AssignedTo = ticketDto.AssignedTo,
                CreatedAt = DateTime.Now
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                status = true,
                message = "Ticket created successfully",
                data = new { ticket.Id, ticket.Title, ticket.Description, ticket.Status, ticket.Priority, ticket.CreatedBy, ticket.AssignedTo, ticket.CreatedAt }
            });
        }

       
        [HttpGet]
        [Authorize]
        public async Task<ActionResult> GetTickets()
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = User.FindFirstValue(ClaimTypes.Role);

            IQueryable<Ticket> query = _context.Tickets
                .Include(t => t.CreatedByNavigation)
                .Include(t => t.AssignedToNavigation);

            if (role == "MANAGER")
            {
               
            }
            else if (role == "SUPPORT")
            {
                query = query.Where(t => t.AssignedTo == currentUserId);
            }
            else
            {
                query = query.Where(t => t.CreatedBy == currentUserId);
            }

            var tickets = await query.Select(t => new
            {
                t.Id,
                t.Title,
                t.Description,
                t.Status,
                t.Priority,
                t.CreatedBy,
                CreatedByName = t.CreatedByNavigation.Name,
                t.AssignedTo,
                AssignedToName = t.AssignedToNavigation != null ? t.AssignedToNavigation.Name : null,
                t.CreatedAt
            }).ToListAsync();

            return Ok(new { status = true, message = "Tickets fetched successfully", data = tickets });
        }

        [HttpPatch("{id}/assign")]
        [Authorize(Roles = "MANAGER,SUPPORT")]
        public async Task<ActionResult> AssignTicket(int id, [FromBody] AssignTicketDTO dto)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null)
                return NotFound(new { status = false, message = "Ticket not found" });

            if (dto.AssignedTo.HasValue)
            {
                var assignee = await _context.Users.AnyAsync(u => u.Id == dto.AssignedTo.Value);
                if (!assignee)
                    return BadRequest(new { status = false, message = "AssignedTo user does not exist" });
            }

            ticket.AssignedTo = dto.AssignedTo;
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Ticket assigned successfully", data = new { ticket.Id, ticket.AssignedTo } });
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "MANAGER,SUPPORT")]
        public async Task<ActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDTO dto)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null)
                return NotFound(new { status = false, message = "Ticket not found" });

            if (!string.IsNullOrEmpty(dto.Status) && ticket.Status != dto.Status)
            {
                var log = new TicketStatusLog
                {
                    TicketId = id,
                    OldStatus = ticket.Status,
                    NewStatus = dto.Status,
                    ChangedBy = currentUserId,
                    ChangedAt = DateTime.Now
                };
                _context.TicketStatusLogs.Add(log);
            }

            ticket.Status = dto.Status ?? ticket.Status;
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Ticket status updated successfully", data = new { ticket.Id, ticket.Status } });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "MANAGER")]
        public async Task<ActionResult> DeleteTicket(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.TicketComments)
                .Include(t => t.TicketStatusLogs)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
                return NotFound(new { status = false, message = "Ticket not found" });

            _context.TicketComments.RemoveRange(ticket.TicketComments);
            _context.TicketStatusLogs.RemoveRange(ticket.TicketStatusLogs);
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            return Ok(new { status = true, message = "Ticket deleted successfully" });
        }
    }
}
