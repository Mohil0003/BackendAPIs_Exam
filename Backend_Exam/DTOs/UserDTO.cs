namespace Backend_Exam.DTOs
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int role_id { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
