using System;
using System.Collections.Generic;
using System.Text;

namespace HConfigs
{
    public abstract class OptionParserBase
    {
        public string Tag { get; }
        public Type Type { get; }

        public abstract string Encode(object data);

        public abstract object Decode(string data);

        protected OptionParserBase(string flag, Type type)
        {
            Tag = flag;
            Type = type;
        }
    }
}
