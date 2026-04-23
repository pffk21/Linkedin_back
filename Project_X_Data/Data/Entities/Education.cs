namespace Project_X_Data.Data.Entities
{
    public class Education
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public string SchoolName { get; set; } = null!;
        public string Degree { get; set; } = null!;
        public string FieldOfStudy { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

    }
}
