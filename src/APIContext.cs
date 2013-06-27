﻿using System;
using System.Collections.Generic;

namespace PayPal
{
    public class APIContext
    {
        /// <summary>
        /// Access Token
        /// </summary>
        private string Token;

        /// <summary>
        /// Request ID
        /// </summary>
        private string ReqID;

        /// <summary>
        /// Explicit default constructor
        /// </summary>
        public APIContext() { }

        /// <summary>
        /// Access Token required for the call
        /// </summary>
        /// <param name="token"></param>
        public APIContext(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException("AccessToken cannot be null");
            }
            this.Token = token;
        }

        /// <summary>
        /// Access Token and Request ID required for the call
        /// </summary>
        /// <param name="token"></param>
        /// <param name="requestID"></param>
        public APIContext(string token, string requestID)
            : this(token)
        {
            if (string.IsNullOrEmpty(requestID))
            {
                throw new ArgumentNullException("RequestId cannot be null");
            }
            this.ReqID = requestID;
        }

        public string AccessToken
        {
            get
            {
                return Token;
            }
        }

        /// <summary>
        /// Mask Request ID
        /// </summary>
        public bool MaskRequestID
        {   
            get;
            set;
        }
         
        /// <summary>
        /// Request ID
        /// </summary>
        public string RequestID
        {
            get
            {
                string returnID = null;
                if (!MaskRequestID)
                {
                    if (string.IsNullOrEmpty(ReqID))
                    {
                        ReqID = Convert.ToString(Guid.NewGuid());
                    }
                    returnID = ReqID;
                }
                return returnID;
            }
        }

        /// <summary>
        /// Dynamic Configuration
        /// </summary>
        public Dictionary<string, string> Config
        {
            get;
            set;
        }
    }
}