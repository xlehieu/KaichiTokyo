using System;
using System.Collections.Generic;

namespace WebsiteKaichiTokyo.Models;

public partial class Contact
{
    public int ContactId { get; set; }

    public string? FullName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public string? Messages { get; set; }

    public DateTime? CreateDate { get; set; }

    public string? Title { get; set; }
}
