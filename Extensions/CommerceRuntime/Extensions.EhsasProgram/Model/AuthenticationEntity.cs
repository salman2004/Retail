using System.Runtime.Serialization;


namespace CDC.Commerce.Runtime.EhsasProgram.Model
{
    [DataContract]
    public class AuthenticationEntity
    {
        [DataMember]
        public int expiry { get; set; }

        [DataMember]
        public string token { get; set; }
    }
}
