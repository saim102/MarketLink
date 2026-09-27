using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityDemoApp.Models
{
    public class CartItem
    {
        [Key]
        public int CartItemId { get; set; }

       
        public int CartId { get; set; }

      
        public int ProductId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

 
        public Cart? Cart { get; set; }

        public Product? Product { get; set; }
    }
}