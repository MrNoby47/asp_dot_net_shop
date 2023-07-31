using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldMarket.Models.View_Models
{
    public class CreateProductVM
    {
        public IEnumerable<Color> Colors { get; set; }
        public Product Product { get; set; }
    }
}
