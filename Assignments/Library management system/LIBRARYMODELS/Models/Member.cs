using System;
using System.Collections.Generic;

namespace LIBRARYMODELS.Models;

public partial class Member
{
    public int MemberId { get; set; }

    public short MembershipTypeId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Address { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public DateOnly JoinedOn { get; set; }

    public bool IsActive { get; set; }

    public DateTime? Deletedat { get; set; }

    public DateTime Createdat { get; set; }

    public DateTime Updatedat { get; set; }

    public virtual ICollection<BookDamageLog> BookDamageLogs { get; set; } = new List<BookDamageLog>();

    public virtual ICollection<Borrowing> Borrowings { get; set; } = new List<Borrowing>();

    public virtual ICollection<Fine> Fines { get; set; } = new List<Fine>();

    public virtual MembershipType MembershipType { get; set; } = null!;

    public override string ToString()
    {
        return $"Member id {MemberId} Name:{FullName} Email:{Email} MembershipType: {MembershipType.TypeName}";
    }
}
