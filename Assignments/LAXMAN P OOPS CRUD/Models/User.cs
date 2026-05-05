namespace NotificationApp.Models
{
    internal class User
    {
        // user properties
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;


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

        public override string ToString()
        {
            return $"Id: {Id}, Name: {Name}, Email: {Email}, Phone: {Phone}";
        }
    }
}