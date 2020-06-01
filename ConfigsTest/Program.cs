using System;
using HConfigs;

namespace ConfigsTest
{
    class Program
    {
        static void Main(string[] args)
        {
            OptionParserBase[] parsers = { new DoubleParser(), new VectorParser() };

            TestConfig test = new TestConfig();
            test.FloatProperty = 0.01f;
            test.StringField = "test";
            test.StringProperty = "Example";
            test.IntProperty = 10;
            test.IntField = 3;
            test.IntProperty2 = 7;
            test.DoubleProperty = 3.14;
            test.VectorProperty1 = new TestVector { x = 1, y = 10, z = -5 };
            test.VectorProperty2 = null;

            Config.Save(test, parsers);

            TestConfig check = Config.Load<TestConfig>(parsers);
            Console.WriteLine(check.FloatProperty);
            Console.WriteLine(check.StringField);
            Console.WriteLine(check.StringProperty);
            Console.WriteLine(check.IntProperty);
            Console.WriteLine(check.IntProperty2);
            Console.WriteLine(check.IntField);
            Console.WriteLine(check.DoubleProperty);
            Console.WriteLine(check.VectorProperty1);
            Console.WriteLine(check.VectorProperty2);
        }

        public class DoubleParser : OptionParserBase
        {
            public override object Decode(string data)
            {
                return double.Parse(data);
            }

            public override string Encode(object data)
            {
                if (data.GetType() != typeof(double))
                    throw new ConfigException($"Cannot convert object of type {data.GetType()} to a double");
                return ((double)data).ToString();
            }

            public DoubleParser() : base("double", typeof(double))
            {

            }
        }

        public class TestVector
        {
            public int x, y, z;
            public override string ToString()
            {
                return $"<{x},{y},{z}>";
            }
        }

        public class VectorParser : OptionParserBase
        {
            public VectorParser() : base("Vector", typeof(TestVector))
            {
            }

            public override object Decode(string data)
            {
                if (data.ToLower().Equals("null"))
                    return null;
                else
                {
                    try
                    {
                        string interior = data.Substring(1, data.Length - 2);
                        string[] comp = interior.Split(',');
                        TestVector val = new TestVector();
                        val.x = int.Parse(comp[0]);
                        val.y = int.Parse(comp[1]);
                        val.z = int.Parse(comp[2]);
                        return val;
                    }
                    catch
                    {
                        return null;
                    }
                }
            }

            public override string Encode(object data)
            {
                TestVector vec = data as TestVector;
                if (vec == null)
                    return "null";
                else
                    return $"<{vec.x},{vec.y},{vec.z}>";
            }
        }
    }
}
