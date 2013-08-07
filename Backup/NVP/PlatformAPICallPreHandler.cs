using System;
using System.Collections.Generic;
using System.Text;
using PayPal.Authentication;
using PayPal.Manager;
using PayPal.Exception;

namespace PayPal.NVP
{
    public class PlatformAPICallPreHandler : IAPICallPreHandler
    {
        /// <summary>
        /// Service Name
        /// </summary>
	    private readonly string serviceName;

        /// <summary>
        /// API method
        /// </summary>
	    private readonly string method;

        /// <summary>
        /// Raw payload from stubs
        /// </summary>
		private readonly string rawPayLoad;

	    /// <summary>
	    /// API Username for authentication
	    /// </summary>
	    private string apiUsername;

	    /// <summary>
	    /// {@link ICredential} for authentication
	    /// </summary>
	    private ICredential credential;
        
        /// <summary>
        /// Access token if any for authorization
        /// </summary>
		private string accessToken;
        
        /// <summary>
        /// TokenSecret if any for authorization
        /// </summary>
        private string tokenSecret;

	    /// <summary>
	    /// SDK Name used in tracking
	    /// </summary>
        private string sdkNme;

        /// <summary>
        /// SDK Version
        /// </summary>
	    private string sdkVrsion;
        
        /// <summary>
        /// Internal variable to hold headers
        /// </summary>
	    private Dictionary<string, string> headers;

        /// <summary>
        /// Port name
        /// </summary>
        private string prtName;

        /// <summary>
        /// SDK Configuration
        /// </summary>
        private Dictionary<string, string> config;

        /// <summary>
	    /// Private constructor
	    /// </summary>
	    /// <param name="rawPayLoad"></param>
	    /// <param name="serviceName"></param>
	    /// <param name="method"></param>
        private PlatformAPICallPreHandler(string rawPayLoad, string serviceName, string method, Dictionary<string, string> config)
            : base()
        {
            this.rawPayLoad = rawPayLoad;
		    this.serviceName = serviceName;
		    this.method = method;
            this.config = (config == null) ? ConfigManager.Instance.GetProperties() : config;
	    }

        /// <summary>
        /// NVPAPICallPreHandler
        /// </summary>
        /// <param name="rawPayLoad"></param>
        /// <param name="serviceName"></param>
        /// <param name="method"></param>
        /// <param name="apiUsername"></param>
        /// <param name="accessToken"></param>
        /// <param name="tokenSecret"></param>
	    public PlatformAPICallPreHandler(Dictionary<string, string> config, string rawPayLoad, string serviceName, string method,
            string apiUsername, string accessToken, string tokenSecret)
            : this(rawPayLoad, serviceName, method, config)
        {
            try
            {
                this.apiUsername = apiUsername;
                this.accessToken = accessToken;
                this.tokenSecret = tokenSecret;
                InitCredential();
            }
            catch(System.Exception ex)
            {
                throw ex;
            }		    
	    }

	    /// <summary>
        /// NVPAPICallPreHandler
	    /// </summary>
	    /// <param name="rawPayLoad"></param>
	    /// <param name="serviceName"></param>
	    /// <param name="method"></param>
	    /// <param name="credential"></param>
	    public PlatformAPICallPreHandler(Dictionary<string, string> config, string rawPayLoad, string serviceName,string method,
            ICredential credential)
            : this(rawPayLoad, serviceName, method, config)
        {  		
		    if (credential == null) 
            {
			    throw new ArgumentException("Credential is null in NVPAPICallPreHandler");
		    }
		    this.credential = credential;
	    }
        	    
        /// <summary>
        /// Gets and sets the SDK Name
        /// </summary>
	    public string SDKName
        {
            get
            {
                return sdkNme;
            }
            set
            {
                this.sdkNme = value;
            }
	    }

	    /// <summary>
        /// Gets and sets the SDK version
	    /// </summary>
	    public string SDKVersion
        {
           get
           {
               return sdkVrsion;
           }
           set
           {
               this.sdkVrsion = value;
           }
	    }

        /// <summary>
        /// Gets and sets the port name
        /// </summary>
        public string PortName
        {
            get
            {
                return prtName;
            }
            set
            {
                this.prtName = value;
            }
        }

        /// <summary>
        /// Returns the Header
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetHeaderMap()
        {
            try
            {
                if (headers == null)
                {
                    headers = new Dictionary<string, string>();
                    if (credential is SignatureCredential)
                    {
                        SignatureHttpHeaderAuthStrategy signatureHttpHeaderAuthStrategy = new SignatureHttpHeaderAuthStrategy(GetEndPoint());
                        headers = signatureHttpHeaderAuthStrategy.GenerateHeaderStrategy((SignatureCredential)credential);
                    }
                    else if (credential is CertificateCredential)
                    {
                        CertificateHttpHeaderAuthStrategy certificateHttpHeaderAuthStrategy = new CertificateHttpHeaderAuthStrategy(GetEndPoint());
                        headers = certificateHttpHeaderAuthStrategy.GenerateHeaderStrategy((CertificateCredential)credential);
                    }
                    foreach (KeyValuePair<string, string> pair in GetDefaultHttpHeadersNVP())
                    {
                        headers.Add(pair.Key, pair.Value);
                    }
                }
            }
            catch (OAuthException ae)
            {
                throw ae;
            }
            return headers;
        }

        /// <summary>
        /// Returns the raw payload as no processing necessary for NVP
        /// </summary>
        /// <returns></returns>
	    public string GetPayLoad() 
        {
		    return rawPayLoad;
	    }

        /// <summary>
        /// Returns the endpoint url
        /// </summary>
        /// <returns></returns>
	    public string GetEndPoint()
        {
            string endpoint = null;
            if (PortName != null && config.ContainsKey(PortName) && !string.IsNullOrEmpty(config[PortName]))
            {
                endpoint = config[PortName];
            }
            else if (config.ContainsKey(BaseConstants.END_POINT_CONFIG))
            {
                endpoint = config[BaseConstants.END_POINT_CONFIG];
            }
            else if (config.ContainsKey(BaseConstants.APPLICATION_MODE_CONFIG))
            {
                switch (config[BaseConstants.APPLICATION_MODE_CONFIG].ToLower())
                {
                    case BaseConstants.LIVE_MODE:
                        endpoint = BaseConstants.PLATFORM_LIVE_ENDPOINT;
                        break;
                    case BaseConstants.SANDBOX_MODE:
                        endpoint = BaseConstants.PLATFORM_SANDBOX_ENDPOINT;
                        break;
                    default:
                        throw new ConfigException("You must specify one of mode(live/sandbox) OR endpoint in the configuration");
                }                
            }
            else
            {
                throw new ConfigException("You must specify one of mode or endpoint in the configuration");
            }
            
            if (endpoint != null)
            {
                if(!endpoint.EndsWith("/"))
                {
                    endpoint = endpoint + "/";
                }
                endpoint = endpoint + serviceName + "/" + method;
            }
            return endpoint;
        }

        /// <summary>
        /// Reurns instance of ICredential
        /// </summary>
        /// <returns></returns>
	    public ICredential GetCredential() 
        {
		    return credential;
	    }

        /// <summary>
        /// Returns the credentials
        /// </summary>
        /// <returns></returns>
	    private ICredential GetCredentials()  
        {
		    ICredential returnCredential = null;

            try
            {
                CredentialManager credentialMngr = CredentialManager.Instance;
                returnCredential = credentialMngr.GetCredentials(this.config, apiUsername);

                if (!string.IsNullOrEmpty(accessToken))
                {
                    IThirdPartyAuthorization toknAuthuthorization = new TokenAuthorization(accessToken, tokenSecret);

                    if (returnCredential is SignatureCredential)
                    {
                        SignatureCredential sigCred = (SignatureCredential)returnCredential;
                        sigCred.ThirdPartyAuthorization = toknAuthuthorization;
                    }
                    else if (returnCredential is CertificateCredential)
                    {
                        CertificateCredential certCred = (CertificateCredential)returnCredential;
                        certCred.ThirdPartyAuthorization = toknAuthuthorization;
                    }
                }
            }
            catch(System.Exception ex)
            {
                throw ex;
            }
		    return returnCredential;
	    }

        /// <summary>
        /// Returns the Default Http Headers NVP
        /// </summary>
        /// <returns></returns>
	    private Dictionary<string, string> GetDefaultHttpHeadersNVP() 
        {
		    Dictionary<string, string> returnMap = new Dictionary<string, string>();

            try
            {
                returnMap.Add(BaseConstants.PAYPAL_APPLICATION_ID_HEADER, GetApplicationID());
                returnMap.Add(BaseConstants.PAYPAL_REQUEST_DATA_FORMAT_HEADER, BaseConstants.NVP);
                returnMap.Add(BaseConstants.PAYPAL_RESPONSE_DATA_FORMAT_HEADER, BaseConstants.NVP);
                returnMap.Add(BaseConstants.PAYPAL_REQUEST_SOURCE_HEADER, SDKName + "-" + SDKVersion);
                returnMap.Add(BaseConstants.PAYPAL_SANDBOX_EMAIL_ADDRESS_HEADER, GetSandboxEmailAddress());
                returnMap.Add(BaseConstants.PAYPAL_SANDBOX_DEVICE_IPADDRESS, GetDeviceIPAddress());
            }
            catch(System.Exception ex)
            {
                throw ex;
            }
		    return returnMap;
	    }

        /// <summary>
        /// Returns Application ID
        /// </summary>
        /// <returns></returns>
	    private string GetApplicationID() 
        {
		    string applicationID = string.Empty;
		    if (credential is CertificateCredential) 
            {
			    applicationID = ((CertificateCredential) credential).ApplicationID;
		    } 
            else if (credential is SignatureCredential) 
            {
			    applicationID = ((SignatureCredential) credential).ApplicationID;
		    }
		    return applicationID;
	    }

	    private void InitCredential() 
        {
		    if (credential == null) 
            {
                try
                {
                    credential = GetCredentials();
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }                
		    }
	    }

        private string GetDeviceIPAddress()
        {
            if (config.ContainsKey(BaseConstants.CLIENT_IP_ADDRESS_CONFIG) && 
                !string.IsNullOrEmpty(config[BaseConstants.CLIENT_IP_ADDRESS_CONFIG]))
            {
                return config[BaseConstants.CLIENT_IP_ADDRESS_CONFIG];
            }
            else
            {
                return string.Empty;
            }
        }

        private string GetSandboxEmailAddress()
        {
            if (config.ContainsKey(BaseConstants.PAYPAL_SANDBOX_EMAIL_ADDRESS_CONFIG) && 
                !string.IsNullOrEmpty(config[BaseConstants.PAYPAL_SANDBOX_EMAIL_ADDRESS_CONFIG]))
            {
                return config[BaseConstants.PAYPAL_SANDBOX_EMAIL_ADDRESS_CONFIG];
            }
            else
            {
                return BaseConstants.PayPalSandboxEmailAddressDefault;
            }
        }    
    }
}
