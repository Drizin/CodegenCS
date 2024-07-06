﻿using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace CodegenCS.Tools.Tests
{
    internal class TemplateBuildTests : BaseTest
    {
        private static string[] GetTemplates()
        {
            var files = Directory.GetFiles(TemplatesFolder);
            files = files.Where(f => !f.EndsWith(".g.cs") && !f.EndsWith(".generated.cs") && !f.EndsWith(".json")).ToArray();
            return files;
        }

        [Test, TestCaseSource("GetTemplates")]
        public async Task TestTemplate(string templateFile)
        {
#if NETFRAMEWORK
            // #r "System.Private.Xml.dll" is only for NET5+ .. and CLI tool is also NET5+ so maybe we should just remove net472 target for unit tests
            if (templateFile.EndsWith(@"\0091-EmbeddedReferences.cs"))
                return;
#endif
            _tmpFolder = Path.Combine(Path.GetTempPath() ?? Directory.GetCurrentDirectory(), Guid.NewGuid().ToString());
            new DirectoryInfo(_tmpFolder).Create();
            _tmpDll = Path.Combine(_tmpFolder, Guid.NewGuid().ToString() + ".dll");
            _templateFileName = Path.GetFileName(templateFile);
            await BuildAsync(templateFile);

            var exitCode = await LaunchAsync();
            Assert.AreEqual(0, exitCode);

            Assert.That(_context.OutputFilesPaths.Contains(_launcherArgs.DefaultOutputFile));

            if (_context.OutputFiles.Count == 1)
            {
                var filePath = Path.Combine(TemplatesFolder, _context.DefaultOutputFile.RelativePath);
                Assert_That_Content_IsEqual_To_File(_context.DefaultOutputFile, filePath);
            }
            else
            {
                var folder = Path.Combine(TemplatesFolder, Path.GetFileNameWithoutExtension(templateFile) + "-Output");
                Assert_That_ContextOutput_IsEqual_To_Folder(_context, folder);
            }
        }

    }
}
