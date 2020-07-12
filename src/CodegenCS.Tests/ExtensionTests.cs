using CodegenCS;
using CodegenCS.Extensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public class ExtensionTests
    {
        CodegenTextWriter _w = null;

        [SetUp]
        public void Setup()
        {
            _w = new CodegenTextWriter();
        }

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


        /// <summary>
        /// Tests passing Func(FormattableString), and tests WriteLines() extension
        /// </summary>
        [Test]
        public void TestCallbackExtension()
        {

            Func<IEnumerable<TodoItem>, FormattableString> printList = null;

            printList = (items) => (FormattableString) $"{items.Select<TodoItem, Func<FormattableString>>(i => () => (FormattableString)$"- {i.Description}").WriteLines()}";

            _w.Write(() => printList(todoList));

            string expected = @"
- Get milk
- Clean the house
- Mow the lawn
";

            Assert.AreEqual(_w.ToString(), expected.TrimStart());
        }

        /// <summary>
        /// Tests passing Func(FormattableString) inside the same (recursive) Func(FormattableString) function, and tests LinesWrite() extension
        /// </summary>
        [Test]
        public void TestRecursiveCallback()
        {

            Func<IEnumerable<TodoItem>, FormattableString> printList2 = null;
            printList2 = (items) => (FormattableString)$"{items.Select<TodoItem, Func<FormattableString>>(i => () => (FormattableString)$"{i.Description}{printList2(i.SubTasks)}").LinesWrite()}";

            _w.Write(() => printList2(todoList));

            // TODO: add indentation extensions? maybe some control string (like "[BEGININDENT]") which would be intercepted by the TextWriter?
            string expected = @"
Get milk
Clean the house
Living room
Bathrooms
Guest bathroom
Family bathroom
Bedroom
Mow the lawn";

            Assert.AreEqual(_w.ToString(), expected);
        }





    }
}
