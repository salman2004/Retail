using System;
using System.Collections.Generic;
using System.Text;

namespace CDC.Commerce.Runtime.FBRIntegration.Entities
{
    public class FbrResponse
    {
        public string InvoiceNumber { get; set; }
        public string Code { get; set; }
        public string Response { get; set; }
        public object Errors { get; set; }
    }    
}
