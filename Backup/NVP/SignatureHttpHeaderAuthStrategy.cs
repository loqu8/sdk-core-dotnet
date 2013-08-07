using System;
using System.Collections.Generic;
using System.Text;
using PayPal.Exception;
using PayPal.Authentication;
using log4net;

namespace PayPal.NVP
{
    public class SignatureHttpHeaderAuthStrategy : AbstractSignatureHttpHeaderAuthStrategy
    {
        /// <summary>
        /// Logger
        /// </summary>
        private static readonly ILog logger = LogManagerWrapper.GetLogger(typeof(SignatureHttpHeaderAuthStrategy));

        /// <summary>
        /// SignatureHttpHeaderAuthStrategy
        /// </summary>
        /// <param name="endPointUrl"></param>
        public SignatureHttpHeaderAuthStrategy(string endpointURL) : base(endpointURL) { }

	    
        /// <summary>
        /// Processing TokenAuthorization} using SignatureCredential
        /// </summary>
        /// <param name="signCredential"></param>
        /// <param name="toknAuthorization"></param>
        /// <returns></returns>
        protected internal override Dictionary<string, string> ProcessTokenAuthorization(
                SignatureCredential signCredential, TokenAuthorization toknAuthorization)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            try
            {
                OAuthGenerator sigGenerator = new OAuthGenerator(signCredential.UserName, signCredential.Password);
                sigGenerator.setHTTPMethod(OAuthGenerator.HTTPMethod.POST);
                sigGenerator.setToken(toknAuthorization.AccessToken);
                sigGenerator.setTokenSecret(toknAuthorization.TokenSecret);
                string tokenTimeStamp = Timestamp;
                sigGenerator.setTokenTimestamp(tokenTimeStamp);
                logger.Debug("token = " + toknAuthorization.AccessToken + " tokenSecret=" + toknAuthorization.TokenSecret + " uri=" + endpointURL);
                sigGenerator.setRequestURI(endpointURL);

                //Compute Signature
                string sign = sigGenerator.ComputeSignature();
                logger.Debug("Permissions signature: " + sign);
                string authorization = "token=" + toknAuthorization.AccessToken + ",signature=" + sign + ",timestamp=" + tokenTimeStamp;
                logger.Debug("Authorization string: " + authorization);
                headers.Add(BaseConstants.PAYPAL_AUTHORIZATION_PLATFORM_HEADER, authorization);
            }
            catch (OAuthException ae)
            {
                throw ae;
            }
            return headers;
        }

        /// <summary>
        /// Gets the UTC Timestamp
        /// </summary>
        private static string Timestamp
        {
            get
            {
                TimeSpan span = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                return Convert.ToInt64(span.TotalSeconds).ToString();
            }
        }
    }
}
