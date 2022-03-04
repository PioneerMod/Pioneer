using System.Reflection;

namespace Pioneer.Common.Commands.Converters;

internal static class EnumConverter
{
    internal static bool IsHandlerFor(ParameterInfo parameter)
    {
        return parameter.ParameterType.IsEnum;
    }

    internal static object Convert(string val, Type type)
    {
        return Enum.Parse(type, val);
    }
}