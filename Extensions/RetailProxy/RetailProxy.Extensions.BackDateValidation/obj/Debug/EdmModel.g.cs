// <auto-generated />
  namespace CDC.Commerce.RetailProxy.BackDateValidation
  {
  using System.CodeDom.Compiler;
  using System.IO;
  using System.Xml;

    /// <summary>
    /// Represents the EDM model.
    /// </summary>
    [GeneratedCodeAttribute("CDC.Commerce.RetailProxy.BackDateValidation", "1.0")]
    public class EdmModel: Microsoft.Dynamics.Commerce.RetailProxy.IEdmModelExtension
    {
        private static string edmx = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""Microsoft.Dynamics.Retail.RetailServerLibrary"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <Action Name=""ValidateTime"">
        <Parameter Name=""deviceDateTime"" Type=""Edm.String"" Unicode=""false"" />
        <ReturnType Type=""Edm.Boolean"" Nullable=""false"" />
      </Action>
      <EntityContainer Name=""CommerceContext"">
        <ActionImport Name=""ValidateTime"" Action=""Microsoft.Dynamics.Retail.RetailServerLibrary.ValidateTime"" />
      </EntityContainer>
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";
        private static string apiVersion = "7.3";
        private static System.Collections.Generic.Dictionary<System.Type, string> proxyTypeToRuntimeTypeNameMap = new System.Collections.Generic.Dictionary<System.Type, string>()
        {
        
        };
        private static System.Collections.Generic.Dictionary<string, System.Type> runtimeTypeNameToProxyTypeMap = new System.Collections.Generic.Dictionary<string, System.Type>()
        {
        
        };
        
        /// <summary>
        /// Gets the EDMX string.
        /// </summary>
        public string Edmx
        {
            get
            {
                return edmx;
            }
        }

        /// <summary>
        /// Gets the matched Retail Server API version.
        /// </summary>
        public string ApiVersion
        {
            get
            {
                return apiVersion;
            }
        }
        
        /// <summary>
        /// Gets the map from retail proxy type to commerce runtime type names.
        /// </summary>
        public System.Collections.Generic.Dictionary<System.Type, string> ProxyTypeToRuntimeTypeNameMap
        {
            get { return proxyTypeToRuntimeTypeNameMap; }
        }

        /// <summary>
        /// Gets the map from commerce runtime type names to retail proxy type.
        /// </summary>
        public System.Collections.Generic.Dictionary<string, System.Type> RuntimeTypeNameToProxyTypeMap
        {
            get { return runtimeTypeNameToProxyTypeMap; }
        }
    }
}