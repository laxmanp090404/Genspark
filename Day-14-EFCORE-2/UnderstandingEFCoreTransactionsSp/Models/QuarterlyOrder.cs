using System;
using System.Collections.Generic;

namespace UnderstandingEFCoreTransactionsSp.Models;

public partial class QuarterlyOrder
{
    public string? Customerid { get; set; }

    public string? Companyname { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }
}
