using System;
using System.Collections.Generic;
using System.Text;
using HConfigs;
using static ConfigsTest.Program;

namespace ConfigsTest
{
    [Config("testconfig.txt")]
    class TestConfig
    {
        [Config.Option]
        public int IntField;

        [Config.Option(Comment = "An example string property")]
        public string StringProperty { get; set; }

        [Config.Option(Region = "Region 1")]
        public int IntProperty { get; set; }

        [Config.Option]
        public int IntProperty2 { get; set; }

        [Config.Option(Name = "ExampleFloat", Region = "Region 1")]
        public float FloatProperty { get; set; }

        [Config.Option(Name = "FieldExample", Comment = "Example using a field")]
        public string StringField;

        [Config.Option(Name = "DoubleProperty", Comment = "Requires a custom parser", Region = "Region 2")]
        public double DoubleProperty { get; set; }

        [Config.Option]
        public TestVector VectorProperty1 { get; set; }

        [Config.Option]
        public TestVector VectorProperty2 { get; set; }

    }
}
