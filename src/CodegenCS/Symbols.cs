using CodegenCS.ControlFlow;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodegenCS
{
    public class Symbols
    {
        /// <summary>
        /// Starts a conditional block. If condition is false, everything written to TextWriter until the matching ELSE or ENDIF will be discarded.
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static IfSymbol IF(bool condition)
        {
            return new IfSymbol(condition);
        }

        /// <summary>
        /// Starts the ELSE part of a conditional block. If the IF condition is true then everything written to TextWriter from the ELSE until the matching ENDIF will be discarded.
        /// </summary>
        public static ElseSymbol ELSE => new ElseSymbol();
        
        /// <summary>
        /// Finishes a conditional block. Everything written to TextWritter after this will get back to normal (won't be discarded, even if the IF condition was false).
        /// </summary>
        public static EndIfSymbol ENDIF => new EndIfSymbol();

        /// <summary>
        /// Immediate IF: Returns one of two objects, depending on the evaluation of an expression.
        /// </summary>
        public static FormattableString IIF(bool condition, FormattableString truePart, FormattableString falsePart = null)
        {
            if (condition)
                return truePart;
            return falsePart;
        }

        //TODO: Add something like $"{TWS}" (trim whitespace) that would behave like Jinja templates:
        //https://stackoverflow.com/questions/45719062/jinja-docx-template-avoiding-new-line-in-nested-for
        //https://ttl255.com/jinja2-tutorial-part-3-whitespace-control/
    }
}
