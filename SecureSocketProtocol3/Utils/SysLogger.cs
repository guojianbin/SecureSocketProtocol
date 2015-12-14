﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SecureSocketProtocol3.Utils
{
    public class SysLogger
    {
        public static event SysLogDeletegate onSysLog;
        private static object Locky = new object();

        internal static void Log(string Message, SysLogType Type, Exception ex = null)
        {
            lock (Locky)
            {
                if (onSysLog != null)
                {
                    onSysLog(Message, Type, ex);
                }
            }
        }
    }
}