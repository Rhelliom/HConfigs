using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HConfigs
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class Config : Attribute
    {
        public string Header { get; set; } = null;
        public string File { get; }
        public Config(string path)
        {
            File = path;
        }

        public static T Load<T>(OptionParserBase[] customParsers = null)
        {
            object[] cfgAttr = typeof(T).GetCustomAttributes(typeof(Config), true);
            if (cfgAttr.Length != 1)
                throw new ConfigException($"Type {typeof(T)} is either not marked as a Config or has multiple Config attributes");
            string filepath = ((Config)cfgAttr[0]).File;
            string[] cfgFile = System.IO.File.ReadAllLines(filepath);
            //Parse
            if (customParsers == null)
                customParsers = new OptionParserBase[0];

            object cfg = Activator.CreateInstance(typeof(T));
            for(int i = 0; i < cfgFile.Length; i++)
            {
                if (cfgFile[i].StartsWith("#") || cfgFile[i].Trim().Length == 0)
                    continue; //Skip comments
                else if (cfgFile[i].StartsWith("[")) //This is a normal option
                {
                    string opName;
                    object opVal = decode(cfgFile[i], customParsers, out opName);
                    if (opVal == null)
                        continue;
                    var pfit = typeof(T).GetProperties().Where(p =>
                    {
                        object[] attr = p.GetCustomAttributes(typeof(Option), true);
                        if (attr.Length > 1)
                            throw new ConfigException($"Member {p} has more than one Option attribute");
                        if (attr.Length == 0)
                            return false;

                        if (((Option)attr[0]).Name == null)
                            return p.Name.Equals(opName);
                        else
                            return ((Option)attr[0]).Name.Equals(opName);
                    });
                    var ffit = typeof(T).GetFields().Where(p =>
                    {
                        object[] attr = p.GetCustomAttributes(typeof(Option), true);
                        if (attr.Length > 1)
                            throw new ConfigException($"Member {p} has more than one Option attribute");
                        if (attr.Length == 0)
                            return false;

                        if (((Option)attr[0]).Name == null)
                            return p.Name.Equals(opName);
                        else
                            return ((Option)attr[0]).Name.Equals(opName);
                    });
                    if(ffit.Count() + pfit.Count() > 1)
                    {
                        throw new ConfigException($"More than one element in {typeof(T)} is labeled with the option name {opName}");
                    }
                    if(pfit.Count() > 0)
                    {
                        pfit.First().SetValue(cfg, opVal);
                    }
                    else if(ffit.Count() > 0)
                    {
                        ffit.First().SetValue(cfg, opVal);
                    }
                }
            }

            return (T)cfg;
        }

        public static void Save<T>(T cfg, OptionParserBase[] customParsers = null)
        {
            object[] cfgAttr = typeof(T).GetCustomAttributes(typeof(Config), true);
            if (cfgAttr.Length != 1)
                throw new ConfigException($"Type {typeof(T)} is either not marked as a Config or has multiple Config attributes");
            string filepath = ((Config)cfgAttr[0]).File;
            //Parse the config object
            if (customParsers == null)
                customParsers = new OptionParserBase[0];
            List<KeyValuePair<Option,string[]>> lines = new List<KeyValuePair<Option,string[]>>();
            foreach(var member in typeof(T).GetProperties().Where(p => IsDefined(p, typeof(Option))))
            {
                object[] attr = member.GetCustomAttributes(typeof(Option), true);
                Option optionAttribute = (Option)attr[0];
                string opstring = (optionAttribute.Comment == null ? "" : "#" + optionAttribute.Comment + Environment.NewLine);
                opstring += encode((optionAttribute.Name == null ? member.Name : optionAttribute.Name), member.GetValue(cfg), member.PropertyType, customParsers);
                lines.Add(new KeyValuePair<Option, string[]>(optionAttribute, new string[]{ opstring, optionAttribute.Name == null ? member.Name : optionAttribute.Name }));
            }
            foreach (var member in typeof(T).GetFields().Where(p => IsDefined(p, typeof(Option))))
            {
                object[] attr = member.GetCustomAttributes(typeof(Option), true);
                Option optionAttribute = (Option)attr[0];
                string opstring = (optionAttribute.Comment == null ? "" : "#" + optionAttribute.Comment + Environment.NewLine);
                opstring += encode((optionAttribute.Name == null ? member.Name : optionAttribute.Name), member.GetValue(cfg), member.FieldType, customParsers);
                lines.Add(new KeyValuePair<Option, string[]>(optionAttribute, new string[]{ opstring, optionAttribute.Name == null ? member.Name : optionAttribute.Name }));
            }
            //Sorting
            lines.Sort((first, second) =>
           {
               if (first.Key.Region == null && second.Key.Region != null)
                   return -1;
               if (second.Key.Region == null && first.Key.Region != null)
                   return 1;

               int regionCompare = first.Key.Region == null && second.Key.Region == null ? 0 : first.Key.Region.CompareTo(second.Key.Region);
               if (regionCompare != 0)
                   return regionCompare;

               return first.Value[1].CompareTo(second.Value[1]);
           });
            List<string> fileLines = new List<string>();
            string currentRegion = null;
            string head = ((Config)cfgAttr[0]).Header;
            if (head != null)
                fileLines.Add($"### {head} ###");
            for(int i = 0; i<lines.Count; i++)
            {
                if(lines[i].Key.Region != currentRegion) //New Region
                {
                    currentRegion = lines[i].Key.Region;
                    fileLines.Add($"#--{currentRegion}--#");
                }
                fileLines.Add(lines[i].Value[0]);
            }

            System.IO.File.WriteAllLines(filepath, fileLines);
        }

        private static string encode(string name, object val, Type t, OptionParserBase[] customParsers)
        {
            
            if (t.Equals(typeof(string))){
                return $"[string]{name.Trim()}: {val}";
            }
            else if (t.Equals(typeof(int)))
            {
                return $"[int]{name.Trim()}: {val}";
            }
            else if (t.Equals(typeof(float)))
            {
                return $"[float]{name.Trim()}: {val}";
            }
            else if (t.Equals(typeof(bool)))
            {
                return $"[bool]{name.Trim()}: {val}";
            }
            else
            {
                foreach(OptionParserBase parser in customParsers)
                {
                    if(parser.Type == t)
                    {
                        return $"[{parser.Tag}]{name.Trim()}: {parser.Encode(val)}";
                    }
                }
            }
            throw new ConfigException($"{name} is marked as an option, but the specified type is not supported (type {t})");
        }

        private static object decode(string data, OptionParserBase[] customParsers, out string optionName)
        {
            string[] split = data.Split(':');
            if (split.Length != 2)
            {
                optionName = "";
                return null;
            }
            string value = split[1].Trim();
            string[] front = split[0].Split(']');
            if(front.Length != 2)
            {
                optionName = "";
                return null;
            }
            string typetag = front[0].Substring(1).Trim();
            optionName = front[1].Trim();

            try
            {
                switch (typetag)
                {
                    case "string": return value;
                    case "int": return int.Parse(value);
                    case "float": return float.Parse(value);
                    case "bool": return float.Parse(value);
                    default: 
                        foreach(OptionParserBase parser in customParsers)
                        {
                            if (parser.Tag.Equals(typetag))
                            {
                                return parser.Decode(value);
                            }
                        }
                        return null;
                }
            }
            catch
            {
                return null;
            }
        }

        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
        public class Option : Attribute
        {
            public string Name { get; set; } = null;
            public string Comment { get; set; } = null;
            public string Region { get; set; } = null;
        }

        
    }
}
