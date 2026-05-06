namespace NotificationApp.Models
{
    internal partial class User : IComparable<User>, IEquatable<User>
    {

        public override string ToString()
        {
            return $"Id: {Id}, Name: {Name}, Email: {Email}, Phone: {Phone}";
        }
        public int CompareTo(User? other)
        {
           return Id.CompareTo(other?.Id);
        }

        public bool Equals(User? other)
        {
            return Id.Equals(other?.Id);
        }
        
    }
}