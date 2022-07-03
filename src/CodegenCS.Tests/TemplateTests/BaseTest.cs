using CodegenCS.DbSchema;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace CodegenCS.Tests.TemplateTests;

internal class BaseTest
{
    protected static string GetCurrentFolder([CallerFilePath] string path = null) => Path.GetDirectoryName(path);

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

    protected string _currentFolder;
    protected string _outputFolder;

    protected BaseTest(string currentFolder)
    {
        _currentFolder = currentFolder;
        _outputFolder = currentFolder + "-TestsOutput";
    }

    protected void AssertContentIsEqualToFile(string generatedContent, string relativeFilePath)
    {

        string file = Path.Combine(_outputFolder, relativeFilePath);
        if (!File.Exists(file))
        {
            if (!new FileInfo(file).Directory.Exists)
                new FileInfo(file).Directory.Create();
            File.WriteAllText(file, generatedContent);
        }
        
        string fileContents = File.ReadAllText(file);
        Assert.AreEqual(fileContents, generatedContent);
    }

    protected void AssertContextIsEqualToFolder(ICodegenContext context, string relativeFolderPath = null, [CallerMemberName] string testName = "")
    {
        // If there's a single test in a given folder it can use relativeFolderPath = "." and output on the main folder. Else it will create a folder for each test.
        string folder = Path.Combine(_outputFolder, relativeFolderPath ?? testName);
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(_outputFolder);
            context.SaveFiles(folder);
        }

        var files = Directory.GetFiles(folder);
        Assert.AreEqual(files.Length, context.OutputFilesPaths.Count);
        foreach(var relativeFilePath in context.OutputFilesPaths)
        {
            string relativePath = Path.Combine(folder, relativeFilePath);
            string contents = context[relativeFilePath].GetContents();
            AssertContentIsEqualToFile(contents, relativePath);
        }
    }

}
