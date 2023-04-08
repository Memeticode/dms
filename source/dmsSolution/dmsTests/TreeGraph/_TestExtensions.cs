namespace dmsTests.TreeGraph;


public static class TestHelperExtensions
{

	public static void CheckStateMatchesExpected<T>(
		this TreeGraph<T> tree
		, uint nodeCount
		, uint height
		)
	{
		Assert.IsNotNull(tree, "Tree is null");
		Assert.AreEqual(nodeCount, tree.GetNodeCount(), "Tree Node Count");
		Assert.AreEqual(height, tree.GetHeight(), "Tree Height");
	}

	public static void CheckStateMatchesExpected<T>(
		this TreeNode<T> node
		//, uint id
		, TreeNodeType nodeType
		, string pathString
		, uint depth
		, uint position
		, ITreeNode<T>? stem
		, ITreeNode<T>? prevPeer
		, ITreeNode<T>? nextPeer
		, ITreeNode<T>? firstBranch
		, ITreeNode<T>? lastBranch
		)
	{
		Assert.IsNotNull(node, "Node is null");
		//Assert.AreEqual(id, node.Id, "Node Id");		
		Assert.AreEqual(nodeType, node.NodeType, "Node TreeNodeType");
		Assert.AreEqual(depth, node.GetDepth(), "Node Depth");		
		Assert.IsTrue(node.GetPath().Equals(new TreePath(pathString)), "Node Path");
		Assert.AreEqual(position, node.Position, "Node Position");

		// Check that results of node methods which get related nodes match expected if not null, otherwise are null.
		if (node.TryGetStem(out ITreeNode<T>? aStem)) { Assert.AreEqual(stem?.Id, aStem?.Id, "Node StemId"); }
		else { Assert.IsNull(stem); }

		if (node.TryGetPrevPeer(out ITreeNode<T>? aPrevPeer)) { Assert.AreEqual(prevPeer?.Id, aPrevPeer?.Id, "Node PrevPeer"); }
		else { Assert.IsNull(prevPeer); }

		if (node.TryGetNextPeer(out ITreeNode<T>? aNextPeer)) { Assert.AreEqual(nextPeer?.Id, aNextPeer?.Id, "Node NextPeer"); }
		else { Assert.IsNull(nextPeer); }

		if (node.TryGetFirstBranch(out ITreeNode<T>? aFirstBranch)) { Assert.AreEqual(firstBranch?.Id, aFirstBranch?.Id, "Node FirstBranch"); }
        else { Assert.IsNull(firstBranch); }

		if (node.TryGetLastBranch(out ITreeNode<T>? aLastBranch)) { Assert.AreEqual(lastBranch?.Id, aLastBranch?.Id, "Node LastBranch"); }
		else { Assert.IsNull(lastBranch); }
	}


	public static void CheckStateMatchesExpected<T>(
		this ITreeNode<T> node
		//, uint id
		, TreeNodeType nodeType
		, string pathString
		, uint depth
		, uint position
		, ITreeNode<T>? stem
		, ITreeNode<T>? prevPeer
		, ITreeNode<T>? nextPeer
		, ITreeNode<T>? firstBranch
		, ITreeNode<T>? lastBranch
		)
		=> ((TreeNode<T>)node).CheckStateMatchesExpected(nodeType, pathString, depth, position, stem, prevPeer, nextPeer, firstBranch, lastBranch);


	public static void CheckStateMatchesExpected(this ITreePath path, IEnumerable<uint> positions, string str, uint len)
	{
		Assert.IsTrue(path.Equals(new TreePath(positions)), "Path Positions");
		Assert.AreEqual(path.String, str, "Path String");
		Assert.AreEqual(path.Length, len, "Path Length");
	}

}

