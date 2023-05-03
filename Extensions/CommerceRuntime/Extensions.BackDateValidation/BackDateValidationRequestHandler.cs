using CDC.Commerce.Runtime.BackDateValidation.Model;
using Microsoft.Dynamics.Commerce.Runtime;
using Microsoft.Dynamics.Commerce.Runtime.Data;
using Microsoft.Dynamics.Commerce.Runtime.DataModel;
using Microsoft.Dynamics.Commerce.Runtime.DataServices.Messages;
using Microsoft.Dynamics.Commerce.Runtime.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDC.Commerce.Runtime.BackDateValidation
{
    public class BackDateValidationRequestHandler : IRequestHandler
    {
        public IEnumerable<Type> SupportedRequestTypes
        {
            get
            {
                return new[]
                {
                    typeof(BackDateValidationRequest)
                };
            }
        }

        public Response Execute(Request request)
        {
            ThrowIf.Null(request, "request");
            Type reqType = request.GetType();

            if (reqType == typeof(BackDateValidationRequest))
            {
                BackDateValidationRequest dateValidationRequest = (BackDateValidationRequest)request;
                ValidateDeviceDateTime(request.RequestContext, dateValidationRequest.DeviceDateTime, out bool result);
                return new BackDateValidationResponse(result);
            }
            else
            {
                string message = string.Format("Request '{0}' is not supported.", reqType);
                throw new NotSupportedException(message);
            }
           
        }

        public void ValidateDeviceDateTime(RequestContext context, string deviceDateTime, out bool result)
        {
            result = false;
            ExtensionsEntity entity;
            
            if (deviceDateTime == null || DateTime.Equals(DateTime.MinValue, deviceDateTime))
            {
                return;
            }

            using (DatabaseContext databaseContext = new DatabaseContext(context))
            {
                SqlQuery query = new SqlQuery();

                if (!context.Runtime.Configuration.IsMasterDatabaseConnectionString)
                {
                    query.QueryString = $@"Select CREATEDDATETIME from [ext].[CDCLASTTRANSACTIONDATETIME]";
                }
                else
                {
                    query.QueryString = $@"Select TOP 1 CREATEDDATETIME from ax.RETAILTRANSACTIONTABLE R1 WHERE TERMINAL = @terminal AND STORE = @store order by R1.CREATEDDATETIME desc";
                    query.Parameters["@terminal"] = context.GetTerminalId();
                    query.Parameters["@store"] = context.GetDeviceConfiguration().InventLocationId;
                }

                try
                {   
                    entity = databaseContext.ReadEntity<ExtensionsEntity>(query).ToList().FirstOrDefault();
                    DateTime.TryParse(entity?.GetProperty("CREATEDDATETIME")?.ToString() ?? DateTime.MinValue.ToString(), out DateTime lastTransactionDateTime);
                    DateTime.TryParse(deviceDateTime, out DateTime deviceLocalDateTime);
                    if (deviceLocalDateTime > lastTransactionDateTime || lastTransactionDateTime == DateTime.MinValue)
                    {
                        result = true;
                        return;
                    }
                }
                catch (Exception ex)
                {
                    result = false;
                    return;
                }
            }
        }
    }
}
