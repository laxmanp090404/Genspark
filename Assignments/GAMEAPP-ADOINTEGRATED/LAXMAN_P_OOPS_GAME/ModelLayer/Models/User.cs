namespace GameApp.ModelLayer.Models
{
    internal class User
    {
        public int Id{get;set;}
        public string Username{get;set;} = string.Empty;
        public string Password{get;set;} = string.Empty;
        public bool IsActive {get;set;} = true;

        public User(int id,string username,string password)
        {
            Id = id;
            Username = username;
            Password=password;
        }
        public override string ToString()
        {
            return $"User Id: {Id} and UserName: {Username}";
        }
    }
}