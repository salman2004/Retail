
namespace CDC.Commerce.Runtime.EhsasProgram.Model
{
    using Microsoft.Dynamics.Commerce.Runtime.ComponentModel.DataAnnotations;
    using Microsoft.Dynamics.Commerce.Runtime.DataModel;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using SystemDataAnnotations = System.ComponentModel.DataAnnotations;

    public class EhsasProgramEntity : CommerceEntity
    {
        private const string IdColumn = "RECID";

        public EhsasProgramEntity(bool isEhsasProgramAllowed, SubsidyInquiryResponse subsidyInquiryResponse, string authToken)
                : base("EhsasProgramEntity")
        {
            this.IsEhsasProgramAllowed = isEhsasProgramAllowed;
            this.SubsidyInquiryResponse = subsidyInquiryResponse;
            this.AuthToken = authToken;
        }

        [SystemDataAnnotations.Key]
        [Key]
        [DataMember]
        [Column("isEhsasProgramAllowed")]
        public bool IsEhsasProgramAllowed { get; set; }

        [DataMember]
        [Column("ehsasProgramOfferId")]
        public SubsidyInquiryResponse SubsidyInquiryResponse { get; set; }

        [DataMember]
        [Column("ehsasProgramOfferId")]
        public string AuthToken{ get; set; }

    }
}
