using log4net;
using PayPal.Authentication;
using PayPal.Exception;
using System.Collections.Generic;
using System;

namespace PayPal.Manager
{
    /// <summary>
    /// Reads API credentials to be used with the application
    /// </summary>
    public sealed class CredentialManager
    {
        /// <summary>
        /// Logger
        /// </summary>
        private static readonly ILog logger = LogManagerWrapper.GetLogger(typeof(CredentialManager));

        /// <summary>
        /// Singleton instance of ConnectionManager
        /// </summary>
        private static readonly CredentialManager singletonInstance = new CredentialManager();

        private static string ACCOUNT_PREFIX = "account";
        /// <summary>
        /// Explicit static constructor to tell C# compiler
        /// not to mark type as beforefieldinit
        /// </summary>
        static CredentialManager() 
        {
            //Load log configuration
            //log4net.Config.XmlConfigurator.Configure();
        }

        /// <summary>
        /// Private constructor
        /// </summary>
        private CredentialManager() { }
        
        /// <summary>
        /// Gets the Singleton instance of ConnectionManager
        /// </summary>
        public static CredentialManager Instance
        {
            get
            {
                return singletonInstance;
            }
        }

        /// <summary>
        /// Returns the default Account Name
        /// </summary>
        /// <returns></returns>
        private Account GetAccount(Dictionary<string, string> config, string apiUsername)
        {                        
            foreach (KeyValuePair<string, string> kvPair in config)
            {
                if(kvPair.Key.EndsWith(".apiUsername"))
                {
                    if (apiUsername == null || apiUsername.Equals(kvPair.Value)) 
                    {

                        string s = kvPair.Key.Substring(ACCOUNT_PREFIX.Length, kvPair.Key.IndexOf('.') - ACCOUNT_PREFIX.Length );

                        int i = Int32.Parse(kvPair.Key.Substring(ACCOUNT_PREFIX.Length, kvPair.Key.IndexOf('.') - ACCOUNT_PREFIX.Length ));
                        Account acct = new Account();
                        if (config.ContainsKey(ACCOUNT_PREFIX +  i + ".apiUsername")) 
                        {
                            acct.APIUsername = config[ACCOUNT_PREFIX +  i + ".apiUsername"];
                        }
                        if(config.ContainsKey(ACCOUNT_PREFIX +  i + ".apiPassword"))
                        {
                            acct.APIPassword = config[ACCOUNT_PREFIX +  i + ".apiPassword"];
                        }
                        if(config.ContainsKey(ACCOUNT_PREFIX +  i + ".apiSignature")) 
                        {
                            acct.APISignature = config[ACCOUNT_PREFIX +  i + ".apiSignature"];
                        }
                        if(config.ContainsKey(ACCOUNT_PREFIX +  i + ".apiCertificate")) 
                        {
                            acct.APICertificate = config[ACCOUNT_PREFIX +  i + ".apiCertificate"];
                        }
                        if (config.ContainsKey(ACCOUNT_PREFIX +  i + ".privateKeyPassword")) 
                        {
                            acct.PrivateKeyPassword = config[ACCOUNT_PREFIX +  i + ".privateKeyPassword"];
                        }            
                        if(config.ContainsKey(ACCOUNT_PREFIX +  i + ".subject"))
                        {
                            acct.CertificateSubject = config[ACCOUNT_PREFIX +  i + ".subject"];
                        }
                        if(config.ContainsKey(ACCOUNT_PREFIX +  i + ".applicationId"))
                        {
                            acct.ApplicationId = config[ACCOUNT_PREFIX +  i + ".applicationId"];
                        }
                        return acct;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the API Credentials
        /// </summary>
        /// <param name="apiUserName"></param>
        /// <returns></returns>
        public ICredential GetCredentials(Dictionary<string, string> config, string apiUserName)
        {
            ICredential credential = null;
            Account accnt = GetAccount(config, apiUserName);
            if (accnt == null)
            {
                throw new MissingCredentialException("Missing credentials for " + apiUserName);
            }
            if (!string.IsNullOrEmpty(accnt.APICertificate))
            {
                CertificateCredential certCredential = new CertificateCredential(accnt.APIUsername, accnt.APIPassword, accnt.APICertificate, accnt.PrivateKeyPassword);
                certCredential.ApplicationID = accnt.ApplicationId;
                if (!string.IsNullOrEmpty(accnt.CertificateSubject))
                {
                    SubjectAuthorization subAuthorization = new SubjectAuthorization(accnt.CertificateSubject);
                    certCredential.ThirdPartyAuthorization = subAuthorization;
                }
                credential = certCredential;
            }
            else
            {
                SignatureCredential signCredential = new SignatureCredential(accnt.APIUsername, accnt.APIPassword, accnt.APISignature);
                signCredential.ApplicationID = accnt.ApplicationId;
                if (!string.IsNullOrEmpty(accnt.SignatureSubject))
                {
                    SubjectAuthorization subjectAuthorization = new SubjectAuthorization(accnt.SignatureSubject);
                    signCredential.ThirdPartyAuthorization = subjectAuthorization;
                }
                credential = signCredential;
            }
            ValidateCredentials(credential);
            
            return credential;            
        }

        /// <summary>
        /// Validates the API Credentials
        /// </summary>
        /// <param name="apiCredentials"></param>
        private void ValidateCredentials(ICredential apiCredentials)
        {
            if (apiCredentials is SignatureCredential)
            {
                SignatureCredential credential = (SignatureCredential)apiCredentials;
                Validate(credential);
            }
            else if (apiCredentials is CertificateCredential)
            {
                CertificateCredential credential = (CertificateCredential)apiCredentials;
                Validate(credential);
            }
        }

        /// <summary>
        /// Validates the Signature Credentials
        /// </summary>
        /// <param name="apiCredentials"></param>
        private void Validate(SignatureCredential apiCredentials)
        {
            if (string.IsNullOrEmpty(apiCredentials.UserName))
            {
                throw new InvalidCredentialException(BaseConstants.ErrorMessages.err_username);
            }
            if (string.IsNullOrEmpty(apiCredentials.Password))
            {
                throw new InvalidCredentialException(BaseConstants.ErrorMessages.err_passeword);
            }
            if (string.IsNullOrEmpty(((SignatureCredential)apiCredentials).Signature))
            {
                throw new InvalidCredentialException(BaseConstants.ErrorMessages.err_signature);
            }
        }

        /// <summary>
        /// Validates the Certificate Credentials
        /// </summary>
        /// <param name="apiCredentials"></param>
        private void Validate(CertificateCredential apiCredentials)
        {
            if (string.IsNullOrEmpty(apiCredentials.UserName))
            {
                throw new InvalidCredentialException(BaseConstants.ErrorMessages.err_username);
            }
            if (string.IsNullOrEmpty(apiCredentials.Password))
            {
                throw new InvalidCredentialException(BaseConstants.ErrorMessages.err_passeword);
            }

            if (string.IsNullOrEmpty(((CertificateCredential)apiCredentials).CertificateFile))
            {
                throw new InvalidCredentialException(BaseConstants.ErrorMessages.err_certificate);
            }

            if (string.IsNullOrEmpty(((CertificateCredential)apiCredentials).PrivateKeyPassword))
            {
                throw new InvalidCredentialException(BaseConstants.ErrorMessages.err_privatekeypassword);
            }
        }      
    }
}
