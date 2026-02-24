namespace Backend_Exam.DTOs
{
    // Used by PATCH /tickets/{id}/assign
    public class AssignTicketDTO
    {
        public int? AssignedTo { get; set; }
    }

    // Used by PATCH /tickets/{id}/status
    public class UpdateStatusDTO
    {
        public string? Status { get; set; }
    }
}
