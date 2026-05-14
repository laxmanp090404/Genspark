using System;
using System.Collections.Generic;

namespace UnderstandingEFCoreTransactionsSp.Models;

public partial class User
{
    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? Role { get; set; }
}
