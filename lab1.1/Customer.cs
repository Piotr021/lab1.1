using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace lab1_1_net10
{
    public class Customer
    {
        // XmlAttribute - atrybut określający, że dana właściwość będzie reprezentowana jako atrybut w formacie XML
        [XmlAttribute("id")]
        public int Id { get; set; }
        // JsonPropertyName - zmienia nazwę w formacie JSON
        [JsonPropertyName("fullName")]
        // XMLElement - zmienia nazwę elementu (tagu) XML
        [XmlElement("full_name")]
        public string Name { get; set; } = "";

        public string Email { get; set; } = "";
        // XmlAttribute - atrybut określający, że dana właściwość będzie reprezentowana jako atrybut w formacie XML
        [XmlAttribute("vip")]
        public bool IsVip { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public List<Order> Orders { get; set; } = new List<Order>();
    }
}
