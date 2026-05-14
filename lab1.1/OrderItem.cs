using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace lab1_1_net10
{
    public class OrderItem
    {

        public int Id { get; set; }
        // XMLElement - zmienia nazwę elementu (tagu) XML
        [XmlElement("productData")]
        public Product Product { get; set; } = new Product();
        // JsonPropertyName - atrybut który zmienia nazwę w formacie JSON
        [XmlAttribute("qty")]
        public int Quantity { get; set; }

        public int OrderId { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public Order? Order { get; set; }

        public int ProductId { get; set; }

        public decimal UnitPrice { get; set; }

        // JsonIgnore - ignoruj podczas serializacji do formatu JSON/ deserializacji z formatu JSON
        [JsonIgnore]
        // XmlIgnore - ignoruj podczas serializacji do formatu XML/ deserializacji z formatu XML
        [XmlIgnore]
        public decimal TotalPrice => Product.Price * Quantity;
    }

}
