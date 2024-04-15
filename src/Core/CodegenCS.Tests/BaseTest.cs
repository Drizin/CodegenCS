using CodegenCS.IO;
using CodegenCS.Models.DbSchema;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace CodegenCS.Tests;

internal class BaseTest
{
    protected static string GetCurrentFolder([CallerFilePath] string path = null) => Path.GetDirectoryName(path);

    #region Sample Inputs
    #region DatabaseSchema
    public DatabaseSchema MyDbSchema { get; } =
        new DatabaseSchema()
        {
            Tables = new List<Table>()
            {
                new Table()
                {
                    TableName = "Users",
                    Columns = new List<Column>()
                    {
                        new Column() {  ColumnName = "UserId", ClrType = "int", SqlDataType = "int" },
                        new Column() {  ColumnName = "FirstName", ClrType = "string", SqlDataType = "nvarchar" },
                        new Column() {  ColumnName = "LastName", ClrType = "string", SqlDataType = "nvarchar" },
                    }
                },
                new Table()
                {
                    TableName = "Products",
                    Columns = new List<Column>()
                    {
                        new Column() {  ColumnName = "Description", ClrType = "int", SqlDataType = "int" },
                        new Column() {  ColumnName = "ProductId", ClrType = "string", SqlDataType = "nvarchar" }
                    }
                }
            }
        };
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

    protected TodoItem[] todoList =
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
    protected TodoItem[] emptyTodoList = new TodoItem[0];
    #endregion

    #endregion

    #region For tests that have their own output folder to compare results.
    #region Members
    //protected string _currentFolder;
    //protected string _outputFolder;
    #endregion

    #region ctor
    /// <summary>
    /// Use this for tests that have their own output folder to compare results.
    /// </summary>
    /// <param name="currentFolder"></param>
    //protected BaseTest(/*string currentFolder*/)
    //{
    //    //_currentFolder = currentFolder;
    //    //_outputFolder = currentFolder + "-TestsOutput";
    //}
    #endregion

    private string GetTestOutputFolder(string testClassPath, string testName) => Path.Combine(new FileInfo(testClassPath).Directory.FullName + "-TestsOutput", testName);

    #region Asserts
    protected void Assert_That_Content_IsEqual_To_File(ICodegenTextWriter writer, string fileName, [CallerMemberName] string testName = "", [CallerFilePath] string testClassPath = "")
    {
        string outputFolder = GetTestOutputFolder(testClassPath, testName);
        string file = Path.Combine(outputFolder, fileName);
        if (!File.Exists(file))
        {
            if (!new FileInfo(file).Directory.Exists)
                new FileInfo(file).Directory.Create();
            File.WriteAllText(file, writer.GetContents());
        }

        string fileContents = File.ReadAllText(file);
        Assert.AreEqual(fileContents, writer.GetContents());
    }

    /// <summary>
    /// Compares the Context Outputs with the files in the folder specific for the test outputs (each test have it's own folder)
    /// </summary>
    /// <param name="context"></param>
    /// <param name="testName">Do NOT provide any value here. It will get the test name</param>
    protected void Assert_That_ContextOutput_IsEqual_To_TestOutputFolder(ICodegenContext context, [CallerMemberName] string testName = "", [CallerFilePath] string testClassPath = "")
    {
        string outputFolder = GetTestOutputFolder(testClassPath, testName);
        Assert_That_ContextOutput_IsEqual_To_Folder(context, outputFolder);
    }

    private void Assert_That_ContextOutput_IsEqual_To_Folder(ICodegenContext context, string folder)
    {
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
            context.SaveToFolder(folder);
        }

        var files = Directory.GetFiles(folder);
        Assert.AreEqual(files.Length, context.OutputFilesPaths.Count);
        foreach (var relativeFilePath in context.OutputFilesPaths)
        {
            string relativePath = Path.Combine(folder, relativeFilePath);
            ICodegenTextWriter writer = context[relativeFilePath];
            Assert_That_Content_IsEqual_To_File(writer, relativePath);
        }
    }
    #endregion


    #endregion

}
