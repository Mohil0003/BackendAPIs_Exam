namespace Backend_Exam.DTOs
{
    public class CommentsDTO
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public int UserId { get; set; }
        public int TicketId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
