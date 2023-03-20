namespace dms.DataTree;

public class TreeNodeException : Exception
{
    public TreeNodeException() : base() { }
    public TreeNodeException(string message) : base(message) { }
    public TreeNodeException(string message, Exception innerException) : base(message, innerException) { }
}

public class TreeNode<T> : ITreeNode<T>
{
    protected internal TreeNodeType GetTreeNodeType()
    {
        if (IsRoot) return TreeNodeType.Root;
        if (!HasStem) return TreeNodeType.Floating;
        if (!HasBranch) return TreeNodeType.Leaf;
        return TreeNodeType.Stem;
    }
    protected internal uint GetPosition()
    {
        if (Tree.PositionMap.TryGetValue(Id, out uint position)) return position;
        return TreeGraph<T>.BasePosition;
    }

    // Constructor
    public TreeNode(ITreeGraph<T> tree, uint id)
    {
        if (tree == null) throw new ArgumentNullException(nameof(tree));
        Tree = tree;
        Id = id;
    }

    // ITreeNode Implementation
    public ITreeGraph<T> Tree { get; }
    public uint Id { get; }
    public TreeNodeType NodeType => GetTreeNodeType();

    public ITreePath GetPath()
    {
        if (!HasStem)
        {
            if (IsRoot) return new TreePath();
            else throw new TreeNodeException("Cannot get node path because specified node is a floating node (has no stem but is not root).");
        }
        var pathList = new List<uint>() { Position };
        var node = GetStem();
        while (node.HasStem)
        {
            pathList.Insert(0, node.Position);
            node = node.GetStem();
        }
        if (!node.IsRoot)
        {
            if (IsRoot) return new TreePath();
            else throw new TreeNodeException("Cannot get node path because specified node descends from a floating node (has no stem but is not root).");
        }
        return new TreePath(pathList);
    }
    public uint GetDepth() => GetPath().Length;

    public T GetValue() => Tree.GetNodeValue(this);
    public void SetValue(T value) => Tree.SetNodeValue(this, value);
    public void DeleteValue() => Tree.DeleteNodeValue(this);

    public uint Position => GetPosition();
    public bool IsRoot => Tree.Root.Id.Equals(Id);
    public bool HasValue => Tree.NodeHasValue(this);
    public bool HasStem => Tree.StemMap.ContainsKey(Id);
    public bool HasBranch => GetBranches().Any();
    public bool HasNextPeer { get; }
    public bool HasPrevPeer { get; }

    public ITreeNode<T> GetStem()
    {
        if (HasStem) return Tree.GetNode(Tree.StemMap[Id]);
        throw new TreeNodeException($"Specified node does not have any branches.");
    }
    public ITreeNode<T>[] GetBranches()
    {
        if (HasBranch)
            return Tree.BranchMap[Id].OrderBy(b => Tree.PositionMap[b]).Select(b => Tree.GetNode(b)).ToArray();
        return new ITreeNode<T>[] { };
    }
    public ITreeNode<T> GetFirstBranch()
    {
        if (HasBranch) return Tree.GetNode(GetBranches().First().Id);
        throw new TreeNodeException($"Specified node does not have any branches.");
    }
    public ITreeNode<T> GetLastBranch()
    {
        if (HasBranch) return Tree.GetNode(GetBranches().Last().Id);
        throw new TreeNodeException($"Specified node does not have any branches.");
    }
    public ITreeNode<T>[] GetPeers()
    {
        if (HasStem)
            return Tree.BranchMap[Tree.StemMap[Id]]
                .OrderBy(b => Tree.PositionMap[b])
                //.Where(b => !(Tree.GetNode(b).Id.Equals(Id)))
                .Select(b => Tree.GetNode(b))
                .ToArray();
        else
            throw new TreeNodeException($"Specified node does not have a stem and thus has no peer nodes.");
    }
    public ITreeNode<T>[] GetPeersExcludingSelf()
    {
        if (HasStem)
            return Tree.BranchMap[Tree.StemMap[Id]]
                .OrderBy(b => Tree.PositionMap[b])
                .Where(b => !(Tree.GetNode(b).Id.Equals(Id)))
                .Select(b => Tree.GetNode(b))
                .ToArray();
        else
            throw new TreeNodeException($"Specified node does not have a stem and thus has no peer nodes.");
    }
    public ITreeNode<T> GetPrevPeer()
    {
        if (HasPrevPeer)
            return Tree.GetNode(
                Tree.BranchMap[Tree.StemMap[Id]]
                    .Where(b => Tree.PositionMap[b] < Position)
                    .OrderByDescending(b => Tree.PositionMap[b])
                    .First()
                );
        throw new TreeNodeException($"Specified node does node have a previous peer node to get.");
    }
    public ITreeNode<T> GetNextPeer()
    {
        if (HasNextPeer)
            return Tree.GetNode(
                Tree.BranchMap[Tree.StemMap[Id]]
                    .Where(b => Tree.PositionMap[b] > Position)
                    .OrderBy(b => Tree.PositionMap[b])
                    .First()
                );
        throw new TreeNodeException($"Specified node does node have a next peer node to get.");
    }


}


