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
        //if (!HasStem)
        //{
        //    if (IsRoot) return new TreePath();
        //    else throw new TreeNodeException("Cannot get node path because specified node is a floating node (has no stem but is not root).");
        //}

        if (IsRoot) 
            return new TreePath();

        var pathList = new List<uint>() { Position };
        int count = 1;
        var node = this;
        while (node.HasStem)
        {
            count++;
            if (count > TreeGraph<T>.MaxDepth)
                throw new TreeGraphException($"Max path length {TreeGraph<T>.MaxDepth} reached when traversing graph to get path for node id {Id}.");

            node = (TreeNode<T>)node.GetStem();
            if (node.HasStem)
                pathList.Insert(0, node.Position);
        }
        return new TreePath(pathList);
    }


    public T? GetValue() => Tree.GetNodeValue(this);
    public bool TryGetValue(out T? value)
    {
        try
        {
            value = Tree.GetNodeValue(this);
            return true;
        }
        catch
        {
            value = default(T?);
            return false;
        }
    }
    public void SetValue(T? value) => Tree.SetNodeValue(this, value);
    public void DeleteValue() => Tree.DeleteNodeValue(this);

    public string PathString => GetPath().String;
    public uint Position => GetPosition();
    public bool IsRoot => Tree.Root.Id.Equals(Id);
    public bool HasValue => Tree.NodeHasValue(this);
    public bool HasStem => Tree.StemMap.ContainsKey(Id);
    public bool HasBranch => Tree.BranchMap.ContainsKey(Id) ? Tree.BranchMap[Id].Any() : false;
    public bool HasNextPeer => HasStem ? Position < GetStem().GetBranches().Count() : false;
    public bool HasPrevPeer => HasStem ? Position > TreeGraph<T>.BasePosition: false;

    public uint GetDepth() => HasStem ? GetPath().Length : 0;

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

    public bool TryGetStem(out ITreeNode<T>? stem)
    {
        try
        {
            stem = GetStem();
            return true;
        }
        catch
        {
            stem = null;
            return false;
        }
    }
    public bool TryGetFirstBranch(out ITreeNode<T>? firstBranch)
    {
        try
        {
            firstBranch = GetFirstBranch();
            return true;
        }
        catch
        {
            firstBranch = null;
            return false;
        }
    }
    public bool TryGetLastBranch(out ITreeNode<T>? lastBranch)
    {
        try
        {
            lastBranch = GetLastBranch();
            return true;
        }
        catch
        {
            lastBranch = null;
            return false;
        }
    }
    public bool TryGetPrevPeer(out ITreeNode<T>? prevPeer)
    {
        try
        {
            prevPeer = GetPrevPeer();
            return true;
        }
        catch
        {
            prevPeer = null;
            return false;
        }
    }
    public bool TryGetNextPeer(out ITreeNode<T>? nextPeer)
    {
        try
        {
            nextPeer = GetNextPeer();
            return true;
        }
        catch
        {
            nextPeer = null;
            return false;
        }
    }

}


public class TreeNodeTopologyCache<T>
{
    public uint Id { get; }
    public TreeNodeType NodeType { get; }
    public uint Position { get; }
    public bool HasValue { get; }
    public T? Value { get; }
    public uint? StemId { get; }
    public Dictionary<uint, uint> BranchPositions { get; }

    public TreeNodeTopologyCache(ITreeNode<T> node)
    {
        Id = node.Id;
        NodeType = node.NodeType;
        Position = node.Position;
        HasValue = node.HasValue;
        if (HasValue) { Value = node.GetValue(); }
        StemId = node.HasStem ? node.GetStem().Id : null;
        BranchPositions = new Dictionary<uint, uint>();
        if (node.HasBranch)
            foreach (var b in node.GetBranches())
                BranchPositions[b.Id] = b.Position;
    }

}