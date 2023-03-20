namespace dms.DataTree;


public interface ITreePath: IEquatable<ITreePath>, IComparable<ITreePath>
{
	public IList<uint> List { get; }
	public string String { get; }
	public uint Length { get; }

	public bool IsBasePathOf(ITreePath other);
	public bool IsAncestorOf(ITreePath other);
	public bool IsDescendantOf(ITreePath other);
}

public interface ITreeNode<T>
{
	public ITreeGraph<T> Tree { get; }
	public TreeNodeType NodeType { get; }
	public uint Id { get; }
	public ITreePath GetPath();
	public uint GetDepth();
	
	public uint Position { get; }
	public bool IsRoot { get; }
	public bool HasValue { get; }
	public bool HasStem { get; }
	public bool HasBranch { get; }
	public bool HasNextPeer { get; }
	public bool HasPrevPeer { get; }

	public ITreeNode<T> GetStem();
	public ITreeNode<T>[] GetBranches();
	public ITreeNode<T> GetFirstBranch();
	public ITreeNode<T> GetLastBranch();
	public ITreeNode<T>[] GetPeers();
	public ITreeNode<T>[] GetPeersExcludingSelf();
	public ITreeNode<T> GetPrevPeer();
	public ITreeNode<T> GetNextPeer();

	public T GetValue();
	public void SetValue(T value);
	public void DeleteValue();


	//public void SetStem(ITreeNode<T> node);
	//public void SetBranch(ITreeNode<T> node);
	//public void SetBranch(ITreeNode<T> node, uint position);
	//public void SetFirstBranch(ITreeNode<T> node);
	//public void SetLastBranch(ITreeNode<T> node);
	//public void SetPrevPeer(ITreeNode<T> node);
	//public void SetNextPeer(ITreeNode<T> node);

}



/// <summary>
/// Base tree graph interface that:
/// 1. Stores a collection of tree nodes
/// 2. Maintains and updates graph structure (node branch/stem relationships)
/// 3. Handles creating and destroying nodes
/// 4. Gets and sets node values
/// </summary>
public interface ITreeGraph<T>
{
	///////////////////////////////////////
	// Store nodes and track node relationships
	///////////////////////////////////////
	public IReadOnlyDictionary<uint, ITreeNode<T>> NodeMap { get; }
	public IReadOnlyDictionary<uint, uint> StemMap { get; }
	public IReadOnlyDictionary<uint, HashSet<uint>> BranchMap { get; }
	public IReadOnlyDictionary<uint, uint> PositionMap { get; }
	public IReadOnlyDictionary<uint, T> ValueMap { get; }

	/// <summary>
	/// Root is the root node of the graph; there must be one and only one root.
	/// </summary>
	public ITreeNode<T> Root { get; }

	/// <summary>
	/// GetHeight returns height of tree graph (if tree has only one node, height equals 0).
	/// </summary>
	public uint GetHeight();

	/// <summary>
	/// GetNodeCount returns total number of nodes in tree graph.
	/// </summary>
	public uint GetNodeCount();

	//public uint GetFloatingNodeCount();
	//public IEnumerable<ITreeNode<T>> GetFloatingNodes();
	//public IEnumerable<ITreeNode<T>> EnumerateNodes(); // dfs
	//public IEnumerable<ITreeNode<T>> EnumerateNodes(IEnumerable<uint> path);

	/// <summary>
	/// Validate can be run to confirm instance is in a valid state for operations.
	/// </summary>
	public void Validate();



	///////////////////////////////////////
	// Access nodes 
	///////////////////////////////////////

	/// <summary>
	/// GetNode returns TreeNode with specified id (uint or int).
	/// </summary>
	public ITreeNode<T> GetNode(uint id);
	public ITreeNode<T> GetNode(int id);

	/// <summary>
	/// GetNodeByPath returns TreeNode with specified path (ITreePath, string, or uint/int enumerable).
	/// </summary>
	public ITreeNode<T> GetNodeByPath(ITreePath path);
	public ITreeNode<T> GetNodeByPath(string path);
	public ITreeNode<T> GetNodeByPath(IEnumerable<int> path);
	public ITreeNode<T> GetNodeByPath(IEnumerable<uint> path);


	///////////////////////////////////////
	// Create & destroy nodes 
	///////////////////////////////////////

	/// <summary>
	/// CreateNode generates a new "floating" node (exists in graph but does not have a stem specified).
	/// </summary>
	// public ITreeNode<T> CreateNode();

	/// <summary>
	/// CreateBranch generates a new branch node for specified stem node.
	/// </summary>
	public ITreeNode<T> CreateBranch(ITreeNode<T> node);

	/// <summary>
	/// CreateBranchAt generates a new branch node for specified stem node at specified position (uint or int).
	/// </summary>
	public ITreeNode<T> CreateBranchAt(ITreeNode<T> node, uint position);
	public ITreeNode<T> CreateBranchAt(ITreeNode<T> node, int position);

	/// <summary>
	/// CreatePrevPeer generates a new peer node before the specified node.
	/// </summary>
	public ITreeNode<T> CreatePrevPeer(ITreeNode<T> node);

	/// <summary>
	/// CreateNextPeer generates a new peer node after the specified node.
	/// </summary>
	public ITreeNode<T> CreateNextPeer(ITreeNode<T> node);


	/// <summary>
	/// DeleteNode removes a node from the graph (descendants optional).
	/// Will error by default if specified node has any descendants.
	/// To delete descendants, set optional parameter deleteDescendants to true.
	/// </summary>
	public void DeleteNode(ITreeNode<T> node);
	public void DeleteNode(ITreeNode<T> node, bool deleteDescendants);


	///////////////////////////////////////
	// Handle node value operations
	///////////////////////////////////////

	/// <summary>
	/// NodeHasValue returns true if the tree has stored value for specified node (even if value is null).
	/// </summary>
	public bool NodeHasValue(ITreeNode<T> node);

	/// <summary>
	/// SetNodeValue stores specified node value in tree.
	/// </summary>
	public void SetNodeValue(ITreeNode<T> node, T value);

	/// <summary>
	/// GetNodeValue returns the stored value for specified node.
	/// Throws error if node does not have a stored value.
	/// </summary>
	public T GetNodeValue(ITreeNode<T> node);

	/// <summary>
	/// DeleteNodeValue deletes stored value if one is specified.
	/// Throws error if node does not have a stored value.
	/// </summary>
	public void DeleteNodeValue(ITreeNode<T> node);


	///////////////////////////////////////
	// Manipulate node positions
	///////////////////////////////////////

	/// <summary>
	/// Swap will replace position of node1 with node2, and vice-versa.
	/// </summary>
	public void Swap(ITreeNode<T> node1, ITreeNode<T> node2);

	/// <summary>
	/// MoveToPosition will place node at the specified position (uint or int) within its peer group.
	/// </summary>
	public void MoveToPosition(ITreeNode<T> node, uint position);
	public void MoveToPosition(ITreeNode<T> node, int position);

	/// <summary>
	/// MoveToStem will place node as branch of specified stemNode.
	/// Defaults to setting as last branch, but can optionally specify position (uint or int).
	/// </summary>
	public void MoveToStem(ITreeNode<T> node, ITreeNode<T> stemNode);
	public void MoveToStem(ITreeNode<T> node, ITreeNode<T> stemNode, uint position);

	/// <summary>
	/// MoveBefore will place node as the previous peer of beforeNode.
	/// </summary>
	public void MoveBefore(ITreeNode<T> node, ITreeNode<T> beforeNode);

	/// <summary>
	/// MoveAfter will place node as the next peer of afterNode
	/// </summary>
	public void MoveAfter(ITreeNode<T> node, ITreeNode<T> afterNode);

	/// <summary>
	/// MoveToStart will place node as the first node in its peer group.
	/// </summary>
	public void MoveToStart(ITreeNode<T> node);

	/// <summary>
	/// MoveToEnd will place node as the last node in its peer group.
	/// </summary>
	public void MoveToEnd(ITreeNode<T> node);



	// REMOVE?

	/// <summary>
	/// DetachNode removes the stem relationship for a given node (makes it a floating node)
	/// </summary>
	//public void DetachNode(ITreeNode<T> node);
	//public void DetachBranchAt(ITreeNode<T> stem, int position);
	//public void DetachBranchAt(ITreeNode<T> stem, uint position);
	//public void DetachBranch(ITreeNode<T> stem, ITreeNode<T> branch);


	//public void SetBranchAt(ITreeNode<T> stem, ITreeNode<T> branch, int position);
	//public void SetBranchAt(ITreeNode<T> stem, ITreeNode<T> branch, uint position);
	//public void SetBranch(ITreeNode<T> stem, ITreeNode<T> branch);
	//public void SetFirstBranch(ITreeNode<T> stem, ITreeNode<T> branch);
	//public void SetLastBranch(ITreeNode<T> stem, ITreeNode<T> branch);
	//public void SetPrevPeer(ITreeNode<T> node, ITreeNode<T> peer);
	//public void SetNextPeer(ITreeNode<T> node, ITreeNode<T> peer);

	/*
	public IEnumerable<ITreeNode<T>> EnumerateNodes(Guid id);
	public ITreeNode<T> GetNode(Guid id);
	public void DeleteNode(Guid id);
	public ITreeNode<T> CreateBranchAt(Guid addToNodeId, int position);
	public ITreeNode<T> CreateBranch(Guid addToNodeId);
	public ITreeNode<T> CreatePrevPeer(Guid nodeId);
	public ITreeNode<T> CreateNextPeer(Guid nodeId);

	public void SetBranchAt(Guid stemId, Guid branchId, int position);
	public void SetBranch(Guid stemId, Guid branchId);
	public void SetFirstBranch(Guid stem, Guid branchId);
	public void SetLastBranch(Guid stemId, Guid branchId);
	public void SetPrevPeer(Guid nodeId, Guid nextPeerId);
	public void SetNextPeer(Guid nodeId, Guid prevPeerId);

	public void RemoveNode(Guid nodeId);
	public void RemoveBranchAt(Guid stemId, int position);
	public void RemoveBranch(Guid stemId, Guid branchId);

	public void Swap(Guid firstNodeId, Guid secondNodeId);
	public void MoveBefore(Guid beforeNodeId, Guid moveNodeId);
	public void MoveAfter(Guid beforeNodeId, Guid moveNodeId);

	public bool NodeHasValue(Guid nodeId);
	public T GetNodeValue(Guid nodeId);
	public void SetNodeValue(Guid nodeId, object value);
	public void DeleteNodeValue(Guid nodeId);
	*/

}

