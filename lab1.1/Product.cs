using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace lab1_1_net10
{
    public class Product
    {
        // XmlAttribute - atrybut określający, że dana właściwość będzie reprezentowana jako atrybut w formacie XML
        [XmlAttribute("id")]
        public int Id { get; set; }

        // XmlElement - zmienia nazwę elementu (tagu) XML
        [XmlElement("product_name")]
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public decimal Price { get; set; }

        public int Stock { get; set; }

        [XmlIgnore]
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
