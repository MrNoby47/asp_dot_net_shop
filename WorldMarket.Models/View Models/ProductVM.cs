using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldMarket.Models.View_Models
{
    public class ProductVM
    {
        public Product Product { get; set; }
        public IEnumerable<SelectListItem> CoverTypeList { get; set; }
        public IEnumerable <SelectListItem> CategoryList{ get;set;}
    }
}
