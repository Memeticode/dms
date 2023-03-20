
namespace dmsTests.DataTree;

[TestClass]
public class TreePathTests
{
    // Test helpers

    protected internal void CheckPath(ITreePath path, IList<uint> list, string str, uint len)
    {
        Assert.IsTrue(ListsAreEqual(path.List, list));
        Assert.AreEqual(path.String, str);
        Assert.AreEqual(path.Length, len);
    }
    protected bool ListsAreEqual(IList<uint> list1, IList<uint> list2)
    {
        if (list1 is null && list2 is null) return true;
        if (list1 is null || list2 is null) return false;

        var len1 = list1.Count();
        var len2 = list2.Count();

        if (len1 != len2) return false;
        for (int i = 0; i < len1; i++)
            if (list1[i] != list2[i])
                return false;

        return true;
    }


    // Tests

    [TestMethod]
    public void ConstructorTest()
    {
        ITreePath path1 = new TreePath();
        ITreePath path2 = new TreePath(new int[] { 1, 2, 3 });
        ITreePath path3 = new TreePath(new uint[] { 1, 2, 3 });

        CheckPath(path1, new uint[] { }, "", 0);
        CheckPath(path2, new uint[] { 1, 2, 3 }, "1.2.3", 3);
        CheckPath(path3, new uint[] { 1, 2, 3 }, "1.2.3", 3);
    }

    [TestMethod]
    public void ConstructorExceptionTest()
    {
        try
        {
            ITreePath path = new TreePath(new int[] { -1 });
        }
        catch (TreePathException) { }
        catch (Exception ex)
        {
            throw new Exception("Unexpected exception thrown during tests.", ex);
        }
    }

    [TestMethod]
    public void EqualsTest()
    {
        ITreePath path1 = new TreePath();
        ITreePath path2 = new TreePath(new int[] { 1, 2, 3 });
        ITreePath path3 = new TreePath(new uint[] { 1, 2, 3 });
        ITreePath path4 = new TreePath(new uint[] { 1, 2, 3, 4 });

        Assert.IsFalse(path1.Equals(path2));
        Assert.IsTrue(path2.Equals(path3));
        Assert.IsFalse(path3.Equals(path4));
    }

    [TestMethod]
    public void CompareToTest()
    {
        ITreePath path1 = new TreePath();
        ITreePath path2 = new TreePath(new int[] { 1, 2, 3 });
        ITreePath path3 = new TreePath(new uint[] { 1, 2, 3 });
        ITreePath path4 = new TreePath(new uint[] { 1, 2, 3, 4 });
        ITreePath path5 = new TreePath(new uint[] { 1, 0, 5, 6, 7 });

        Assert.AreEqual(-1, path1.CompareTo(path2));
        Assert.AreEqual(0, path2.CompareTo(path3));
        Assert.AreEqual(-1, path3.CompareTo(path4));
        Assert.AreEqual(-1, path5.CompareTo(path4));
        Assert.AreEqual(1, path4.CompareTo(path5));
    }

    [TestMethod]
    public void IsBasePathOfTest()
    {
        ITreePath path1 = new TreePath();
        ITreePath path2 = new TreePath(new int[] { 1 });
        ITreePath path3 = new TreePath(new int[] { 1, 2 });
        ITreePath path4 = new TreePath(new int[] { 1, 2, 3 });
        ITreePath path5 = new TreePath(new int[] { 1, 3, 3 });

        Assert.IsFalse(path1.IsBasePathOf(null));

        Assert.IsTrue(path1.IsBasePathOf(path2));
        Assert.IsTrue(path2.IsBasePathOf(path3));
        Assert.IsTrue(path3.IsBasePathOf(path4));

        Assert.IsFalse(path2.IsBasePathOf(path1));
        Assert.IsFalse(path3.IsBasePathOf(path2));
        Assert.IsFalse(path4.IsBasePathOf(path3));

        Assert.IsFalse(path4.IsBasePathOf(path4));
        Assert.IsFalse(path3.IsBasePathOf(path5));
    }

    [TestMethod]
    public void IsAncestorOfTest()
    {
        ITreePath path1 = new TreePath();
        ITreePath path2 = new TreePath(new int[] { 1 });
        ITreePath path3 = new TreePath(new int[] { 1, 2 });
        ITreePath path4 = new TreePath(new int[] { 1, 2, 3 });
        ITreePath path5 = new TreePath(new int[] { 1, 10, 0 });

        Assert.IsFalse(path1.IsAncestorOf(null));
        Assert.IsFalse(path2.IsAncestorOf(null));

        Assert.IsTrue(path1.IsAncestorOf(path2));
        Assert.IsTrue(path1.IsAncestorOf(path3));
        Assert.IsTrue(path1.IsAncestorOf(path4));
        Assert.IsTrue(path1.IsAncestorOf(path5));

        Assert.IsFalse(path2.IsAncestorOf(path1));
        Assert.IsTrue(path2.IsAncestorOf(path3));
        Assert.IsTrue(path2.IsAncestorOf(path4));
        Assert.IsTrue(path2.IsAncestorOf(path5));

        Assert.IsFalse(path3.IsAncestorOf(path1));
        Assert.IsFalse(path3.IsAncestorOf(path2));
        Assert.IsTrue(path3.IsAncestorOf(path4));
        Assert.IsFalse(path3.IsAncestorOf(path5));

        Assert.IsFalse(path4.IsAncestorOf(path1));
        Assert.IsFalse(path4.IsAncestorOf(path2));
        Assert.IsFalse(path4.IsAncestorOf(path3));
        Assert.IsFalse(path4.IsAncestorOf(path5));

        Assert.IsFalse(path5.IsAncestorOf(path1));
        Assert.IsFalse(path5.IsAncestorOf(path2));
        Assert.IsFalse(path5.IsAncestorOf(path3));
        Assert.IsFalse(path5.IsAncestorOf(path4));
    }

    [TestMethod]
    public void IsDescendantOfTest()
    {
        ITreePath path1 = new TreePath();
        ITreePath path2 = new TreePath(new int[] { 1 });
        ITreePath path3 = new TreePath(new int[] { 1, 2 });
        ITreePath path4 = new TreePath(new int[] { 1, 2, 3 });
        ITreePath path5 = new TreePath(new int[] { 1, 10, 0 });

        Assert.IsFalse(path1.IsDescendantOf(null));
        Assert.IsFalse(path2.IsDescendantOf(null));

        Assert.IsFalse(path1.IsDescendantOf(path2));
        Assert.IsFalse(path1.IsDescendantOf(path3));
        Assert.IsFalse(path1.IsDescendantOf(path4));
        Assert.IsFalse(path1.IsDescendantOf(path5));

        Assert.IsTrue(path2.IsDescendantOf(path1));
        Assert.IsFalse(path2.IsDescendantOf(path3));
        Assert.IsFalse(path2.IsDescendantOf(path4));
        Assert.IsFalse(path2.IsDescendantOf(path5));

        Assert.IsTrue(path3.IsDescendantOf(path1));
        Assert.IsTrue(path3.IsDescendantOf(path2));
        Assert.IsFalse(path3.IsDescendantOf(path4));
        Assert.IsFalse(path3.IsDescendantOf(path5));

        Assert.IsTrue(path4.IsDescendantOf(path1));
        Assert.IsTrue(path4.IsDescendantOf(path2));
        Assert.IsTrue(path4.IsDescendantOf(path3));
        Assert.IsFalse(path4.IsDescendantOf(path5));

        Assert.IsTrue(path5.IsDescendantOf(path1));
        Assert.IsTrue(path5.IsDescendantOf(path2));
        Assert.IsFalse(path5.IsDescendantOf(path3));
        Assert.IsFalse(path5.IsDescendantOf(path4));
    }

    [TestMethod]
    public void TryParseStringAsPathTest()
    {
        var path1 = new TreePath();
        var path2 = new TreePath(new uint[] { 0, 1, 2, 3 });
        var ps1 = "";
        var ps2 = "0.1.2.3";
        var ps3 = "0.-1.2.3";
        var ps4 = "a.1.2.3";
        var ps5 = "aSDfSDF";
        var ps6 = ".1.2.3";
        var ps7 = "1.2.3.";
        var ps8 = "1..3";

        Assert.IsTrue(TreePath.TryParseStringAsPath(ps1, out ITreePath parsePath1));
        Assert.IsTrue(TreePath.TryParseStringAsPath(ps2, out ITreePath parsePath2));
        Assert.IsFalse(TreePath.TryParseStringAsPath(ps3, out ITreePath parsePath3));
        Assert.IsFalse(TreePath.TryParseStringAsPath(ps4, out ITreePath parsePath4));
        Assert.IsFalse(TreePath.TryParseStringAsPath(ps5, out ITreePath parsePath5));
        Assert.IsFalse(TreePath.TryParseStringAsPath(ps6, out ITreePath parsePath6));
        Assert.IsFalse(TreePath.TryParseStringAsPath(ps7, out ITreePath parsePath7));
        Assert.IsFalse(TreePath.TryParseStringAsPath(ps8, out ITreePath parsePath8));

        Assert.IsTrue(parsePath1.Equals(path1));
        Assert.IsTrue(parsePath2.Equals(path2));

        Assert.IsTrue(parsePath3 is null);
        Assert.IsTrue(parsePath4 is null);
        Assert.IsTrue(parsePath5 is null);
        Assert.IsTrue(parsePath6 is null);
        Assert.IsTrue(parsePath7 is null);
        Assert.IsTrue(parsePath8 is null);

    }

}

