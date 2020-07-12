using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace CodegenCS.Extensions
{
    /// <summary>
    /// FormattableStrings-based templates can contain {arguments} which are lazy-executed (Func[strings] or Func[FormattableStrings]). <br />
    /// Extensions can convert arguments to other lazy-evaluatable types (Func[strings] Func[FormattableStrings]) to add behavior. <br />
    /// Example: given a IEnumerable[strings] we can render one by one with line breaks at the end.
    /// </summary>
    public static class CodegenTextWriterExtensions
    {
        /// <summary>
        /// Sample extension: given an IEnumerable[Func[FormattableString]], this will execute each one (lazy execution) and will add a line break after each item.
        /// </summary>
        public static Func<FormattableString> WriteLines(this IEnumerable<Func<FormattableString>> items)
        {
            if (items.Any())
            {
                var teste = items.ToList()[0].Invoke();
            }
            return new Func<FormattableString>(() =>
            {
                int i = 0;
                StringBuilder format = new StringBuilder();
                List<object> parms = new List<object>();
                foreach (var fn in items)
                {
                    format.Append("{" + i.ToString() + "}");
                    format.Append(Environment.NewLine);
                    parms.Add(fn);
                    i++;
                }
                return FormattableStringFactory.Create(format.ToString(), parms.ToArray());
            });
        }

        /// <summary>
        /// Sample extension: given an IEnumerable[Func[FormattableString]], this will execute each one (lazy execution) and will add a line break before each item.
        /// </summary>
        public static Func<FormattableString> LinesWrite(this IEnumerable<Func<FormattableString>> items)
        {
            if (items.Any())
            {
                var teste = items.ToList()[0].Invoke();
            }
            return new Func<FormattableString>(() =>
            {
                int i = 0;
                StringBuilder format = new StringBuilder();
                List<object> parms = new List<object>();
                foreach (var fn in items)
                {
                    format.Append(Environment.NewLine);
                    format.Append("{" + i.ToString() + "}");
                    parms.Add(fn);
                    i++;
                }
                return FormattableStringFactory.Create(format.ToString(), parms.ToArray());
            });
        }

    }
}
