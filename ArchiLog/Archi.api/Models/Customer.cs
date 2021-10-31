using System;
using System.ComponentModel.DataAnnotations;
using Archi.Library.Models;

namespace Archi.api.Models
{
    public class Customer : ModelBase
    {
        [Required]
        public string Email { get; set; }
        public string Phone { get; set; }
        [Required]
        public string Lastname { get; set; }
        public string Firstname { get; set; }
    }
}
