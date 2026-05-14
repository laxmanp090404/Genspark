using System;
using System.Collections.Generic;

namespace UnderstandingEFCoreTransactionsSp.Models;

public partial class Transac
{
    public int Id { get; set; }

    public int Fromaccno { get; set; }

    public int Toaccno { get; set; }

    public decimal Amount { get; set; }

    public virtual Account FromaccnoNavigation { get; set; } = null!;

    public virtual Account ToaccnoNavigation { get; set; } = null!;
}
