namespace Project_X_Data.Data.Entities
{
    public class ContactInformation
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }  
        public User User { get; set; } = null!;  

        public string PhoneNumber { get; set; } = null!;
        public string Slug { get; set; } = null!;
    }
}
