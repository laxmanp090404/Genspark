namespace NotificationModels.Models
{
    public partial class User : IComparable<User>, IEquatable<User>
    {

        // this partial class contains certain operations pertaining to user
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