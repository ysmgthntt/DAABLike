#if NET7_0_OR_GREATER
global using ANE = System.ArgumentNullException;
#else
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

internal static class ANE
{
    // ArgumentException

    public static void ThrowIfNullOrEmpty(/*[NotNull]*/ string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (string.IsNullOrEmpty(argument))
        {
            ThrowNullOrEmptyException(argument, paramName);
        }
    }

    //[DoesNotReturn]
    private static void ThrowNullOrEmptyException(string? argument, string? paramName)
    {
        ANE.ThrowIfNull(argument, paramName);
        throw new ArgumentException("The value cannot be an empty string.", paramName);
    }

    // ArgumentNullException

    public static void ThrowIfNull(/*[NotNull]*/ object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument is null)
        {
            Throw(paramName);
        }
    }

    //[DoesNotReturn]
    private static void Throw(string? paramName) =>
        throw new ArgumentNullException(paramName);
}

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
#endif
