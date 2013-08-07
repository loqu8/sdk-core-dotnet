using System;
using System.Collections.Generic;
using System.Net;
using log4net;
using PayPal.Exception;

namespace PayPal.Manager
{
    /// <summary>
    ///  ConnectionManager retrieves HttpConnection objects used by API service
    /// </summary>
    public sealed class ConnectionManager
    {
        /// <summary>
        /// Logger
        /// </summary>
        private static ILog logger = LogManagerWrapper.GetLogger(typeof(ConnectionManager));
        
        /// <summary>
        /// Singleton instance of ConnectionManager
        /// </summary>
        private static readonly ConnectionManager singletonInstance = new ConnectionManager();

        /// <summary>
        /// Explicit static constructor to tell C# compiler
        /// not to mark type as beforefieldinit
        /// </summary>
        static ConnectionManager() { }
        
        /// <summary>
        /// Private constructor
        /// </summary>
        private ConnectionManager() { }

        /// <summary>
        /// Gets the Singleton instance of ConnectionManager
        /// </summary>
        public static ConnectionManager Instance
        {
            get
            {
                return singletonInstance;
            }
        }

        /// <summary>
        /// Create and Config a HttpWebRequest
        /// </summary>
        /// <param name="config">Config properties</param>
        /// <param name="url">Url to connect to</param>
        /// <returns></returns>
        public HttpWebRequest GetConnection(Dictionary<string, string> config, string url)
        {

            HttpWebRequest httpRequest = null;                        
            try
            {
                httpRequest = (HttpWebRequest)WebRequest.Create(url);
            }
            catch (UriFormatException ex)
            {
                logger.Error(ex.Message);
                throw new ConfigException("Invalid URI " + url);
            }

            // Set connection timeout
            int ConnectionTimeout = 0;
            if(!config.ContainsKey(BaseConstants.HTTP_CONNECTION_TIMEOUT_CONFIG) ||
                !int.TryParse(config[BaseConstants.HTTP_CONNECTION_TIMEOUT_CONFIG], out ConnectionTimeout)) {
                int.TryParse(ConfigManager.getDefault(BaseConstants.HTTP_CONNECTION_TIMEOUT_CONFIG), out ConnectionTimeout);
            }            
            httpRequest.Timeout = ConnectionTimeout;

            // Set request proxy for tunnelling http requests via a proxy server
            if(config.ContainsKey(BaseConstants.HTTP_PROXY_ADDRESS_CONFIG))
            {
                WebProxy requestProxy = new WebProxy();
                requestProxy.Address = new Uri(config[BaseConstants.HTTP_PROXY_ADDRESS_CONFIG]);                
                if (config.ContainsKey(BaseConstants.HTTP_PROXY_CREDENTIAL_CONFIG))
                {
                    string proxyCredentials = config[BaseConstants.HTTP_PROXY_CREDENTIAL_CONFIG];
                    string[] proxyDetails = proxyCredentials.Split(':');
                    if (proxyDetails.Length == 2)
                    {
                        requestProxy.Credentials = new NetworkCredential(proxyDetails[0], proxyDetails[1]);
                    }
                }                
                httpRequest.Proxy = requestProxy;
            }
            return httpRequest;
        }
    }
}
