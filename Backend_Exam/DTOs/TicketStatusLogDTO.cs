namespace Backend_Exam.DTOs
{
    public class TicketStatusLogDTO
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string OldStatus { get; set; } = null!;
        public string NewStatus { get; set; } = null!;
        public DateTime ChangedAt { get; set; }
    }
}
