using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodegenCS
{
    public class CodegenContext : ICodegenContext
    {
        public string OutputFolder;
        Dictionary<string, CodegenOutputFile> _outputFiles = new Dictionary<string, CodegenOutputFile>(StringComparer.InvariantCultureIgnoreCase); // key insensitive
        public List<string> Errors { get; } = new List<string>();

        /// <summary>
        /// Output files, indexed by their relative paths
        /// </summary>
        public Dictionary<string, CodegenOutputFile> OutputFiles { get { return _outputFiles; } }

        /// <summary>
        /// Output files, indexed by their absolute paths
        /// </summary>
        public Dictionary<string, CodegenOutputFile> OutputFilesAbsolute { get { return _outputFiles.Values.ToDictionary(v => System.IO.Path.Combine(OutputFolder, v.RelativePath), v => v); } }


        public CodegenContext(string outputFolder)
        {
            this.OutputFolder = outputFolder;
        }

        public CodegenContext() : this(outputFolder: Environment.CurrentDirectory) // maybe Directory.GetParent(Environment.CurrentDirectory).Parent.FullName?
        {
        }

        public void SaveFiles()
        {
            if (this.Errors.Any())
                throw new Exception(this.Errors.First());
            foreach(var f in this._outputFiles)
            {
                string absolutePath = System.IO.Path.Combine(this.OutputFolder, f.Value.RelativePath);
                System.IO.Directory.CreateDirectory(new FileInfo(absolutePath).Directory.FullName);
                System.IO.File.WriteAllText(absolutePath, f.Value.GetContents());
            }
        }

        public CodegenTextWriter GetTextWriter(string relativePath)
        {
            if (!this._outputFiles.ContainsKey(relativePath))
                this._outputFiles[relativePath] = new CodegenOutputFile(relativePath);            return this._outputFiles[relativePath].Writer;
        }
    }

    public enum OutputFileType
    {
        None,
        Compile,
        Content,
        EmbeddedResource,

        /// <summary>
        /// File is generated but is not added to the project
        /// </summary>
        NonProjectItem
    }


    public class CodegenOutputFile
    {
        public CodegenTextWriter Writer = new CodegenTextWriter();
        public OutputFileType ItemType = OutputFileType.Compile;
        public string RelativePath; // under BaseFolder

        public CodegenOutputFile(string relativePath)
        {
            this.RelativePath = relativePath;
        }

        public string GetContents()
        {
            return this.Writer.ToString();
        }
    }

}
