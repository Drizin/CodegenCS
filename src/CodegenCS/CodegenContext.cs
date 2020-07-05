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
            this.OutputFolder = new System.IO.DirectoryInfo(outputFolder).FullName;
        }

        public CodegenContext() : this(outputFolder: Environment.CurrentDirectory) // maybe Directory.GetParent(Environment.CurrentDirectory).Parent.FullName?
        {
        }

        public void SaveFiles(bool deleteOtherFiles = false)
        {
            if (this.Errors.Any())
                throw new Exception(this.Errors.First());
            foreach(var f in this._outputFiles)
            {
                string absolutePath = System.IO.Path.Combine(this.OutputFolder, f.Value.RelativePath);
                System.IO.Directory.CreateDirectory(new FileInfo(absolutePath).Directory.FullName);
                FileInfo fi = new FileInfo(absolutePath);
                if (fi.Exists && new DirectoryInfo(fi.Directory.FullName).GetFiles(fi.Name).Single().Name != fi.Name)
                    fi.Delete();
                //if (File.Exists(absolutePath) && new FileInfo(absolutePath).Name != )
                System.IO.File.WriteAllText(absolutePath, f.Value.GetContents());
            }
            if (deleteOtherFiles)
            {
                var files = new DirectoryInfo(this.OutputFolder).GetFiles("*.*", SearchOption.AllDirectories);
                var generatedFiles = this.OutputFilesAbsolute.Keys.Select(p => p.ToLower()).ToList();
                foreach(var file in files)
                {
                    if (!generatedFiles.Contains(file.FullName.ToLower()))
                    {
                        File.Delete(file.FullName); // TODO: delete with Recycle bin? https://stackoverflow.com/questions/3282418/send-a-file-to-the-recycle-bin
                    }
                }

            }
        }

        public CodegenOutputFile GetOutputFile(string relativePath, OutputFileType fileType = OutputFileType.NonProjectItem)
        {
            if (!this._outputFiles.ContainsKey(relativePath))
            {
                this._outputFiles[relativePath] = new CodegenOutputFile(relativePath);
                this._outputFiles[relativePath].ItemType = fileType;
            }
            return this._outputFiles[relativePath];
        }
        public CodegenTextWriter GetTextWriter(string relativePath)
        {
            return GetOutputFile(relativePath).Writer;
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
