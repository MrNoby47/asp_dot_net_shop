using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WorldMarket.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [DisplayName("Display Order")]
        [Range(0,100,ErrorMessage ="Display Order Value Must Be Between 0 And 100")]
        public int DisplayOrder { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
    }
}
