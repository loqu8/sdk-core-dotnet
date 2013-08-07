using System;
using System.Collections.Generic;
using System.Text;
using PayPal.Authentication;

namespace PayPal.SOAP
{
    public class SignatureSOAPHeaderAuthStrategy : IAuthenticationStrategy<string, SignatureCredential>
    {
        /// <summary>
        /// Instance of ThirdPartyAuthorization
        /// </summary>
        private IThirdPartyAuthorization thrdPartyAuthorization;

        /// <summary>
        /// Constructor
        /// </summary>
        public SignatureSOAPHeaderAuthStrategy() { }

        /// <summary>
        /// Gets and sets any instance of {@link ThirdPartyAuthorization}
        /// </summary>
        public IThirdPartyAuthorization ThirdPartyAuthorization
        {
            get
            {
                return thrdPartyAuthorization;
            }
            set
            {
                this.thrdPartyAuthorization = value;
            }
        }

        public string GenerateHeaderStrategy(SignatureCredential credential)
        {
            string payLoad = null;
            if (thrdPartyAuthorization is TokenAuthorization)
            {
                payLoad = TokenAuthPayLoad();
            }
            else if (thrdPartyAuthorization is SubjectAuthorization)
            {
                payLoad = AuthPayLoad(credential, (SubjectAuthorization)thrdPartyAuthorization);
            }
            else
            {
                payLoad = AuthPayLoad(credential, null);
            }
            return payLoad;
        }

        private string TokenAuthPayLoad()
        {
            StringBuilder soapMessage = new StringBuilder();
            soapMessage.Append("<ns:RequesterCredentials/>");
            return soapMessage.ToString();
        }

        private string AuthPayLoad(SignatureCredential signCredential,
                SubjectAuthorization subjectAuth)
        {  
            StringBuilder soapMessage = new StringBuilder();
            soapMessage.Append("<ns:RequesterCredentials>");
            soapMessage.Append("<ebl:Credentials>");
            soapMessage.Append("<ebl:Username>" + signCredential.UserName
                    + "</ebl:Username>");
            soapMessage.Append("<ebl:Password>" + signCredential.Password
                    + "</ebl:Password>");
            soapMessage.Append("<ebl:Signature>" + signCredential.Signature
                    + "</ebl:Signature>");
            if (subjectAuth != null)
            {
                soapMessage.Append("<ebl:Subject>" + subjectAuth.Subject
                        + "</ebl:Subject>");
            }
            soapMessage.Append("</ebl:Credentials>");
            soapMessage.Append("</ns:RequesterCredentials>");
            return soapMessage.ToString();
        }
    }
}
