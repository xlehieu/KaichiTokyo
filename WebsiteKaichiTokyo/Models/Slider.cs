using System;
using System.Collections.Generic;

namespace WebsiteKaichiTokyo.Models;

public partial class Slider
{
    public int SliderId { get; set; }

    public string? SliderName { get; set; }

    public string? Alias { get; set; }

    public bool Active { get; set; }

    public bool HomeFlag { get; set; }

    public string? Thumb { get; set; }
}
