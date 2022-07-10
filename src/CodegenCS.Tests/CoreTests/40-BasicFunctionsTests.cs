using CodegenCS;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using static CodegenCS.Symbols;

namespace CodegenCS.Tests.CoreTests
{
    internal class BasicFunctionsTests : BaseTest
    {
        CodegenTextWriter _w = null;

        #region Setup
        [SetUp]
        public void Setup()
        {
            _w = new CodegenTextWriter();
        }
        #endregion

        #region Different types of callbacks that can be used inside of Indented-Blocks or even used outside as regular parameters of Write method
        [Test]
        public void TestCallback1()
        {
            Action<ICodegenTextWriter> callback = (w) => w.WriteLine("Hello3");

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

        #region Explicitly invoking a method that takes ICodegenTextWriter
        private static void WriteToDoItem(ICodegenTextWriter writer, TodoItem item)
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

        #region Recursive FormattableString with IIF
        private FormattableString WriteToDoList(TodoItem item)
            => $$"""
                - {{item.Description}}{{IF(item.SubTasks.Any())}}
                    {{item.SubTasks.Select(item => WriteToDoList(item)).Render()}}{{ENDIF}}
                """;

        [Test]
        public void TestRecursiveFunction2()
        {
            _w.DefaultIEnumerableRenderOptions = RenderEnumerableOptions.LineBreaksWithoutSpacer; // since this is recursive there's no point in adding spacers .. just use regular linebreaks
            _w.WriteLine(todoList.Select(item => WriteToDoList(item)));
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

        #region Testing IEnumerable extension, which can be used to write multiple items (one by one, with default or custom behavior for separating items), and while keeping "inline cursor position"
        [Test]
        public void TestIEnumerable()
        {
            _w.Write($@"
                I have a LOT of things to do today:
                    {todoList.Select(item => $"- {item.Description}").Render(new RenderEnumerableOptions() { BetweenItemsBehavior = ItemsSeparatorBehavior.WriteLineBreak })}");
            string expected = @"
I have a LOT of things to do today:
    - Get milk
    - Clean the house
    - Mow the lawn".TrimStart(Environment.NewLine.ToCharArray());

            Assert.AreEqual(expected, _w.GetContents());
        }

        [Test]
        public void TestIEnumerable2()
        {
            _w.Write($@"I have a LOT of things to do today: {todoList.Select(item => $"{item.Description}").Render(new RenderEnumerableOptions() { BetweenItemsBehavior = ItemsSeparatorBehavior.WriteCustomSeparator })}");
            string expected = "I have a LOT of things to do today: Get milk, Clean the house, Mow the lawn";

            Assert.AreEqual(expected, _w.GetContents());
        }
        #endregion


    }
}
