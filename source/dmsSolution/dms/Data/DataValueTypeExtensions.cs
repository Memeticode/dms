namespace dms.Data;

public static class DataValueTypeExtensions
{
    public class DataValueTypeException : Exception
    {
        public DataValueTypeException() : base() { }
        public DataValueTypeException(string message) : base(message) { }
        public DataValueTypeException(string message, Exception innerException) : base(message, innerException) { }
    }

    // Methods for parsing system types to plausible data value type enum values
    // Basically... these are dms data types the object can be cast to without losing any data

    // object
    public static HashSet<DataValueType> GetPossibleTypes(this object? obj)
    {
        if (obj == null)
            return new HashSet<DataValueType>() { DataValueType.Null };
        var t = obj.GetType();
        if (t == typeof(char))
            return ((char)obj).GetPossibleTypes();
        if (t == typeof(string))
            return ((string)obj).GetPossibleTypes();
        if (t == typeof(byte))
            return ((byte)obj).GetPossibleTypes();
        if (t == typeof(sbyte))
            return ((sbyte)obj).GetPossibleTypes();
        if (t == typeof(int))
            return ((int)obj).GetPossibleTypes();
        if (t == typeof(uint))
            return ((uint)obj).GetPossibleTypes();
        if (t == typeof(short))
            return ((short)obj).GetPossibleTypes();
        if (t == typeof(ushort))
            return ((ushort)obj).GetPossibleTypes();
        if (t == typeof(long))
            return ((long)obj).GetPossibleTypes();
        if (t == typeof(ulong))
            return ((ulong)obj).GetPossibleTypes();
        if (t == typeof(decimal))
            return ((decimal)obj).GetPossibleTypes();
        if (t == typeof(double))
            return ((double)obj).GetPossibleTypes();
        if (t == typeof(float))
            return ((float)obj).GetPossibleTypes();
        if (t == typeof(bool))
            return ((bool)obj).GetPossibleTypes();
        if (t == typeof(DateOnly))
            return ((DateOnly)obj).GetPossibleTypes();
        if (t == typeof(DateTime))
            return ((DateTime)obj).GetPossibleTypes();
        return new HashSet<DataValueType>() { DataValueType.Unknown };
    }

    // char
    public static HashSet<DataValueType> GetPossibleTypes(this char? obj)
    {
        if (obj is null)
            return new HashSet<DataValueType>() { DataValueType.Null };
        return ((char)obj).GetPossibleTypes();
    }
    public static HashSet<DataValueType> GetPossibleTypes(this char obj)
    {
        var res = new HashSet<DataValueType>() { DataValueType.String };
        if (char.IsWhiteSpace(obj))
            res.Add(DataValueType.Null);
        if (char.IsDigit(obj))
            res.Add(DataValueType.Integer);
        var booleans = new List<char>() { '0', '1', 'Y', 'N', 'y', 'n', 'T', 'F', 't', 'f' };
        if (booleans.Contains(obj))
            res.Add(DataValueType.Boolean);
        return res;
    }

    // string
    public static HashSet<DataValueType> GetPossibleTypes(this string obj)
    {
        if (obj is null)
            return new HashSet<DataValueType>() { DataValueType.Null };

        var res = new HashSet<DataValueType>() { DataValueType.String };
        if (string.IsNullOrWhiteSpace(obj))
            res.Add(DataValueType.Null);
        if (Int128.TryParse(obj, out _))
            res.Add(DataValueType.Integer);
        if (decimal.TryParse(obj, out _))
            res.Add(DataValueType.Decimal);
        if (double.TryParse(obj, out _) || float.TryParse(obj, out _))
            res.Add(DataValueType.Scientific);
        var booleans = new List<string>() { "0", "1", "yes", "no", "true", "false" };
        if (booleans.Contains(obj, StringComparer.OrdinalIgnoreCase))
            res.Add(DataValueType.Boolean);
        if (decimal.TryParse(obj, out _))
            res.Add(DataValueType.Decimal);
        if (DateOnly.TryParse(obj, out _))
            res.Add(DataValueType.Date);
        if (DateTime.TryParse(obj, out _))
            res.Add(DataValueType.DateTime);
        return res;
    }

    // bool
    public static HashSet<DataValueType> GetPossibleTypes(this bool? obj)
    {
        if (obj is null)
            return new HashSet<DataValueType>() { DataValueType.Null };
        return ((bool)obj).GetPossibleTypes();
    }
    public static HashSet<DataValueType> GetPossibleTypes(this bool obj)
    {
        var res = new HashSet<DataValueType>() { DataValueType.Boolean };
        return res;
    }

    // byte / sbyte
    public static HashSet<DataValueType> GetPossibleTypes(this byte? obj)
    {
        if (obj is null)
            return new HashSet<DataValueType>() { DataValueType.Null };
        return ((byte)obj).GetPossibleTypes();
    }
    public static HashSet<DataValueType> GetPossibleTypes(this byte obj)
    {
        var res = new HashSet<DataValueType>() { DataValueType.Integer };
        if (obj == 0 || obj == 1)
            res.Add(DataValueType.Boolean);
        return res;
    }
    public static HashSet<DataValueType> GetPossibleTypes(this sbyte? obj)
    {
        if (obj is null)
            return new HashSet<DataValueType>() { DataValueType.Null };
        return ((sbyte)obj).GetPossibleTypes();
    }
    public static HashSet<DataValueType> GetPossibleTypes(this sbyte obj)
    {
        var res = new HashSet<DataValueType>() { DataValueType.Integer };
        if (obj == 0 || obj == 1)
            res.Add(DataValueType.Boolean);
        return res;
    }

    // int / uint
    public static HashSet<DataValueType> GetPossibleTypes(this int? obj)
    {
        if (obj is null)
            return new HashSet<DataValueType>() { DataValueType.Null };
        return ((int)obj).GetPossibleTypes();
    }
    public static HashSet<DataValueType> GetPossibleTypes(this int obj)
    {
        var res = new HashSet<DataValueType>() { DataValueType.Integer };
        if (obj == 0 || obj == 1)
            res.Add(DataValueType.Boolean);
        return res;
    }
    public static HashSet<DataValueType> GetPossibleTypes(this uint? obj)
    {
        if (obj is null)
            return new HashSet<DataValueType>() { DataValueType.Null };
        return ((uint)obj).GetPossibleTypes();
    }
    public static HashSet<DataValueType> GetPossibleTypes(this uint obj)
    {
        var res = new HashSet<DataValueType>() { DataValueType.Integer };
        if (obj == 0 || obj == 1)
            res.Add(DataValueType.Boolean);
        return res;
    }

    // short / ushort
    public static HashSet<DataValueType> GetPossibleTypes(this short? obj)
    {
        if (obj is null)
            return new HashSet<DataValueType>() { DataValueType.Null };
        return ((short)obj).GetPossibleTypes();
    }
    public static HashSet<DataValueType> GetPossibleTypes(this short obj)
    {
        var res = new HashSet<DataValueType>() { DataValueType.Integer };
        if (obj == 0 || obj == 1)
            res.Add(DataValueType.Boolean);
        return res;
    }
    public static HashSet<DataValueType> GetPossibleTypes(this ushort? obj)
    {
        if (obj is null)
            return new HashSet<DataValueType>() { DataValueType.Null };
        return ((ushort)obj).GetPossibleTypes();
    }
    public static HashSet<DataValueType> GetPossibleTypes(this ushort obj)
    {
        var res = new HashSet<DataValueType>() { DataValueType.Integer };
        if (obj == 0 || obj == 1)
            res.Add(DataValueType.Boolean);
        return res;
    }

    // long / ulong
    public static HashSet<DataValueType> GetPossibleTypes(this long? obj)
    {
        if (obj is null)
            return new HashSet<DataValueType>() { DataValueType.Null };
        return ((long)obj).GetPossibleTypes();
    }
    public static HashSet<DataValueType> GetPossibleTypes(this long obj)
    {
        var res = new HashSet<DataValueType>() { DataValueType.Integer };
        if (obj == 0 || obj == 1)
            res.Add(DataValueType.Boolean);
        return res;
    }
    public static HashSet<DataValueType> GetPossibleTypes(this ulong? obj)
    {
        if (obj is null)
            return new HashSet<DataValueType>() { DataValueType.Null };
        return ((ulong)obj).GetPossibleTypes();
    }
    public static HashSet<DataValueType> GetPossibleTypes(this ulong obj)
    {
        var res = new HashSet<DataValueType>() { DataValueType.Integer };
        if (obj == 0 || obj == 1)
            res.Add(DataValueType.Boolean);
        return res;
    }

    // decimal
    public static HashSet<DataValueType> GetPossibleTypes(this decimal? obj)
    {
        if (obj is null)
            return new HashSet<DataValueType>() { DataValueType.Null };
        return ((decimal)obj).GetPossibleTypes();
    }
    public static HashSet<DataValueType> GetPossibleTypes(this decimal obj)
    {
        var res = new HashSet<DataValueType>() { DataValueType.Decimal };
        if (obj.Equals(Math.Truncate(obj)))
            res.Add(DataValueType.Integer);
        return res;
    }

    // double
    public static HashSet<DataValueType> GetPossibleTypes(this double? obj)
    {
        if (obj is null)
            return new HashSet<DataValueType>() { DataValueType.Null };
        return ((double)obj).GetPossibleTypes();
    }
    public static HashSet<DataValueType> GetPossibleTypes(this double obj)
    {
        var res = new HashSet<DataValueType>() { DataValueType.Scientific };
        if (obj.Equals(Math.Truncate(obj)))
            res.Add(DataValueType.Integer);

        decimal decimalV = (decimal)obj;
        if (obj.Equals((double)decimalV))
            res.Add(DataValueType.Decimal);

        return res;
    }

    // float
    public static HashSet<DataValueType> GetPossibleTypes(this float? obj)
    {
        if (obj is null)
            return new HashSet<DataValueType>() { DataValueType.Null };
        return ((float)obj).GetPossibleTypes();
    }
    public static HashSet<DataValueType> GetPossibleTypes(this float obj)
    {
        var res = new HashSet<DataValueType>() { DataValueType.Scientific };
        if (obj.Equals(Math.Truncate(obj)))
            res.Add(DataValueType.Integer);

        decimal decimalV = (decimal)obj;
        if (obj.Equals((float)decimalV))
            res.Add(DataValueType.Decimal);

        return res;
    }

    // dateonly
    public static HashSet<DataValueType> GetPossibleTypes(this DateOnly? obj)
    {
        if (obj is null)
            return new HashSet<DataValueType>() { DataValueType.Null };
        return ((DateOnly)obj).GetPossibleTypes();
    }
    public static HashSet<DataValueType> GetPossibleTypes(this DateOnly obj)
    {
        var res = new HashSet<DataValueType>() { DataValueType.Date };
        return res;
    }

    // datetime
    public static HashSet<DataValueType> GetPossibleTypes(this DateTime? obj)
    {
        if (obj is null)
            return new HashSet<DataValueType>() { DataValueType.Null };
        return ((DateTime)obj).GetPossibleTypes();
    }
    public static HashSet<DataValueType> GetPossibleTypes(this DateTime obj)
    {
        var res = new HashSet<DataValueType>() { DataValueType.DateTime };
        if (obj.TimeOfDay == TimeSpan.Zero)
            res.Add(DataValueType.Date);
        return res;
    }

    //public static bool IsNumeric(this DataValueType dvt)
    //{
    //    switch (dvt)
    //    {
    //        case DataValueType.None:
    //            return new HashSet<DataValueType>() { DataValueType.None };
    //        case DataValueType.Null:
    //            return new HashSet<DataValueType>() { DataValueType.Null };
    //        case DataValueType.String:
    //            return new HashSet<DataValueType>() { DataValueType.String };
    //        case DataValueType.Integer:
    //            return new HashSet<DataValueType>() { DataValueType.Integer };
    //        default:
    //            throw new DataValueTypeException($"No equatable types defined for DataValueType {dvt}");
    //    }
    //}

    //public static HashSet<DataValueType> GetEquatableTypes(this DataValueType dvt)
    //{
    //    switch (dvt)
    //    {
    //        case DataValueType.None:
    //            return new HashSet<DataValueType>() { DataValueType.None };
    //        case DataValueType.Null:
    //            return new HashSet<DataValueType>() { DataValueType.Null };
    //        case DataValueType.String:
    //            return new HashSet<DataValueType>() { DataValueType.String };
    //        case DataValueType.Integer:
    //            return new HashSet<DataValueType>() { DataValueType.Integer };
    //        default:
    //            throw new DataValueTypeException($"No equatable types defined for DataValueType {dvt}");
    //    }
    //}
}
