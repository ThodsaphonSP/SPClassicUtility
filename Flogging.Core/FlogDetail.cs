using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Flogging.Core
{
    public class FlogDetail
    {
        public FlogDetail()
        {
            Timestamp = DateTime.Now;
        }

        public DateTime Timestamp { get; set; }

        public string Message { get; set; }

        // WHERE
        public string Product { get; set; }
        
        public string Layer { get; set; }  //front end or back end
        public string Location { get; set; }
        public string Hostname { get; set; }

        // WHO
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }

        // EVERYTHING ELSE
        public string CorrelationId { get; set; } // exception shielding from server to client
        public long? ElapsedMilliseconds { get; set; }  // only for performance entries

        public CustomException CustomException { get; set; }
        public Dictionary<string, object> AdditionalInfo { get; set; }  // catch-all for anything else
        public Exception Exception { get; set; }  // the exception for error logging



    }
}
