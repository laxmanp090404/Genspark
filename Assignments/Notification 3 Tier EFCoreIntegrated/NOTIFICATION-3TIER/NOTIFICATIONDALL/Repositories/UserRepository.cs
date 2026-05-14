using NotificationDall.Context;
using NotificationModels.Models;
using Npgsql;

namespace NotificationDall.Repositories
{
    // data access for user
    public class UserRepository : AbstractRepository<int, User>
    {
        public UserRepository(NotificationContext _context) : base(_context)
        {
            
        }
        public override User CreateEntity(User user)
        {

            try
            {
                //add user and save changes
                context.Users.Add(user);
                context.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception at creating user " + e.Message);
            }

            return user;
        }

        public override User? DeleteEntity(int id)
        {
            User? existinguser = GetEntityById(id);
            if (existinguser == null)
            {
                return null;
            }

            try
            {
                // soft delete user by changing status
                existinguser.IsDeleted = true;
                context.SaveChanges();
                return existinguser;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while deleting user" + e.Message);
            }


            return null;
        }

        public override List<User>? GetAllEntity()
        {
            // list to store users
            List<User> users = new();

            try
            {
                // filter and send users who are not deleted
                users = context.Users.Where(u => !u.IsDeleted).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while getting all users " + e.Message);
            }
            return users;
        }

        public override User? GetEntityById(int id)
        {
            User? user = null;
            try
            {
                // sends user with id if not found send defalt null
                user = context.Users.FirstOrDefault(u => u.Id == id && !u.IsDeleted);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while getting user by id " + e.Message);
            }
            return user;

        }

        public override User? UpdateEntity(int id, User item)
        {
            User? existinguser = GetEntityById(id);
            if (existinguser == null) return null;

            try
            {
                // update the properties of users
                existinguser.Name = item.Name;
                existinguser.Email = item.Email;
                existinguser.Phone = item.Phone;
                //    save changes
                context.SaveChanges();
            }
            catch (Exception e)
            {

                Console.WriteLine("Error while updating user " + e.Message);
            }
            return existinguser;
        }
    }
}