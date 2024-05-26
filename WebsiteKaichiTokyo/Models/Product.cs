using System;
using System.Collections.Generic;

namespace WebsiteKaichiTokyo.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string? ProductName { get; set; }

    public string? ShortDesc { get; set; }

    public string? Description { get; set; }

    public int? CatId { get; set; }

    public int? Price { get; set; }

    public int? Discount { get; set; }

    public string? Thumb { get; set; }

    public string? Video { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public bool BestSeller { get; set; }

    public bool HomeFlag { get; set; }

    public bool Active { get; set; }

    public string? Tags { get; set; }

    public string? Title { get; set; }

    public string? Alias { get; set; }

    public string? MetaDesc { get; set; }

    public string? MetaKey { get; set; }

    public int? UnitInstock { get; set; }

    public int? GiaNhap { get; set; }

    public virtual ICollection<AttributePrice> AttributePrices { get; set; } = new List<AttributePrice>();

    public virtual Category? Cat { get; set; }

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
