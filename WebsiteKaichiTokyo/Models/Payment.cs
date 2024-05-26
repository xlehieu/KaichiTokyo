using System;
using System.Collections.Generic;

namespace WebsiteKaichiTokyo.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public string? PaymentName { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
