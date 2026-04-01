using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab1_1_net10
{
    public class Order
    {
        public int Id { get; set; }
        public Customer Customer { get; set; } = new Customer();
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
    }
}
