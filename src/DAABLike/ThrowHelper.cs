#if !NET7_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

internal static class ANE
{
    // ArgumentException

    extension(ArgumentException)
    {
        public static void ThrowIfNullOrEmpty([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
#if !NETFRAMEWORK
            if (string.IsNullOrEmpty(argument))
#else
            if (argument is null || argument.Length == 0)
#endif
            {
                ThrowNullOrEmptyException(argument, paramName);
            }
        }
    }

    [DoesNotReturn]
    private static void ThrowNullOrEmptyException(string? argument, string? paramName)
    {
        ArgumentNullException.ThrowIfNull(argument, paramName);
        throw new ArgumentException("The value cannot be an empty string.", paramName);
    }

    // ArgumentNullException

    extension(ArgumentNullException)
    {
        public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (argument is null)
            {
                Throw(paramName);
            }
        }
    }

    [DoesNotReturn]
    private static void Throw(string? paramName) =>
        throw new ArgumentNullException(paramName);
}

#pragma warning disable IDE0130

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class CallerArgumentExpressionAttribute : Attribute
    {
        public CallerArgumentExpressionAttribute(string parameterName)
        {
            ParameterName = parameterName;
        }

        public string ParameterName { get; }
    }
}
#if NETFRAMEWORK
namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false)]
    internal sealed class NotNullAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    internal sealed class DoesNotReturnAttribute : Attribute { }
}
#endif
#endif
