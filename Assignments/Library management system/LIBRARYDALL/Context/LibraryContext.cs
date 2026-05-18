using System;
using System.Collections.Generic;
using LIBRARYMODELS.Models;
using Microsoft.EntityFrameworkCore;

namespace LIBRARYDALL.Context;

public partial class LibraryContext : DbContext
{
    public LibraryContext()
    {
    }

    public LibraryContext(DbContextOptions<LibraryContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<BookCategory> BookCategories { get; set; }

    public virtual DbSet<BookCopy> BookCopies { get; set; }

    public virtual DbSet<BookDamageLog> BookDamageLogs { get; set; }

    public virtual DbSet<BookEdition> BookEditions { get; set; }

    public virtual DbSet<Borrowing> Borrowings { get; set; }

    public virtual DbSet<Fine> Fines { get; set; }

    public virtual DbSet<FinePayment> FinePayments { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<MembershipType> MembershipTypes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=LibraryManagementSystem;Username=laxmanp;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("book_pkey");

            entity.ToTable("book");

            entity.HasIndex(e => e.CategoryId, "idx_book_category");

            entity.HasIndex(e => new { e.Author, e.Title }, "uniq_authorandtitle").IsUnique();

            entity.Property(e => e.BookId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("book_id");
            entity.Property(e => e.Author).HasColumnName("author");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.IsbnBase)
                .HasMaxLength(13)
                .IsFixedLength()
                .HasColumnName("isbn_base");
            entity.Property(e => e.Title).HasColumnName("title");

            entity.HasOne(d => d.Category).WithMany(p => p.Books)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_book_to_category");
        });

        modelBuilder.Entity<BookCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("book_category_pkey");

            entity.ToTable("book_category");

            entity.HasIndex(e => e.CategoryName, "book_category_category_name_key").IsUnique();

            entity.Property(e => e.CategoryId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("category_id");
            entity.Property(e => e.CategoryName).HasColumnName("category_name");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Description).HasColumnName("description");
        });

        modelBuilder.Entity<BookCopy>(entity =>
        {
            entity.HasKey(e => e.CopyId).HasName("book_copy_pkey");

            entity.ToTable("book_copy");

            entity.HasIndex(e => new { e.EditionId, e.CopyStatus }, "idx_copy_edition_status");

            entity.Property(e => e.CopyId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("copy_id");
            entity.Property(e => e.AcquiredOn).HasColumnName("acquired_on");
            entity.Property(e => e.CopyStatus)
                .HasDefaultValueSql("'Available'::text")
                .HasColumnName("copy_status");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.EditionId).HasColumnName("edition_id");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");

            entity.HasOne(d => d.Edition).WithMany(p => p.BookCopies)
                .HasForeignKey(d => d.EditionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_copytoedition");
        });

        modelBuilder.Entity<BookDamageLog>(entity =>
        {
            entity.HasKey(e => e.DamageLogId).HasName("book_damage_log_pkey");

            entity.ToTable("book_damage_log");

            entity.HasIndex(e => e.CopyId, "idx_damage_copy");

            entity.HasIndex(e => e.MemberId, "idx_damage_member").HasFilter("(member_id IS NOT NULL)");

            entity.Property(e => e.DamageLogId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("damage_log_id");
            entity.Property(e => e.CopyId).HasColumnName("copy_id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.DamageDescription).HasColumnName("damage_description");
            entity.Property(e => e.DamageSeverity).HasColumnName("damage_severity");
            entity.Property(e => e.FineApplied)
                .HasPrecision(7, 2)
                .HasColumnName("fine_applied");
            entity.Property(e => e.MemberId).HasColumnName("member_id");
            entity.Property(e => e.ReportedOn)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("reported_on");
            entity.Property(e => e.ResultingStatus).HasColumnName("resulting_status");

            entity.HasOne(d => d.Copy).WithMany(p => p.BookDamageLogs)
                .HasForeignKey(d => d.CopyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_damagetocopy");

            entity.HasOne(d => d.Member).WithMany(p => p.BookDamageLogs)
                .HasForeignKey(d => d.MemberId)
                .HasConstraintName("book_damage_log_member_id_fkey");
        });

        modelBuilder.Entity<BookEdition>(entity =>
        {
            entity.HasKey(e => e.EditionId).HasName("book_edition_pkey");

            entity.ToTable("book_edition");

            entity.HasIndex(e => e.Isbn, "unique_isbn").IsUnique();

            entity.HasIndex(e => new { e.BookId, e.EditionNumber }, "uq_bookediton").IsUnique();

            entity.Property(e => e.EditionId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("edition_id");
            entity.Property(e => e.BookId).HasColumnName("book_id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.EditionLabel).HasColumnName("edition_label");
            entity.Property(e => e.EditionNumber).HasColumnName("edition_number");
            entity.Property(e => e.Isbn)
                .HasMaxLength(13)
                .IsFixedLength()
                .HasColumnName("isbn");
            entity.Property(e => e.PublishedYear).HasColumnName("published_year");
            entity.Property(e => e.Publisher).HasColumnName("publisher");

            entity.HasOne(d => d.Book).WithMany(p => p.BookEditions)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bookeditiontobook");
        });

        modelBuilder.Entity<Borrowing>(entity =>
        {
            entity.HasKey(e => e.BorrowingId).HasName("borrowing_pkey");

            entity.ToTable("borrowing");

            entity.HasIndex(e => new { e.CopyId, e.BorrowStatus }, "uq_copy_active_borrow").IsUnique();

            entity.Property(e => e.BorrowingId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("borrowing_id");
            entity.Property(e => e.BorrowStatus)
                .HasDefaultValueSql("'Active'::text")
                .HasColumnName("borrow_status");
            entity.Property(e => e.BorrowedOn)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("borrowed_on");
            entity.Property(e => e.CopyId).HasColumnName("copy_id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.MemberId).HasColumnName("member_id");
            entity.Property(e => e.ReturnedOn).HasColumnName("returned_on");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");

            entity.HasOne(d => d.Copy).WithMany(p => p.Borrowings)
                .HasForeignKey(d => d.CopyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_borrowingtocopy");

            entity.HasOne(d => d.Member).WithMany(p => p.Borrowings)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_borrowingtomembers");
        });

        modelBuilder.Entity<Fine>(entity =>
        {
            entity.HasKey(e => e.FineId).HasName("fine_pkey");

            entity.ToTable("fine");

            entity.HasIndex(e => e.BorrowingId, "fine_borrowing_id_key").IsUnique();

            entity.HasIndex(e => new { e.MemberId, e.FineStatus }, "idx_fine_member_status");

            entity.Property(e => e.FineId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("fine_id");
            entity.Property(e => e.BorrowingId).HasColumnName("borrowing_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DaysOverdue).HasColumnName("days_overdue");
            entity.Property(e => e.FinePerDay)
                .HasPrecision(6, 2)
                .HasDefaultValue(10.00m)
                .HasColumnName("fine_per_day");
            entity.Property(e => e.FineStatus)
                .HasDefaultValueSql("'pending'::text")
                .HasColumnName("fine_status");
            entity.Property(e => e.MemberId).HasColumnName("member_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.WaiverReason).HasColumnName("waiver_reason");

            entity.HasOne(d => d.Borrowing).WithOne(p => p.Fine)
                .HasForeignKey<Fine>(d => d.BorrowingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_finetoborrow");

            entity.HasOne(d => d.Member).WithMany(p => p.Fines)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_finetomember");
        });

        modelBuilder.Entity<FinePayment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("fine_payment_pkey");

            entity.ToTable("fine_payment");

            entity.HasIndex(e => e.FineId, "fine_payment_fine_id_key").IsUnique();

            entity.HasIndex(e => e.FineId, "idx_fine_payment_fine");

            entity.Property(e => e.PaymentId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("payment_id");
            entity.Property(e => e.AmountPaid)
                .HasPrecision(8, 2)
                .HasColumnName("amount_paid");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.FineId).HasColumnName("fine_id");
            entity.Property(e => e.PaidOn)
                .HasDefaultValueSql("now()")
                .HasColumnName("paid_on");
            entity.Property(e => e.PaymentMode)
                .HasDefaultValueSql("'cash'::text")
                .HasColumnName("payment_mode");
            entity.Property(e => e.Remarks).HasColumnName("remarks");

            entity.HasOne(d => d.Fine).WithOne(p => p.FinePayment)
                .HasForeignKey<FinePayment>(d => d.FineId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_finepaytofine");
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasKey(e => e.MemberId).HasName("members_pkey");

            entity.ToTable("members");

            entity.HasIndex(e => e.IsActive, "idx_member_active").HasFilter("(deletedat IS NULL)");

            entity.HasIndex(e => e.Phone, "idx_member_phone").HasFilter("(deletedat IS NULL)");

            entity.HasIndex(e => e.Email, "members_email_key").IsUnique();

            entity.HasIndex(e => e.Phone, "members_phone_key").IsUnique();

            entity.Property(e => e.MemberId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("member_id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.Deletedat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deletedat");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.FullName).HasColumnName("full_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.JoinedOn)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("joined_on");
            entity.Property(e => e.MembershipTypeId).HasColumnName("membership_type_id");
            entity.Property(e => e.Phone)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("phone");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");

            entity.HasOne(d => d.MembershipType).WithMany(p => p.Members)
                .HasForeignKey(d => d.MembershipTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_membertotype");
        });

        modelBuilder.Entity<MembershipType>(entity =>
        {
            entity.HasKey(e => e.MembershipTypeId).HasName("membership_type_pkey");

            entity.ToTable("membership_type");

            entity.HasIndex(e => e.TypeName, "membership_type_type_name_key").IsUnique();

            entity.Property(e => e.MembershipTypeId)
                .UseIdentityAlwaysColumn()
                .HasColumnName("membership_type_id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.FineBlockLimit)
                .HasPrecision(6, 2)
                .HasDefaultValue(500.00m)
                .HasColumnName("fine_block_limit");
            entity.Property(e => e.MaxActiveBorrows).HasColumnName("max_active_borrows");
            entity.Property(e => e.MaxBorrowDays).HasColumnName("max_borrow_days");
            entity.Property(e => e.TypeName).HasColumnName("type_name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
