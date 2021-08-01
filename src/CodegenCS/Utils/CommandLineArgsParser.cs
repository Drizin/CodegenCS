using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CodegenCS.Utils
{
    /// <summary>
    /// Parse command-line arguments. Based on https://www.codeproject.com/Articles/3111/C-NET-Command-Line-Arguments-Parser
    /// </summary>
    [Obsolete("Please use System.CommandLine")]
    public class CommandLineArgsParser
    {
        public Dictionary<string, string> Parameters;

        /// <summary>
        /// Parse command-line arguments.
        /// </summary>
        /// <returns>Dictionary (case insensitive) of arguments. Orphaned arguments (those that don't have "=value") will return "true"</returns>
        public CommandLineArgsParser(string[] Args)
        {
            Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Regex Spliter = new Regex(@"^-{1,2}|^/|=|:",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            Regex Remover = new Regex(@"^['""]?(.*?)['""]?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            string Parameter = null;
            string[] Parts;

            // Valid parameters forms:
            // {-,/,--}param{ ,=,:}((",')value(",'))
            // Examples: 
            // -param1 value1 --param2 /param3:"Test-:-work" 
            //   /param4=happy -param5 '--=nice=--'
            foreach (string Txt in Args)
            {
                // Look for new parameters (-,/ or --) and a
                // possible enclosed value (=,:)
                Parts = Spliter.Split(Txt, 3);

                switch (Parts.Length)
                {
                    // Found a value (for the last parameter 
                    // found (space separator))
                    case 1:
                        if (Parameter != null)
                        {
                            if (!Parameters.ContainsKey(Parameter))
                            {
                                Parts[0] =
                                    Remover.Replace(Parts[0], "$1");

                                Parameters.Add(Parameter, Parts[0]);
                            }
                            Parameter = null;
                        }
                        // else Error: no parameter waiting for a value (skipped)
                        break;

                    // Found just a parameter
                    case 2:
                        // The last parameter is still waiting. 
                        // With no value, set it to true.
                        if (Parameter != null)
                        {
                            if (!Parameters.ContainsKey(Parameter))
                                Parameters.Add(Parameter, "true");
                        }
                        Parameter = Parts[1];
                        break;

                    // Parameter with enclosed value
                    case 3:
                        // The last parameter is still waiting. 
                        // With no value, set it to true.
                        if (Parameter != null)
                        {
                            if (!Parameters.ContainsKey(Parameter))
                                Parameters.Add(Parameter, "true");
                        }

                        Parameter = Parts[1];

                        // Remove possible enclosing characters (",')
                        if (!Parameters.ContainsKey(Parameter))
                        {
                            Parts[2] = Remover.Replace(Parts[2], "$1");
                            Parameters.Add(Parameter, Parts[2]);
                        }

                        Parameter = null;
                        break;
                }
            }
            // In case a parameter is still waiting
            if (Parameter != null)
            {
                if (!Parameters.ContainsKey(Parameter))
                    Parameters.Add(Parameter.ToLower(), "true");
            }
        }

        /// <summary>
        /// Retrieve a parameter value if it exists. Null if not found. "true" (lowecase) if "/parameter" was provided but without passing a value.
        /// </summary>
        public string this[string param]
        {
            get
            {
                if (!Parameters.ContainsKey(param))
                    return null;
                return Parameters[param];
            }
        }

        /// <summary>
        /// Determines whether the parameters provided the specified key
        /// </summary>
        public bool ContainsKey(string key) => Parameters.ContainsKey(key);

    }

}
