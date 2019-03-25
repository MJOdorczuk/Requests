using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Requests
{
    public class Request
    {
        public static readonly string CLIENTID = "Client's ID";
        public static readonly string REQUESTID = "Request's ID";
        public static readonly string NAME = "Name";
        public static readonly string QUANTITY = "Quantity";
        public static readonly string PRICE = "Price";
        [XmlElement("clientId")]
        public int ClientId { get; set; }
        [XmlElement("requestId")]
        public int RequestId { get; set; }
        [XmlElement("name")]
        public string Name { get; set; }
        [XmlElement("quantity")]
        public int Quantity { get; set; }
        [XmlElement("price")]
        public float Price { get; set; }
    }
    
    public class RequestList
    {
        [XmlElement("request")]
        public List<Request> Requests { get; set; }

        public RequestList()
        {
            this.Requests = new List<Request>();
        }

        /// <summary>
        /// Removes all the requests with invalid information
        /// Id cannot be non-positive
        /// Name cannot be empty
        /// Quantity cannot be non-positive
        /// Price cannot be non-positive
        /// </summary>
        public void RemoveIncompletes()
        {
            Requests.RemoveAll(r => r.ClientId <= 0
                                || r.RequestId <= 0
                                || r.Name == ""
                                || r.Quantity <= 0
                                || r.Price <= 0);
        }
    }
}
