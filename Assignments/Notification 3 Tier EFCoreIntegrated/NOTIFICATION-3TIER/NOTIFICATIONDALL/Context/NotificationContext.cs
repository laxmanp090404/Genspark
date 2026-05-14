using Microsoft.EntityFrameworkCore;
using Npgsql;
using NotificationModels.Models;
namespace NotificationDall.Context
{
    public class NotificationContext:DbContext
    {
        // I have used Code-First Approach
        // connectionString
        private string connectionString =
            "Host=localhost;Username=laxmanp;Database=EFnotificationdb";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(connectionString);
        }

        // entities stored in EFlocal ie disconnected arch
        public DbSet<User> Users{get;set;}
        public DbSet<Notification> Notifications{get;set;}

        // configure models or db instructions
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // user entity
            modelBuilder.Entity<User>(user =>
            {
                // constraints
                user.HasKey(user=>user.Id).HasName("PK_forUsers");
                user.Property(user=>user.Name).IsRequired();
                user.Property(user=>user.Email).IsRequired();
                user.HasIndex(user=>user.Email).IsUnique();
                user.Property(user=>user.Phone).IsRequired();
                user.HasIndex(user=>user.Phone).IsUnique();
                user.Property(user=>user.IsDeleted).HasDefaultValue(false);
            });

            // notification entity
            modelBuilder.Entity<Notification>(notification =>
            {
               notification.HasKey(notification=>notification.Id);
               notification.Property(notification=>notification.Message).IsRequired();
               notification.Property(notification=>notification.Sentdate).IsRequired().HasColumnType("timestamp without time zone");
               //relationship
               notification.HasOne(notification=>notification.Recipient).WithMany(user=>user.Notifications).HasForeignKey(notification=>notification.RecipientId).HasConstraintName("FK_Notifications_to_User")
               .OnDelete(DeleteBehavior.Restrict);

               //inheritance
               notification.HasDiscriminator(notification=>notification.NotificationType).HasValue<EmailNotification>(NotificationType.Email).HasValue<SmsNotification>(NotificationType.SMS);
               
            });
            // ADDING CHILD entities
            modelBuilder.Entity<EmailNotification>();
            modelBuilder.Entity<SmsNotification>();
        }
    }
}