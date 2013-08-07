using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Net;
using System.Web;
using System.IO;
using log4net;
using PayPal.Exception;
using PayPal.Manager;

namespace PayPal
{
    public class IPNMessage
    {
        /// <summary>
        /// Result from ipn validation call
        /// </summary>
        private bool? ipnValidationResult;
   
        /// <summary>
        /// Name value collection containing incoming IPN message key / value pair
        /// </summary>
        private NameValueCollection nvcMap = new NameValueCollection();

        /// <summary>
        /// Incoming IPN message converted to query string format. Used when validating the IPN message.
        /// </summary>
        private string ipnRequest = string.Empty;

        /// <summary>
        /// Encoding format for IPN messages
        /// </summary>
        private const string ipnEncoding = "windows-1252";

        /// <summary>
        /// SDK configuration parameters
        /// </summary>
        private Dictionary<string, string> config;

        /// <summary>
        /// Logger
        /// </summary>
        private static readonly ILog logger = LogManagerWrapper.GetLogger(typeof(IPNMessage));

        
        /// <summary>
        /// Initializing nvcMap and constructing query string
        /// </summary>
        /// <param name="nvc"></param>
        private void Initialize(NameValueCollection nvc)
        {
            List<string> items = new List<string>();
            try
            {
                if (nvc.HasKeys())
                {
                    foreach (string key in nvc.Keys)
                    {
                        items.Add(string.Concat(key, "=", System.Web.HttpUtility.UrlEncode(nvc[key], Encoding.GetEncoding(ipnEncoding))));
                        nvcMap.Add(key, nvc[key]);
                    }
                    ipnRequest = string.Join("&", items.ToArray())+"&cmd=_notify-validate";
                }
            }
            catch (System.Exception ex)
            {
                logger.Debug(this.GetType().Name + " : " + ex.Message);
            }
        }

        /// <summary>
        /// IPNMessage constructor
        /// </summary>
        /// <param name="nvc"></param>
        [Obsolete("use IPNMessage(byte[] parameters) instead")]
        public IPNMessage(NameValueCollection nvc)
        {
            this.config = ConfigManager.Instance.GetProperties();
            this.Initialize(nvc);
        }

        /// <summary>
        /// Construct a new IPNMessage object using dynamic SDK configuration
        /// </summary>
        /// <param name="config">Dynamic SDK configuration parameters</param>
        /// <param name="parameters">byte array read from request</param>
        public IPNMessage(Dictionary<string, string> config, byte[] parameters)
        {
            this.config = config;
            this.Initialize(HttpUtility.ParseQueryString(Encoding.GetEncoding(ipnEncoding).GetString(parameters), Encoding.GetEncoding(ipnEncoding)));
        }

        /// <summary>
        /// Construct a new IPNMessage object using .Config file based configuration
        /// </summary>
        /// <param name="parameters">byte array read from request</param>
        public IPNMessage(byte[] parameters)
        {
            this.config = ConfigManager.Instance.GetProperties();
            this.Initialize(HttpUtility.ParseQueryString(Encoding.GetEncoding(ipnEncoding).GetString(parameters), Encoding.GetEncoding(ipnEncoding)));
        }

        /// <summary>
        /// Returns the IPN request validation
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            /// If ipn has been previously validated, do not repeat the validation process.
            if (this.ipnValidationResult != null)
            {
                return this.ipnValidationResult.Value;
            }
            else
            {
                try
                {   
                    string ipnEndpoint = GetIPNEndpoint();
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ipnEndpoint);

                    //Set values for the request back
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = ipnRequest.Length;

                    //Send the request to PayPal and get the response
                    StreamWriter streamOut = new StreamWriter(request.GetRequestStream(), Encoding.GetEncoding(ipnEncoding));
                    streamOut.Write(ipnRequest);
                    streamOut.Close();
                    StreamReader streamIn = new StreamReader(request.GetResponse().GetResponseStream());
                    string strResponse = streamIn.ReadToEnd();
                    streamIn.Close();

                    if (strResponse.Equals("VERIFIED"))
                    {
                        this.ipnValidationResult = true;
                    }
                    else
                    {
                        logger.Info("IPN validation failed. Got response: " + strResponse);
                        this.ipnValidationResult = false;
                    }
                }
                catch (System.Exception ex)
                {
                    logger.Info(this.GetType().Name + " : " + ex.Message);

                }
                return this.ipnValidationResult.HasValue ? this.ipnValidationResult.Value : false;
            }
        }

        private string GetIPNEndpoint()
        {
            if(config.ContainsKey(BaseConstants.IPN_ENDPOINT_CONFIG) && !String.IsNullOrEmpty(config[BaseConstants.IPN_ENDPOINT_CONFIG]))
            {
                return config[BaseConstants.IPN_ENDPOINT_CONFIG];
            }
            else if (config.ContainsKey(BaseConstants.APPLICATION_MODE_CONFIG))
            {
                switch (config[BaseConstants.APPLICATION_MODE_CONFIG].ToLower())
                {
                    case BaseConstants.SANDBOX_MODE:
                        return BaseConstants.IPN_SANDBOX_ENDPOINT;
                    case BaseConstants.LIVE_MODE:
                        return BaseConstants.IPN_LIVE_ENDPOINT;
                    default:
                        throw new ConfigException("You must configure either the application mode (sandbox/live) or an IPN endpoint");
                }
            }
            else
            {
                throw new ConfigException("You must configure either the application mode (sandbox/live) or an IPN endpoint");
            }
        }

        /// <summary>
        /// Gets the IPN request NameValueCollection
        /// </summary>
        public NameValueCollection IpnMap
        {
            get
            {
                return nvcMap;
            }
        }
      
        /// <summary>
        /// Gets the IPN request parameter value for the given name
        /// </summary>
        /// <param name="ipnName"></param>
        /// <returns></returns>
        public string IpnValue(string ipnName)
        {
            return this.nvcMap[ipnName];
        }
        
        /// <summary>
        /// Gets the IPN request transaction type
        /// </summary>
        public string TransactionType
        {
            get
            {
                return this.nvcMap["txn_type"] != null ? this.nvcMap["txn_type"] :
                    (this.nvcMap["transaction_type"] != null ? this.nvcMap["transaction_type"] : null);
            }
        }
    }
}
