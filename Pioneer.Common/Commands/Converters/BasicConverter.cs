using System;

namespace Pioneer.Common.Commands.Converters;

public static class BasicConverter
{
    public static readonly Type UShortType = typeof(ushort);
    public static readonly Type IntType = typeof(int);
    public static readonly Type LongType = typeof(long);
    public static readonly Type ShortType = typeof(short);
    public static readonly Type ByteType = typeof(byte);
    public static readonly Type CharType = typeof(char);
    public static readonly Type FloatType = typeof(float);
    public static readonly Type DoubleType = typeof(double);
    public static readonly Type BoolType = typeof(bool);
    public static readonly Type StringType = typeof(string);
    private static readonly Type DecimalType = typeof(decimal);
    private static readonly Type UIntType = typeof(uint);
    private static readonly Type ULongType = typeof(ulong);

    internal static object Convert(string givenObject, Type wantedType)
    {
        if (wantedType == UIntType)
        {
            return uint.Parse(givenObject);
        }

        if (wantedType == ULongType)
        {
            return ulong.Parse(givenObject);
        }

        if (wantedType == UShortType)
        {
            return ushort.Parse(givenObject);
        }

        if (wantedType == IntType)
        {
            return int.Parse(givenObject);
        }

        if (wantedType == LongType)
        {
            return long.Parse(givenObject);
        }

        if (wantedType == ShortType)
        {
            return short.Parse(givenObject);
        }

        if (wantedType == ByteType)
        {
            return byte.Parse(givenObject);
        }

        if (wantedType == CharType)
        {
            return char.Parse(givenObject);
        }

        if (wantedType == FloatType)
        {
            return float.Parse(givenObject);
        }

        if (wantedType == DoubleType)
        {
            return double.Parse(givenObject);
        }

        if (wantedType == DecimalType)
        {
            return decimal.Parse(givenObject);
        }

        if (wantedType == BoolType)
        {
            return bool.Parse(givenObject);
        }

        return givenObject;
    }
}