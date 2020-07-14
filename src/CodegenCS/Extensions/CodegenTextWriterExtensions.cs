using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace CodegenCS.Extensions
{
    /// <summary>
    /// One of the major features of CodegenTextWriter is that it allows to write FormattableStrings (interpolated strings)  <br />
    /// which can contain {arguments} which are lazy-executed. <br />
    /// With extensions we can convert some arguments to add behavior or make shorter syntax. <br />
    /// By returning Func{FormattableString} we ensure that the arguments are still lazy-executed.
    /// Example: given a IEnumerable[strings] we can render one by one with line breaks at the end.
    /// </summary>
    public static class CodegenTextWriterExtensions
    {
        #region Join(IEnumerable<T>) - Concatenates all the elements of the list using the specified separator (defaults to NewLine) between elements.
        
        #region IEnumerable<Func<FormattableString>>
        /// <summary>
        /// Concatenates all the elements of the list using the specified separator (defaults to NewLine) between elements. <br />
        /// Like string.Join, but will return Func[FormattableString] so still keeps deferred execution.
        /// </summary>
        public static Func<FormattableString> Join(this IEnumerable<Func<FormattableString>> items, string separator = null)
        {
            return new Func<FormattableString>(() =>
            {
                StringBuilder format = new StringBuilder();
                List<object> parms = new List<object>();
                for (int i = 0; i < items.Count(); i++)
                {
                    Func<FormattableString> fn = items.ElementAt(i);
                    format.Append("{" + i.ToString() + "}");
                    if (i < items.Count() - 1)
                        format.Append(separator ?? Environment.NewLine);
                    parms.Add(fn);
                }
                return FormattableStringFactory.Create(format.ToString(), parms.ToArray());
            });
        }
        #endregion

        #region IEnumerable<FormattableString>
        /// <summary>
        /// Concatenates all the elements of the list using the specified separator (defaults to NewLine) between elements. <br />
        /// Like string.Join, but will return Func[FormattableString] so still keeps deferred execution.
        /// </summary>
        public static Func<FormattableString> Join(this IEnumerable<FormattableString> items, string separator = null)
        {
            return new Func<FormattableString>(() =>
            {
                StringBuilder format = new StringBuilder();
                List<object> parms = new List<object>();
                for (int i = 0; i < items.Count(); i++)
                {
                    FormattableString formattable = items.ElementAt(i);
                    format.Append("{" + i.ToString() + "}");
                    if (i < items.Count() - 1)
                        format.Append(separator ?? Environment.NewLine);
                    parms.Add(formattable);
                }
                return FormattableStringFactory.Create(format.ToString(), parms.ToArray());
            });
        }
        #endregion

        #region IEnumerable<Func<string>>
        /// <summary>
        /// Concatenates all the elements of the list using the specified separator (defaults to NewLine) between elements. <br />
        /// Like string.Join, but will return Func[FormattableString] so still keeps deferred execution.
        /// </summary>
        public static Func<FormattableString> Join(this IEnumerable<Func<string>> items, string separator = null)
        {
            return new Func<FormattableString>(() =>
            {
                StringBuilder format = new StringBuilder();
                List<object> parms = new List<object>();
                for (int i = 0; i < items.Count(); i++)
                {
                    Func<string> fn = items.ElementAt(i);
                    format.Append("{" + i.ToString() + "}");
                    if (i < items.Count() - 1)
                        format.Append(separator ?? Environment.NewLine);
                    parms.Add(fn);
                }
                return FormattableStringFactory.Create(format.ToString(), parms.ToArray());
            });
        }
        #endregion

        #region IEnumerable<string>
        /// <summary>
        /// Concatenates all the elements of the list using the specified separator (defaults to NewLine) between elements. <br />
        /// Like string.Join, but will return Func[FormattableString] so still keeps deferred execution.
        /// </summary>
        public static Func<FormattableString> Join(this IEnumerable<string> items, string separator = null)
        {
            return new Func<FormattableString>(() =>
            {
                StringBuilder format = new StringBuilder();
                List<object> parms = new List<object>();
                for (int i = 0; i < items.Count(); i++)
                {
                    string fn = items.ElementAt(i);
                    format.Append("{" + i.ToString() + "}");
                    if (i < items.Count() - 1)
                        format.Append(separator ?? Environment.NewLine);
                    parms.Add(fn);
                }
                return FormattableStringFactory.Create(format.ToString(), parms.ToArray());
            });
        }
        #endregion

        #endregion

    }
}
