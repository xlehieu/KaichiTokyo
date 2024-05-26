using System;
using System.Collections.Generic;

namespace WebsiteKaichiTokyo.Models;

public partial class OrderDetail
{
    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public int? Quantity { get; set; }

    public int? Discount { get; set; }

    public int? Total { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
