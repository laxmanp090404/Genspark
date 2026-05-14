using System;
using System.Collections.Generic;

namespace UnderstandingEFCoreTransactionsSp.Models;

public partial class Account
{
    public int Accno { get; set; }

    public decimal? Balance { get; set; }

    public virtual ICollection<Transac> TransacFromaccnoNavigations { get; set; } = new List<Transac>();

    public virtual ICollection<Transac> TransacToaccnoNavigations { get; set; } = new List<Transac>();
}
