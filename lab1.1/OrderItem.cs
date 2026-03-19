using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab1._1
{
    public class OrderItem
    {
        public Product Product { get; set; } = new Product();
        public int Quantity { get; set; }

        public decimal TotalPrice => Product.Price * Quantity;
    }

}
