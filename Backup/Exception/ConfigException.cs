using log4net;

namespace PayPal.Exception
{
    public class ConfigException : System.Exception
    {
        /// <summary>
        /// Logger
        /// </summary>
        private static readonly ILog logger = LogManagerWrapper.GetLogger(typeof(ConfigException));

		/// <summary>
		/// Represents application configuration errors 
		/// </summary>
        public ConfigException() : base() { }
        
		/// <summary>
		/// Represents errors that occur during application execution
		/// </summary>
		/// <param name="message">The message that describes the error</param>
		public ConfigException(string message): base(message)
		{
			if (logger.IsErrorEnabled)
			{
				logger.Error(message, this);
			}
		}

		/// <summary>
		/// Represents errors that occur during application execution
		/// </summary>
		/// <param name="message">The message that describes the error</param>
		/// <param name="cause">The exception that is the cause of the current exception</param>
		public ConfigException(string message, System.Exception cause): base(message, cause)
		{
			if (logger.IsErrorEnabled) 
			{
				logger.Error(message, this);
			}
		}
	} // ConfigException
}
