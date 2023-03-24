using System.IO;

namespace dmsTests.DataTree;



/// <summary>
/// These tests perform graph operations (creating & manipulating nodes) and then check the state of the graph and it's nodes to confirm everything is working as expected. 
/// Tests apply to TreeGraph<object> and TreeNode<object>.
/// Graph state checks include: tree height and node count.
/// Node state checks include: node type, path, depth, value, and linked/related nodes.
/// </summary>
[TestClass]
public class TreeGraphNodeObjectTests
{

    [TestMethod]
    public void ConstructorTest()
    {
        var tree = new TreeGraph<object>();
        var root = tree.Root;
        tree.Validate();

        tree.CheckStateMatchesExpected(1, 0);
        root.CheckStateMatchesExpected(TreeNodeType.Root, "", 0, 0, null, null, null, null, null);
    }

    /// <summary>
    /// Test GetNode works as expected
    /// </summary>
    [TestMethod]
    public void GetNodeTest()
    {
        var tree = new TreeGraph<object>();
        var node = tree.CreateBranch(tree.Root);
        
        tree.Validate();
        Assert.AreEqual(tree.GetNode(node.Id).Id, node.Id);
        Assert.IsTrue(true);
        tree.Validate();
    }

    /// <summary>
    /// Test GetNodeByPath works as expected
    /// </summary>
    [TestMethod]
    public void GetNodeByPathTest()
    {
        var tree = new TreeGraph<object>();
        var node1 = tree.CreateBranch(tree.Root);
        var node2 = tree.CreateBranch(node1);
        var node3 = tree.CreateBranch(node2);
        var node4 = tree.CreateBranch(node2);
        tree.Validate();

        Assert.AreEqual(tree.GetNodeByPath("").Id, tree.Root.Id);
        Assert.AreEqual(tree.GetNodeByPath("0").Id, node1.Id);
        Assert.AreEqual(tree.GetNodeByPath("0.0").Id, node2.Id);
        Assert.AreEqual(tree.GetNodeByPath("0.0.0").Id, node3.Id);
        Assert.AreEqual(tree.GetNodeByPath("0.0.1").Id, node4.Id);
        tree.Validate();
    }


    /// <summary>
    /// Test value operations (check has value, get value, set value, delete value) all work as expected
    /// </summary>
    [TestMethod]
    public void ValueOperationsTest()
    {
        Assert.IsTrue(true);
        var tree = new TreeGraph<object>();
        var node = tree.Root;

        Assert.IsFalse(node.HasValue);
        Assert.IsFalse(node.TryGetValue(out object? v1));

        node.SetValue(1);
        Assert.IsTrue(node.HasValue);
        Assert.IsTrue(node.TryGetValue(out object? v2) && (int?)v2 == 1);

        node.SetValue(null);
        Assert.IsTrue(node.HasValue);
        Assert.IsTrue(node.TryGetValue(out object? v3) && v3 is null);


        node.SetValue("ABC");
        Assert.IsTrue(node.HasValue);
        Assert.IsTrue(node.TryGetValue(out object? v4) && (v4?.Equals("ABC") ?? false));

        node.DeleteValue();
        Assert.IsFalse(node.HasValue);
        Assert.IsFalse(node.TryGetValue(out object? v5));
    }


    /// <summary>
    /// Test "create branch" method works as expected
    /// </summary>
    [TestMethod]
    public void CreateBranchTest()
    {
        var tree = new TreeGraph<object>();
        var node = tree.CreateBranch(tree.Root);

        tree.Validate();
        tree.CheckStateMatchesExpected(2, 1);
        node.CheckStateMatchesExpected(TreeNodeType.Leaf, "0", 1, 0, tree.Root, null, null, null, null);
        tree.Validate();
    }

    /// <summary>
    /// Test all "create branch" methods work as expected
    /// </summary>
    [TestMethod]
    public void CreateBranchesTest()
    {
        var tree = new TreeGraph<object>();
        var node1d = tree.CreateBranch(tree.Root);
        var node1c = tree.CreateBranchAt(tree.Root, 0);
        var node1b = tree.CreatePrevPeer(node1c);
        var node1e = tree.CreateNextPeer(node1d);
        var node1f = tree.CreateBranch(tree.Root);
        var node1a = tree.CreateBranchAt(tree.Root, 0);

        tree.Validate();
        tree.CheckStateMatchesExpected(7, 1);
        node1a.CheckStateMatchesExpected(TreeNodeType.Leaf, "0", 1, 0, tree.Root, null, node1b, null, null);
        node1b.CheckStateMatchesExpected(TreeNodeType.Leaf, "1", 1, 1, tree.Root, node1a, node1c, null, null);
        node1c.CheckStateMatchesExpected(TreeNodeType.Leaf, "2", 1, 2, tree.Root, node1b, node1d, null, null);
        node1d.CheckStateMatchesExpected(TreeNodeType.Leaf, "3", 1, 3, tree.Root, node1c, node1e, null, null);
        node1e.CheckStateMatchesExpected(TreeNodeType.Leaf, "4", 1, 4, tree.Root, node1d, node1f, null, null);
        node1f.CheckStateMatchesExpected(TreeNodeType.Leaf, "5", 1, 5, tree.Root, node1e, null, null, null);
        tree.Validate();
    }


    /// <summary>
    /// Test "delete node" method
    /// </summary>
    [TestMethod]
    public void DeleteNodeTest()
    {
        var tree = new TreeGraph<object>();
        var node = tree.CreateBranch(tree.Root);
        tree.Validate();

        tree.CheckStateMatchesExpected(2, 1);
        tree.Root.CheckStateMatchesExpected(TreeNodeType.Root, "", 0, 0, null, null, null, node, node);
        node.CheckStateMatchesExpected(TreeNodeType.Leaf, "0", 1, 0, tree.Root, null, null, null, null);

        tree.DeleteNode(node);

        tree.CheckStateMatchesExpected(1, 0);
        tree.Root.CheckStateMatchesExpected(TreeNodeType.Root, "", 0, 0, null, null, null, null, null);
        tree.Validate();
    }

    /// <summary>
    /// Test "delete node" method will error when node has branches unless deleteDescendants=true
    /// </summary>
    [TestMethod]
    public void DeleteNodeWithBranchTest()
    {
        var tree = new TreeGraph<object>();
        var node1 = tree.CreateBranch(tree.Root);
        var node2 = tree.CreateBranch(node1);
        tree.Validate();

        tree.CheckStateMatchesExpected(3, 2);
        tree.Root.CheckStateMatchesExpected(TreeNodeType.Root, "", 0, 0, null, null, null, node1, node1);
        node1.CheckStateMatchesExpected(TreeNodeType.Stem, "0", 1, 0, tree.Root, null, null, node2, node2);
        node2.CheckStateMatchesExpected(TreeNodeType.Leaf, "0.0", 2, 0, node1, null, null, null, null);

        // confirm throws error when deleting node with branch
        try
        {
            tree.DeleteNode(node1);
        }
        catch (InvalidOperationException)
        { }
        catch (Exception ex)
        {
            throw new Exception("Unexpected error type encountered when testing DeleteNode tree method.", ex);
        }

        tree.CheckStateMatchesExpected(3, 2);
        tree.Root.CheckStateMatchesExpected(TreeNodeType.Root, "", 0, 0, null, null, null, node1, node1);
        node1.CheckStateMatchesExpected(TreeNodeType.Stem, "0", 1, 0, tree.Root, null, null, node2, node2);
        node2.CheckStateMatchesExpected(TreeNodeType.Leaf, "0.0", 2, 0, node1, null, null, null, null);


        // confirm dletes node and descendants when delete descendents is true
        tree.DeleteNode(node1, true);

        tree.CheckStateMatchesExpected(1, 0);
        tree.Root.CheckStateMatchesExpected(TreeNodeType.Root, "", 0, 0, null, null, null, null, null);
        tree.Validate();
    }




    /// <summary>
    /// Test "swap node" which swaps the position of 2 nodes. Nodes retain their branches, but change stem & sibling node (if applicable).
    /// </summary>
    [TestMethod]
    public void SwapNodesTest()
    {
        var tree = new TreeGraph<object>();

        // ACTION - Add nodes with nested depth, then swap nodes
        var node1 = tree.CreateBranch(tree.Root);
        var node1a = tree.CreateBranch(node1);
        var node1b = tree.CreateBranch(node1);
        var node2 = tree.CreateBranch(tree.Root);
        var node3 = tree.CreateBranch(tree.Root);
        var node3a = tree.CreateBranch(node3);
        var node3b = tree.CreateBranch(node3);
        tree.Validate();

        var t = node3.GetBranches();
        var t2 = node3.GetLastBranch();
        tree.CheckStateMatchesExpected(8, 2);
        node1a.CheckStateMatchesExpected(TreeNodeType.Leaf, "0.0", 2, 0, node1, null, node1b, null, null);
        node3.CheckStateMatchesExpected(TreeNodeType.Stem, "2", 1, 2, tree.Root, node2, null, node3a, node3b);

        tree.SwapNodes(node1a, node3);

        tree.CheckStateMatchesExpected(8, 3);
        node1a.CheckStateMatchesExpected(TreeNodeType.Leaf, "2", 1, 2, tree.Root, node2, null, null, null);
        node3.CheckStateMatchesExpected(TreeNodeType.Stem, "0.0", 2, 0, node1, null, node1b, node3a, node3b);
        tree.Validate();
    }



    /// <summary>
    /// Test "swap node" errors when attempting to swap nodes if one is a descendant of the other
    /// </summary>
    [TestMethod]
    public void SwapErrorTest()
    {
        var tree = new TreeGraph<object>();
        var node1 = tree.CreateBranch(tree.Root);
        var node2 = tree.CreateBranch(node1);
        var node3 = tree.CreateBranch(node2);

        tree.Validate();
        tree.CheckStateMatchesExpected(4, 3);
        node1.CheckStateMatchesExpected(TreeNodeType.Stem, "0", 1, 0, tree.Root, null, null, node2, node2);
        node2.CheckStateMatchesExpected(TreeNodeType.Stem, "0.0", 2, 0, node1, null, null, node3, node3);
        node3.CheckStateMatchesExpected(TreeNodeType.Leaf, "0.0.0", 3, 0, node2, null, null, null, null);

        Assert.IsFalse(node1.GetPath().IsBasePathOf(node3.GetPath()));
        Assert.IsTrue(node1.GetPath().IsAncestorOf(node3.GetPath()));
        Assert.IsTrue(node3.GetPath().IsDescendantOf(node1.GetPath()));

        // confirm throws error when deleting node with branch
        try
        {
            tree.SwapNodes(node1, node3);
        }
        catch (InvalidOperationException)
        { }
        catch (Exception ex)
        {
            throw new Exception("Unexpected error type encountered.", ex);
        }

        tree.CheckStateMatchesExpected(4, 3);
        node1.CheckStateMatchesExpected(TreeNodeType.Stem, "0", 1, 0, tree.Root, null, null, node2, node2);
        node2.CheckStateMatchesExpected(TreeNodeType.Stem, "0.0", 2, 0, node1, null, null, node3, node3);
        node3.CheckStateMatchesExpected(TreeNodeType.Leaf, "0.0.0", 3, 0, node2, null, null, null, null);
        tree.Validate();
    }



    /// <summary>
    /// Test "move position" assigns node position (moving nodes within peer group)
    /// </summary>
    [TestMethod]
    public void SetNodePositionTest()
    {
        // root with 3 branch nodes
        var tree = new TreeGraph<object>();
        var node1 = tree.CreateBranch(tree.Root);
        var node2 = tree.CreateBranch(tree.Root);
        var node3 = tree.CreateBranch(tree.Root);

        tree.Validate();
        tree.CheckStateMatchesExpected(4, 1);
        tree.Root.CheckStateMatchesExpected(TreeNodeType.Root, "", 0, 0, null, null, null, node1, node3);
        node1.CheckStateMatchesExpected(TreeNodeType.Leaf, "0", 1, 0, tree.Root, null, node2, null, null);
        node2.CheckStateMatchesExpected(TreeNodeType.Leaf, "1", 1, 1, tree.Root, node1, node3, null, null);
        node3.CheckStateMatchesExpected(TreeNodeType.Leaf, "2", 1, 2, tree.Root, node2, null, null, null);

        tree.SetNodePosition(node1, 2);

        tree.Validate();
        tree.Root.CheckStateMatchesExpected(TreeNodeType.Root, "", 0, 0, null, null, null, node2, node1);
        node2.CheckStateMatchesExpected(TreeNodeType.Leaf, "0", 1, 0, tree.Root, null, node3, null, null);
        node3.CheckStateMatchesExpected(TreeNodeType.Leaf, "1", 1, 1, tree.Root, node2, node1, null, null);
        node1.CheckStateMatchesExpected(TreeNodeType.Leaf, "2", 1, 2, tree.Root, node3, null, null, null);

        tree.SetNodePosition(node3, 0);

        tree.Validate();
        tree.Root.CheckStateMatchesExpected(TreeNodeType.Root, "", 0, 0, null, null, null, node3, node1);
        node3.CheckStateMatchesExpected(TreeNodeType.Leaf, "0", 1, 0, tree.Root, null, node2, null, null);
        node2.CheckStateMatchesExpected(TreeNodeType.Leaf, "1", 1, 1, tree.Root, node3, node1, null, null);
        node1.CheckStateMatchesExpected(TreeNodeType.Leaf, "2", 1, 2, tree.Root, node2, null, null, null);

        tree.SetNodePosition(node2, 1);

        tree.Validate();
        tree.Root.CheckStateMatchesExpected(TreeNodeType.Root, "", 0, 0, null, null, null, node3, node1);
        node3.CheckStateMatchesExpected(TreeNodeType.Leaf, "0", 1, 0, tree.Root, null, node2, null, null);
        node2.CheckStateMatchesExpected(TreeNodeType.Leaf, "1", 1, 1, tree.Root, node3, node1, null, null);
        node1.CheckStateMatchesExpected(TreeNodeType.Leaf, "2", 1, 2, tree.Root, node2, null, null, null);

        tree.Validate();
    }


    /// <summary>
    /// Test "move position" errors when attempting to move node to an invalid position
    /// </summary>
    [TestMethod]
    public void SetNodePositionErrorTest()
    {
        // root with 3 branch nodes
        var tree = new TreeGraph<object>();
        var node1 = tree.CreateBranch(tree.Root);
        var node2 = tree.CreateBranch(tree.Root);
        var node3 = tree.CreateBranch(tree.Root);

        tree.Validate();
        tree.CheckStateMatchesExpected(4, 1);
        tree.Root.CheckStateMatchesExpected(TreeNodeType.Root, "", 0, 0, null, null, null, node1, node3);
        node1.CheckStateMatchesExpected(TreeNodeType.Leaf, "0", 1, 0, tree.Root, null, node2, null, null);
        node2.CheckStateMatchesExpected(TreeNodeType.Leaf, "1", 1, 1, tree.Root, node1, node3, null, null);
        node3.CheckStateMatchesExpected(TreeNodeType.Leaf, "2", 1, 2, tree.Root, node2, null, null, null);


        // position not uint
        try
        {
            tree.SetNodePosition(node1, -1);
        }
        catch (ArgumentException)
        { }
        catch (Exception negativePositionEx)
        {
            throw new Exception("Unexpected error type encountered.", negativePositionEx);
        }

        tree.Validate();
        tree.CheckStateMatchesExpected(4, 1);
        tree.Root.CheckStateMatchesExpected(TreeNodeType.Root, "", 0, 0, null, null, null, node1, node3);
        node1.CheckStateMatchesExpected(TreeNodeType.Leaf, "0", 1, 0, tree.Root, null, node2, null, null);
        node2.CheckStateMatchesExpected(TreeNodeType.Leaf, "1", 1, 1, tree.Root, node1, node3, null, null);
        node3.CheckStateMatchesExpected(TreeNodeType.Leaf, "2", 1, 2, tree.Root, node2, null, null, null);

        // position invalid based on # of nodes in peer group
        try
        {
            tree.SetNodePosition(node1, 3);
        }
        catch (InvalidOperationException)
        { }
        catch (Exception invalidPositionEx)
        {
            throw new Exception("Unexpected error type encountered.", invalidPositionEx);
        }

        tree.Validate();
        tree.CheckStateMatchesExpected(4, 1);
        tree.Root.CheckStateMatchesExpected(TreeNodeType.Root, "", 0, 0, null, null, null, node1, node3);
        node1.CheckStateMatchesExpected(TreeNodeType.Leaf, "0", 1, 0, tree.Root, null, node2, null, null);
        node2.CheckStateMatchesExpected(TreeNodeType.Leaf, "1", 1, 1, tree.Root, node1, node3, null, null);
        node3.CheckStateMatchesExpected(TreeNodeType.Leaf, "2", 1, 2, tree.Root, node2, null, null, null);

    }





    /// <summary>
    /// Test "move to stem" works.
    /// </summary>
    [TestMethod]
    public void MoveToStemTest()
    {
        // root with 3 branch nodes
        var tree = new TreeGraph<object>();
        var node1 = tree.CreateBranch(tree.Root);
        var node2 = tree.CreateBranch(node1);
        var node3 = tree.CreateBranch(node2);

        tree.Validate();
        tree.CheckStateMatchesExpected(4, 3);
        tree.Root.CheckStateMatchesExpected(TreeNodeType.Root, "", 0, 0, null, null, null, node1, node1);
        node1.CheckStateMatchesExpected(TreeNodeType.Stem, "0", 1, 0, tree.Root, null, null, node2, node2);
        node2.CheckStateMatchesExpected(TreeNodeType.Stem, "0.0", 2, 0, node1, null, null, node3, node3);
        node3.CheckStateMatchesExpected(TreeNodeType.Leaf, "0.0.0", 3, 0, node2, null, null, null, null);

        tree.SetNodeStem(node3, node2);

        tree.Validate();
        tree.CheckStateMatchesExpected(4, 3);
        tree.Root.CheckStateMatchesExpected(TreeNodeType.Root, "", 0, 0, null, null, null, node1, node1);
        node1.CheckStateMatchesExpected(TreeNodeType.Stem, "0", 1, 0, tree.Root, null, null, node2, node2);
        node2.CheckStateMatchesExpected(TreeNodeType.Stem, "0.0", 2, 0, node1, null, null, node3, node3);
        node3.CheckStateMatchesExpected(TreeNodeType.Leaf, "0.0.0", 3, 0, node2, null, null, null, null);

        tree.SetNodeStem(node3, node1);

        tree.Validate();
        tree.CheckStateMatchesExpected(4, 2);
        tree.Root.CheckStateMatchesExpected(TreeNodeType.Root, "", 0, 0, null, null, null, node1, node1);
        node1.CheckStateMatchesExpected(TreeNodeType.Stem, "0", 1, 0, tree.Root, null, null, node2, node3);
        node2.CheckStateMatchesExpected(TreeNodeType.Leaf, "0.0", 2, 0, node1, null, node3, null, null);
        node3.CheckStateMatchesExpected(TreeNodeType.Leaf, "0.1", 2, 1, node1, node2, null, null, null);

    }

    /// <summary>
    /// Test "move Stem" errors when attempting to move node
    /// </summary>
    [TestMethod]
    public void MoveToStemErrorTest()
    {
        // root with 3 branch nodes
        var tree = new TreeGraph<object>();
        var node1 = tree.CreateBranch(tree.Root);
        var node2 = tree.CreateBranch(tree.Root);
        var node3 = tree.CreateBranch(tree.Root);

        tree.Validate();
        tree.CheckStateMatchesExpected(4, 1);
        tree.Root.CheckStateMatchesExpected(TreeNodeType.Root, "", 0, 0, null, null, null, node1, node3);
        node1.CheckStateMatchesExpected(TreeNodeType.Leaf, "0", 1, 0, tree.Root, null, node2, null, null);
        node2.CheckStateMatchesExpected(TreeNodeType.Leaf, "1", 1, 1, tree.Root, node1, node3, null, null);
        node3.CheckStateMatchesExpected(TreeNodeType.Leaf, "2", 1, 2, tree.Root, node2, null, null, null);


        // position not uint
        try
        {
            tree.SetNodePosition(node1, -1);
        }
        catch (ArgumentException)
        { }
        catch (Exception negativePositionEx)
        {
            throw new Exception("Unexpected error type encountered.", negativePositionEx);
        }

        tree.Validate();
        tree.CheckStateMatchesExpected(4, 1);
        tree.Root.CheckStateMatchesExpected(TreeNodeType.Root, "", 0, 0, null, null, null, node1, node3);
        node1.CheckStateMatchesExpected(TreeNodeType.Leaf, "0", 1, 0, tree.Root, null, node2, null, null);
        node2.CheckStateMatchesExpected(TreeNodeType.Leaf, "1", 1, 1, tree.Root, node1, node3, null, null);
        node3.CheckStateMatchesExpected(TreeNodeType.Leaf, "2", 1, 2, tree.Root, node2, null, null, null);

        // position invalid based on # of nodes in peer group
        try
        {
            tree.SetNodePosition(node1, 3);
        }
        catch (InvalidOperationException)
        { }
        catch (Exception invalidPositionEx)
        {
            throw new Exception("Unexpected error type encountered.", invalidPositionEx);
        }

        tree.Validate();
        tree.CheckStateMatchesExpected(4, 1);
        tree.Root.CheckStateMatchesExpected(TreeNodeType.Root, "", 0, 0, null, null, null, node1, node3);
        node1.CheckStateMatchesExpected(TreeNodeType.Leaf, "0", 1, 0, tree.Root, null, node2, null, null);
        node2.CheckStateMatchesExpected(TreeNodeType.Leaf, "1", 1, 1, tree.Root, node1, node3, null, null);
        node3.CheckStateMatchesExpected(TreeNodeType.Leaf, "2", 1, 2, tree.Root, node2, null, null, null);

    }
}

