using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace lab1_1_net10
{
    public class Order
    {
        // JsonPropertyName - atrybut który zmienia nazwę w formacie JSON
        [JsonPropertyName("orderId")]
        // XmlAttribute - atrybut określający, że dana właściwość będzie reprezentowana jako atrybut w formacie XML
        [XmlAttribute("id")]
        public int Id { get; set; }

        // XMLElement - zmienia nazwę elementu (tagu) XML
        [XmlElement("buyer")]
        public Customer Customer { get; set; } = new Customer();
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }

        // XMLElement - zmienia nazwę elementu (tagu) XML
        [XmlElement("lineItem")]
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        
        // JsonIgnore - atrybut określający, że dana właściwość będzie ignorowana podczas serializacji do formatu JSON/ deserializacji z formatu JSON
        [JsonIgnore]
        // XmlIgnore - atrybut określający, że dana właściwość będzie ignorowana podczas serializacji do formatu XML/ deserializacji z formatu XML
        [XmlIgnore]
        public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
    }
}
