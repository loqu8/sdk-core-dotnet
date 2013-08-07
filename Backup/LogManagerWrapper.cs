using System;
// NuGet Install
// install PayPalCoreSDK -excludeversion -outputDirectory .\Packages
// 2.0
using log4net;

namespace PayPal
{
    public static class LogManagerWrapper
    {
        public static ILog GetLogger(Type type)
        {
            if (LogManager.GetCurrentLoggers().Length == 0)
            {
                LoadConfig();
            }
            return LogManager.GetLogger(type);
        }

        private static void LoadConfig()
        {
            log4net.Config.XmlConfigurator.Configure();
        }
    }
}