using System;
using System.ComponentModel.DataAnnotations;
using Archi.Library.Models;

namespace Archi.api.Models
{
    public class Pizza : ModelBase
    {
        [Required]
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Topping { get; set; }
    }
}
