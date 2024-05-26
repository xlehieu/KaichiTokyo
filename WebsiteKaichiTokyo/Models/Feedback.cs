using System;
using System.Collections.Generic;

namespace WebsiteKaichiTokyo.Models;

public partial class Feedback
{
    public int FeedbackId { get; set; }

    public int ProductId { get; set; }

    public int CustomerId { get; set; }

    public int? NumberStar { get; set; }

    public string? Comment { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
