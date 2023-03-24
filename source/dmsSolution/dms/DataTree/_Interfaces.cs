namespace dms.DataTree;


public interface ITreePath: IEquatable<ITreePath>, IComparable<ITreePath>
{
	public IReadOnlyCollection<uint> List { get; }
	public string String { get; }
	public uint Length { get; }

	public uint this[uint pathIdx] { get; }
    public uint this[int pathIdx] { get; }

    public ITreePath GetBasePath();
    public ITreePath GetBranchPath(uint position);
    public ITreePath GetBranchPath(int position);

    public bool IsBasePathOf(ITreePath other);
	public bool IsAncestorOf(ITreePath other);
	public bool IsDescendantOf(ITreePath other);
}

public interface ITreeNode<T>
{
	public ITreeGraph<T> Tree { get; }
	public uint Id { get; }
	public TreeNodeType NodeType { get; }
	public ITreePath GetPath();

	public uint Position { get; }
	public bool IsRoot { get; }
	public bool HasValue { get; }
	public bool HasStem { get; }
	public bool HasBranch { get; }
	public bool HasNextPeer { get; }
	public bool HasPrevPeer { get; }


	// Value operations (get, set, etc.)
	public T? GetValue();
	public bool TryGetValue(out T? value);
	public void SetValue(T? value);
	public void DeleteValue();

	// Get related nodes
	public uint GetDepth();
	public ITreeNode<T> GetStem();
	public ITreeNode<T> GetFirstBranch();
	public ITreeNode<T> GetLastBranch();
	public ITreeNode<T> GetPrevPeer();
	public ITreeNode<T> GetNextPeer();

	public ITreeNode<T>[] GetBranches();    // will return empty list if no branches are specified
	public ITreeNode<T>[] GetPeers();		// will fail if node not have stem
	public ITreeNode<T>[] GetPeersExcludingSelf();

	public bool TryGetStem(out ITreeNode<T>? stem);
	public bool TryGetFirstBranch(out ITreeNode<T>? firstBranch);
	public bool TryGetLastBranch(out ITreeNode<T>? lastBranch);
	public bool TryGetPrevPeer(out ITreeNode<T>? prevPeer);
	public bool TryGetNextPeer(out ITreeNode<T>? nextPeer);

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
	public IReadOnlyDictionary<uint, ITreeNode<T>> NodeMap { get; }
	public IReadOnlyDictionary<uint, uint> StemMap { get; }
	public IReadOnlyDictionary<uint, HashSet<uint>> BranchMap { get; }
	public IReadOnlyDictionary<uint, uint> PositionMap { get; }
	public IReadOnlyDictionary<uint, T?> ValueMap { get; }

	public ITreeNode<T> Root { get; }
	public uint GetHeight();

	public uint GetNodeCount();

	//public uint GetFloatingNodeCount();
	//public IEnumerable<ITreeNode<T>> GetFloatingNodes();
	//public IEnumerable<ITreeNode<T>> EnumerateNodes(); // dfs
	//public IEnumerable<ITreeNode<T>> EnumerateNodes(IEnumerable<uint> path);

	public void Validate();



	// Access nodes 
	public ITreeNode<T> GetNode(uint id);
	public ITreeNode<T> GetNode(int id);
	public ITreeNode<T> GetNodeByPath(ITreePath path);
	public ITreeNode<T> GetNodeByPath(string path);
	public ITreeNode<T> GetNodeByPath(IEnumerable<int> path);
	public ITreeNode<T> GetNodeByPath(IEnumerable<uint> path);



    // Create & destroy nodes 
    public ITreeNode<T> CreateBranch(ITreeNode<T> node);
	public ITreeNode<T> CreateBranchAt(ITreeNode<T> node, uint position);
	public ITreeNode<T> CreateBranchAt(ITreeNode<T> node, int position);
	public ITreeNode<T> CreatePrevPeer(ITreeNode<T> node);
	public ITreeNode<T> CreateNextPeer(ITreeNode<T> node);

	public void DeleteNode(ITreeNode<T> node);
	public void DeleteNode(ITreeNode<T> node, bool deleteDescendants);


	// Node value operations
	public bool NodeHasValue(ITreeNode<T> node);
	public void SetNodeValue(ITreeNode<T> node, T? value);
	public T? GetNodeValue(ITreeNode<T> node);
	public void DeleteNodeValue(ITreeNode<T> node);


    // Manipulate node positions
    public void SetNodePosition(ITreeNode<T> node, uint position);
    public void SetNodePosition(ITreeNode<T> node, int position);
	public void SetNodeFirstPosition(ITreeNode<T> node);
	public void SetNodeLastPosition(ITreeNode<T> node);
    
	public void SetNodeStem(ITreeNode<T> node, ITreeNode<T> stemNode);
	public void SetNodeStemPosition(ITreeNode<T> node, ITreeNode<T> stemNode, uint position);
    public void SetNodeStemPosition(ITreeNode<T> node, ITreeNode<T> stemNode, int position);
	
	public void MoveNodeBefore(ITreeNode<T> node, ITreeNode<T> beforeNode);
	public void MoveNodeAfter(ITreeNode<T> node, ITreeNode<T> afterNode);

    public void SwapNodes(ITreeNode<T> node1, ITreeNode<T> node2);


}


