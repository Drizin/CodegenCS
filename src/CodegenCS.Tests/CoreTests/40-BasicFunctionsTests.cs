using CodegenCS;
using CodegenCS.Extensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public class BasicFunctionsTests
    {
        CodegenTextWriter _w = null;

        #region Setup
        [SetUp]
        public void Setup()
        {
            _w = new CodegenTextWriter();
        }
        #endregion

        #region Different types of callbacks that can be used inside of Indented-Blocks or even used outside as regular paramters of Write method
        [Test]
        public void TestCallback1()
        {
            Action<CodegenTextWriter> callback = (w) => w.WriteLine("Hello3");

            _w
                // Inside indented block we call anonymous Action
                .WithCurlyBraces("void MyMethod1()", () => 
                {
                    _w.WriteLine("Hello");
                })

                // Inside indented block we call anonymous Action<CodegenTextWriter>
                .WithCurlyBraces("void MyMethod2()", (w) =>
                {
                    w.WriteLine("Hello2"); 
                })

                // Inside indented block we call a named Action<CodegenTextWriter>
                .WithCurlyBraces("void MyMethod3()", callback)

                // After (outside) the indented block we call a named Action<CodegenTextWriter>
                .Write(callback)
                ;

            string expected = @"
void MyMethod1()
{
    Hello
}
void MyMethod2()
{
    Hello2
}
void MyMethod3()
{
    Hello3
}
Hello3
".TrimStart();
            Assert.AreEqual(expected, _w.GetContents());
        }
        #endregion

        #region TodoItem[] - recursive list of to-do items
        public class TodoItem
        {
            public string Description { get; private set; }
            public IList<TodoItem> SubTasks { get; private set; }

            public TodoItem(string description)
            {
                Description = description;
                SubTasks = new List<TodoItem>();
            }
        }

        TodoItem[] todoList =
        {
            new TodoItem("Get milk"),
            new TodoItem("Clean the house")
            {
                SubTasks =
                {
                    new TodoItem("Living room"),
                    new TodoItem("Bathrooms")
                    {
                        SubTasks =
                        {
                            new TodoItem("Guest bathroom"),
                            new TodoItem("Family bathroom")
                        }
                    },
                    new TodoItem("Bedroom")
                }
            },
            new TodoItem("Mow the lawn")
        };
        #endregion

        #region Using method which takes a CodegenTextWriter
        private static void WriteToDoItem(CodegenTextWriter writer, TodoItem item)
        {
            writer.WriteLine($"- {item.Description}");
        }

        [Test]
        public void TestMethod1()
        {
            foreach (var item in todoList)
                WriteToDoItem(_w, item);
            string expected = @"
- Get milk
- Clean the house
- Mow the lawn
".TrimStart(Environment.NewLine.ToCharArray());
            Assert.AreEqual(expected, _w.GetContents());
        }
        #endregion

        #region Recursive method
        private static void WriteToDoList(CodegenTextWriter writer, IEnumerable<TodoItem> todoItems)
        {
            foreach (var item in todoItems)
            {
                writer.WriteLine("- {0}", item.Description);

                if (item.SubTasks.Any())
                {
                    writer.IncreaseIndent();
                    WriteToDoList(writer, item.SubTasks);
                    writer.DecreaseIndent();
                }
            }
        }
        [Test]
        public void TestRecursiveFunction()
        {
            WriteToDoList(_w, todoList);
            string expected = @"- Get milk
- Clean the house
    - Living room
    - Bathrooms
        - Guest bathroom
        - Family bathroom
    - Bedroom
- Mow the lawn
";
            Assert.AreEqual(expected, _w.GetContents());
        }
        #endregion

        #region Testing Join() extension, which can be used to write multiple items (one by one, with separator which by default is NewLine), and while keeping "inline cursor position"
        [Test]
        public void TestJoin()
        {
            _w.Write($@"
                I have a LOT of things to do today:
                    {todoList.Select(item => $"- {item.Description}").Join()}");
            string expected = @"
I have a LOT of things to do today:
    - Get milk
    - Clean the house
    - Mow the lawn".TrimStart(Environment.NewLine.ToCharArray());

            Assert.AreEqual(expected, _w.GetContents());
        }
        #endregion


    }
}
