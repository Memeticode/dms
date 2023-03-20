
namespace dms.DataTree;



public class TreeGraphException : Exception
{
    public TreeGraphException() : base() { }
    public TreeGraphException(string message) : base(message) { }
    public TreeGraphException(string message, Exception innerException) : base(message, innerException) { }
}

public class TreeGraph<T> : ITreeGraph<T>
{
    // Static fields
    public static uint MaxDepth = 1024;
    public static uint BasePosition = default(uint);


    ///////////////////////////////////////
    // Protected (functionality implementation)
    ///////////////////////////////////////

    // Backing fields
    protected internal IdFactory _idFactory { get; }
    protected internal uint _rootId { get; set; }
    protected internal IDictionary<uint, ITreeNode<T>> _nodeMap { get; set; }
    protected internal IDictionary<uint, uint> _stemMap { get; set; }
    protected internal IDictionary<uint, HashSet<uint>> _branchMap { get; set; }
    protected internal IDictionary<uint, uint> _positionMap { get; set; }
    protected internal IDictionary<uint, T> _valueMap { get; set; }

    // Helper checks
    protected internal void CheckNodeExists(uint nodeId)
    {
        if (!_nodeMap.ContainsKey(nodeId))
            throw new ArgumentException($"Tree graph does not contain a reference to specified nodeId {nodeId}.");
    }
    protected internal void CheckNodeIsFloating(uint nodeId)
    {
        if (nodeId != _rootId && !_stemMap.ContainsKey(nodeId))
            throw new ArgumentException($"NodeId {nodeId} is not a floating node (it has a stem specified but is not the root).");
    }
    protected internal void CheckCanPlaceNodeAt(uint nodeId, uint position)
    {
        uint branchCount = _branchMap.TryGetValue(nodeId, out HashSet<uint>? branches) ? (uint)branches.Count() : 0;
        if (position > branchCount)
            throw new InvalidOperationException($"Specified branch position {position} is invalid for stem node {nodeId} because the node only has {branchCount} branches (branch position is zero-based.");
    }
    protected internal void CheckNodeHasStem(uint nodeId)
    {
        if (!_stemMap.ContainsKey(nodeId))
            throw new ArgumentException($"Tree graph does not have a stem node specified for nodeId {nodeId}.");
    }
    protected internal void CheckNodeIsNotRoot(uint nodeId)
    {
        if (nodeId == _rootId)
            throw new ArgumentException($"NodeId {nodeId} is the root node.");
    }
    protected internal void CheckNodeIsBranchOf(uint nodeId, uint stemId)
    {
        if (!_branchMap.ContainsKey(stemId))
            throw new ArgumentException($"Tree graph does not have any branch nodes specified for nodeId {stemId}.");
        if (!_branchMap[stemId].Contains(nodeId))
            throw new ArgumentException($"NodeId {nodeId} is not a branch of stem nodeId {stemId}.");
    }
    protected internal void CheckNodeHasPosition(uint nodeId)
    {
        if (!_positionMap.ContainsKey(nodeId))
            throw new ArgumentException($"Tree graph does not have a branch position specified for nodeId {nodeId}.");
    }
    
    // Validations
    protected internal void ValidateNodeBranchPositions(uint nodeId)
    {
        if (!_branchMap.ContainsKey(nodeId)) 
            return;
        
        var branchIds = _branchMap[nodeId];

        if (!branchIds.Any()) 
            return;

        int branchCount = branchIds.Count();
        uint[] branchPositions = branchIds.Select(b => _positionMap[b]).ToArray();

        if (branchCount != branchPositions.Length)
            throw new TreeGraphException($"Tree graph does not specify a position for all branches nodes of nodeId {nodeId}.");

        if (branchPositions.ToLookup(p => p).Where(group => group.Count() > 1).Any())
            throw new TreeGraphException($"Tree graph branch nodes for nodeId {nodeId} have duplicate positions. Duplicate positions: {string.Join(',', branchPositions.ToLookup(p => p).Where(group => group.Count() > 1))}.");

        uint sumPositions = 0;
        for (int i = 0; i < branchPositions.Length; i++)
            sumPositions += branchPositions[i];

        if (sumPositions != Enumerable.Range(1, branchCount - 1).Sum())
            throw new TreeGraphException($"Tree graph branch nodes for nodeId {nodeId} are not a sequence of consecutive unsigned integers starting with {BasePosition}. Branch positions are: {string.Join(',', branchPositions.OrderBy(p => p))}.");

    }
    protected internal void ValidatePathMatchesExpected(ITreePath expectedPath, ITreePath instancePath)
    {
        if (!instancePath.Equals(expectedPath))
            throw new TreeGraphException($"Expected path {expectedPath.String} but instead got path {instancePath.String}.");
    }

    // Create nodes
    protected internal ITreeNode<T> CreateFloatingNode()
    {
        try
        {
            var node = new TreeNode<T>(this, _idFactory.NewId());
            
            if (_nodeMap.ContainsKey(node.Id)) 
                throw new InvalidDataException($"The new node was assigned an id ({node.Id}) which already exists in the tree graph.");

            _nodeMap.Add(node.Id, node);
            return node;
        }
        catch (Exception ex)
        {
            throw new TreeGraphException($"Encountered error when attempting to create tree graph node.", ex);
        }
    }
    protected internal ITreeNode<T> CreateBranchAt(uint nodeId, uint position)
    {
        var branchNode = CreateFloatingNode();
        try
        {
            SetBranchAt(nodeId, branchNode.Id, position);
            return branchNode;
        }
        catch (Exception ex)
        {
            DeleteNode(branchNode.Id);
            throw new TreeGraphException($"Error when attempting to create branch at position {position} for node id {nodeId}.", ex);
        }
        // consider adding path match check
    }

    // Detach and delete nodes
    protected internal void DetachNode(uint nodeId) 
    {
        CheckNodeExists(nodeId);
        CheckNodeHasStem(nodeId);

        var stemId = _stemMap[nodeId];
        CheckNodeIsBranchOf(nodeId, stemId);

        try
        {
            if (_positionMap.TryGetValue(nodeId, out uint position))
            {
                _positionMap.Remove(nodeId);
                DecrementBranchPositionsStartingAt(stemId, position);
            }
            _stemMap.Remove(nodeId);

        }
        catch (Exception ex)
        {
            throw new TreeGraphException($"Encountered error when attempting to detach nodeId {nodeId} from stem nodeId {stemId}.", ex);
        }

    }
    protected internal void DeleteNode(uint nodeId, bool deleteDescendants)
    {
        DetachNode(nodeId);
        try
        {
            if (_branchMap.ContainsKey(nodeId))
            {
                if (_branchMap[nodeId].Any())
                    if (!deleteDescendants)
                        throw new InvalidOperationException($"Node {nodeId} has {_branchMap[nodeId].Count()} branches and cannot be deleted.");
                    else
                        foreach (var branchId in _branchMap[nodeId])
                            DeleteNode(branchId, deleteDescendants);

                _branchMap.Remove(nodeId);
            }

            if (_valueMap.ContainsKey(nodeId))
                _valueMap.Remove(nodeId);

            _nodeMap.Remove(nodeId);
        }
        catch (Exception ex)
        {
            throw new TreeGraphException($"Encountered error when attempting to delete nodeId {nodeId}.", ex);
        }
    }
    protected internal void DeleteNode(uint nodeId) => DeleteNode(nodeId, false);

    // Set floating node graph position
    protected internal void SetBranchAt(uint stemId, uint branchId, uint position)
    {

        CheckNodeExists(stemId);
        CheckNodeExists(branchId);
        CheckNodeIsFloating(branchId);
        CheckCanPlaceNodeAt(stemId, position);

        try
        {
            if (!_branchMap.ContainsKey(stemId))
                _branchMap[stemId] = new HashSet<uint>();

            var branchIds = _branchMap[stemId];

            // If node is not being added to last position, update subsequent peer node positions
            if (position < branchIds.Count)
                IncrementBranchPositionsStartingAt(stemId, position);

            _branchMap[stemId].Add(branchId);
            _stemMap[branchId] = stemId;
            _positionMap[branchId] = position;

        }
        catch (Exception ex)
        {
            throw new TreeGraphException($"Encountered error when setting nodeId {branchId} as a branch of nodeId {stemId} at position {position}.", ex);
        }

        ValidateNodeBranchPositions(stemId);
        ValidatePathMatchesExpected(new TreePath(_nodeMap[stemId].GetPath().List.Append(position)), GetNode(branchId).GetPath());
    }
    protected internal void SetBranchAt(uint stemId, uint branchId, int position) => SetBranchAt(stemId, branchId, (uint)position);
    protected internal void SetBranch(uint stemId, uint branchId) => SetBranchAt(stemId, branchId, GetNode(stemId).GetBranches().Length);
    protected internal void SetFirstBranch(uint stemId, uint branchId) => SetBranchAt(stemId, branchId, 0);
    protected internal void SetLastBranch(uint stemId, uint branchId) => SetBranchAt(stemId, branchId, GetNode(stemId).GetBranches().Length);
    protected internal void SetPrevPeer(uint nodeId, uint peerId) => SetBranchAt(GetNode(nodeId).GetStem().Id, peerId, GetNode(peerId).Position);
    protected internal void SetNextPeer(uint nodeId, uint peerId) => SetBranchAt(GetNode(nodeId).GetStem().Id, peerId, GetNode(peerId).Position + 1);

    // Update node positions after change
    protected internal void IncrementBranchPositionsStartingAt(uint nodeId, uint position)
    {
        foreach (uint branchId in _branchMap[nodeId])
            if (_positionMap[branchId] >= position)
                _positionMap[branchId] += 1;
    }
    protected internal void DecrementBranchPositionsStartingAt(uint nodeId, uint position)
    {
        foreach (uint branchId in _branchMap[nodeId])
            if (_positionMap[branchId] >= position)
                _positionMap[branchId] -= 1;
    }

    // Node value operations
    protected internal bool NodeHasValue(uint nodeId)
    {
        CheckNodeExists(nodeId);
        return _valueMap.ContainsKey(nodeId);
    }
    protected internal T GetNodeValue(uint nodeId)
    {
        CheckNodeExists(nodeId);
        if (_valueMap.TryGetValue(nodeId, out T? val)) return val;
        throw new TreeGraphException($"Tree does not store a value for node id {nodeId}.");
    }
    protected internal void SetNodeValue(uint nodeId, T value) 
    {
        CheckNodeExists(nodeId);
        _valueMap[nodeId] = value;
    }
    protected internal void DeleteNodeValue(uint nodeId)
    {
        CheckNodeExists(nodeId);
        _valueMap.Remove(nodeId);
    }

    // Manipulate node positions
    protected internal void Swap(uint node1, uint node2) => throw new NotImplementedException();
    protected internal void MoveToPosition(uint nodeId, uint position) => throw new NotImplementedException();
    protected internal void MoveToPosition(uint nodeId, int position) => throw new NotImplementedException();
    protected internal void MoveToStem(uint nodeId, uint stemId) => throw new NotImplementedException();
    protected internal void MoveToStem(uint nodeId, uint stemId, uint position) => throw new NotImplementedException();
    protected internal void MoveBefore(uint nodeId, uint beforeNodeId) => throw new NotImplementedException();
    protected internal void MoveAfter(uint nodeId, uint afterNodeId) => throw new NotImplementedException();
    protected internal void MoveToStart(uint nodeId) => throw new NotImplementedException();
    protected internal void MoveToEnd(uint nodeId) => throw new NotImplementedException();


    ///////////////////////////////////////
    // Constructor
    ///////////////////////////////////////

    public TreeGraph()
    {
        _idFactory = new IdFactory();

        _nodeMap = new Dictionary<uint, ITreeNode<T>>();
        _stemMap = new Dictionary<uint, uint>();
        _branchMap = new Dictionary<uint, HashSet<uint>>();
        _positionMap = new Dictionary<uint, uint>();
        _valueMap = new Dictionary<uint, T>();

        _rootId = CreateFloatingNode().Id;
        _positionMap.Add(_rootId, TreeGraph<T>.BasePosition);
    }


    ///////////////////////////////////////
    // ITreeGraph (interface implementation)
    ///////////////////////////////////////

    public IReadOnlyDictionary<uint, ITreeNode<T>> NodeMap => new ReadOnlyDictionary<uint, ITreeNode<T>>(_nodeMap);
    public IReadOnlyDictionary<uint, uint> StemMap => new ReadOnlyDictionary<uint, uint>(_stemMap);
    public IReadOnlyDictionary<uint, HashSet<uint>> BranchMap => new ReadOnlyDictionary<uint, HashSet<uint>>(_branchMap);
    public IReadOnlyDictionary<uint, uint> PositionMap => new ReadOnlyDictionary<uint, uint>(_positionMap);
    public IReadOnlyDictionary<uint, T> ValueMap => new ReadOnlyDictionary<uint, T>(_valueMap);

    public ITreeNode<T> Root => GetNode(_rootId);

    public uint GetHeight() => _nodeMap.Values.Max(n => n.GetDepth());
    public uint GetNodeCount() => (uint)_nodeMap.Count;

    //public uint GetFloatingNodeCount() => (uint)_nodeMap.Where(kv => !_stemMap.ContainsKey(kv.Key) && kv.Key != _rootId).Count();
    //public IEnumerable<ITreeNode<T>> GetFloatingNodes() => _nodeMap.Where(kv => !_stemMap.ContainsKey(kv.Key) && kv.Key != _rootId).Select(kv => kv.Value);
    //public IEnumerable<ITreeNode<T>> EnumerateNodes(); // dfs
    //public IEnumerable<ITreeNode<T>> EnumerateNodes(IEnumerable<uint> path);

    public void Validate() => throw new NotImplementedException();

    // Access nodes
    public ITreeNode<T> GetNode(int id) => GetNode((uint)id);
    public ITreeNode<T> GetNode(uint id)
    {
        try
        {
            if (!_nodeMap.ContainsKey(id)) throw new ArgumentException($"Tree does not contain node with id ({id}).");
            return _nodeMap[id];
        }
        catch (Exception ex)
        {
            throw new TreeGraphException($"Unable to get node with id {id}.", ex);
        }
    }

    public ITreeNode<T> GetNodeByPath(ITreePath path)
    {
        if (path is null) return Root;
        if (path.Length == 0) return Root;

        if (path.Length > MaxDepth)
            throw new TreeGraphException($"Specified path length ({path.Length}) is greater than graph's maximum allowed depth ({TreeGraph<T>.MaxDepth}).");

        var node = Root;
        for (int pathIdx = 0; pathIdx < path.Length; pathIdx++)
        {
            uint positionIdx = path.List[pathIdx];
            if (!node.HasBranch)
                throw new TreeGraphException($"Graph does not contain specified path ({path.String}). \r\nNo branches found at node with subpath ({node.GetPath().String}).");
            var branches = node.GetBranches();
            if (branches.Length < positionIdx)
                throw new InvalidOperationException($"Path index value at position {positionIdx} ({pathIdx}) is invalid. Node only has {branches.Length} branches.");
            node = branches[positionIdx];
        }
        return node;
    }
    public ITreeNode<T> GetNodeByPath(string path) => GetNodeByPath(new TreePath(path));
    public ITreeNode<T> GetNodeByPath(IEnumerable<uint> path) => GetNodeByPath(new TreePath(path));
    public ITreeNode<T> GetNodeByPath(IEnumerable<int> path) => GetNodeByPath(new TreePath(path));
    

    // Create nodes
    public ITreeNode<T> CreateBranchAt(ITreeNode<T> node, uint position)
    {
        var newNode = CreateFloatingNode();
        SetBranchAt(node.Id, newNode.Id, position);
        return newNode;
    }
    public ITreeNode<T> CreateBranchAt(ITreeNode<T> node, int position) => CreateBranchAt(node, (uint)position);
    public ITreeNode<T> CreateBranch(ITreeNode<T> node) => CreateBranchAt(node, (uint)node.GetBranches().Length);
    public ITreeNode<T> CreatePrevPeer(ITreeNode<T> node)
    {
        var stem = node.GetStem();
        var newNode = CreateFloatingNode();
        SetBranchAt(stem.Id, newNode.Id, node.Position);
        return newNode;
    }
    public ITreeNode<T> CreateNextPeer(ITreeNode<T> node)
    {
        var stem = node.GetStem();
        var newNode = CreateFloatingNode();
        SetBranchAt(stem.Id, newNode.Id, node.Position + 1);
        return newNode;
    }


    // Delete nodes (can't delete node if it has any branches)
    public void DeleteNode(ITreeNode<T> node) => DeleteNode(node.Id);
    public void DeleteNode(ITreeNode<T> node, bool deleteDescendants) => DeleteNode(node.Id, deleteDescendants);


    // Node value operations
    public bool NodeHasValue(ITreeNode<T> node) => NodeHasValue(node.Id);
    public void SetNodeValue(ITreeNode<T> node, T value) => SetNodeValue(node.Id, value);
    public T GetNodeValue(ITreeNode<T> node) => GetNodeValue(node.Id);
    public void DeleteNodeValue(ITreeNode<T> node) => DeleteNodeValue(node.Id);


    // Manipulate node positions
    public void Swap(ITreeNode<T> node1, ITreeNode<T> node2) => Swap(node1.Id, node2.Id);
    public void MoveToPosition(ITreeNode<T> node, uint position) => MoveToPosition(node.Id, position);
    public void MoveToPosition(ITreeNode<T> node, int position) => MoveToPosition(node.Id, (uint)position);
    public void MoveToStem(ITreeNode<T> node, ITreeNode<T> stem) => MoveToStem(node.Id, stem.Id);
    public void MoveToStem(ITreeNode<T> node, ITreeNode<T> stem, uint position) => MoveToStem(node.Id, stem.Id, position);
    public void MoveToStem(ITreeNode<T> node, ITreeNode<T> stem, int position) => MoveToStem(node.Id, stem.Id, (uint)position);
    public void MoveBefore(ITreeNode<T> node, ITreeNode<T> beforeNode) => MoveBefore(node.Id, beforeNode.Id);
    public void MoveAfter(ITreeNode<T> node, ITreeNode<T> afterNode) => MoveBefore(node.Id, afterNode.Id);
    public void MoveToStart(ITreeNode<T> node) => MoveToStart(node.Id);
    public void MoveToEnd(ITreeNode<T> node) => MoveToEnd(node.Id);


}



//	public IIdFactory<T> NodeIdFactory { get; }
//}

