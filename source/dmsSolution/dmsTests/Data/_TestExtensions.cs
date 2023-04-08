using dms.Data;
using System.IO;

namespace dmsTests.Data;

public static class DataValueTestExtensions
{

    public static void AssertEquals(this HashSet<DataValueType> observed, HashSet<DataValueType> expected)
    {
        Assert.IsTrue(CheckAreEqual(observed, expected)
            , $"Expected: ({string.Join(", ", expected)}) Observed: ({string.Join(", ", observed)})");
    }

    public static bool CheckAreEqual(HashSet<DataValueType> a, HashSet<DataValueType> b)
    {
        if (b is null && a is null)
            return true;
        else if (b is null || a is null)
            return false;

        if (a.Count != b.Count)
            return false;

        foreach (DataValueType dvt in a)
            if (!b.TryGetValue(dvt, out _))
                return false;

        return true;

    }


    public static void CheckStateMatchesExpected(this IDataReference reference, string expectedString)
    {
        var refStr = reference.ToString();
        Assert.IsFalse(refStr is null, "Reference string evaluates to null");
        Assert.IsTrue(refStr == expectedString, $"Reference string {{{refStr}}} does not match expected value {{{expectedString}}}");
    }
}
