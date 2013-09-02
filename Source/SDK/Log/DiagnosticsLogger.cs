﻿using System;
using System.Diagnostics;
using System.Globalization;

namespace PayPal.Log
{
    /// <summary>
    /// System.Diagnostics wrapper
    /// </summary>
    internal class DiagnosticsLogger : BaseLogger
    {
        volatile int id = 0;
        TraceSource sourceTrace;

        public DiagnosticsLogger(Type givenType) : base(givenType)
        {
            this.sourceTrace = TraceSourceUtil.GetTraceSource(givenType);
        }

        public override bool IsDebugEnabled
        {
            get
            {
                return (sourceTrace != null);
            }
        }

        public override bool IsErrorEnabled
        {
            get
            {
                return (sourceTrace != null);
            }
        }

        public override bool IsInfoEnabled
        {
            get
            {
                return (sourceTrace != null);
            }
        }

        public override bool IsWarnEnabled
        {
            get
            {
                return (sourceTrace != null);
            }
        }

        /// <summary>
        /// Override the wrapper for System.Diagnostics TraceEventType.Verbose
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public override void Debug(string message, System.Exception exception)
        {
            sourceTrace.TraceData(TraceEventType.Verbose, id++, new LogMessage(message), exception);
        }
        
        /// <summary>
        /// Override the wrapper for System.Diagnostics TraceEventType.Verbose overload
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public override void DebugFormat(string format, params object[] args)
        {
            sourceTrace.TraceData(TraceEventType.Verbose, id++, new LogMessage(format, args));
        }
        
        /// <summary>
        /// Override the wrapper for System.Diagnostics TraceEventType.Error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public override void Error(string message, System.Exception exception)
        {
            sourceTrace.TraceData(TraceEventType.Error, id++, new LogMessage(message), exception);
        }

        /// <summary>
        /// Override the wrapper for System.Diagnostics TraceEventType.Error overload
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public override void ErrorFormat(string format, params object[] args)
        {
            sourceTrace.TraceData(TraceEventType.Error, id++, new LogMessage(format, args));
        }

        /// <summary>
        /// Override the wrapper for System.Diagnostics TraceEventType.Information
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public override void Info(string message, System.Exception exception)
        {
            sourceTrace.TraceData(TraceEventType.Information, id++, new LogMessage(message), exception);
        }

        /// <summary>
        /// Override the wrapper for System.Diagnostics TraceEventType.Information overload
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public override void InfoFormat(string format, params object[] args)
        {
            sourceTrace.TraceData(TraceEventType.Information, id++, new LogMessage(format, args));
        }

        /// <summary>
        /// Override the wrapper for System.Diagnostics TraceEventType.Warning
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public override void Warn(string message, System.Exception exception)
        {
            sourceTrace.TraceData(TraceEventType.Warning, id++, new LogMessage(message), exception);
        }

        /// <summary>
        /// Override the wrapper for System.Diagnostics TraceEventType.Warning overload
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public override void WarnFormat(string format, params object[] args)
        {
            sourceTrace.TraceData(TraceEventType.Warning, id++, new LogMessage(format, args));
        }
        
        /// <summary>
        /// Override flush
        /// </summary>
        public override void Flush()
        {
            if (sourceTrace != null)
            {
                this.sourceTrace.Flush();
            }
        }
    }
}
