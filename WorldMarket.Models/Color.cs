using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldMarket.Models
{
    public class Color
    {
        [Key]
        public string Id { get; set; }
        public bool IsChecked { get; set; }
        public string Description { get; set; }
        public string Value { get; set; }
        [Required]
        public int  ProductId { get; set; }
        [ValidateNever]
        [ForeignKey("ProductId")]
        public Product Product { get; set; }

    }
}
