using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodegenCS
{
    /// <summary>
    /// BaseCodegenContext keeps track of multiple files which can be saved at once in the output folder.
    /// </summary>
    /// <typeparam name="O">Class of OutputFiles. Should inherit from CodegenOutputFile</typeparam>
    public abstract class BaseCodegenContext<O>
        where O : CodegenOutputFile
    {
        #region Members
        /// <summary>
        /// Output files indexed by their relative paths
        /// </summary>
        protected Dictionary<string, O> _outputFiles = new Dictionary<string, O>(StringComparer.InvariantCultureIgnoreCase); // key insensitive

        /// <summary>
        /// If your template finds any error you can just append the errors here in this list <br />
        /// SaveFiles() does not work if there is any error.
        /// </summary>
        public List<string> Errors { get; } = new List<string>();

        /// <summary>
        /// Output files
        /// </summary>
        public List<O> OutputFiles { get { return _outputFiles.Values.ToList(); } }

        /// <summary>
        /// Output files, indexed by their relative paths
        /// </summary>
        public Dictionary<string, O> OutputFilesRelative { get {return _outputFiles; } }

        /// <summary>
        /// Output files, indexed by their absolute paths
        /// </summary>
        public Dictionary<string, O> OutputFilesAbsolute(string outputFolder) 
        {
            outputFolder = new DirectoryInfo(outputFolder).FullName;
            return _outputFiles.Values.ToDictionary(v => Path.Combine(outputFolder, v.RelativePath), v => v); 
        }

        #endregion

        #region ctors

        /// <summary>
        /// Creates new in-memory context.
        /// </summary>
        protected BaseCodegenContext()
        {
        }

        #endregion

        #region I/O
        /// <summary>
        /// Saves all files in the outputFolder. <br />
        /// According to the RelativePath of each file they may be saved in different folders
        /// </summary>
        /// <param name="outputFolder"></param>
        public void SaveFiles(string outputFolder)
        {
            if (this.Errors.Any())
                throw new Exception(this.Errors.First());
            
            outputFolder = new DirectoryInfo(outputFolder).FullName;
            foreach (var f in this._outputFiles)
            {
                string absolutePath = Path.Combine(outputFolder, f.Value.RelativePath);
                f.Value.SaveToFile(absolutePath);
            }
        }
        /// <summary>
        /// Saves all files in the current directory. <br />
        /// According to the RelativePath of each file they may be saved in different folders
        /// </summary>
        public void SaveFiles()
        {
            SaveFiles(Environment.CurrentDirectory);
        }

        /// <summary>
        /// After calling SaveFiles() you may decide to clean-up the Outputfolder (assuming it only has code-generation output). <br />
        /// This returns all files which are in the Outputfolder (and subfolders) and which were NOT generated as part of this Context. <br />
        /// Beware that files which are deleted using File.Delete do NOT get moved to Recycle bin.
        /// </summary>
        /// <returns></returns>
        public List<string> GetUnknownFiles(string outputFolder)
        {
            var files = new DirectoryInfo(outputFolder).GetFiles("*.*", SearchOption.AllDirectories);
            outputFolder = new DirectoryInfo(outputFolder).FullName;
            var generatedFiles = OutputFilesAbsolute(outputFolder).Keys.Select(p => p.ToLower()).ToList();
            List<string> unknownFiles = new List<string>();

            if (!generatedFiles.Any())
                return unknownFiles;

            foreach (var file in files)
            {
                if (!generatedFiles.Contains(file.FullName.ToLower()))
                    unknownFiles.Add(file.FullName);
            }
            return unknownFiles;
        }
        #endregion

    }

    /// <summary>
    /// CodegenContext keeps track of multiple files which can be saved at once in the output folder.
    /// </summary>
    public class CodegenContext : BaseCodegenContext<CodegenOutputFile>, ICodegenContext
    {
        #region ctors
        /// <inheritdocs />
        public CodegenContext()
        {
        }
        #endregion

        #region Indexer this[relativeFilePath]
        /// <summary>
        /// Output files are indexed by their relative path. <br />
        /// If context doesn't have an OutputFile with this relative path, a new one will automatically be created
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        public CodegenOutputFile this[string relativePath]
        {
            get
            {
                if (!this._outputFiles.ContainsKey(relativePath))
                {
                    this._outputFiles[relativePath] = new CodegenOutputFile(relativePath);
                }
                return this._outputFiles[relativePath];
            }
            set
            {
                this._outputFiles[relativePath] = value;
            }
        }
        #endregion

    }

    /// <summary>
    /// CodegenContext keeps track of multiple files which can be saved at once in the output folder, <br />
    /// while also tracking the type for each output file
    /// </summary>
    /// <typeparam name="FT">Enum which defines the Types that each file can have</typeparam>
    public class CodegenContext<FT> : BaseCodegenContext<CodegenOutputFile<FT>>, ICodegenContext<FT>
        where FT : struct, IComparable, IConvertible, IFormattable // FT should be enum. 
    {
        #region Members
        /// <summary>
        /// Default Type for new OutputFiles
        /// </summary>
        protected FT _defaultType { get; set; }
        #endregion

        #region ctors
        /// <summary>
        /// Creates new in-memory context.
        /// </summary>
        /// <param name="defaultType">Default Type for files</param>
        public CodegenContext(FT defaultType)
        {
            _defaultType = defaultType;
        }
        #endregion

        #region Indexer this[relativeFilePath]
        /// <summary>
        /// Output files are indexed by their relative path. <br />
        /// If context doesn't have an OutputFile with this relative path, a new one will automatically be created
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        public CodegenOutputFile<FT> this[string relativePath]
        {
            get
            {
                return this[relativePath, _defaultType];
            }
            set
            {
                this._outputFiles[relativePath] = value;
            }
        }

        /// <summary>
        /// Output files are indexed by their relative path. <br />
        /// If context doesn't have an OutputFile with this relative path, a new one will automatically be created
        /// </summary>
        /// <param name="relativePath"></param>
        /// <param name="fileType"></param>
        /// <returns></returns>
        public CodegenOutputFile<FT> this[string relativePath, FT fileType]
        {
            get
            {
                if (!this._outputFiles.ContainsKey(relativePath))
                {
                    this._outputFiles[relativePath] = new CodegenOutputFile<FT>(relativePath, fileType);
                    this._outputFiles[relativePath].FileType = fileType;
                }
                return this._outputFiles[relativePath];
            }
            set
            {
                this._outputFiles[relativePath] = value;
            }
        }
        #endregion

    }

}
