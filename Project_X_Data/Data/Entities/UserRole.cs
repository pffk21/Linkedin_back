namespace Project_X_Data.Data.Entities
{
    public class UserRole
    {
        public string Id { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }


    }
}
