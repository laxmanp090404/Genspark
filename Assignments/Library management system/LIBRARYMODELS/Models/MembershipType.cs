using System;
using System.Collections.Generic;

namespace LIBRARYMODELS.Models;

public partial class MembershipType
{
    public short MembershipTypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public short MaxActiveBorrows { get; set; }

    public short MaxBorrowDays { get; set; }

    public decimal FineBlockLimit { get; set; }

    public DateTime Createdat { get; set; }

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();
}
