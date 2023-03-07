using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CDC.Commerce.Runtime.EhsasProgram.Model
{
    public class AuthenticationRequestParameterEntity
    {
        public string channel { get; set; }
        public string password { get; set; }
        public string username { get; set; }
    }
}
