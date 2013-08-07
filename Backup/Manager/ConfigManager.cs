using System;
using System.Collections.Generic;
using System.Configuration;
using PayPal.Exception;

namespace PayPal.Manager
{    
    /// <summary>
    /// ConfigManager loads the configuration file and hands out
    /// appropriate parameters to application
    /// </summary>
    public sealed class ConfigManager
    {
        private SDKConfigHandler configHandler;

        private Dictionary<string, string> configValues;

        private static readonly Dictionary<string, string> defaultConfig;
        
        
        static ConfigManager()
        {
            defaultConfig = new Dictionary<string, string>();
            // Default connection timeout in milliseconds
            defaultConfig[BaseConstants.HTTP_CONNECTION_TIMEOUT_CONFIG] = "30000";
            defaultConfig[BaseConstants.HTTP_CONNECTION_RETRY_CONFIG] = "1";
            defaultConfig[BaseConstants.CLIENT_IP_ADDRESS_CONFIG] = "127.0.0.1";
        }

        /// <summary>
        /// Singleton instance of the ConfigManager
        /// </summary>
        private static volatile ConfigManager singletonInstance;

        private static object syncRoot = new Object();


        /// <summary>
        /// Gets the Singleton instance of the ConfigManager
        /// </summary>
        public static ConfigManager Instance
        {
            get
            {
                if (singletonInstance == null)
                {
                    lock (syncRoot)
                    {
                        if (singletonInstance == null)
                            singletonInstance = new ConfigManager();
                    }
                }
                return singletonInstance;
            }
        }

        /// <summary>
        /// Private constructor
        /// </summary>
        private ConfigManager()
        {
            configHandler = (SDKConfigHandler)ConfigurationManager.GetSection("paypal");
            if (configHandler == null)
            {
                throw new ConfigException("Cannot parse *.Config file. Ensure you have configured the 'paypal' section correctly.");
            }
            this.configValues = new Dictionary<string, string>();

            NameValueConfigurationCollection settings = this.configHandler.Settings;
            foreach (string key in settings.AllKeys)
            {
                this.configValues.Add(settings[key].Name, settings[key].Value);
            }

            int i = 0;
            foreach (ConfigurationElement elem in this.configHandler.Accounts)
            {
                Account account = (Account)elem;
                if (account.APIUsername != null && account.APIUsername != "")
                {
                    this.configValues.Add("account" + i + ".apiUsername", account.APIUsername);
                }
                if (account.APIPassword != null && account.APIPassword != "")
                {
                    this.configValues.Add("account" + i + ".apiPassword", account.APIPassword);
                }
                if (account.APISignature != null && account.APISignature != "")
                {
                    this.configValues.Add("account" + i + ".apiSignature", account.APISignature);
                }
                if (account.APICertificate != null && account.APICertificate != "")
                {
                    this.configValues.Add("account" + i + ".apiCertificate", account.APICertificate);
                }
                if (account.PrivateKeyPassword != null && account.PrivateKeyPassword != "")
                {
                    this.configValues.Add("account" + i + ".privateKeyPassword", account.PrivateKeyPassword);
                }
                if (account.CertificateSubject != null && account.CertificateSubject != "")
                {
                    this.configValues.Add("account" + i + ".subject", account.CertificateSubject);
                }
                if (account.ApplicationId != null && account.ApplicationId != "")
                {
                    this.configValues.Add("account" + i + ".applicationId", account.ApplicationId);
                }
                i++;
            }
        }

        /// <summary>
        /// Returns all properties from the config file
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetProperties()
        {
            return this.configValues;
        }
    
        /// <summary>
        /// Creates new configuration that combines incoming configuration dictionary
        /// and defaults
        /// </summary>
        /// <returns>Default configuration dictionary</returns>
        public static Dictionary<string, string> getConfigWithDefaults(Dictionary<string, string> config) {
            Dictionary<string, string> ret = new Dictionary<string, string>(config);
            foreach (string key in ConfigManager.defaultConfig.Keys)
            {
                if(!ret.ContainsKey(key))
                {
                    ret.Add(key, defaultConfig[key]);
                }
            }
            return ret;
        }

        public static string getDefault(string configKey)
        {
            if (ConfigManager.defaultConfig.ContainsKey(configKey))
            {
                return ConfigManager.defaultConfig[configKey];
            }
            return null;
        }
    }
}
