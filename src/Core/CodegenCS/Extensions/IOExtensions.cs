using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CodegenCS.IO
{
    public static class Extensions
    {
        public class SaveFilesResult
        {
            public List<string> SavedFiles { get; internal set; }

            /// <summary>
            /// List of files that exist under outputFolder and that were NOT saved as part of this ICodegenContext.
            /// Only returned if getUnknownFiles is true
            /// </summary>
            public List<string> UnknownFiles { get; internal set; }
        }
        internal static void SaveToFile(string contents, string path, Encoding encoding)
        {
            FileInfo fi = new FileInfo(path);
            // If file exists with different case, delete to overwrite with the right name //TODO: case sensitive filesystem?
            if (fi.Exists && new DirectoryInfo(fi.Directory.FullName).GetFiles(fi.Name).Single().Name != fi.Name)
                fi.Delete();
            File.WriteAllText(fi.FullName, contents, encoding);
        }

        /// <summary>
        /// Writes in-memory contents to physical files. If the files already exist they will be overwritten. <br />
        /// </summary>
        /// <param name="context"></param>
        /// <param name="outputFolder">Base output folder (can be absolute or relative path, and will be combined with relative path of files)</param>
        /// <param name="encoding">If not specified will save as UTF-8</param>
        /// <param name="autoCreateFolders">Automatically create folders if they don't exist (default is true)</param>
        /// <param name="getUnknownFiles">If true (default is false) will return <see cref="SaveFilesResult.UnknownFiles"/> with a list
        /// of files that exist under outputFolder and that were NOT saved as part of this ICodegenContext. 
        /// Useful to cleanup unknown files (but beware that files deleted using File.Delete do NOT get moved to Recycle bin)</param>
        /// <exception cref="System.IO.DirectoryNotFoundException"></exception>
        /// <exception cref="Exception">If context has any error</exception>
        /// <returns>Number of saved files</returns>
        public static SaveFilesResult SaveToFolder(this ICodegenContext context, string outputFolder, Encoding encoding = null, bool autoCreateFolders = true, bool getUnknownFiles = false)
        {
            if (context.Errors.Any())
                throw new Exception(context.Errors.First());

            if (encoding == null)
                encoding = Encoding.UTF8;

            outputFolder = new DirectoryInfo(outputFolder).FullName;

            // First check
            foreach (var file in context.OutputFiles)
            {
                if (string.IsNullOrEmpty(file.RelativePath))
                {
                    if (file.Equals(context.DefaultOutputFile))
                        throw new Exception($"{nameof(context.DefaultOutputFile.RelativePath)} was not defined for {nameof(context.DefaultOutputFile)}");
                    else
                        throw new Exception($"{nameof(context.DefaultOutputFile.RelativePath)} was not defined for {nameof(ICodegenOutputFile)}");
                }
                string absolutePath = Path.Combine(outputFolder, file.RelativePath);
                FileInfo fi = new FileInfo(absolutePath);
                absolutePath = fi.FullName;

                string folder = fi.Directory.FullName;
                if (!Directory.Exists(folder))
                {
                    if (autoCreateFolders)
                        Directory.CreateDirectory(folder);
                    else
                        throw new System.IO.DirectoryNotFoundException($"Folder \"{folder}\" not found");
                }
            }

            List<string> savedFiles = new List<string>();
            List<string> unknownFiles = null;

            // Then save.
            foreach (var file in context.OutputFiles)
            {
                string absolutePath = Path.Combine(outputFolder, file.RelativePath);
                FileInfo fi = new FileInfo(absolutePath);
                absolutePath = fi.FullName;
                savedFiles.Add(absolutePath);
                SaveToFile(file.GetContents(), absolutePath, encoding);
            }

            if (getUnknownFiles)
            {
                unknownFiles = new List<string>();
                var files = new DirectoryInfo(outputFolder).GetFiles("*.*", SearchOption.AllDirectories);
                outputFolder = new DirectoryInfo(outputFolder).FullName;
                var generatedFiles = savedFiles.Select(p => p.ToLower()).ToList(); //TODO: case insensitive filesystem?
                if (generatedFiles.Any())
                {
                    foreach (var file in files)
                    {
                        if (!generatedFiles.Contains(file.FullName.ToLower()))
                            unknownFiles.Add(file.FullName);
                    }
                }
            }

            return new SaveFilesResult() { SavedFiles = savedFiles, UnknownFiles = unknownFiles };
        }

        /// <summary>
        /// Writes in-memory contents to a physical file. If the target file already exists, it is overwritten. <br />
        /// </summary>
        /// <param name="file"></param>
        /// <param name="outputFolder">Base output folder (can be absolute or relative path, and will be combined with relative path of file)</param>
        /// <param name="autoCreateFolder">Automatically create folder if it doesn't exist (default is true)</param>
        /// <param name="encoding">If not specified will save as UTF-8</param>
        /// <exception cref="System.IO.DirectoryNotFoundException"></exception>
        public static void SaveToFolder(this ICodegenOutputFile file, string outputFolder, Encoding encoding = null, bool autoCreateFolder = true)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;

            outputFolder = new DirectoryInfo(outputFolder).FullName;
            string absolutePath = Path.Combine(outputFolder, file.RelativePath);
            FileInfo fi = new FileInfo(absolutePath);
            absolutePath = fi.FullName;

            string folder = fi.Directory.FullName;
            if (!Directory.Exists(folder))
            {
                if (autoCreateFolder)
                    Directory.CreateDirectory(folder);
                else
                    throw new System.IO.DirectoryNotFoundException($"Folder \"{folder}\" not found");
            }
            SaveToFile(file.GetContents(), absolutePath, encoding);
        }

    }
}
