namespace Backend_Exam.DTOs
{
    public class TicketsDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public int CreatedBy { get; set; }
        public int? AssignedTo { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
