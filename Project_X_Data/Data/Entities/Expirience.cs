namespace Project_X_Data.Data.Entities
{
    public class Expirience
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public string CompanyName { get; set; } = null!;
        public string Position { get; set; } = null!;
        public string Location { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
