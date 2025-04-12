using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GuideMe.Models;

public partial class Province
{
    public int ProvinceId { get; set; }

    public string? ProvinceName { get; set; }

    public string? ProvinceImage { get; set; }

    public string? ProvinceDescription { get; set; }


    [NotMapped]
    [DataType(DataType.Upload)]
    public IFormFile? ProvinceImages { get; set; }

    public virtual ICollection<UrbanTreasure> UrbanTreasures { get; set; } = new List<UrbanTreasure>();
}
