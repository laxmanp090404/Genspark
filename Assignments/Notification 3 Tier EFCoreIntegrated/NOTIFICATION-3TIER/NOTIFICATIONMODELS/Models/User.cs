namespace NotificationModels.Models
{
    public partial class User
    {
        // user properties
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool IsDeleted {get;set;} = false;

        //navigation property
        public ICollection<Notification> Notifications{get;set;} = new List<Notification>();

        // default constructor
        public User()
        {
            Name = Email = Phone = "unknown";
            
        }
    
        public User(int id, string name, string email, string phone)
        {
            Id = id;
            Name = name;
            Email = email;
            Phone = phone;
        }

    }
}