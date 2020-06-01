using System;
using System.Collections.Generic;
using System.Text;

namespace HConfigs
{
    public class ConfigException : Exception
    {
        public ConfigException() { }
        public ConfigException(string message) : base(message) { }
        public ConfigException(string message, Exception inner) { }
    }
}
