using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CodegenCS
{
    /// <summary>
    /// RawString is just a wrapper around string (with implicit conversion to/from string) which allows us to prioritize methods which use IFormattable (interpolated strings) instead of strings <br />
    /// If you use interpolated strings (which allow to use a wide range of action delegates) you'll end up using the methods overloads which accept IFormattable. <br />
    /// If you just pass a regular string it will be converted to RawString. 
    /// (In other words, if we had overloads taking plain strings then all interpolated strings would be converted to strings and we would break all the magic)
    /// Based on https://www.damirscorner.com/blog/posts/20180921-FormattableStringAsMethodParameter.html
    /// </summary>
    [DebuggerDisplay("{Value,nq}")]
    [Serializable]
    public class RawString
    {
        private string Value { get; }

        private RawString(string str)
        {
            Value = str;
        }

        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator RawString(string str) => new RawString(str);

        /// <summary>
        /// If caller passes an interpolated string to a method with overloads for FormattableString and RawString
        /// the compiler wouldn't know which overload to invoke (ambiguous call).
        /// By having an implicit conversion from T1 to T2 and NOT the opposite (from T2 to T1)
        /// the compiler will choose T1 as conversion target.
        /// In other words with this implicit conversion from FormattableString to RawString we ensure
        /// that FormattableString overloads are always preferred, and RawString (as a plain string wrapper)
        /// will only be used when the object cannot be converted to FormattableString.
        /// https://stackoverflow.com/a/60807577/3606250
        /// </summary>
        public static implicit operator RawString(FormattableString formattable) => new RawString(formattable.ToString());

        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator string(RawString raw) => raw.Value;

        public override string ToString()
        {
            return this.Value;
        }
    }
}
