using dms.Data;

namespace dmsTests.Data;

[TestClass]
public class DataValueTypeExtensionsTests
{
    // Add tests for multi-type scenarios (i.e. a string that's a valid date);
    [TestMethod]
    public void GetPossibleTypes_Object_Test()
    {
        object a = null;
        DataValueTestExtensions.AssertEquals(a.GetPossibleTypes()
            , new HashSet<DataValueType>() { DataValueType.Null });

        object b = new object();
        DataValueTestExtensions.AssertEquals(b.GetPossibleTypes()
            , new HashSet<DataValueType>() { DataValueType.Unknown });

        object c = 'c';
        DataValueTestExtensions.AssertEquals(c.GetPossibleTypes()
            , new HashSet<DataValueType>() { DataValueType.String });

        object d = "str";
        DataValueTestExtensions.AssertEquals(d.GetPossibleTypes()
            , new HashSet<DataValueType>() { DataValueType.String });

        object e = (byte)2;
        DataValueTestExtensions.AssertEquals(e.GetPossibleTypes()
            , new HashSet<DataValueType>() { DataValueType.Integer });

        object f = (sbyte)2;
        DataValueTestExtensions.AssertEquals(f.GetPossibleTypes()
            , new HashSet<DataValueType>() { DataValueType.Integer });

        object g = (int)2;
        DataValueTestExtensions.AssertEquals(g.GetPossibleTypes()
            , new HashSet<DataValueType>() { DataValueType.Integer });

        object h = (uint)2;
        DataValueTestExtensions.AssertEquals(h.GetPossibleTypes()
            , new HashSet<DataValueType>() { DataValueType.Integer });

        object i = (short)2;
        DataValueTestExtensions.AssertEquals(i.GetPossibleTypes()
            , new HashSet<DataValueType>() { DataValueType.Integer });

        object j = (ushort)2;
        DataValueTestExtensions.AssertEquals(j.GetPossibleTypes()
            , new HashSet<DataValueType>() { DataValueType.Integer });

        object k = (long)2;
        DataValueTestExtensions.AssertEquals(k.GetPossibleTypes()
            , new HashSet<DataValueType>() { DataValueType.Integer });

        object l = (ulong)2;
        DataValueTestExtensions.AssertEquals(l.GetPossibleTypes()
            , new HashSet<DataValueType>() { DataValueType.Integer });

        object m = (decimal)2.01;
        DataValueTestExtensions.AssertEquals(m.GetPossibleTypes()
            , new HashSet<DataValueType>() { DataValueType.Decimal });

        object n = (double)2.01;
        DataValueTestExtensions.AssertEquals(n.GetPossibleTypes()
            , new HashSet<DataValueType>() { DataValueType.Scientific, DataValueType.Decimal });

        object o = (float)2.01;
        DataValueTestExtensions.AssertEquals(o.GetPossibleTypes()
            , new HashSet<DataValueType>() { DataValueType.Scientific, DataValueType.Decimal });

        object p = true;
        DataValueTestExtensions.AssertEquals(p.GetPossibleTypes()
            , new HashSet<DataValueType>() { DataValueType.Boolean });

        object q = DateTime.UtcNow;
        DataValueTestExtensions.AssertEquals(q.GetPossibleTypes()
            , new HashSet<DataValueType>() { DataValueType.DateTime });

        object r = new DateOnly(2000, 1, 1);
        DataValueTestExtensions.AssertEquals(r.GetPossibleTypes()
            , new HashSet<DataValueType>() { DataValueType.Date });
    }
}