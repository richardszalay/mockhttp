#if !NET5_0_OR_GREATER

namespace System.Diagnostics.CodeAnalysis;

[DebuggerNonUserCode]
[AttributeUsage(AttributeTargets.Parameter)]
sealed class NotNullWhenAttribute : Attribute
{
    public bool ReturnValue { get; }

    public NotNullWhenAttribute(bool returnValue) =>
        ReturnValue = returnValue;
}

#endif