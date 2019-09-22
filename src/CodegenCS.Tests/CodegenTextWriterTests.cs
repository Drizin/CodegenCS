using CodegenCS;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public class Tests
    {
        CodegenTextWriter _w = null;

        [SetUp]
        public void Setup()
        {
            _w = new CodegenTextWriter();
        }

        [Test]
        public void TestMultiline()
        {
            _w.WriteLine(@"
                namespace codegencs
                {
                    public class Test1
                    {
                        // My Properties start here
                    }
                }");
            System.Diagnostics.Debug.WriteLine(_w.ToString());
            string expected =
@"namespace codegencs
{
    public class Test1
    {
        // My Properties start here
    }
}
";

            Assert.AreEqual(_w.ToString(), expected);
        }


        [Test]
        public void TestCStyleBlock()
        {
            string myNamespace = "codegencs";
            string myClass = "Test1";
            _w.WriteLine($"namespace {myNamespace}");
            using (_w.WithCStyleBlock())
            {
                _w.WriteLine($"public class {myClass}");
                using (_w.WithCStyleBlock())
                {
                    _w.WriteLine("// My Properties start here");
                }
            }
            System.Diagnostics.Debug.WriteLine(_w.ToString());
            string expected =
@"namespace codegencs
{
    public class Test1
    {
        // My Properties start here
    }
}
";

            Assert.AreEqual(_w.ToString(), expected);
        }

        [Test]
        public void TestCStyleBlock2()
        {
            string myNamespace = "codegencs";
            string myClass = "Test1";
            using (_w.WithCStyleBlock($"namespace {myNamespace}"))
            {
                using (_w.WithCStyleBlock($"public class {myClass}"))
                {
                    _w.WriteLine("// My Properties start here");
                }
            }
            System.Diagnostics.Debug.WriteLine(_w.ToString());
            string expected =
@"namespace codegencs
{
    public class Test1
    {
        // My Properties start here
    }
}
";
            Assert.AreEqual(_w.ToString(), expected);
        }

        [Test]
        public void TestInlineTemplate()
        {
            List<Property> props = new List<Property>() { new Property() { Name = "Name", Type = "string" }, new Property() { Name = "Age", Type = "int" } };
            string myNamespace = "codegencs";
            string myClass = "Test1";
            _w.Write($@"
                namespace {myNamespace}
                {{
                    public class {myClass}
                    {{
                        // My Properties start here
                        { RenderProperties(props) }
                    }}
                }}");
            System.Diagnostics.Debug.WriteLine(_w.ToString());
            string expected =
@"namespace codegencs
{
    public class Test1
    {
        // My Properties start here
        Properties.Count=2
        [0]. public string Name { get; set; }
        [1]. public int Age { get; set; }
    }
}";

            Assert.AreEqual(_w.ToString(), expected);
        }
        public class Property
        {
            public string Name;
            public string Type;
        }
        Func<FormattableString> RenderProperties(List<Property> props)
        {
            int i = 0;
            return () => $@"
                Properties.Count={props.Count}
                {string.Join(Environment.NewLine, props.Select(prop => $"[{i++}]. public {prop.Type} {prop.Name} {{ get; set; }}"))}"
            ;
        }


    }
}