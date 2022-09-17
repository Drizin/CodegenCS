using CodegenCS.___InternalInterfaces___;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodegenCS
{

    #region InlineAction<...>
    /// <summary>
    /// This is just a wrapper to pass Action delegates with specific arguments
    /// </summary>
    public class InlineAction<T>
    {
        public Action<T> Action { get; set; }
        public T Arg1 { get; set; }
    }

    /// <summary>
    /// This is just a wrapper to pass Action delegates with specific arguments
    /// </summary>
    public class InlineAction<T1, T2>
    {
        public Action<T1, T2> Action { get; set; }
        public T1 Arg1 { get; set; }
        public T2 Arg2 { get; set; }
    }

    /// <summary>
    /// This is just a wrapper to pass Action delegates with specific arguments
    /// </summary>
    public class InlineAction<T1, T2, T3>
    {
        public Action<T1, T2, T3> Action { get; set; }
        public T1 Arg1 { get; set; }
        public T2 Arg2 { get; set; }
        public T3 Arg3 { get; set; }
    }

    /// <summary>
    /// This is just a wrapper to pass Action delegates with specific arguments
    /// </summary>
    public class InlineAction<T1, T2, T3, T4>
    {
        public Action<T1, T2, T3, T4> Action { get; set; }
        public T1 Arg1 { get; set; }
        public T2 Arg2 { get; set; }
        public T3 Arg3 { get; set; }
        public T4 Arg4 { get; set; }
    }
    #endregion

    #region InlineFunc<.., FormattableString>
    /// <summary>
    /// This is just a wrapper to pass Func delegates with specific arguments
    /// </summary>
    public class InlineFunc<T, TRet>
    {
        public Func<T, TRet> Func { get; set; }
        public T Arg1 { get; set; }
    }

    /// <summary>
    /// This is just a wrapper to pass Func delegates with specific arguments
    /// </summary>
    public class InlineFunc<T1, T2, TRet>
    {
        public Func<T1, T2, TRet> Func { get; set; }
        public T1 Arg1 { get; set; }
        public T2 Arg2 { get; set; }
    }

    /// <summary>
    /// This is just a wrapper to pass Func delegates with specific arguments
    /// </summary>
    public class InlineFunc<T1, T2, T3, TRet>
    {
        public Func<T1, T2, T3, TRet> Func { get; set; }
        public T1 Arg1 { get; set; }
        public T2 Arg2 { get; set; }
        public T3 Arg3 { get; set; }
    }

    /// <summary>
    /// This is just a wrapper to pass Func delegates with specific arguments
    /// </summary>
    public class InlineFunc<T1, T2, T3, T4, TRet>
    {
        public Func<T1, T2, T3, T4, TRet> Func { get; set; }
        public T1 Arg1 { get; set; }
        public T2 Arg2 { get; set; }
        public T3 Arg3 { get; set; }
        public T4 Arg4 { get; set; }
    }
    #endregion


    #region DelegateExtensions
    /// <summary>
    /// One of the major features of ICodegenTextWriter is that it can render a large number of object types that you can just interpolate. <br />
    /// Action and Func delegates can be embedded directly and they will be automatically rendered by ICodegenTextWriter, but the extensions below 
    /// allow users to specify custom arguments to be passed to the delegates.
    /// </summary>
    public static class DelegateExtensions
    {
        public static InlineAction<T> WithArguments<T>(this Action<T> action, T arg1)
            => new InlineAction<T>() { Action = action, Arg1 = arg1 };
        
        public static InlineAction<T1, T2> WithArguments<T1, T2>(this Action<T1, T2> action, T1 arg1, T2 arg2)
            => new InlineAction<T1, T2>() { Action = action, Arg1 = arg1, Arg2 = arg2 };

        public static InlineAction<T1, T2, T3> WithArguments<T1, T2, T3>(this Action<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
            => new InlineAction<T1, T2, T3>() { Action = action, Arg1 = arg1, Arg2 = arg2, Arg3 = arg3 };

        public static InlineAction<T1, T2, T3, T4> WithArguments<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            => new InlineAction<T1, T2, T3, T4>() { Action = action, Arg1 = arg1, Arg2 = arg2, Arg3 = arg3, Arg4 = arg4 };

        public static InlineFunc<T, TRet> WithArguments<T, TRet>(this Func<T, TRet> func, T arg1)
            => new InlineFunc<T, TRet>() { Func = func, Arg1 = arg1 };

        public static InlineFunc<T1, T2, TRet> WithArguments<T1, T2, TRet>(this Func<T1, T2, TRet> func, T1 arg1, T2 arg2)
            => new InlineFunc<T1, T2, TRet>() { Func = func, Arg1 = arg1, Arg2 = arg2 };

        public static InlineFunc<T1, T2, T3, TRet> WithArguments<T1, T2, T3, TRet>(this Func<T1, T2, T3, TRet> func, T1 arg1, T2 arg2, T3 arg3)
            => new InlineFunc<T1, T2, T3, TRet>() { Func = func, Arg1 = arg1, Arg2 = arg2, Arg3 = arg3 };

        public static InlineFunc<T1, T2, T3, T4, TRet> WithArguments<T1, T2, T3, T4, TRet>(this Func<T1, T2, T3, T4, TRet> func, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
            => new InlineFunc<T1, T2, T3, T4, TRet>() { Func = func, Arg1 = arg1, Arg2 = arg2, Arg3 = arg3, Arg4 = arg4 };

    }
    #endregion
}
