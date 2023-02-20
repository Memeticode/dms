<Query Kind="Program">
  <Namespace>System.Runtime.InteropServices.ComTypes</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

void Main()
{
	var tests = new TreeGraphTests();
	tests.RunAllTests();

}


#region TreeGraph Interfaces
/// <summary>
/// Node id-creating interface, used by tree
/// </summary>
public interface ITreeNodeIdFactory<T> where T : IEquatable<T>
{
	public T NewId();
}
/// <summary>
/// Node interface
/// </summary>
public interface ITreeNode<T, Tval> where T : IEquatable<T>
{
	public T Id { get; }
	public int Position { get; }
	public int Depth { get; }
	public IList<int> Path { get; }

	public T StemId { get; }
	public T[] BranchIds { get; }
	public T FirstBranchId { get; }
	public T LastBranchId { get; }

	public T[] SiblingIds { get; }
	public T PrevSiblingId { get; }
	public T NextSiblingId { get; }

	public bool HasPosition { get; }
	public bool HasStem { get; }
	public bool HasBranch { get; }
	public bool HasNextSibling { get; }
	public bool HasPrevSibling { get; }
	
	public bool HasValue { get; }
	public Tval GetValue();
	public void SetValue(Tval value);
	public void DeleteValue();
}
/// <summary>
/// Base tree graph interface
/// 1. Stores a collection of tree nodes
/// 2. Maintains and updates graph structure (node branch/stem relationships)
/// 3. Handles creating and destroying nodes
/// 4. Gets and sets node values
/// </summary>
public interface ITreeGraph<T, Tval> where T : IEquatable<T>
{	
	public ITreeNodeIdFactory<T> NodeIdFactory { get; } // used to create nodes

	public ITreeNode<T, Tval> Root { get; }
	public int NodeCount { get; }
	public int Height { get; }

	public IDictionary<T, ITreeNode<T, Tval>> NodeMap { get; }
	public IDictionary<T, T> StemMap { get; }
	public IDictionary<T, HashSet<T>> BranchMap { get; }
	public IDictionary<T, int> PositionMap { get; }

	public ITreeNode<T, Tval> CreateNode();
	public ITreeNode<T, Tval> CreateAndRegisterNode();
	public void RegisterNode(ITreeNode<T, Tval> node);

	public ITreeNode<T, Tval> GetNode(T id);
	public IEnumerable<ITreeNode<T, Tval>> EnumerateNodes();

	public void DeleteNode(T id);	// can only delete node if it has no branches
	public void AddBranch(T stemId, T branchId);
	public void RemoveBranch(T stemId, T branchId);
	public void SwapPosition(T firstNodeId, T secondNodeId);
	
	public void AddFirstBranch(T stemId, T branchId);
	public void AddLastBranch(T stemId, T branchId);
	public void AddBranchBefore(T beforeBranchId, T branchId);
	public void AddBranchAfter(T afterBranchId, T branchId);
	
	public bool NodeHasValue(T id);
	public Tval GetNodeValue(T id);
	public void SetNodeValue(T id, Tval value);
	public void DeleteNodeValue(T id);
	
	public void Validate();

	// sugar
	public void DeleteNode(ITreeNode<T, Tval> node);
	public void AddBranch(ITreeNode<T, Tval> stem, ITreeNode<T, Tval> branch);
	public void RemoveBranch(ITreeNode<T, Tval> stem, ITreeNode<T, Tval> branch);
	public void SwapPosition(ITreeNode<T, Tval> firstNode, ITreeNode<T, Tval> secondNode);
	public void AddFirstBranch(ITreeNode<T, Tval> stem, ITreeNode<T, Tval> branch);
	public void AddLastBranch(ITreeNode<T, Tval> stem, ITreeNode<T, Tval> branch);
	public void AddBranchBefore(ITreeNode<T, Tval> beforeBranch, ITreeNode<T, Tval> branch);
	public void AddBranchAfter(ITreeNode<T, Tval> afterBranch, ITreeNode<T, Tval> branch);
	public bool NodeHasValue(ITreeNode<T, Tval> node);
	public object GetNodeValue(ITreeNode<T, Tval> node);
	public void SetNodeValue(ITreeNode<T, Tval> node, Tval value);
	public void DeleteNodeValue(ITreeNode<T, Tval> node);

}
#endregion

#region TreeGraph Implementation
public class GuidFactory : ITreeNodeIdFactory<Guid>
{
	public Guid NewId() => Guid.NewGuid();
}
public class TreeNode<T, Tval> : ITreeNode<T, Tval> where T : IEquatable<T>
{
	protected ITreeGraph<T, Tval> Tree { get; }

	public T Id { get; }
	public IList<int> Path => GetPath().ToList();
	private IEnumerable<int> GetPath()
	{
		if (HasStem)
			return Tree.GetNode(StemId).Path.Append(Position);
		else
			return new List<int>() { Position };
	}

	public int Position => Tree.PositionMap.ContainsKey(Id) ? Tree.PositionMap[Id] : -1;
	public int Depth => Path.Count - 1;

	public T StemId => HasStem ? Tree.StemMap[Id] : default(T);
	public T[] BranchIds => HasBranch ? Tree.BranchMap[Id].OrderBy(b => Tree.PositionMap[b]).ToArray() : new T[] { };
	public T FirstBranchId => HasBranch ? BranchIds.First() : default(T);
	public T LastBranchId => HasBranch ? BranchIds.Last() : default(T);

	public T[] SiblingIds => HasStem ? Tree.BranchMap[StemId].Where(b => !(Tree.GetNode(b).Id.Equals(Id))).OrderBy(b => Tree.PositionMap[b]).ToArray() : new T[] { };
	public T PrevSiblingId => HasPrevSibling ? Tree.BranchMap[StemId].Where(b => Tree.PositionMap[b] < Position).OrderByDescending(b => Tree.PositionMap[b]).First() : default(T);
	public T NextSiblingId => HasNextSibling ? Tree.BranchMap[StemId].Where(b => Tree.PositionMap[b] > Position).OrderBy(b => Tree.PositionMap[b]).First() : default(T);

	public bool HasPosition => Tree.PositionMap.ContainsKey(Id);
	public bool HasStem => Tree.StemMap.ContainsKey(this.Id);
	public bool HasBranch => Tree.BranchMap.ContainsKey(this.Id);
	public bool HasNextSibling => Tree.BranchMap.ContainsKey(StemId) ? Tree.BranchMap[StemId].Where(b => Tree.PositionMap[b] > Position).Any() : false;
	public bool HasPrevSibling => Tree.BranchMap.ContainsKey(StemId) ? Tree.BranchMap[StemId].Where(b => Tree.PositionMap[b] < Position).Any() : false;

	public bool HasValue => Tree.NodeHasValue(Id);
	public Tval GetValue() => Tree.GetNodeValue(Id);
	public void SetValue(Tval value) => Tree.SetNodeValue(Id, value);
	public void DeleteValue() => Tree.DeleteNodeValue(Id);
	
	public TreeNode(ITreeGraph<T, Tval> tree)
	{
		Tree = tree;
		Id = Tree.NodeIdFactory.NewId();
	}
}
public class TreeGraph<T, Tval> : ITreeGraph<T, Tval> where T : IEquatable<T>
{
	public class TreeGraphException : Exception
	{
		public TreeGraphException() : base() { }
		public TreeGraphException(string message) : base(message) { }
		public TreeGraphException(string message, Exception innerException) : base(message, innerException) { }
	}
	
	protected static int _basePosition = 0;
	protected T _rootId { get; set; }
	protected IDictionary<T, ITreeNode<T, Tval>> _nodeMap { get; set; }
	protected IDictionary<T, T> _stemMap { get; set; }
	protected IDictionary<T, HashSet<T>> _branchMap { get; set; }
	protected IDictionary<T, int> _positionMap { get; set; }
	protected IDictionary<T, Tval> _valueMap { get; set; }

	public TreeGraph(ITreeNodeIdFactory<T> nodeIdFactory)
	{
		NodeIdFactory = nodeIdFactory;
		_stemMap = new Dictionary<T, T>();
		_branchMap = new Dictionary<T, HashSet<T>>();
		_nodeMap = new Dictionary<T, ITreeNode<T, Tval>>();
		_valueMap = new Dictionary<T, Tval>();
		_positionMap = new Dictionary<T, int>();
		
		_rootId = CreateAndRegisterNode().Id;
		_positionMap.Add(_rootId, _basePosition);
	}

	public ITreeNodeIdFactory<T> NodeIdFactory { get; }

	public ITreeNode<T, Tval> Root => _nodeMap.ContainsKey(_rootId) ? _nodeMap[_rootId] : null;
	public int NodeCount => _nodeMap.Count;
	public int Height => _nodeMap.Values.Max(n => n.Depth);

	public IDictionary<T, ITreeNode<T, Tval>> NodeMap => _nodeMap;
	public IDictionary<T, T> StemMap => _stemMap;
	public IDictionary<T, HashSet<T>> BranchMap => _branchMap;
	public IDictionary<T, int> PositionMap => _positionMap;
	public IDictionary<T, Tval> ValueMap => _valueMap;


	public ITreeNode<T, Tval> CreateNode() => new TreeNode<T, Tval>(this);
	public ITreeNode<T, Tval> CreateAndRegisterNode()
	{
		try
		{
			var node = CreateNode();
			RegisterNode(node);
			return node;
		}
		catch (Exception ex)
		{
			throw new TreeGraphException($"Unable to create and register node.", ex);
		}
	}
	public void RegisterNode(ITreeNode<T, Tval> node)
	{
		try 
		{
			if (_nodeMap.ContainsKey(node.Id)) throw new InvalidOperationException($"Tree already has node with id {node.Id}.");
			_nodeMap.Add(node.Id, node);
		}
		catch (Exception ex)
		{
			throw new TreeGraphException($"Unable to register node ({node.Id}).", ex);
		}
	}

	public ITreeNode<T, Tval> GetNode(T id)
	{
		try
		{
			if (!_nodeMap.ContainsKey(id)) throw new TreeGraphException($"Tree does not contain node with specified id ({id}).");
			return _nodeMap[id];
		}
		catch (Exception ex)
		{
			throw new TreeGraphException($"Unable to get node ({id}).", ex);
		}
	}
	
	public IEnumerable<ITreeNode<T, Tval>> EnumerateNodes()
	{
		foreach (T id in _EnumerateNodeIdsDfs(_rootId))
			yield return GetNode(id);		
	}
	private IEnumerable<T> _EnumerateNodeIdsDfs(T nodeId)
	{
		yield return nodeId;
		if (_branchMap.ContainsKey(nodeId))
			foreach (T id in _branchMap[nodeId])
				foreach (T eId in _EnumerateNodeIdsDfs(id))
					yield return eId;
	}
	
	
	public void DeleteNode(T id)
	{
		try
		{
			if (!_nodeMap.ContainsKey(id))
				throw new InvalidOperationException($"Tree does not contain node with specified id ({id}).");

			if (Root.Id.Equals(id))
				throw new InvalidOperationException("Can't delete root node.");

			if (_branchMap.ContainsKey(id))
				if (_branchMap[id].Any())
					throw new InvalidOperationException($"Node {id} has {_branchMap[id].Count()} items and cannot be deleted.");

			if (_valueMap.ContainsKey(id))
				throw new InvalidOperationException($"Node {id} has a value specified and cannot be deleted.");

			if (!_stemMap.ContainsKey(id))
				throw new InvalidOperationException("Can't delete node because graph is malformed - node does not have a stem id but is not the root node.");
			else if (!_branchMap.ContainsKey(_stemMap[id]))
				throw new InvalidOperationException("Can't delete node because graph is malformed - node's stem id does not have any associated branch ids.");
			else if (!_branchMap[_stemMap[id]].Contains(id))
				throw new InvalidOperationException("Can't delete node because graph is malformed - node stem's set of branch ids does not contain node's id.");

			// Clear node references to node from internal memory
			if (_branchMap.ContainsKey(id))
				_branchMap.Remove(id);
			RemoveBranch(_stemMap[id], id);  // this will also remove node id from _stemMap and _positionMap and update sibling positions				
			_nodeMap.Remove(id);
		}
		catch (Exception ex)
		{
			throw new TreeGraphException($"Unable to delete node ({id}).", ex);
		}
	}

	public void AddBranch(T stemId, T branchId)
	{
		try 
		{
			if (!_nodeMap.ContainsKey(stemId)) throw new InvalidOperationException($"Specified stem node id {stemId} not found in tree nodes.");
			if (!_nodeMap.ContainsKey(branchId)) throw new InvalidOperationException($"Specified branch node id {branchId} not found in tree nodes.");
			if (_positionMap.ContainsKey(branchId)) throw new InvalidOperationException($"Can't add branch id {branchId} because this id has a position specified in graph's position map.");
			if (_stemMap.ContainsKey(branchId)) throw new InvalidOperationException($"Branch id {branchId} already has a stem node associated (id {_stemMap[branchId]}).");
			if (_branchMap.TryGetValue(stemId, out HashSet<T> branches))
			{
				if (branches.Contains(branchId)) throw new InvalidOperationException($"Node {branchId} is already branch of node {stemId}.");
				else
				{
					_branchMap[stemId].Add(branchId);
					_stemMap[branchId] = stemId;
				}
			}
			else
			{
				_branchMap.Add(stemId, new HashSet<T>() { branchId });
				_stemMap[branchId] = stemId;
			}
			
			// Default to adding branch to last position
			int position = _branchMap[stemId].Count - 1;
			_positionMap.Add(branchId, position);
		}
		catch (Exception ex)
		{
			throw new TreeGraphException($"Unable to add node ({branchId}) as a branch of node ({stemId}).", ex);
		}
	}

	public void RemoveBranch(T stemId, T branchId)
	{
		try
		{
			if (!_nodeMap.ContainsKey(stemId)) throw new InvalidOperationException($"Specified stem node id {stemId} not found in tree nodes.");
			if (!_nodeMap.ContainsKey(branchId)) throw new InvalidOperationException($"Specified branch node id {branchId} not found in tree nodes.");

			if (_valueMap.ContainsKey(branchId))
				throw new InvalidOperationException($"Branch node {branchId} has a value specified and cannot be removed from stem {stemId}.");

			if (!_positionMap.ContainsKey(branchId))
				throw new InvalidOperationException($"Can't remove branch id {branchId} because node does not have a position specified in graph's position map.");

			if (!_stemMap.TryGetValue(branchId, out T specifiedStemId))
				throw new InvalidOperationException($"Branch id {branchId} does not have a stem id specified.");
			if (!stemId.Equals(specifiedStemId))
				throw new InvalidOperationException($"Branch id {branchId}'s specified stem id ({_stemMap[branchId]}) does not match input stem id ({_stemMap[branchId]}).");

			if (_branchMap.TryGetValue(stemId, out HashSet<T> branches))
				if (!branches.Contains(branchId)) throw new InvalidOperationException($"Node {branchId} is already branch of node {stemId}.");

			_stemMap.Remove(branchId);
			_branchMap[stemId].Remove(branchId);

			if (!_branchMap[stemId].Any())
				_branchMap.Remove(stemId);
			else
			{
				// decrement sibling positions
				int deletedPosition = _positionMap[branchId];
				_positionMap.Remove(branchId);
				foreach (T siblingId in _branchMap[stemId])
					if (_positionMap.ContainsKey(siblingId))
						if (_positionMap[siblingId] > deletedPosition)
							_positionMap[siblingId] -= 1;
			}
		}
		catch (Exception ex)
		{
			throw new TreeGraphException($"Unable to remove branch ({branchId}) from stem ({stemId}).", ex);
		}
	}
	
	public void SwapPosition(T firstNodeId, T secondNodeId)
	{
		try
		{
			if (!_nodeMap.ContainsKey(firstNodeId)) throw new InvalidOperationException($"Specified first node id {firstNodeId} not found in tree nodes.");
			if (!_nodeMap.ContainsKey(secondNodeId)) throw new InvalidOperationException($"Specified second branch node id {secondNodeId} not found in tree nodes.");
			if (_rootId.Equals(firstNodeId) || _rootId.Equals(secondNodeId)) throw new InvalidOperationException("Can't swap position of root node.");

			T firstStemId = _stemMap[firstNodeId];
			T secondStemId = _stemMap[secondNodeId];

			int firstStemPosition = _positionMap[firstNodeId];
			int secondStemPosition = _positionMap[secondNodeId];

			_stemMap[firstNodeId] = secondStemId;
			_stemMap[secondNodeId] = firstStemId;

			_positionMap[firstNodeId] = secondStemPosition;
			_positionMap[secondNodeId] = firstStemPosition;
		}
		catch (Exception ex)
		{
			throw new TreeGraphException($"Unable to swap positions of nodes ({firstNodeId}) and ({secondNodeId}).", ex);
		}
	}
	
	public void AddFirstBranch(T stemId, T branchId)
	{
		try 
		{
			if (_positionMap.ContainsKey(branchId))
				throw new InvalidOperationException("Can't add branch because branch node already has a position specified in graph's position map.");

			AddBranch(stemId, branchId);    // adds branch to end

			foreach (T siblingId in _branchMap[stemId])
				if (branchId.Equals(siblingId))
					_positionMap[siblingId] = _basePosition;
				else
					_positionMap[siblingId] += 1;
		}
		catch (Exception ex) 
		{
			throw new TreeGraphException($"Unable to add node ({branchId}) as first branch of ({stemId}).", ex);
		}
	}

	public void AddLastBranch(T stemId, T branchId)
	{
		try
		{
			AddBranch(stemId, branchId);
		}
		catch (Exception ex)
		{
			throw new TreeGraphException($"Unable to add node ({branchId}) as last branch of ({stemId}).", ex);
		}
	}

	public void AddBranchBefore(T beforeBranchId, T branchId)
	{
		try
		{
			if (!_stemMap.ContainsKey(beforeBranchId))
				throw new InvalidOperationException("Can't add node before specified branch because after branch node does not have a stem id specified in graph's stem map.");
			if (!_branchMap.ContainsKey(_stemMap[beforeBranchId]))
				throw new InvalidOperationException("Can't add node before specified branch because graph does not contain any branches for specified branch's stem (malformed graph).");
			if (!_branchMap[_stemMap[beforeBranchId]].Contains(beforeBranchId))
				throw new InvalidOperationException("Can't add node before specified branch because graph's branch nodes for specified branch's stem does not contain specified branch (malformed graph).");
			if (!_positionMap.ContainsKey(beforeBranchId))
				throw new InvalidOperationException("Can't add branch before specified branch because after branch node does not have a position specified in graph's position map.");
			if (_positionMap.ContainsKey(branchId))
				throw new InvalidOperationException("Can't add branch because branch node already has a position specified in graph's position map.");

			T stemId = _stemMap[beforeBranchId];
			int beforePosition = _positionMap[beforeBranchId];

			AddBranch(stemId, branchId);
			foreach (T siblingId in _branchMap[stemId])
			{
				if (branchId.Equals(siblingId))
					_positionMap[branchId] = beforePosition;
				else if (_positionMap.ContainsKey(siblingId))
					if (_positionMap[siblingId] >= beforePosition)
						_positionMap[siblingId] += 1;
			}
		}
		catch (Exception ex)
		{
			throw new TreeGraphException($"Unable to add branch node ({branchId}) before branch ({beforeBranchId}).", ex);
		}
	}

	public void AddBranchAfter(T afterBranchId, T branchId)
	{
		try
		{
			if (!_stemMap.ContainsKey(afterBranchId))
				throw new InvalidOperationException("Can't add node after specified branch because after branch node does not have a stem id specified in graph's stem map.");
			if (!_branchMap.ContainsKey(_stemMap[afterBranchId]))
				throw new InvalidOperationException("Can't add node after specified branch because graph does not contain any branches for specified branch's stem (malformed graph).");
			if (!_branchMap[_stemMap[afterBranchId]].Contains(afterBranchId))
				throw new InvalidOperationException("Can't add node after specified branch because graph's branch nodes for specified branch's stem does not contain specified branch (malformed graph).");
			if (!_positionMap.ContainsKey(afterBranchId))
				throw new InvalidOperationException("Can't add branch after specified branch because after branch node does not have a position specified in graph's position map.");
			if (_positionMap.ContainsKey(branchId))
				throw new InvalidOperationException("Can't add branch because branch node already has a position specified in graph's position map.");

			T stemId = _stemMap[afterBranchId];
			int afterPosition = _positionMap[afterBranchId];
			
			AddBranch(stemId, branchId);
			foreach (T siblingId in _branchMap[stemId])
			{
				if (branchId.Equals(siblingId))
					_positionMap[branchId] = afterPosition + 1;
				else if (_positionMap.ContainsKey(siblingId))
					if (_positionMap[siblingId] > afterPosition)
						_positionMap[siblingId] += 1;
			}
		}
		catch (Exception ex)
		{
			throw new TreeGraphException($"Unable to add branch node ({branchId}) after branch ({afterBranchId}).", ex);
		}
	}

	public bool NodeHasValue(T id) => _valueMap.ContainsKey(id);
	public Tval GetNodeValue(T id)
	{
		try
		{
			return _valueMap[id];
		}
		catch (Exception ex)
		{
			throw new TreeGraphException($"Unable to get value for node id ({id}).", ex);
		}
	}
	public void SetNodeValue(T id, Tval value)
	{
		try
		{
			_valueMap[id] = value;
		}
		catch (Exception ex)
		{
			throw new TreeGraphException($"Unable to set value for node id ({id}).", ex);
		}
	}
	public void DeleteNodeValue(T id)
	{
		try
		{
			_valueMap.Remove(id);
		}
		catch (Exception ex)
		{
			throw new TreeGraphException($"Unable to set value for node id ({id}).", ex);
		}
	}
	
	public void Validate() => throw new NotImplementedException();
	
	// sugar
	public void DeleteNode(ITreeNode<T, Tval> node) => DeleteNode(node.Id);
	public void AddBranch(ITreeNode<T, Tval> stem, ITreeNode<T, Tval> branch) => AddBranch(stem.Id, branch.Id);
	public void RemoveBranch(ITreeNode<T, Tval> stem, ITreeNode<T, Tval> branch) => RemoveBranch(stem.Id, branch.Id);
	public void SwapPosition(ITreeNode<T, Tval> firstNode, ITreeNode<T, Tval> secondNode) => SwapPosition(firstNode.Id, secondNode.Id);
	public void AddFirstBranch(ITreeNode<T, Tval> stem, ITreeNode<T, Tval> branch) => AddFirstBranch(stem.Id, branch.Id);
	public void AddLastBranch(ITreeNode<T, Tval> stem, ITreeNode<T, Tval> branch) => AddLastBranch(stem.Id, branch.Id);
	public void AddBranchBefore(ITreeNode<T, Tval> beforeBranch, ITreeNode<T, Tval> branch) => AddBranchBefore(beforeBranch.Id, branch.Id);
	public void AddBranchAfter(ITreeNode<T, Tval> afterBranch, ITreeNode<T, Tval> branch) => AddBranchAfter(afterBranch.Id, branch.Id);
	public bool NodeHasValue(ITreeNode<T, Tval> node) => NodeHasValue(node.Id);
	public object GetNodeValue(ITreeNode<T, Tval> node) => GetNodeValue(node.Id);
	public void SetNodeValue(ITreeNode<T, Tval> node, Tval value) => SetNodeValue(node.Id, value);
	public void DeleteNodeValue(ITreeNode<T, Tval> node) => DeleteNodeValue(node.Id);
}
#endregion

#region TreeGraph Tests
public class TreeGraphTests
{
	private TreeGraph<Guid, object> CreateNewTree() => new TreeGraph<Guid, object>(new GuidFactory());
	
	public void RunAllTests()
	{
		CreateTreeTest();
		AddNodesTest();
		AddFirstBranchTest();
		RemoveNodeTest();
		AddBranchBeforeAndAfterTest();
		SwapPositionTest();
	}

	public void CreateTreeTest()
	{
		// ACTION - Create tree
		var tree = CreateNewTree();

		// TEST - Does tree have root node and no other nodes?
		int expectedNodeCount = 1;
		Debug.Assert(tree.Height == 0);
		Debug.Assert(tree.NodeCount == expectedNodeCount);
		Debug.Assert(tree.NodeMap.Count == expectedNodeCount);
		Debug.Assert(tree.BranchMap.Values.Sum(v => v.Count) == tree.NodeCount - 1);
		Debug.Assert(tree.StemMap.Count == tree.NodeCount - 1);

		// TEST - Does root node have expected values?
		Debug.Assert(!(tree.Root is null));
		Debug.Assert(tree.Root.HasPosition);
		Debug.Assert(!tree.Root.HasStem);
		Debug.Assert(!tree.Root.HasBranch);
		Debug.Assert(!tree.Root.HasPrevSibling);
		Debug.Assert(!tree.Root.HasNextSibling);

		Debug.Assert(tree.Root.Position == 0);
		Debug.Assert(tree.Root.Depth == 0);
		Debug.Assert(tree.Root.Path.Count == 1
					&& tree.Root.Path[0] == 0);
		Debug.Assert(tree.Root.StemId == default(Guid));
		Debug.Assert(!tree.Root.BranchIds.Any());
		Debug.Assert(tree.Root.FirstBranchId == default(Guid));
		Debug.Assert(tree.Root.LastBranchId == default(Guid));
		Debug.Assert(tree.Root.PrevSiblingId == default(Guid));
		Debug.Assert(tree.Root.NextSiblingId == default(Guid));
	}

	public void AddNodesTest()
	{
		// ACTION - Create tree and add 3 nodes, all branches of root
		var tree = CreateNewTree();
		var node1a = tree.CreateAndRegisterNode();
		var node1b = tree.CreateAndRegisterNode();
		var node1c = tree.CreateAndRegisterNode();
		tree.AddBranch(tree.Root, node1a);
		tree.AddBranch(tree.Root, node1b);
		tree.AddBranch(tree.Root, node1c);

		// TEST - Do tree node counts match expected?

		int expectedNodeCount = 4;
		Debug.Assert(tree.Height == 1);
		Debug.Assert(tree.NodeCount == expectedNodeCount);
		Debug.Assert(tree.NodeMap.Count == expectedNodeCount);
		Debug.Assert(tree.BranchMap.Values.Sum(v => v.Count) == tree.NodeCount - 1);
		Debug.Assert(tree.StemMap.Count == tree.NodeCount - 1);

		// TEST - Does each root node have expected values?	
		Debug.Assert(!(tree.Root is null));
		Debug.Assert(tree.Root.HasPosition);
		Debug.Assert(!tree.Root.HasStem);
		Debug.Assert(tree.Root.HasBranch);  // confirm root now has branches
		Debug.Assert(!tree.Root.HasPrevSibling);
		Debug.Assert(!tree.Root.HasNextSibling);

		Debug.Assert(tree.Root.Position == 0);
		Debug.Assert(tree.Root.Depth == 0);
		Debug.Assert(tree.Root.Path.Count == 1
					&& tree.Root.Path[0] == 0);
		Debug.Assert(tree.Root.StemId == default(Guid));
		Debug.Assert(tree.Root.BranchIds.Count() == 3);
		Debug.Assert(tree.Root.FirstBranchId == node1a.Id);
		Debug.Assert(tree.Root.LastBranchId == node1c.Id);
		Debug.Assert(tree.Root.PrevSiblingId == default(Guid));
		Debug.Assert(tree.Root.NextSiblingId == default(Guid));

		// TEST - Does each new node have expected values?
		Debug.Assert(node1a.HasPosition);
		Debug.Assert(node1a.HasStem);
		Debug.Assert(!node1a.HasBranch);
		Debug.Assert(!node1a.HasPrevSibling);
		Debug.Assert(node1a.HasNextSibling);
		Debug.Assert(node1a.Position == 0);
		Debug.Assert(node1a.Depth == 1);
		Debug.Assert(node1a.Path.Count == 2
					&& node1a.Path[0] == 0
					&& node1a.Path[1] == 0);
		Debug.Assert(node1a.StemId == tree.Root.Id);
		Debug.Assert(node1a.BranchIds.Count() == 0);
		Debug.Assert(node1a.FirstBranchId == default(Guid));
		Debug.Assert(node1a.LastBranchId == default(Guid));
		Debug.Assert(node1a.PrevSiblingId == default(Guid));
		Debug.Assert(node1a.NextSiblingId == node1b.Id);

		Debug.Assert(node1b.HasPosition);
		Debug.Assert(node1b.HasStem);
		Debug.Assert(!node1b.HasBranch);
		Debug.Assert(node1b.HasPrevSibling);
		Debug.Assert(node1b.HasNextSibling);
		Debug.Assert(node1b.Position == 1);
		Debug.Assert(node1b.Depth == 1);
		Debug.Assert(node1b.Path.Count == 2
					&& node1b.Path[0] == 0
					&& node1b.Path[1] == 1);
		Debug.Assert(node1b.StemId == tree.Root.Id);
		Debug.Assert(node1b.BranchIds.Count() == 0);
		Debug.Assert(node1b.FirstBranchId == default(Guid));
		Debug.Assert(node1b.LastBranchId == default(Guid));
		Debug.Assert(node1b.PrevSiblingId == node1a.Id);
		Debug.Assert(node1b.NextSiblingId == node1c.Id);

		Debug.Assert(node1c.HasPosition);
		Debug.Assert(node1c.HasStem);
		Debug.Assert(!node1c.HasBranch);
		Debug.Assert(node1c.HasPrevSibling);
		Debug.Assert(!node1c.HasNextSibling);
		Debug.Assert(node1c.Position == 2);
		Debug.Assert(node1c.Depth == 1);
		Debug.Assert(node1c.Path.Count == 2
					&& node1c.Path[0] == 0
					&& node1c.Path[1] == 2);
		Debug.Assert(node1c.StemId == tree.Root.Id);
		Debug.Assert(node1c.BranchIds.Count() == 0);
		Debug.Assert(node1c.FirstBranchId == default(Guid));
		Debug.Assert(node1c.LastBranchId == default(Guid));
		Debug.Assert(node1c.PrevSiblingId == node1b.Id);
		Debug.Assert(node1c.NextSiblingId == default(Guid));
	}

	public void AddFirstBranchTest()
	{
		// ACTION - Add 1 node as first branch of root
		var tree = CreateNewTree();
		var node1a = tree.CreateAndRegisterNode();
		var node1b = tree.CreateAndRegisterNode();
		var node1c = tree.CreateAndRegisterNode();
		tree.AddBranch(tree.Root, node1a);
		tree.AddBranch(tree.Root, node1b);
		tree.AddBranch(tree.Root, node1c);

		var node1d = tree.CreateAndRegisterNode();
		tree.AddFirstBranch(tree.Root, node1d);

		// TEST - Do tree node counts match expected?

		int expectedNodeCount = 5;
		Debug.Assert(tree.Height == 1);
		Debug.Assert(tree.NodeCount == expectedNodeCount);
		Debug.Assert(tree.NodeMap.Count == expectedNodeCount);
		Debug.Assert(tree.BranchMap.Values.Sum(v => v.Count) == tree.NodeCount - 1);
		Debug.Assert(tree.StemMap.Count == tree.NodeCount - 1);


		// TEST - Was node inserted correctly to maintain position orders?
		Debug.Assert(node1d.HasPosition);
		Debug.Assert(node1d.HasStem);
		Debug.Assert(!node1d.HasBranch);
		Debug.Assert(!node1d.HasPrevSibling);
		Debug.Assert(node1d.HasNextSibling);
		Debug.Assert(node1d.Position == 0);
		Debug.Assert(node1d.Depth == 1);
		Debug.Assert(node1d.Path.Count == 2
					&& node1d.Path[0] == 0
					&& node1d.Path[1] == 0);
		Debug.Assert(node1d.StemId == tree.Root.Id);
		Debug.Assert(node1d.BranchIds.Count() == 0);
		Debug.Assert(node1d.FirstBranchId == default(Guid));
		Debug.Assert(node1d.LastBranchId == default(Guid));
		Debug.Assert(node1d.PrevSiblingId == default(Guid));
		Debug.Assert(node1d.NextSiblingId == node1a.Id);    // new first node should match former first node and point to former first node as next sibling

		Debug.Assert(node1a.HasPosition);
		Debug.Assert(node1a.HasStem);
		Debug.Assert(!node1a.HasBranch);
		Debug.Assert(node1a.HasPrevSibling);    // former first node should now have prev sibling
		Debug.Assert(node1a.HasNextSibling);
		Debug.Assert(node1a.Position == 1);     // former first node should have position incremented
		Debug.Assert(node1a.Depth == 1);
		Debug.Assert(node1a.Path.Count == 2
					&& node1a.Path[0] == 0
					&& node1a.Path[1] == 1);    // former first node should have path updated
		Debug.Assert(node1a.StemId == tree.Root.Id);
		Debug.Assert(node1a.BranchIds.Count() == 0);
		Debug.Assert(node1a.FirstBranchId == default(Guid));
		Debug.Assert(node1a.LastBranchId == default(Guid));
		Debug.Assert(node1a.PrevSiblingId == node1d.Id);    // former first node should point to new first node as prev sibling
		Debug.Assert(node1a.NextSiblingId == node1b.Id);
	}

	public void RemoveNodeTest()
	{
		var tree = CreateNewTree();
		var node1a = tree.CreateAndRegisterNode();
		var node1b = tree.CreateAndRegisterNode();
		var node1c = tree.CreateAndRegisterNode();
		tree.AddBranch(tree.Root, node1a);
		tree.AddBranch(tree.Root, node1b);
		tree.AddBranch(tree.Root, node1c);

		// ACTION - Remove first branch of root node
		var node1d = tree.CreateAndRegisterNode();
		tree.AddFirstBranch(tree.Root, node1d);
		tree.DeleteNode(node1d);

		// TEST - Do tree node counts match expected? (Retest AddNodesTest)
		int expectedNodeCount = 4;
		Debug.Assert(tree.Height == 1);
		Debug.Assert(tree.NodeCount == expectedNodeCount);
		Debug.Assert(tree.NodeMap.Count == expectedNodeCount);
		Debug.Assert(tree.BranchMap.Values.Sum(v => v.Count) == tree.NodeCount - 1);
		Debug.Assert(tree.StemMap.Count == tree.NodeCount - 1);

		// TEST - Does root note still have expected values? (Retest AddNodesTest)
		Debug.Assert(!(tree.Root is null));
		Debug.Assert(tree.Root.HasPosition);
		Debug.Assert(!tree.Root.HasStem);
		Debug.Assert(tree.Root.HasBranch);
		Debug.Assert(!tree.Root.HasPrevSibling);
		Debug.Assert(!tree.Root.HasNextSibling);

		Debug.Assert(tree.Root.Position == 0);
		Debug.Assert(tree.Root.Depth == 0);
		Debug.Assert(tree.Root.Path.Count == 1
					&& tree.Root.Path[0] == 0);
		Debug.Assert(tree.Root.StemId == default(Guid));
		Debug.Assert(tree.Root.BranchIds.Count() == 3);
		Debug.Assert(tree.Root.FirstBranchId == node1a.Id);
		Debug.Assert(tree.Root.LastBranchId == node1c.Id);
		Debug.Assert(tree.Root.PrevSiblingId == default(Guid));
		Debug.Assert(tree.Root.NextSiblingId == default(Guid));

		// TEST - Does each remaining node have expected values? (Retest)
		Debug.Assert(node1a.HasPosition);
		Debug.Assert(node1a.HasStem);
		Debug.Assert(!node1a.HasBranch);
		Debug.Assert(!node1a.HasPrevSibling);
		Debug.Assert(node1a.HasNextSibling);
		Debug.Assert(node1a.Position == 0);
		Debug.Assert(node1a.Depth == 1);
		Debug.Assert(node1a.Path.Count == 2
					&& node1a.Path[0] == 0
					&& node1a.Path[1] == 0);
		Debug.Assert(node1a.StemId == tree.Root.Id);
		Debug.Assert(node1a.BranchIds.Count() == 0);
		Debug.Assert(node1a.FirstBranchId == default(Guid));
		Debug.Assert(node1a.LastBranchId == default(Guid));
		Debug.Assert(node1a.PrevSiblingId == default(Guid));
		Debug.Assert(node1a.NextSiblingId == node1b.Id);

		Debug.Assert(node1b.HasPosition);
		Debug.Assert(node1b.HasStem);
		Debug.Assert(!node1b.HasBranch);
		Debug.Assert(node1b.HasPrevSibling);
		Debug.Assert(node1b.HasNextSibling);
		Debug.Assert(node1b.Position == 1);
		Debug.Assert(node1b.Depth == 1);
		Debug.Assert(node1b.Path.Count == 2
					&& node1b.Path[0] == 0
					&& node1b.Path[1] == 1);
		Debug.Assert(node1b.StemId == tree.Root.Id);
		Debug.Assert(node1b.BranchIds.Count() == 0);
		Debug.Assert(node1b.FirstBranchId == default(Guid));
		Debug.Assert(node1b.LastBranchId == default(Guid));
		Debug.Assert(node1b.PrevSiblingId == node1a.Id);
		Debug.Assert(node1b.NextSiblingId == node1c.Id);

		Debug.Assert(node1c.HasPosition);
		Debug.Assert(node1c.HasStem);
		Debug.Assert(!node1c.HasBranch);
		Debug.Assert(node1c.HasPrevSibling);
		Debug.Assert(!node1c.HasNextSibling);
		Debug.Assert(node1c.Position == 2);
		Debug.Assert(node1c.Depth == 1);
		Debug.Assert(node1c.Path.Count == 2
					&& node1c.Path[0] == 0
					&& node1c.Path[1] == 2);
		Debug.Assert(node1c.StemId == tree.Root.Id);
		Debug.Assert(node1c.BranchIds.Count() == 0);
		Debug.Assert(node1c.FirstBranchId == default(Guid));
		Debug.Assert(node1c.LastBranchId == default(Guid));
		Debug.Assert(node1c.PrevSiblingId == node1b.Id);
		Debug.Assert(node1c.NextSiblingId == default(Guid));
	}

	public void AddBranchBeforeAndAfterTest()
	{
		var tree = CreateNewTree();
		var node1a = tree.CreateAndRegisterNode();
		var node1b = tree.CreateAndRegisterNode();
		var node1c = tree.CreateAndRegisterNode();
		tree.AddBranch(tree.Root, node1a);
		tree.AddBranch(tree.Root, node1b);
		tree.AddBranch(tree.Root, node1c);

		var node1d = tree.CreateAndRegisterNode();
		tree.AddFirstBranch(tree.Root, node1d);
		tree.DeleteNode(node1d);
		
		// ACTION - Add branch before and after specific branch
		var before_node1c = tree.CreateAndRegisterNode();
		var after_node1b = tree.CreateAndRegisterNode();
		tree.AddBranchBefore(node1c, before_node1c);
		tree.AddBranchAfter(node1b, after_node1b);

		// TEST - Confirm tree graph height and node count match expected
		int expectedNodeCount = 6;
		Debug.Assert(tree.Height == 1);
		Debug.Assert(tree.NodeCount == expectedNodeCount);
		Debug.Assert(tree.NodeMap.Count == expectedNodeCount);
		Debug.Assert(tree.BranchMap.Values.Sum(v => v.Count) == tree.NodeCount - 1);
		Debug.Assert(tree.StemMap.Count == tree.NodeCount - 1);
		
		// TEST - Were nodes added correctly?
		Debug.Assert(after_node1b.HasPosition);
		Debug.Assert(after_node1b.HasStem);
		Debug.Assert(!after_node1b.HasBranch);
		Debug.Assert(after_node1b.HasPrevSibling);
		Debug.Assert(after_node1b.HasNextSibling);
		Debug.Assert(after_node1b.Position == 2);
		Debug.Assert(after_node1b.Depth == 1);
		Debug.Assert(after_node1b.Path.Count == 2
					&& after_node1b.Path[0] == 0
					&& after_node1b.Path[1] == 2);
		Debug.Assert(after_node1b.StemId == tree.Root.Id);
		Debug.Assert(after_node1b.BranchIds.Count() == 0);
		Debug.Assert(after_node1b.FirstBranchId == default(Guid));
		Debug.Assert(after_node1b.LastBranchId == default(Guid));
		Debug.Assert(after_node1b.PrevSiblingId == node1b.Id);
		Debug.Assert(after_node1b.NextSiblingId == before_node1c.Id);


		Debug.Assert(before_node1c.HasPosition);
		Debug.Assert(before_node1c.HasStem);
		Debug.Assert(!before_node1c.HasBranch);
		Debug.Assert(before_node1c.HasPrevSibling);
		Debug.Assert(before_node1c.HasNextSibling);
		Debug.Assert(before_node1c.Position == 3);
		Debug.Assert(before_node1c.Depth == 1);
		Debug.Assert(before_node1c.Path.Count == 2
					&& before_node1c.Path[0] == 0
					&& before_node1c.Path[1] == 3);
		Debug.Assert(before_node1c.StemId == tree.Root.Id);
		Debug.Assert(before_node1c.BranchIds.Count() == 0);
		Debug.Assert(before_node1c.FirstBranchId == default(Guid));
		Debug.Assert(before_node1c.LastBranchId == default(Guid));
		Debug.Assert(before_node1c.PrevSiblingId == after_node1b.Id);
		Debug.Assert(before_node1c.NextSiblingId == node1c.Id);

	}

	public void SwapPositionTest()
	{
		var idFactory = new GuidFactory();
		var tree = new TreeGraph<Guid, object>(idFactory);

		// ACTION - Add nodes with nested depth, then swap nodes
		var node1a = tree.CreateAndRegisterNode();
		var node1b = tree.CreateAndRegisterNode();
		var node1c = tree.CreateAndRegisterNode();
		var node1a1 = tree.CreateAndRegisterNode();
		var node1a2 = tree.CreateAndRegisterNode();
		tree.AddBranch(tree.Root, node1a);
		tree.AddBranch(tree.Root, node1b);
		tree.AddBranch(tree.Root, node1c);
		tree.AddBranch(node1a, node1a1);
		tree.AddBranch(node1a, node1a2);
		tree.SwapPosition(node1a, node1b);

		// TEST - Confirm tree graph height and node count match expected
		int expectedNodeCount = 6;
		Debug.Assert(tree.Height == 2);
		Debug.Assert(tree.NodeCount == expectedNodeCount);
		Debug.Assert(tree.NodeMap.Count == expectedNodeCount);
		Debug.Assert(tree.BranchMap.Values.Sum(v => v.Count) == tree.NodeCount - 1);
		Debug.Assert(tree.StemMap.Count == tree.NodeCount - 1);

		// TEST - Confirm swapped node (node1a) is in expected position after swap
		Debug.Assert(node1a.HasPosition);
		Debug.Assert(node1a.HasStem);
		Debug.Assert(node1a.HasBranch);
		Debug.Assert(node1a.HasPrevSibling);
		Debug.Assert(node1a.HasNextSibling);
		Debug.Assert(node1a.Position == 1);
		Debug.Assert(node1a.Depth == 1);
		Debug.Assert(node1a.Path.Count == 2
					&& node1a.Path[0] == 0
					&& node1a.Path[1] == 1);
		Debug.Assert(node1a.StemId == tree.Root.Id);
		Debug.Assert(node1a.BranchIds.Count() == 2);
		Debug.Assert(node1a.FirstBranchId == node1a1.Id);
		Debug.Assert(node1a.LastBranchId == node1a2.Id);
		Debug.Assert(node1a.PrevSiblingId == node1b.Id);
		Debug.Assert(node1a.NextSiblingId == node1c.Id);

		// TEST - Confirm swapped node (node1b) is in expected position after swap
		Debug.Assert(node1b.HasPosition);
		Debug.Assert(node1b.HasStem);
		Debug.Assert(!node1b.HasBranch);
		Debug.Assert(!node1b.HasPrevSibling);
		Debug.Assert(node1b.HasNextSibling);
		Debug.Assert(node1b.Position == 0);
		Debug.Assert(node1b.Depth == 1);
		Debug.Assert(node1b.Path.Count == 2
					&& node1b.Path[0] == 0
					&& node1b.Path[1] == 0);
		Debug.Assert(node1b.StemId == tree.Root.Id);
		Debug.Assert(node1b.BranchIds.Count() == 0);
		Debug.Assert(node1b.FirstBranchId == default(Guid));
		Debug.Assert(node1b.LastBranchId == default(Guid));
		Debug.Assert(node1b.PrevSiblingId == default(Guid));
		Debug.Assert(node1b.NextSiblingId == node1a.Id);
	}
}
#endregion