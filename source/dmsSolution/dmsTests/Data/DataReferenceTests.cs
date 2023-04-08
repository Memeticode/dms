
using dms.Data;

namespace dmsTests.Data;

[TestClass]
public class DataReferenceTests
{
    
    [TestMethod]
    public void ConstructorTest()
    {
        string label = "main";
        TreePath path = new TreePath("0.0.0");

        var ref1 = new DataReference(label);
        var ref2 = new DataReference(label, path);

        ref1.CheckStateMatchesExpected("main:root");
        ref2.CheckStateMatchesExpected("main:0.0.0");
    }


    [TestMethod]
    public void EqualsTest()
    {
        var ref1 = new DataReference("main", new TreePath("0.0.0"));
        var ref2 = new DataReference("main:0.0.0");

        Assert.IsTrue(ref1.Equals(ref2), "DataReference Equals");
    }

}

