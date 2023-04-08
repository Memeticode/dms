
namespace dms.TreeGraph;



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

    // Constructor
    public TreeGraph()
    {
        // Node id factory produces consecutive uint ids
        _idFactory = new IdFactory();

        _nodeMap = new Dictionary<uint, ITreeNode<T>>();
        _stemMap = new Dictionary<uint, uint>();
        _branchMap = new Dictionary<uint, HashSet<uint>>();
        _positionMap = new Dictionary<uint, uint>();
        _valueMap = new Dictionary<uint, T?>();

        // Floating node has 
        _rootId = CreateFloatingNode().Id;
    }


    // ITreeGraph

    ///////////////////////////////////////
    // Store nodes and track node relationships
    ///////////////////////////////////////
    public IReadOnlyDictionary<uint, ITreeNode<T>> NodeMap => new ReadOnlyDictionary<uint, ITreeNode<T>>(_nodeMap);
    public IReadOnlyDictionary<uint, uint> StemMap => new ReadOnlyDictionary<uint, uint>(_stemMap);
    public IReadOnlyDictionary<uint, HashSet<uint>> BranchMap => new ReadOnlyDictionary<uint, HashSet<uint>>(_branchMap);
    public IReadOnlyDictionary<uint, uint> PositionMap => new ReadOnlyDictionary<uint, uint>(_positionMap);
    public IReadOnlyDictionary<uint, T?> ValueMap => new ReadOnlyDictionary<uint, T?>(_valueMap);


    /// <summary>
    /// Root is the root node of the graph; there must be one and only one root.
    /// </summary>
    public ITreeNode<T> Root => GetNode(_rootId);

    /// <summary>
    /// GetHeight returns height of tree graph (if tree has only one node, height equals 0).
    /// </summary>
    public uint GetHeight() //=> _nodeMap.Values.Select(n => n.GetPath().Length).Max();
    {
        var paths = _nodeMap.Values.Select(n => n.GetPath().Length);
        uint m = paths.Max();
        return paths.Max();
        //_nodeMap.Values.Max(n => n.GetPath().Length);
    }


    /// <summary>
    /// GetNodeCount returns total number of nodes in tree graph.
    /// </summary>
    public uint GetNodeCount() => (uint)_nodeMap.Count;

    //public uint GetFloatingNodeCount() => (uint)_nodeMap.Where(kv => !_stemMap.ContainsKey(kv.Key) && kv.Key != _rootId).Count();
    //public IEnumerable<ITreeNode<T>> GetFloatingNodes() => _nodeMap.Where(kv => !_stemMap.ContainsKey(kv.Key) && kv.Key != _rootId).Select(kv => kv.Value);
    //public IEnumerable<ITreeNode<T>> EnumerateNodes(); // dfs
    //public IEnumerable<ITreeNode<T>> EnumerateNodes(IEnumerable<uint> path);


    /// <summary>
    /// Validate can be run to confirm instance is in a valid state for operations.
    /// </summary>
    public void Validate() 
    {
        ValidateRootNode();
        ValidateNodeMap();
        ValidateValueMap();
        ValidateStemBranchMaps();
        ValidatePositionMap();
    }



    // Access nodes 

    /// <summary>
    /// GetNode returns TreeNode with specified id (uint or int).
    /// </summary>
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

    /// <summary>
    /// GetNodeByPath returns TreeNode with specified path (ITreePath, string, or uint/int enumerable).
    /// Will error if invalid path is specified.
    /// </summary>
    public ITreeNode<T> GetNodeByPath(ITreePath path)
    {
        if (path is null) return Root;
        if (path.Length == 0) return Root;

        if (path.Length > MaxDepth)
            throw new TreeGraphException($"Specified path length ({path.Length}) is greater than graph's maximum allowed depth ({TreeGraph<T>.MaxDepth}).");

        var node = Root;
        for (int pathIdx = 0; pathIdx < path.Length; pathIdx++)
        {
            uint positionIdx = path[pathIdx];
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

    // Create & destroy nodes 

    /// <summary>
    /// CreateBranch generates a new branch node for specified stem node.
    /// </summary>
    public ITreeNode<T> CreateBranch(ITreeNode<T> node) => CreateBranchAt(node, (uint)node.GetBranches().Count());

    /// <summary>
    /// CreateBranchAt generates a new branch node for specified stem node at specified position (uint or int).
    /// </summary>
    public ITreeNode<T> CreateBranchAt(ITreeNode<T> node, uint position)
    {
        var newNode = CreateFloatingNode();
        AttachBranchAt(newNode.Id, node.Id, position);
        return newNode;
    }
    public ITreeNode<T> CreateBranchAt(ITreeNode<T> node, int position) => CreateBranchAt(node, (uint)position);


    /// <summary>
    /// CreatePrevPeer generates a new peer node before the specified node.
    /// </summary>
    public ITreeNode<T> CreatePrevPeer(ITreeNode<T> node)
    {
        var stem = node.GetStem();
        var newNode = CreateFloatingNode();
        AttachBranchAt(newNode.Id, stem.Id, node.Position);
        return newNode;
    }

    /// <summary>
    /// CreateNextPeer generates a new peer node after the specified node.
    /// </summary>
    public ITreeNode<T> CreateNextPeer(ITreeNode<T> node)
    {
        var stem = node.GetStem();
        var newNode = CreateFloatingNode();
        AttachBranchAt(newNode.Id, stem.Id, node.Position + 1);
        return newNode;
    }


    /// <summary>
    /// DeleteNode removes a node from the graph (descendants optional).
    /// Will error by default if specified node has any descendants.
    /// To delete descendants, set optional parameter deleteDescendants to true.
    /// </summary>
    public void DeleteNode(ITreeNode<T> node) => DeleteNode(node.Id);
    public void DeleteNode(ITreeNode<T> node, bool deleteDescendants) => DeleteNode(node.Id, deleteDescendants);


    // Node value operations

    /// <summary>
    /// NodeHasValue returns true if the tree has stored value for specified node (even if value is null).
    /// </summary>
    public bool NodeHasValue(ITreeNode<T> node) => NodeHasValue(node.Id);
    
    /// <summary>
    /// SetNodeValue stores specified node value in tree.
    /// </summary>
    public void SetNodeValue(ITreeNode<T> node, T? value) => SetNodeValue(node.Id, value);
    
    /// <summary>
    /// GetNodeValue returns the stored value for specified node.
    /// Throws error if node does not have a stored value.
    /// </summary>
    public T? GetNodeValue(ITreeNode<T> node) => GetNodeValue(node.Id);
    
    /// <summary>
    /// DeleteNodeValue deletes stored value if one is specified.
    /// Throws error if node does not have a stored value.
    /// </summary>
    public void DeleteNodeValue(ITreeNode<T> node) => DeleteNodeValue(node.Id);


    // Manipulate node positions
    
    /// <summary>
    /// SetNodePosition will place node at the specified position (uint or int) within its peer group.
    /// </summary>
    public void SetNodePosition(ITreeNode<T> node, uint position) => SetNodePosition(node.Id, position);
    public void SetNodePosition(ITreeNode<T> node, int position)
    {
        if (position < 0)
            throw new ArgumentException($"Input position ({position}) is invalid. Node position should always be positive integer (uint).");

        SetNodePosition(node.Id, (uint)position);
    }

    /// <summary>
    /// SetNodeFirstPosition will place node as the first node in its peer group.
    /// </summary>
    public void SetNodeFirstPosition(ITreeNode<T> node) => SetNodePosition(node.Id, 0);

    /// <summary>
    /// SetNodeLastPosition will place node as the last node in its peer group.
    /// </summary>
    public void SetNodeLastPosition(ITreeNode<T> node) => SetNodePosition(node.Id, (uint)(node.TryGetStem(out ITreeNode<T>? stem) ? stem.GetBranches().Count() : 0));

    /// <summary>
    /// MoveToStem will place node as branch of specified stemNode.
    /// Defaults to setting as last branch, but can optionally specify position (uint or int).
    /// </summary>
    public void SetNodeStem(ITreeNode<T> node, ITreeNode<T> stem)
    {
        if (_stemMap.TryGetValue(node.Id, out uint currStemId))
            if (stem.Id == currStemId)
                return; //throw new TreeGraphException($"Node {node.Id} is already a branch of node {stem.Id}.");
         SetNodeStemPosition(node.Id, stem.Id, (uint)stem.GetBranches().Count());
    }
    public void SetNodeStemPosition(ITreeNode<T> node, ITreeNode<T> stem, uint position) => SetNodeStemPosition(node.Id, stem.Id, position);
    public void SetNodeStemPosition(ITreeNode<T> node, ITreeNode<T> stem, int position)
    {
        if (position < 0)
            throw new ArgumentException($"Input position ({position}) is invalid. Node position should always be positive integer (uint).");
        SetNodeStemPosition(node.Id, stem.Id, (uint)position);
    }

    /// <summary>
    /// MoveBefore will place node as the previous peer of beforeNode.
    /// </summary>
    public void MoveNodeBefore(ITreeNode<T> node, ITreeNode<T> beforeNode) => SetNodeStemPosition(node.Id, _stemMap[beforeNode.Id], _positionMap[beforeNode.Id]);

    /// <summary>
    /// MoveAfter will place node as the next peer of afterNode
    /// </summary>
    public void MoveNodeAfter(ITreeNode<T> node, ITreeNode<T> afterNode) => SetNodeStemPosition(node.Id, _stemMap[afterNode.Id], _positionMap[afterNode.Id]);

    /// <summary>
    /// Swap will replace position of node1 with node2, and vice-versa.
    /// </summary>
    public void SwapNodes(ITreeNode<T> node1, ITreeNode<T> node2) => SwapNodes(node1.Id, node2.Id);



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
    protected internal IDictionary<uint, T?> _valueMap { get; set; }

    // Helper checks
    protected internal void CheckNodeExists(uint nodeId)
    {
        if (!_nodeMap.ContainsKey(nodeId))
            throw new ArgumentException($"Tree graph does not contain a reference to specified nodeId {nodeId}.");
    }
    protected internal void CheckNodeIsFloating(uint nodeId)
    {
        if (nodeId != _rootId && _stemMap.ContainsKey(nodeId))
            throw new ArgumentException($"NodeId {nodeId} is not a floating node (it has a stem specified but is not the root).");
    }
    protected internal void CheckCanSetNodePosition(uint nodeId, uint position)
    {
        if (position == 0) return;

        uint branchCount = (uint)GetNode(nodeId).GetBranches().Count();
        if (branchCount == 0 || position >= branchCount)
            throw new InvalidOperationException($"Specified (zero-based) branch position {position} is invalid for stem node {nodeId} because the node only has {branchCount} branches.");
    }
    protected internal void CheckCanSetNodeStemPosition(uint nodeId, uint position)
    {
        if (position == 0) return;

        uint branchCount = (uint)GetNode(nodeId).GetBranches().Count();
        if (branchCount == 0 || position > branchCount)
            throw new InvalidOperationException($"Specified (zero-based) branch position {position} is invalid for stem node {nodeId} because the node only has {branchCount} branches.");
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
    protected internal void CheckNodeIsNotDescendantOf(uint nodeId, uint possibleAncestorId)
    {
        if (GetNode(nodeId).GetPath().IsDescendantOf(GetNode(possibleAncestorId).GetPath()))
            throw new InvalidOperationException($"Node {nodeId} is a descendant of node {possibleAncestorId}.");
    }

    // Node value operations
    protected internal bool NodeHasValue(uint nodeId)
    {
        CheckNodeExists(nodeId);
        return _valueMap.ContainsKey(nodeId);
    }
    protected internal T? GetNodeValue(uint nodeId)
    {
        CheckNodeExists(nodeId);

        if (_valueMap.TryGetValue(nodeId, out T? val))
            return val;

        throw new TreeGraphException($"Tree does not store a value for node id {nodeId}.");
    }
    protected internal void SetNodeValue(uint nodeId, T? value)
    {
        CheckNodeExists(nodeId);
        _valueMap[nodeId] = value;
    }
    protected internal void DeleteNodeValue(uint nodeId)
    {
        CheckNodeExists(nodeId);
        _valueMap.Remove(nodeId);
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
            _positionMap.Add(node.Id, BasePosition);
            return node;
        }
        catch (Exception ex)
        {
            throw new TreeGraphException($"Encountered error when attempting to create tree graph node.", ex);
        }
    }

    // Attach floating nodes to graph
    protected internal void AttachBranchAt(uint nodeId, uint stemId, uint position)
    {
        CheckNodeExists(stemId);
        CheckNodeExists(nodeId);
        CheckNodeIsFloating(nodeId);
        CheckCanSetNodeStemPosition(stemId, position);

        try
        {
            if (!_branchMap.ContainsKey(stemId))
                _branchMap[stemId] = new HashSet<uint>();

            var branchIds = _branchMap[stemId];

            // If node is not being added to last position, update subsequent peer node positions
            if (position < branchIds.Count)
                IncrementBranchPositionsStartingAt(stemId, position);

            _branchMap[stemId].Add(nodeId);
            _stemMap[nodeId] = stemId;
            _positionMap[nodeId] = position;

        }
        catch (Exception ex)
        {
            throw new TreeGraphException($"Encountered error when setting nodeId {nodeId} as a branch of nodeId {stemId} at position {position}.", ex);
        }
    }

    // Detach nodes
    protected internal void DetachNode(uint nodeId) 
    {
        CheckNodeExists(nodeId);
        CheckNodeHasStem(nodeId);

        var stemId = _stemMap[nodeId];
        CheckNodeIsBranchOf(nodeId, stemId);

        try
        {
            if (!_positionMap.TryGetValue(nodeId, out uint position))
                throw new TreeNodeException($"Node id {nodeId} does not have a position specified in positon map");

            DecrementBranchPositionsStartingAt(stemId, _positionMap[nodeId]);

            _branchMap[stemId].Remove(nodeId);
            _positionMap.Remove(nodeId);
            _stemMap.Remove(nodeId);

        }
        catch (Exception ex)
        {
            throw new TreeGraphException($"Encountered error when attempting to detach nodeId {nodeId} from stem nodeId {stemId}.", ex);
        }

    }

    // Delete nodes
    protected internal void DeleteNode(uint nodeId, bool deleteDescendants)
    {
        if (!deleteDescendants && GetNode(nodeId).HasBranch)
            throw new InvalidOperationException($"Node {nodeId} has {_branchMap[nodeId].Count()} branches and cannot be deleted.");

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


    // Set node position within peer group
    protected internal void SetNodePosition(uint nodeId, uint position, bool checkState = true)
    {
        if (checkState)
        {
            CheckNodeExists(nodeId);
            CheckNodeHasStem(nodeId);
            CheckNodeIsNotRoot(nodeId);
        }

        var stemId = _stemMap[nodeId];
        var cachePosition = _positionMap[nodeId];
        
        CheckCanSetNodePosition(stemId, position);
    
        if (cachePosition == position) return;

        bool moveForward = cachePosition < position;
        var cachePositions = new Dictionary<uint, uint>();

        // Cache positions of all nodes which will have position changed
        try
        {
            foreach (var branchId in _branchMap[stemId])
            {
                uint branchPosition = _positionMap[branchId];
                if (branchId == nodeId)
                {
                    cachePositions.Add(branchId, branchPosition);
                    _positionMap[branchId] = position;
                    continue;
                }

                if (moveForward 
                    && branchPosition > cachePosition
                    && branchPosition <= position)
                {
                    cachePositions.Add(branchId, branchPosition);
                    _positionMap[branchId]--;
                    continue;
                }

                if (!moveForward
                    && branchPosition >= position
                    && branchPosition < cachePosition)
                {
                    cachePositions.Add(branchId, branchPosition);
                    _positionMap[branchId]++;
                    continue;
                }
            }

        }
        catch (Exception ex)
        {
            // Revert changes on error
            foreach (var changedPosition in cachePositions)
            {
                _positionMap[changedPosition.Key] = _positionMap[changedPosition.Value];
            }

            throw new TreeGraphException($"Unable to to move nodes id {nodeId} to position {position}.", ex);
        }

        // Validate paths match expected after swap
        //var newPath = node.GetPath();
        //var expectedPath = stem.GetPath().GetBranchPath(position);
        //if (!newPath.Equals(expectedPath)) throw new InvalidOperationException($"Node path after moving position ({newPath.String}) does not expected path ({expectedPath.String}).");

    }
    protected internal void SetNodeStemPosition(uint nodeId, uint stemId, uint position)
    {
        CheckNodeExists(nodeId);
        CheckNodeIsNotRoot(nodeId);
        CheckNodeExists(stemId);

        if (_stemMap[nodeId] == stemId)
        {
            CheckCanSetNodePosition(stemId, position);
            SetNodePosition(nodeId, position, false);
        }
        else
        {
            CheckNodeIsNotDescendantOf(stemId, nodeId);
            CheckCanSetNodeStemPosition(stemId, position);
            DetachNode(nodeId);
            AttachBranchAt(nodeId, stemId, position);
        }

    }

    // Swap nodes
    protected internal void SwapNodes(uint nodeId1, uint nodeId2)
    {
        CheckNodeExists(nodeId1);
        CheckNodeExists(nodeId2);
        CheckNodeIsNotRoot(nodeId1);
        CheckNodeIsNotRoot(nodeId2);
        CheckNodeHasStem(nodeId1);
        CheckNodeHasStem(nodeId2);
        CheckNodeIsNotDescendantOf(nodeId1, nodeId2);
        CheckNodeIsNotDescendantOf(nodeId2, nodeId1);

        if (nodeId1 == nodeId2) return;

        var path1 = GetNode(nodeId1).GetPath();
        var path2 = GetNode(nodeId2).GetPath();
        if (path1.IsAncestorOf(path2)) throw new InvalidOperationException($"Unable to swap nodes because node 2 ({path2.String}) is a descendant of node 1 ({path1.String}).");
        if (path2.IsAncestorOf(path1)) throw new InvalidOperationException($"Unable to swap nodes because node 1 ({path1.String}) is a descendant of node 2 ({path2.String}).");

        uint stemId1 = _stemMap[nodeId1];
        uint stemId2 = _stemMap[nodeId2];

        uint position1 = _positionMap[nodeId1];
        uint position2 = _positionMap[nodeId2];

        try
        {
            // Swap nodes
            if (stemId1 != stemId2)
            {
                _branchMap[stemId1].Remove(nodeId1);
                _branchMap[stemId2].Remove(nodeId2);

                _branchMap[stemId1].Add(nodeId2);
                _branchMap[stemId2].Add(nodeId1);

                _stemMap[nodeId1] = stemId2;
                _stemMap[nodeId2] = stemId1;
            }

            _positionMap[nodeId1] = position2;
            _positionMap[nodeId2] = position1;
        }
        catch (Exception ex)
        {
            // Revert swap on error
            if (stemId1 != stemId2)
            {
                if (_branchMap[stemId1].Contains(nodeId2)) _branchMap[stemId1].Remove(nodeId2);
                if (_branchMap[stemId2].Contains(nodeId1)) _branchMap[stemId2].Remove(nodeId1);

                if (!_branchMap[stemId1].Contains(nodeId1)) _branchMap[stemId1].Add(nodeId1);
                if (!_branchMap[stemId2].Contains(nodeId2)) _branchMap[stemId2].Add(nodeId2);

                _stemMap[nodeId1] = stemId1;
                _stemMap[nodeId2] = stemId2;
            }

            _positionMap[nodeId1] = position1;
            _positionMap[nodeId2] = position2;

            throw new TreeGraphException($"Unable to swap positions of nodes ({nodeId1}) and ({nodeId2}).", ex);
        }

        // Validate paths match expected after swap
        var newPath1 = GetNode(nodeId1).GetPath();
        var newPath2 = GetNode(nodeId2).GetPath();
        if (!newPath1.Equals(path2)) throw new InvalidOperationException($"Node 1 path ({newPath1.String}) after swap does not match node 2 path before swap ({path2.String}).");
        if (!newPath2.Equals(path1)) throw new InvalidOperationException($"Node 2 path ({newPath2.String}) after swap does not match node 1 path before swap ({path1.String}).");

    }


    // Validations
    protected internal void ValidateRootNode()
    {
        if (Root is null)
            throw new TreeGraphException("Tree has null root node.");
        if (Root.NodeType != TreeNodeType.Root)
            throw new TreeGraphException($"Root node has node type {Root.NodeType}.");
    }
    protected internal void ValidateNodeMap()
    {
        uint nodeCount = GetNodeCount();
        if (nodeCount < 1) 
            throw new TreeGraphException("Tree has no nodes and expected to have at least one node (the root).");
    }
    protected internal void ValidateValueMap()
    {
        uint unmappedValueCount = (uint)_valueMap.Keys.Where(n => !_nodeMap.ContainsKey(n)).Count();
        if (unmappedValueCount > 0)
            throw new TreeGraphException($"There are {unmappedValueCount} nodes referenced in tree node value map and missing from tree node map.");
    }
    protected internal void ValidateStemBranchMaps()
    {
        uint nodeStemCount = (uint)_stemMap.Count();
        uint unmappedStemKeyCount = (uint)_stemMap.Keys.Where(n => !_nodeMap.ContainsKey(n)).Count();
        uint unmappedStemValueCount = (uint)_stemMap.Values.Where(n => !_nodeMap.ContainsKey(n)).Count();
        uint nodeBranchCount = (uint)_branchMap.Values.Sum(v => v.Count);
        uint unmappedBranchKeyCount = (uint)_branchMap.Keys.Where(n => !_nodeMap.ContainsKey(n)).Count();
        uint unmappedBranchValueCount = (uint)_branchMap.Values.SelectMany(b => b).Where(b => !_nodeMap.ContainsKey(b)).Count();
        uint duplicateBranchCount = (uint)_branchMap.Values.SelectMany(b => b).ToLookup(b => b).Where(group => group.Count() > 1).Count();

        if (nodeBranchCount != nodeStemCount)
            throw new TreeGraphException($"There are {nodeBranchCount} nodes referenced in branch map values and {nodeStemCount} nodes referenced in stem map keys. These values should be equal.");
        if (unmappedBranchKeyCount > 0)
            throw new TreeGraphException($"There are {unmappedBranchKeyCount} nodes referenced in branch map keys and missing from total node map.");
        if (unmappedBranchValueCount > 0)
            throw new TreeGraphException($"There are {unmappedBranchValueCount} nodes referenced in branch map values and missing from total node map.");
        if (duplicateBranchCount > 0)
            throw new TreeGraphException($"There are {duplicateBranchCount} duplicate nodes in branch map values. Each branch node id should only be included once, as branches can only have one stem.");
    }
    protected internal void ValidatePositionMap()
    {
        uint positionMapCount = (uint)_positionMap.Count();
        uint unmappedPositionCount = (uint)_stemMap.Keys.Where(n => !_nodeMap.ContainsKey(n)).Count();

        if (positionMapCount != GetNodeCount())
            throw new TreeGraphException($"Graph has {GetNodeCount()} nodes total and {positionMapCount} nodes referenced in position map. These values should be equal (each node should have a position specified).");
        if (unmappedPositionCount > 0)
            throw new TreeGraphException($"There are {unmappedPositionCount} nodes referenced in tree node position map and missing from tree node map.");

        foreach (var nodeId in _nodeMap.Keys)
            ValidateNodeBranchPositions(nodeId);
    }
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
    protected internal void ValidateNodePathMatchesExpected(ITreePath expectedPath, ITreePath instancePath)
    {
        if (!instancePath.Equals(expectedPath))
            throw new TreeGraphException($"Expected path {expectedPath.String} but instead got path {instancePath.String}.");
    }


}



//	public IIdFactory<T> NodeIdFactory { get; }
//}

