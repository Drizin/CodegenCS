using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Reflection;
using System.Text;

namespace CodegenCS.TemplateBuilder
{
    public static class CliCommand
    {
        public static Command GetCommand(string commandName = "build")
        {
            var command = new Command(commandName);

            command.AddArgument(new Argument<string[]>("Template", description: "Template(s) to build. E.g. \"MyTemplate.cs\"") { Arity = ArgumentArity.OneOrMore });

            command.AddOption(new Option<string>(new[] { "--Output", "-o" }, description:
                """
                Folder and/or filename to save output dll
                If folder is not provided then dll is saved in current folder
                If filename is not provided then dll is named like the input template file 
                   (e.g. MyTemplate.cs will be compiled into MyTemplate.dll)
                Examples: "..\Templates\MyCodeGenerator.dll" (specify a folder AND a file name)
                Examples: "..\Templates\"                    (specify only folder)
                Examples: "MyCodeGenerator.dll"              (specify only filename)
                """
            ) { Arity = ArgumentArity.ExactlyOne, ArgumentHelpName = "Output" });

            command.Handler = CommandHandler.Create<ParseResult, TemplateBuilder.RunCommandArgs>(new TemplateBuilder().HandleCommand);

            return command;
        }
   }
}
