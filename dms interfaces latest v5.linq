<Query Kind="Program">
  <Namespace>DmsEnum</Namespace>
  <Namespace>System.Runtime.InteropServices.ComTypes</Namespace>
  <Namespace>System.Security</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>TreeGraph</Namespace>
  <RemoveNamespace>System.Data</RemoveNamespace>
</Query>

void Main()
{
	var tt = new TreeTests.TreeGraphTests<object>();
	tt.RunAllTests();

	var dtt = new TreeTests.DataTreeTests();
	dtt.RunAllTests();

	var dsrit = new DataStoreTests.RawInputTests();
	dsrit.RunAllTests();
	
	var dsft = new DataStoreTests.FileTests();
	dsft.RunAllTests();
	
	var sampleTree = tt.GetExampleTree1();
	sampleTree.Display();

	//var d = new DataGraph.DataGraph();
	//d.Dump();
	
	object input = "dfdfasdf";
	var a = new Data.Value();

	a.Dump();
	
	a.RawValue = input;
	
	a.Dump();


	var f = new Data.Value();
	
	f.Tree.Dump();
	
	




	//	var d = DataGraph.DataBuilder.NewValue("Hello world!");
//	d.Dump();
//
//	var f = DataGraph.DataBuilder.NewField("field_1", "Hello world!");
//	f.Dump();
//
//
//	var l = DataGraph.DataBuilder.NewList(); //"field_1", "Hello world!");
//	l.AddValue("a");
//	l.AddValue("b");
//	l.AddValue("c");
//	l.AddValue("d");
//	l.AddValue(true);
//	l.AddValue(1);
//	l.AddValue(DateTime.Now);
//	l.AddValue(null);
//	l.AddValue(new { Fake = 1, Object = 1 });
//	l.Dump();

	/*
	Add Tree Reader Back
	
	Data creation flow
		-- Tree Reader reads a source for a given format
		-- Builds tree in Data object and tags tree format enum
		-- Data schema assesmnet
		-- Uses tree and tree format (and perhaps source and source form) to determine data schema
		-- Option to override this (i.e. for querying a sql table)
	*/
}


namespace DmsEnum
{
	/// <summary>
	/// Specifies DMS enum types
	/// </summary>
	public enum EnumIndex
	{
		DataStructure,
		DataValue,
		DataSource,
		DataFormat,
	}

	/// <summary>
	/// DataNode structure type (describes DataNode types)
	/// </summary>
	public enum StructureType
	{
		Value,
		Field,
		List,
		Record,
		Dataset,
	}

	/// <summary>
	/// Data value types (describes DataValue types)
	/// </summary>
	public enum ValueType
	{
		None,       // no value associated
		Null,       // empty value associated
		String,
		Number,
		Boolean,
		DateTime,
		Unknown,    // type is unspecified
		Error,      // program error when attempting to determine type
	}

	/// <summary>
	/// Extensions class maps string values to corresponding enum, may be used for configuring (i.e. specifying list of values)
	/// </summary>
	public static partial class DmsEnumExtensions
	{

		/// <summary>
		/// Create maps to get enum values from string for all DmsEnumType enums
		/// </summary>
		private static Dictionary<string, T> GenerateEnumNameValueMap<T>() where T : Enum
		{
			var res = new Dictionary<string, T>();
			foreach (T value in Enum.GetValues(typeof(T)))
			{
				string name = Enum.GetName(typeof(T), value);
				res.Add(name, value);
			}
			return res;
		}
		private static readonly Dictionary<string, StructureType> DataStructureMap = DmsEnumExtensions.GenerateEnumNameValueMap<StructureType>();
		private static readonly Dictionary<string, ValueType> DataValueMap = DmsEnumExtensions.GenerateEnumNameValueMap<ValueType>();
		//private static readonly Dictionary<string, SourceType> DataSourceMap = DmsEnumExtensions.GenerateEnumNameValueMap<SourceType>();
		//private static readonly Dictionary<string, FormatType> DataFormatMap = DmsEnumExtensions.GenerateEnumNameValueMap<FormatType>();

		//private static readonly Dictionary<DmsEnumType, Dictionary<string, Enum>> DmsEnumMap
		//	= new Dictionary<DmsEnumType, Dictionary<string, Enum>>()
		//	{
		//		{ DmsEnumType.DataStructure,    DataStructureMap },
		//		{ DmsEnumType.DataValue,        DataValueMap },
		//		{ DmsEnumType.DataSource,   DataSourceMap },
		//		{ DmsEnumType.DataFormat,       DataFormatMap },
		//	};

	}
}

/// <summary>
/// Tree graph class can be used to create a tree graph where nodes are arbitrary objects
/// </summary>
#region Tree Graph

/// <summary>
/// Node interface
/// </summary>
public interface ITreeNode<T>
{
	public ITreeGraph<T> Tree { get; }
	public Guid Id { get; }

	public int Position { get; }

	public int[] GetPath();
	public int GetDepth();

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
	public ITreeNode<T> GetPrevPeer();
	public ITreeNode<T> GetNextPeer();

	public T GetValue();
	public void SetValue(T value);
	public void DeleteValue();
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
	public void Display();
	public ITreeNode<T> Root { get; }
	public int GetNodeCount();
	public int GetHeight();

	public IEnumerable<ITreeNode<T>> EnumerateNodes(); // dfs
	public IEnumerable<ITreeNode<T>> EnumerateNodes(IEnumerable<int> path);

	public ITreeNode<T> GetNode(IEnumerable<int> path);
	public IEnumerable<ITreeNode<T>> GetFloatingNodes(); // "floating" nodes exist in graph but are not linked to any other nodes and thus cannot be accessed by path

	public ITreeNode<T> CreateNode(); // create a new "floating" node (exists in graphs but is not linked to any other nodes)
	public ITreeNode<T> CreateBranchAt(ITreeNode<T> addToNode, int position);
	public ITreeNode<T> CreateBranch(ITreeNode<T> addToNode);
	public ITreeNode<T> CreatePrevPeer(ITreeNode<T> node);
	public ITreeNode<T> CreateNextPeer(ITreeNode<T> node);

	public void DeleteNode(ITreeNode<T> node); // can only delete nodes if they are floating
	public void DeleteNode(IEnumerable<int> path); 

	public void SetBranchAt(ITreeNode<T> stem, ITreeNode<T> branch, int position);
	public void SetBranch(ITreeNode<T> stem, ITreeNode<T> branch);
	public void SetFirstBranch(ITreeNode<T> stem, ITreeNode<T> branch);
	public void SetLastBranch(ITreeNode<T> stem, ITreeNode<T> branch);
	public void SetPrevPeer(ITreeNode<T> node, ITreeNode<T> peer);
	public void SetNextPeer(ITreeNode<T> node, ITreeNode<T> peer);

	public void RemoveNode(ITreeNode<T> node);
	public void RemoveBranchAt(ITreeNode<T> stem, int position);
	public void RemoveBranch(ITreeNode<T> stem, ITreeNode<T> branch);

	public void Swap(ITreeNode<T> firstNode, ITreeNode<T> secondNode);
	public void MoveBefore(ITreeNode<T> beforeNode, ITreeNode<T> moveNode);
	public void MoveAfter(ITreeNode<T> beforeNode, ITreeNode<T> moveNode);

	public bool NodeHasValue(ITreeNode<T> node);
	public T GetNodeValue(ITreeNode<T> node);
	public void SetNodeValue(ITreeNode<T> node, T value);
	public void DeleteNodeValue(ITreeNode<T> node);
	


	// Not meant to be used but included here to facilitate implementation
	
	public IDictionary<Guid, ITreeNode<T>> NodeMap { get; }
	public IDictionary<Guid, Guid> StemMap { get; }
	public IDictionary<Guid, HashSet<Guid>> BranchMap { get; }
	public IDictionary<Guid, int> PositionMap { get; }
	public IDictionary<Guid, T> ValueMap { get; }
	
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
	public void SetNodeValue(Guid nodeId, T value);
	public void DeleteNodeValue(Guid nodeId);

	public void Validate();     // Confirms graph implementation is in a valid state for operations

}

/// <summary>
/// Tree graph and data tree implementations here
/// </summary>
namespace TreeGraph
{
	public class TreeNode<T> : ITreeNode<T>
	{
		// could be placed in identifieble interface
		protected internal static readonly Random Random = new Random();
		protected internal static Guid GenerateId()
		{
			byte[] guidBytes = Guid.NewGuid().ToByteArray();
			byte[] seedBytes = BitConverter.GetBytes(Random.Next());
			for (int i = 0; i < 4; i++)
			{
				guidBytes[i + 10] = seedBytes[i];
			}

			return new Guid(guidBytes);
		}
		
		public ITreeGraph<T> Tree { get; }
		public Guid Id { get; }
		
		public TreeNode(ITreeGraph<T> tree)
		{
			Tree = tree;
			Id = GenerateId();
		}

		public int[] GetPath() => GetPathList().ToArray();
		public int GetDepth() => GetPathLength();
		public int Position => Tree.PositionMap.ContainsKey(Id) ? Tree.PositionMap[Id] : -1;

		public bool HasValue => Tree.NodeHasValue(Id);
		public bool HasStem => Tree.StemMap.ContainsKey(this.Id);
		public bool HasBranch => Tree.BranchMap.ContainsKey(this.Id);
		public bool HasNextPeer => Tree.BranchMap.ContainsKey(StemId) ? Tree.BranchMap[StemId].Where(b => Tree.PositionMap[b] > Position).Any() : false;
		public bool HasPrevPeer => Tree.BranchMap.ContainsKey(StemId) ? Tree.BranchMap[StemId].Where(b => Tree.PositionMap[b] < Position).Any() : false;

		public T GetValue() => Tree.GetNodeValue(Id);
		public void SetValue(T value) => Tree.SetNodeValue(Id, value);
		public void DeleteValue() => Tree.DeleteNodeValue(Id);
		
		public ITreeNode<T> GetStem() => HasStem ? Tree.GetNode(Tree.StemMap[Id]) : null;
		public ITreeNode<T>[] GetBranches() => HasBranch ? Tree.BranchMap[Id].OrderBy(b => Tree.PositionMap[b]).Select(b => Tree.GetNode(b)).ToArray() : new ITreeNode<T>[] {};
		public ITreeNode<T> GetFirstBranch() => HasBranch ? Tree.GetNode(BranchIds.First()) : null;
		public ITreeNode<T> GetLastBranch() => HasBranch ? Tree.GetNode(BranchIds.Last()) : null;
		public ITreeNode<T>[] GetPeers()
		{
			if (HasStem)
				return Tree.BranchMap[StemId]
					.Where(b => !(Tree.GetNode(b).Id.Equals(Id)))
					.OrderBy(b => Tree.PositionMap[b])
					.Select(b => Tree.GetNode(b))
					.ToArray();
			return new ITreeNode<T>[] { };
		}
		public ITreeNode<T> GetPrevPeer()
		{
			if (HasPrevPeer)
				return Tree.GetNode(
					Tree.BranchMap[StemId]
						.Where(b => Tree.PositionMap[b] < Position)
						.OrderByDescending(b => Tree.PositionMap[b])
						.First()
					);
			return null;
		}
		public ITreeNode<T> GetNextPeer()
		{
			if (HasNextPeer)
				return Tree.GetNode(
					Tree.BranchMap[StemId]
						.Where(b => Tree.PositionMap[b] > Position)
						.OrderBy(b => Tree.PositionMap[b])
						.First()
					); 
			return null;
		}


		protected internal List<int> GetPathList()
		{
			if (!HasStem) 
				return new List<int>();

			var pathList = new List<int>() { this.Position };
			int pathLen = 1;
			var node = GetStem();
			while (node.HasStem)
			{
				if (pathLen > TreeGraph<T>.MaxDepth)
					throw new TreeGraphException($"Maximum depth ({TreeGraph<T>.MaxDepth}) reached when getting path.");

				pathList.Add(node.Position);
				pathLen++;
				node = node.GetStem();
			}
			pathList.Reverse();
			return pathList;
		}
		protected internal int GetPathLength()
		{
			if (!HasStem)
				return 0;
				
			int pathLen = 1;
			var node = GetStem();
			while (node.HasStem)
			{
				if (pathLen > TreeGraph<T>.MaxDepth)
					throw new TreeGraphException($"Maximum depth ({TreeGraph<T>.MaxDepth}) reached when getting path.");

				pathLen++;
				node = node.GetStem();
			}
			return pathLen;
		}

		// INTERNAL IDS
		protected internal bool HasPosition => Tree.PositionMap.ContainsKey(Id);
		protected internal Guid StemId => HasStem ? Tree.StemMap[Id] : Guid.Empty;
		protected internal Guid[] BranchIds => HasBranch ? Tree.BranchMap[Id].OrderBy(b => Tree.PositionMap[b]).ToArray() : new Guid[] { };
		protected internal Guid FirstBranchId => HasBranch ? BranchIds.First() : Guid.Empty;
		protected internal Guid LastBranchId => HasBranch ? BranchIds.Last() :  Guid.Empty;
		protected internal Guid[] PeerIds => HasStem ? Tree.BranchMap[StemId].Where(b => !(Tree.GetNode(b).Id.Equals(Id))).OrderBy(b => Tree.PositionMap[b]).ToArray() : new Guid[] { };
		protected internal Guid PrevPeerId => HasPrevPeer ? Tree.BranchMap[StemId].Where(b => Tree.PositionMap[b] < Position).OrderByDescending(b => Tree.PositionMap[b]).First() : Guid.Empty;
		protected internal Guid NextPeerId => HasNextPeer ? Tree.BranchMap[StemId].Where(b => Tree.PositionMap[b] > Position).OrderBy(b => Tree.PositionMap[b]).First() : Guid.Empty;

	}

	public class TreeGraph<T> : ITreeGraph<T>
	{
		
		public void Display()
		{
			int height = GetHeight();
			$"Tree : nodes={GetNodeCount()}, height={height}".Dump();
			foreach (var node in EnumerateNodes())
			{
				string ps = node.HasStem ? string.Join(".", node.GetPath()) : "root";
				var sb = new StringBuilder()
					.Append(string.Concat(Enumerable.Repeat(' ', node.GetDepth())))
					.Append($"[{node.GetDepth()}] {ps} -- {node.Id}");
				var s = sb.ToString();
				s.Dump();
			}
		}
		public static int MaxDepth = 1024;
		public static int BasePosition = 0;

		protected internal Guid _rootId { get; set; }
		protected internal IDictionary<Guid, ITreeNode<T>> _nodeMap { get; set; }
		protected internal IDictionary<Guid, Guid> _stemMap { get; set; }
		protected internal IDictionary<Guid, HashSet<Guid>> _branchMap { get; set; }
		protected internal IDictionary<Guid, int> _positionMap { get; set; }
		protected internal IDictionary<Guid, T> _valueMap { get; set; }

		protected internal ITreeNode<T> CreateAndRegisterNode()
		{
			try
			{
				var node = new TreeNode<T>(this);
				RegisterNode(node);
				return node;
			}
			catch (Exception ex)
			{
				throw new TreeGraphException($"Unable to create and register node.", ex);
			}
		}
		protected internal void RegisterNode(ITreeNode<T> node)
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
		protected internal IEnumerable<Guid> EnumerateNodeIdsDepthFirst(Guid nodeId)
		{
				
			yield return nodeId;
			if (_branchMap.ContainsKey(nodeId))
			{
				int currentDepth = 0;
				foreach (Guid id in _branchMap[nodeId])
					foreach (Guid eId in EnumerateNodeIdsDepthFirst(id))
					{
						yield return eId;
						currentDepth++;
						if (currentDepth > MaxDepth)
							throw new TreeGraphException($"Max depth reached ({MaxDepth}).");
					}
			}
		}
		
		protected internal bool CheckPathsMatch(int[] pathA, int[] pathB)
		{
			if (pathA.Length != pathB.Length) return false;
			for (int i = 0; i < pathA.Length; i++)
				if (pathA[i] != pathB[i]) return false;
			return true;
		}

		public TreeGraph()
		{
			_nodeMap = new Dictionary<Guid, ITreeNode<T>>();
			_stemMap = new Dictionary<Guid, Guid>();
			_branchMap = new Dictionary<Guid, HashSet<Guid>>();
			_positionMap = new Dictionary<Guid, int>();
			_valueMap = new Dictionary<Guid, T>();

			_rootId = CreateAndRegisterNode().Id;
			_positionMap.Add(_rootId, BasePosition);
		}
		
		public ITreeNode<T> Root => _nodeMap.ContainsKey(_rootId) ? _nodeMap[_rootId] : null;
		public int GetNodeCount() => _nodeMap.Count;
		public int GetHeight()
		{
			if (_nodeMap.Count == 1) return 0;
			return _nodeMap.Values.Max(n => n.GetDepth());
		}
		
		public IDictionary<Guid, ITreeNode<T>> NodeMap => _nodeMap; 
		public IDictionary<Guid, Guid> StemMap => _stemMap;
		public IDictionary<Guid, HashSet<Guid>> BranchMap => _branchMap;
		public IDictionary<Guid, int> PositionMap => _positionMap;
		public IDictionary<Guid, T> ValueMap => _valueMap;

		public IEnumerable<ITreeNode<T>> EnumerateNodes()
		{
			foreach (Guid id in EnumerateNodeIdsDepthFirst(_rootId))
				yield return GetNode(id);
		}
		public IEnumerable<ITreeNode<T>> EnumerateNodes(Guid id)
		{
			foreach (Guid nid in EnumerateNodeIdsDepthFirst(id))
				yield return GetNode(nid);
		}
		public IEnumerable<ITreeNode<T>> EnumerateNodes(IEnumerable<int> path)
		{
			foreach (Guid id in EnumerateNodeIdsDepthFirst(GetNode(path.ToList()).Id))
				yield return GetNode(id);
		}

		public ITreeNode<T> GetNode(IEnumerable<int> path)
		{
			if (path.Any())
			{
				var pathArr = path.ToArray();
				if (pathArr.Length > MaxDepth)
					throw new TreeGraphException($"Specified path length ({pathArr.Length}) is greater than graph's maximum allowed depth ({TreeGraph<T>.MaxDepth}).");

				var node = Root;
				for (int pathIdx = 0; pathIdx < pathArr.Length; pathIdx++)
				{
					int positionIdx = pathArr[pathIdx];
					if (!node.HasBranch)
						throw new TreeGraphException($"Graph does not contain specified path ({string.Join(',', path)}). \r\nNo branches found at node with subpath ({string.Join(',', node.GetPath())}).");
					var branches = node.GetBranches();
					if (branches.Length < positionIdx)
						throw new InvalidOperationException($"Path index value at position {positionIdx} ({pathIdx}) is invalid. Node only has {branches.Length} branches.");
					node = branches[positionIdx];
				}
				return node;
			}
			else
				return this.Root;
		}
		public IEnumerable<ITreeNode<T>> GetFloatingNodes()
		{
			// returns with no stem that are not root id
			return _nodeMap.Where(kv => !_stemMap.ContainsKey(kv.Key)
										&& kv.Key != _rootId)
							.Select(kv => kv.Value);
		}
		
		public ITreeNode<T> CreateNode() => CreateAndRegisterNode();
		public ITreeNode<T> CreateBranchAt(ITreeNode<T> addToNode, int position) => CreateBranchAt(addToNode.Id, position);
		public ITreeNode<T> CreateBranch(ITreeNode<T> addToNode) => CreateBranch(addToNode.Id);
		public ITreeNode<T> CreatePrevPeer(ITreeNode<T> node) => CreatePrevPeer(node.Id);
		public ITreeNode<T> CreateNextPeer(ITreeNode<T> node) => CreateNextPeer(node.Id);

		public void DeleteNode(ITreeNode<T> node) => DeleteNode(node.Id);
		public void DeleteNode(IEnumerable<int> path) => DeleteNode(GetNode(path));

		public void SetBranchAt(ITreeNode<T> stem, ITreeNode<T> branch, int position) => SetBranchAt(stem.Id, branch.Id, position);
		public void SetBranch(ITreeNode<T> stem, ITreeNode<T> branch) => SetBranch(stem.Id, branch.Id);
		public void SetFirstBranch(ITreeNode<T> stem, ITreeNode<T> branch) => SetFirstBranch(stem.Id, branch.Id);
		public void SetLastBranch(ITreeNode<T> stem, ITreeNode<T> branch) => SetLastBranch(stem.Id, branch.Id);
		public void SetPrevPeer(ITreeNode<T> node, ITreeNode<T> peer) => SetPrevPeer(node.Id, peer.Id);
		public void SetNextPeer(ITreeNode<T> node, ITreeNode<T> peer) => SetNextPeer(node.Id, peer.Id);

		public void RemoveNode(ITreeNode<T> node) => RemoveNode(node.Id);
		public void RemoveBranch(ITreeNode<T> stem, ITreeNode<T> branch) => RemoveBranch(stem.Id, branch.Id);
		public void RemoveBranchAt(ITreeNode<T> stem, int position) => RemoveBranchAt(stem.Id, position);

		public void Swap(ITreeNode<T> firstNode, ITreeNode<T> secondNode) => Swap(firstNode.Id, secondNode.Id);
		public void MoveBefore(ITreeNode<T> beforeNode, ITreeNode<T> moveNode) => MoveBefore(beforeNode.Id, moveNode.Id);
		public void MoveAfter(ITreeNode<T> afterNode, ITreeNode<T> moveNode) => MoveAfter(afterNode.Id, moveNode.Id);

		public bool NodeHasValue(ITreeNode<T> node) => NodeHasValue(node.Id);
		public T GetNodeValue(ITreeNode<T> node) => GetNodeValue(node.Id);
		public void SetNodeValue(ITreeNode<T> node, T value) => SetNodeValue(node.Id, value);
		public void DeleteNodeValue(ITreeNode<T> node) => DeleteNodeValue(node.Id);


		// ID-parameterized methods & Implementation Logic

		public ITreeNode<T> GetNode(Guid id)
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
		public void DeleteNode(Guid id)
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
				RemoveBranch(_stemMap[id], id);  // this will also remove node id from _stemMap and _positionMap and update peer positions				
				_nodeMap.Remove(id);
			}
			catch (Exception ex)
			{
				throw new TreeGraphException($"Unable to delete node ({id}).", ex);
			}
		}
		public ITreeNode<T> CreateBranchAt(Guid addToNodeId, int position)
		{
			var node = CreateNode();
			try
			{
				SetBranchAt(addToNodeId, node.Id, position);
				return node;
			}
			catch (Exception ex)
			{
				DeleteNode(node);
				throw new TreeGraphException($"Unable to create branch in position {position} at node id {addToNodeId}!", ex);
			}
			// consider adding path match check
		}
		public ITreeNode<T> CreateBranch(Guid addToNodeId)
		{
			var node = CreateNode();
			try
			{
				SetBranch(addToNodeId, node.Id);
				return node;
			}
			catch (Exception ex)
			{
				DeleteNode(node);
				throw new TreeGraphException($"Unable to create branch at node id {addToNodeId}!", ex);
			}
		}
		public ITreeNode<T> CreatePrevPeer(Guid nodeId)
		{
			var node = CreateNode();
			try
			{
				SetBranchAt(StemMap[nodeId], node.Id, PositionMap[nodeId]);
				return node;
			}
			catch (Exception ex)
			{
				DeleteNode(node);
				throw new TreeGraphException("Unable to create peer before!", ex);
			}
		}
		public ITreeNode<T> CreateNextPeer(Guid nodeId)
		{
			var node = CreateNode();
			try
			{
				SetBranchAt(StemMap[nodeId], node.Id, PositionMap[nodeId] + 1);
				return node;
			}
			catch (Exception ex)
			{
				DeleteNode(node);
				throw new TreeGraphException("Unable to create peer after!", ex);
			}
		}

		// Main logic for setting a branch. All other "set branch" methods ultimately result in a call to this method. This is to make it easier to add restrictions in derived classes.
		// After setting branch, performs check to make sure path equals expected path based on stem id and position
		public void SetBranchAt(Guid stemId, Guid branchId, int position)
		{
			try
			{
				if (position < 0) throw new InvalidOperationException($"Cannot set node position to a negative number ({position}).");
				if (!_nodeMap.ContainsKey(stemId)) throw new InvalidOperationException($"Specified stem node id {stemId} not found in tree nodes.");
				if (!_nodeMap.ContainsKey(branchId)) throw new InvalidOperationException($"Specified branch node id {branchId} not found in tree nodes.");
				if (_stemMap.ContainsKey(branchId)) throw new InvalidOperationException($"Branch id {branchId} already has a stem node associated (id {_stemMap[branchId]}).");
				if (_positionMap.ContainsKey(branchId)) throw new InvalidOperationException($"Branch node id {branchId} has a position specified in graph's position map ({_positionMap[branchId]}).");

				if (_branchMap.TryGetValue(stemId, out HashSet<Guid> branches))
				{
					// If stem node already has branches
					// Check new branch & specified position are valid then insert
					int branchCount = branches.Count();
					if (branches.Contains(branchId)) 
						throw new InvalidOperationException($"Specified branch node is already a branch node of specified stem node.");
					if (position > branchCount)
						throw new InvalidOperationException($"Specified position ({position}) is invalid because stem node only has {branches.Count()} branches. Node position uses zero-based indexing.");

					// If node is not being added to last position, update subsequent peer node positions
					if (position < branchCount)
						IncrementBranchPositionsStartingAt(stemId, position);
						//foreach (Guid branch in branches.Where(b => _positionMap.TryGetValue(b, out int pos) && pos >= position))
						//	_positionMap[branch] += 1;
					
					_branchMap[stemId].Add(branchId);
					_stemMap[branchId] = stemId;
					_positionMap[branchId] = position;
				}
				else
				{
					// If stem node does not have branches
					// Check specified position is base position then insert
					if (position != BasePosition)
						throw new InvalidOperationException($"Specified stem node {stemId} has no branches, so specified position must be {BasePosition}.");

					_branchMap[stemId] = new HashSet<Guid>() { branchId };
					_stemMap[branchId] = stemId;
					_positionMap.Add(branchId, position);
				}

				var expectedPath = _nodeMap[stemId].GetPath().ToList().Append(position).ToArray();
				if (!CheckPathsMatch(_nodeMap[branchId].GetPath(), expectedPath))
					throw new TreeGraphException($"After setting branch node, the node's path is expected to be {expectedPath} but is instead {_nodeMap[branchId].GetPath()}.");

			}
			catch (Exception ex)
			{
				throw new TreeGraphException($"Unable to add node ({branchId}) as a branch of node ({stemId}) at position ({position}).", ex);
			}
		}
		public void SetBranch(Guid stemId, Guid branchId)
		{
			int position = _branchMap.ContainsKey(stemId) ? _branchMap[stemId].Count() : BasePosition;
			SetBranchAt(stemId, branchId, position);
		}
		public void SetFirstBranch(Guid stemId, Guid branchId) => SetBranchAt(stemId, branchId, BasePosition);   
		public void SetLastBranch(Guid stemId, Guid branchId) => SetBranch(stemId, branchId);
		public void SetPrevPeer(Guid nodeId, Guid peerId)
		{
			try
			{
				if (!_stemMap.ContainsKey(nodeId))
					throw new InvalidOperationException("Specified node to add peer to does not have a stem id specified in graph's stem map.");
				if (!_positionMap.ContainsKey(nodeId))
					throw new InvalidOperationException("Specified node to add peer to does not have a position specified in graph's position map.");

				Guid stemId = _stemMap[nodeId];
				int position = _positionMap[nodeId];
				SetBranchAt(stemId, peerId, position);
			}
			catch (Exception ex)
			{
				throw new TreeGraphException($"Unable to add node ({peerId}) as peer before node ({nodeId}).", ex);
			}
		}
		public void SetNextPeer(Guid nodeId, Guid peerId)
		{
			try
			{
				if (!_stemMap.ContainsKey(nodeId))
					throw new InvalidOperationException("Specified node to add peer to does not have a stem id specified in graph's stem map.");
				if (!_positionMap.ContainsKey(nodeId))
					throw new InvalidOperationException("Specified node to add peer to does not have a position specified in graph's position map.");

				Guid stemId = _stemMap[nodeId];
				int position = _positionMap[nodeId] + 1;
				SetBranchAt(stemId, peerId, position);
			}
			catch (Exception ex)
			{
				throw new TreeGraphException($"Unable to add node ({peerId}) as peer before node ({nodeId}).", ex);
			}
		}


		protected void IncrementBranchPositionsStartingAt(Guid nodeId, int position)
		{
			foreach (Guid branchId in _branchMap[nodeId])
				if (_positionMap[branchId] >= position)
					_positionMap[branchId] += 1;
		}
		protected void DecrementBranchPositionsStartingAt(Guid nodeId, int position)
		{
			foreach (Guid branchId in _branchMap[nodeId])
				if (_positionMap[branchId] >= position)
						_positionMap[branchId] -= 1;
		}
		public void RemoveNode(Guid nodeId)
		{
			try
			{
				if (!_nodeMap.ContainsKey(nodeId)) throw new InvalidOperationException($"Node {nodeId} not found in tree.");
				if (_valueMap.ContainsKey(nodeId)) throw new InvalidOperationException($"Node {nodeId} has a value specified and cannot be removed.");
				if (!_positionMap.ContainsKey(nodeId)) throw new InvalidOperationException($"Node {nodeId} does not have a position specified in graph's position map.");
				if (!_stemMap.TryGetValue(nodeId, out Guid specifiedStemId)) throw new InvalidOperationException($"Node {nodeId} does not have a stem id specified.");
				if (_branchMap.TryGetValue(nodeId, out HashSet<Guid> branches))
					if (branches.Any()) throw new InvalidOperationException($"Cannot remove node {nodeId} as the node has at least one branch node.");

				var cstemId = _stemMap[nodeId];
				var cposition = _positionMap[nodeId];

				_positionMap.Remove(nodeId);
				_branchMap[nodeId].Remove(nodeId);
				_stemMap.Remove(nodeId);

				// If the node being removed has no peers, clear stem's branch map
				if (!_branchMap[cstemId].Any())
					_branchMap.Remove(cstemId);
				else
					DecrementBranchPositionsStartingAt(cstemId, cposition);
			}
			catch (Exception ex)
			{
				throw new TreeGraphException($"Unable to remove node ({nodeId}).", ex);
			}
		}
		public void RemoveBranch(Guid stemId, Guid branchId)
		{
			try
			{
				if (!_nodeMap.ContainsKey(stemId)) throw new InvalidOperationException($"Specified stem node id {stemId} not found in tree nodes.");
				if (!_nodeMap.ContainsKey(branchId)) throw new InvalidOperationException($"Specified branch node id {branchId} not found in tree nodes.");

				if (_valueMap.ContainsKey(branchId))
					throw new InvalidOperationException($"Branch node {branchId} has a value specified and cannot be removed from stem {stemId}.");

				if (!_positionMap.ContainsKey(branchId))
					throw new InvalidOperationException($"Can't remove branch id {branchId} because node does not have a position specified in graph's position map.");

				if (!_stemMap.TryGetValue(branchId, out Guid specifiedStemId))
					throw new InvalidOperationException($"Branch id {branchId} does not have a stem id specified.");
				if (!stemId.Equals(specifiedStemId))
					throw new InvalidOperationException($"Branch id {branchId}'s specified stem id ({_stemMap[branchId]}) does not match input stem id ({_stemMap[branchId]}).");

				if (_branchMap.TryGetValue(stemId, out HashSet<Guid> branches))
					if (!branches.Contains(branchId)) throw new InvalidOperationException($"Node {branchId} is already branch of node {stemId}.");

				_stemMap.Remove(branchId);
				_branchMap[stemId].Remove(branchId);

				if (!_branchMap[stemId].Any())
					_branchMap.Remove(stemId);
				else
				{
					// decrement peer positions
					int deletedPosition = _positionMap[branchId];
					_positionMap.Remove(branchId);
					foreach (Guid peerId in _branchMap[stemId])
						if (_positionMap.ContainsKey(peerId))
							if (_positionMap[peerId] > deletedPosition)
								_positionMap[peerId] -= 1;
				}
			}
			catch (Exception ex)
			{
				throw new TreeGraphException($"Unable to remove branch ({branchId}) from stem ({stemId}).", ex);
			}
		}
		public void RemoveBranchAt(Guid stemId, int position)
		{
			try
			{
				// Get the id of branch at specified position and use it to invoke remove branch
				if (_branchMap.TryGetValue(stemId, out HashSet<Guid> branches))
					if (branches.Where(b => _positionMap.ContainsKey(b)).Where(b => _positionMap[b] == position).Any())
					{
						Guid branchId = branches.Where(b => _positionMap.ContainsKey(b)).Where(b => _positionMap[b] == position).First();
						RemoveBranch(stemId, branchId);
					}
					else
						throw new InvalidOperationException($"Stem node {stemId} does not have a branch at position {position}.");
				else
					throw new InvalidOperationException($"Stem node {stemId} does not have any branches.");
			}
			catch (Exception ex)
			{
				throw new TreeGraphException($"Unable to remove branch at position {position} from stem node {stemId}.", ex);
			}
		}

		public void Swap(Guid firstNodeId, Guid secondNodeId)
		{
			try
			{
				if (!_nodeMap.ContainsKey(firstNodeId)) throw new InvalidOperationException($"Specified first node id {firstNodeId} not found in tree nodes.");
				if (!_nodeMap.ContainsKey(secondNodeId)) throw new InvalidOperationException($"Specified second branch node id {secondNodeId} not found in tree nodes.");
				if (_rootId.Equals(firstNodeId) || _rootId.Equals(secondNodeId)) throw new InvalidOperationException("Can't swap position of root node.");

				Guid firstStemId = _stemMap[firstNodeId];
				Guid secondStemId = _stemMap[secondNodeId];

				int firstStemPosition = _positionMap[firstNodeId];
				int secondStemPosition = _positionMap[secondNodeId];

				_stemMap[firstNodeId] = secondStemId;
				_stemMap[secondNodeId] = firstStemId;

				_positionMap[firstNodeId] = secondStemPosition;
				_positionMap[secondNodeId] = firstStemPosition;

				_branchMap[firstStemId].Remove(firstNodeId);
				_branchMap[firstStemId].Add(secondNodeId);
				_branchMap[secondStemId].Remove(secondNodeId);
				_branchMap[secondStemId].Add(firstNodeId);
			}
			catch (Exception ex)
			{
				throw new TreeGraphException($"Unable to swap positions of nodes ({firstNodeId}) and ({secondNodeId}).", ex);
			}
		}
		public void MoveBefore(Guid beforeNodeId, Guid moveNodeId) 
		{
			try
			{
				if (!_nodeMap.ContainsKey(beforeNodeId)) throw new InvalidOperationException($"Specified before node id {beforeNodeId} not found in tree nodes.");
				if (!_nodeMap.ContainsKey(moveNodeId)) throw new InvalidOperationException($"Specified move branch node id {moveNodeId} not found in tree nodes.");
				if (_rootId.Equals(beforeNodeId)) throw new InvalidOperationException($"Can't move a node before the root node.");
				if (_rootId.Equals(moveNodeId)) throw new InvalidOperationException($"Can't move a root node.");
				if (!_stemMap.ContainsKey(beforeNodeId)) throw new InvalidOperationException($"Specified before node id {beforeNodeId} does not have a stem node specified.");
				if (!_stemMap.ContainsKey(moveNodeId)) throw new InvalidOperationException($"Specified move node id {moveNodeId} does not have a stem node specified.");
				if (!_positionMap.ContainsKey(beforeNodeId)) throw new InvalidOperationException($"Specified before node id {beforeNodeId} does not have a position specified.");
				if (!_positionMap.ContainsKey(moveNodeId)) throw new InvalidOperationException($"Specified move node id {moveNodeId} does not have a position specified.");

				// If move node is already prev peer of before node, do nothing
				if (_stemMap[beforeNodeId] == _stemMap[moveNodeId] && _positionMap[beforeNodeId] == _positionMap[moveNodeId] + 1)
					return;

				Guid beforeNodeStemId = _stemMap[beforeNodeId];
				Guid moveNodeStemId = _stemMap[moveNodeId];
				
				int beforeNodePosition = _positionMap[beforeNodeId];
				int moveNodePosition = _positionMap[moveNodeId];

				_branchMap[moveNodeStemId].Remove(moveNodeId);
				DecrementBranchPositionsStartingAt(moveNodeStemId, moveNodePosition);

				IncrementBranchPositionsStartingAt(beforeNodeStemId, beforeNodePosition);
				_branchMap[beforeNodeStemId].Add(moveNodeId);
				
				_stemMap[moveNodeId] = beforeNodeStemId;
				_positionMap[moveNodeId] = beforeNodePosition;

			}
			catch (Exception ex)
			{
				throw new TreeGraphException($"Unable to move node ({moveNodeId}) before ({beforeNodeId}).", ex);
			}
		}
		public void MoveAfter(Guid afterNodeId, Guid moveNodeId)
		{
			try
			{
				if (!_nodeMap.ContainsKey(afterNodeId)) throw new InvalidOperationException($"Specified after node id {afterNodeId} not found in tree nodes.");
				if (!_nodeMap.ContainsKey(moveNodeId)) throw new InvalidOperationException($"Specified move branch node id {moveNodeId} not found in tree nodes.");
				if (_rootId.Equals(afterNodeId)) throw new InvalidOperationException($"Can't move a node before the root node.");
				if (_rootId.Equals(moveNodeId)) throw new InvalidOperationException($"Can't move a root node.");
				if (!_stemMap.ContainsKey(afterNodeId)) throw new InvalidOperationException($"Specified after node id {afterNodeId} does not have a stem node specified.");
				if (!_stemMap.ContainsKey(moveNodeId)) throw new InvalidOperationException($"Specified move node id {moveNodeId} does not have a stem node specified.");
				if (!_positionMap.ContainsKey(afterNodeId)) throw new InvalidOperationException($"Specified after node id {afterNodeId} does not have a position specified.");
				if (!_positionMap.ContainsKey(moveNodeId)) throw new InvalidOperationException($"Specified move node id {moveNodeId} does not have a position specified.");


				Guid afterNodeStemId = _stemMap[afterNodeId];
				Guid moveNodeStemId = _stemMap[moveNodeId];

				int afterNodePosition = _positionMap[afterNodeId];
				int moveNodePosition = _positionMap[moveNodeId];

				// If move node is already next peer of after node, do nothing
				if (moveNodeId == afterNodeStemId && moveNodePosition + 1 == afterNodePosition)
					return;
				
				_branchMap[moveNodeStemId].Remove(moveNodeId);
				DecrementBranchPositionsStartingAt(moveNodeStemId, moveNodePosition);

				IncrementBranchPositionsStartingAt(afterNodeStemId, afterNodePosition + 1);
				_branchMap[afterNodeStemId].Add(moveNodeId);

				_stemMap[moveNodeId] = afterNodeStemId;
				_positionMap[moveNodeId] = afterNodePosition + 1;
			}
			catch (Exception ex)
			{
				throw new TreeGraphException($"Unable to move node ({moveNodeId}) after ({afterNodeId}).", ex);
			}
		}

		public bool NodeHasValue(Guid id) => _valueMap.ContainsKey(id);
		public T GetNodeValue(Guid id)
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
		public void SetNodeValue(Guid id, T value)
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
		public void DeleteNodeValue(Guid id)
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

		/// <summary>
		/// Checks implementation to make sure methods should all work
		/// </summary>
		public void Validate()
		{
			// Check graph mapping dictionaries to confirm all specified ids are present in Node Map and there are no "dangling" nodes (not included in maps when they should be)
			if (GetFloatingNodes().Count() != 0) throw new TreeGraphException("Graph contains nodes with no stem specified. All nodes except the root should have a stem node.");

			int nodeCount = _nodeMap.Count();
			if (nodeCount < 1) throw new TreeGraphException("Tree has no nodes and expected to have at least one node (the root).");
			
			// Stem Map
			int nodeStemCount = _stemMap.Count();
			int unmappedStemKeyCount = _stemMap.Keys.Where(n => !_nodeMap.ContainsKey(n)).Count();
			int unmappedStemValueCount = _stemMap.Values.Where(n => !_nodeMap.ContainsKey(n)).Count();

			if (nodeStemCount < nodeCount - 1)
				throw new TreeGraphException($"Graph has {nodeCount} nodes total and {nodeStemCount} nodes in stem map. Stem node count should be one less than total node count (because root node has no stem).");
			if (unmappedStemKeyCount > 0)
				throw new TreeGraphException($"Graph has {unmappedStemKeyCount} nodes referenced in stem map keys and missing from total node map.");
			if (unmappedStemValueCount > 0)
				throw new TreeGraphException($"Graph has {unmappedStemValueCount} nodes referenced in stem map values and missing from total node map.");

			// Branch Map			
			int nodeBranchCount = _branchMap.Values.Sum(v => v.Count);
			int unmappedBranchKeyCount = _branchMap.Keys.Where(n => !_nodeMap.ContainsKey(n)).Count();
			int unmappedBranchValueCount = _branchMap.Values.SelectMany(b => b).Where(b => !_nodeMap.ContainsKey(b)).Count();
			int duplicateBranchCount = _branchMap.Values.SelectMany(b => b).ToLookup(b => b).Where(group => group.Count() > 1).Count();

			if (nodeBranchCount != nodeStemCount)
				throw new TreeGraphException($"There are {nodeBranchCount} nodes referenced in branch map values and {nodeStemCount} nodes referenced in stem map keys. These values should be equal.");
			if (unmappedBranchKeyCount > 0)
				throw new TreeGraphException($"There are {unmappedBranchKeyCount} nodes referenced in branch map keys and missing from total node map.");
			if (unmappedBranchValueCount > 0)
				throw new TreeGraphException($"There are {unmappedBranchValueCount} nodes referenced in branch map values and missing from total node map.");
			if (duplicateBranchCount > 0)
				throw new TreeGraphException($"There are {duplicateBranchCount} duplicate nodes in branch map values. Each branch node id should only be included once, as branches can only have one stem.");

			// Position Map
			int positionMapCount = _positionMap.Count();
			int unmappedPositionCount = _stemMap.Keys.Where(n => !_nodeMap.ContainsKey(n)).Count();

			if (positionMapCount != nodeCount)
				throw new TreeGraphException($"Graph has {nodeCount} nodes total and {positionMapCount} nodes referenced in position map. These values should be equal (each node should have a position specified).");
			if (unmappedPositionCount > 0)
				throw new TreeGraphException($"There are {unmappedPositionCount} nodes referenced in position map keys and missing from total node map.");

			// Branch Position Order
			// Check that there are no duplicate or negative positions and that they are all consecutive integers
			foreach (KeyValuePair<Guid, HashSet<Guid>> kv in _branchMap)
			{
				if (!kv.Value.Any()) 
					continue;

				int numBranches = kv.Value.Count();
				int[] positions = kv.Value.Select(b => _positionMap[b]).ToArray();

				if (positions.Length == 1)
				{
					if (positions[0] != BasePosition)
						throw new TreeGraphException($"Node {kv.Key} has only one branch, but its specified position ({positions[0]}) is not the base position ({BasePosition}).");
				}
				else
				{
					if (positions.ToLookup(p => p).Where(group => group.Count() > 1).Any())
						throw new TreeGraphException($"Branches of node {kv.Key} have duplicate positions specified in position map.");
					if (positions.Where(p => p < 0).Any())
						throw new TreeGraphException($"There are negative positions specified for branches of node {kv.Key}.");
					if (positions.Sum() != Enumerable.Range(1, numBranches - 1).Sum())
						throw new TreeGraphException($"Branch positions of node {kv.Key} are not a sequence of consecutive integers starting with {BasePosition}. Branch positions are: {string.Join(',', positions.OrderBy(p => p))}.");
				}

			}

			// Value Map
			int valueMapCount = _valueMap.Count();
			int unmappedValueCount = _stemMap.Keys.Where(n => !_nodeMap.ContainsKey(n)).Count();
			
			if (valueMapCount > nodeCount)
				throw new TreeGraphException($"Graph has {nodeCount} nodes total and {valueMapCount} nodes with a value specified in value map. There should not be more nodes in value map than graph.");
			if (unmappedValueCount > 0)
				throw new TreeGraphException($"There are {unmappedValueCount} nodes referenced in value map keys and missing from total node map.");

		}

	}

	public class TreeGraphException : Exception
	{
		public TreeGraphException() : base() { }
		public TreeGraphException(string message) : base(message) { }
		public TreeGraphException(string message, Exception innerException) : base(message, innerException) { }
	}

}
#endregion



#region Data Tree

namespace DmsEnum
{
	public enum DataTreeLayoutType
	{
		Hierarchal,
		Tabular,
		Columnar
	}
}
/// <summary>
/// Data tree tree graph is specialized for tracking data
/// </summary>
public interface IDataTree : ITreeGraph<object>
{
	public DmsEnum.DataTreeLayoutType Layout { get; }
}

/// <summary>
/// DataTree is a modified tree graph, only allows values to be set for leaf nodes
/// </summary>
public class DataTree : TreeGraph<object>, IDataTree
{
	public DmsEnum.DataTreeLayoutType Layout { get; }
	public DataTree(DataTreeLayoutType layout) : base()
	{
		Layout = layout;
	}
	// Overridden Tree Graph methods
	public new void SetBranchAt(Guid stemId, Guid branchId, int position)
	{
		if (NodeHasValue(stemId))
			throw new DataTreeException($"Can't add a branch to node {stemId} because this node has a value specified. Data tree only allows values in leaf nodes.");
		base.SetBranchAt(stemId, branchId, position);
	}
	public new void SetNodeValue(Guid id, object value)
	{
		if (_branchMap.TryGetValue(id, out HashSet<Guid> branchIds) && branchIds.Any())
			throw new DataTreeException($"Can't set a value for node {id} because the node has {branchIds.Count} branch node(s). Data tree only allows values in leaf nodes.");
		base.SetNodeValue(id, value);
	}
	public new void Validate()
	{
		if (_valueMap.Keys.Join(_branchMap.Keys, v => v, b => b, (v, b) => v).Any())
			throw new DataTreeException($"There are {_valueMap.Keys.Join(_branchMap.Keys, v => v, b => b, (v, b) => v).Count()} non-leaf nodes with a value specified. Data tree only allows values in leaf nodes.");
		base.Validate();
	}
}

public class DataTreeException : Exception
{
	public DataTreeException() : base() { }
	public DataTreeException(string message) : base(message) { }
	public DataTreeException(string message, Exception innerException) : base(message, innerException) { }
}

#endregion



/// <summary>
/// Data source encompasses storage and retrieval of data
/// </summary>
#region DataSource


public class DataStoreException : Exception
{
	public DataStoreException() : base() { }
	public DataStoreException(string message) : base(message) { }
	public DataStoreException(string message, Exception innerException) : base(message, innerException) { }
}
public class DataFormatException : Exception
{
	public DataFormatException() : base() { }
	public DataFormatException(string message) : base(message) { }
	public DataFormatException(string message, Exception innerException) : base(message, innerException) { }
}
public class DataReaderException : Exception
{
	public DataReaderException() : base() { }
	public DataReaderException(string message) : base(message) { }
	public DataReaderException(string message, Exception innerException) : base(message, innerException) { }
}


namespace DmsEnum
{
	/// <summary>
	/// Data sources which can be read from
	/// </summary>
	public enum StoreType
	{
		RawInput,   // i.e. hard-coded string
		File,
		Database,
		Api,
	}

	/// <summary>
	/// Data storage formats used by data source
	/// </summary>
	public enum DataFormatType
	{
		Dsv,    // delimitted string values
		Json,
		Xml,
		PDBx_mmCIF,
		Table,
		Query,
	}

	public partial class DmsEnumExtensions
	{
		public static HashSet<DataFormatType> GetFormatTypes(this StoreType st) => StoreFormatTypeMap[st];
		
		public static IDataStore GetSourceObject(this StoreType st) => StoreObjectConstructorMap[st]();
		//public static IDataStoreFormat GetFormatObject(this DataFormatType ft) => FormatObjectConstructorMap[ft]();

		private static readonly Dictionary<StoreType, HashSet<DataFormatType>> StoreFormatTypeMap
		= new Dictionary<StoreType, HashSet<DataFormatType>>()
		{
			{ StoreType.RawInput,    new HashSet<DataFormatType>() { DataFormatType.Dsv, DataFormatType.Json, DataFormatType.Xml } },
			{ StoreType.File,        new HashSet<DataFormatType>() { DataFormatType.Dsv, DataFormatType.Json, DataFormatType.Xml } },
			{ StoreType.Database,    new HashSet<DataFormatType>() { DataFormatType.Table, DataFormatType.Query } },
			{ StoreType.Api,         new HashSet<DataFormatType>() },
		};

		private static readonly Dictionary<StoreType, Func<IDataStore>> StoreObjectConstructorMap
		= new Dictionary<StoreType, Func<IDataStore>>()
		{
			{ StoreType.RawInput,     () => new DataStore.RawInput() } ,
			{ StoreType.File,         () => new DataStore.File() } ,
			//{ SourceType.Database,     () => new  } ,
			//{ SourceType.Api,          () => new  } 
		};

		//private static readonly Dictionary<DataFormatType, Func<IDataStoreFormat>> FormatObjectConstructorMap
		//= new Dictionary<DataFormatType, Func<IDataStoreFormat>>()
		//{
		//	{ DataFormatType.Dsv,     () => new SourceData.SourceFormat.Dsv() } ,
		//	{ DataFormatType.Json,    () => new SourceData.SourceFormat.Json() } ,
		//	//{ SourceType.Database,     () => new  } ,
		//	//{ SourceType.Api,          () => new  } 
		//};
	}
}

/// <summary>
/// IDataStore implementations contains all info necessary for confirming store exists and can be read from
/// </summary>
public interface IDataStore
{
	public DmsEnum.StoreType StoreType { get; }
	
	public bool Exists();
	public bool CanRead();
	public bool CanWrite();
	public Stream GetStream();

	public Task<bool> ExistsAsync();
	public Task<bool> CanReadAsync();
	public Task<bool> CanWriteAsync();
	public Task<Stream> GetStreamAsync();
}
public interface IRawInputDataStore : IDataStore
{
	public string Input { get; set; }
	public Encoding Encoding { get; set; }
}
public interface IFileDataStore : IDataStore
{
	public string Directory { get; set; }
	public string FileName { get; set; }

	public string FilePath { get; }
}
public interface IDatabaseDataStore : IDataStore
{
	public string Server { get; set; }
	public string Database { get; set; }
}

public abstract partial class DataStore : IDataStore
{
	// Static constructor
	public static IDataStore New(StoreType st)
	{
		try { 
			return StoreObjectConstructorMap[st](); 
		}
		catch (Exception ex)
		{
			throw new DataStoreException($"Unable to create new configurable data store of store type {st}.", ex);
		}
	}
	private static readonly Dictionary<StoreType, Func<IDataStore>> StoreObjectConstructorMap
	= new Dictionary<StoreType, Func<IDataStore>>()
	{
		{ StoreType.RawInput,     () => new DataStore.RawInput() } ,
		{ StoreType.File,         () => new DataStore.File() } ,
		//{ SourceType.Database,     () => new  } ,
		//{ SourceType.Api,          () => new  } 
	};
	
	// IDataStore Interface implementation
	public abstract StoreType StoreType { get; }

	public Stream GetStream()
	{
		try
		{
			return GetStreamImplementation();
		}
		catch (Exception ex)
		{
			throw new DataStoreException("Error when attempting to stream data from data store.", ex);
		}
	}
	public bool Exists()
	{
		try
		{
			return ExistsImplementation();
		}
		catch (Exception ex)
		{
			throw new DataStoreException("Error when attempting to check data store exists.", ex);
		}
	}
	public bool CanRead()
	{
		try
		{
			return CanReadImplementation();
		}
		catch (Exception ex)
		{
			throw new DataStoreException("Error when attempting to check whether data store can be read from.", ex);
		}
	}
	public bool CanWrite()
	{
		try
		{
			return CanWriteImplementation();
		}
		catch (Exception ex)
		{
			throw new DataStoreException("Error when attempting to check whether data store can be written to.", ex);
		}
	}

	public async Task<Stream> GetStreamAsync()
	{
		try 
		{ 
			return await GetStreamAsyncImplementation();
		}
		catch (Exception ex) 
		{ 
			throw new DataStoreException("Error when attempting to asynchronously stream data from data store.", ex);
		}
	}
	public async Task<bool> ExistsAsync()
	{
		try
		{
			return await ExistsAsyncImplementation();
		}
		catch (Exception ex)
		{
			throw new DataStoreException("Error when attempting to asynchronously check data store exists.", ex);
		}
	}
	public async Task<bool> CanReadAsync()
	{
		try
		{
			return await CanReadAsyncImplementation();
		}
		catch (Exception ex)
		{
			throw new DataStoreException("Error when attempting to asynchronously check whether data store can be read from.", ex);
		}
	}
	public async Task<bool> CanWriteAsync()
	{
		try
		{
			return await CanWriteAsyncImplementation();
		}
		catch (Exception ex)
		{
			throw new DataStoreException("Error when attempting to asynchronously check whether data store can be written to.", ex);
		}
	}

	// Optionally implement these in concrete classes
	protected virtual Stream GetStreamImplementation() => Task.Run(() => GetStreamAsyncImplementation()).GetAwaiter().GetResult();
	protected virtual bool ExistsImplementation() => Task.Run(() => ExistsAsyncImplementation()).GetAwaiter().GetResult();
	protected virtual bool CanReadImplementation() => Task.Run(() => CanReadAsyncImplementation()).GetAwaiter().GetResult();
	protected virtual bool CanWriteImplementation() => Task.Run(() => CanWriteAsyncImplementation()).GetAwaiter().GetResult();

	// Implement these in concrete classes
	protected abstract Task<Stream> GetStreamAsyncImplementation();
	protected abstract Task<bool> ExistsAsyncImplementation();
	protected abstract Task<bool> CanReadAsyncImplementation();
	protected abstract Task<bool> CanWriteAsyncImplementation();
}

// Data store implementations stored here, class as namespace, probably bad idea for wasm
public abstract partial class DataStore : IDataStore
{

	public class RawInput : DataStore, IRawInputDataStore
	{
		public string Input { get; set; }
		public Encoding Encoding { get; set; }

		public override StoreType StoreType => StoreType.RawInput;
		protected override Task<Stream> GetStreamAsyncImplementation() => Task.FromResult((Stream)new MemoryStream(this.Encoding.GetBytes(Input)));
		protected override Task<bool> ExistsAsyncImplementation() => Task.FromResult(!String.IsNullOrWhiteSpace(Input));
		protected override Task<bool> CanReadAsyncImplementation() => Task.FromResult(!String.IsNullOrWhiteSpace(Input));
		protected override Task<bool> CanWriteAsyncImplementation() =>  Task.FromResult(false);
	}
	
	public class File : DataStore, IFileDataStore
	{
		public string Directory { get; set; }
		public string FileName { get; set; }

		public string FilePath => Directory != null && FileName != null ? Path.Combine(Directory, FileName) : null;
		
		public override StoreType StoreType => StoreType.File;
		protected override async Task<Stream> GetStreamAsyncImplementation()
		{
			// Not efficent for big data
			MemoryStream memoryStream = new MemoryStream();
			try
			{
				using FileStream fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
				await fileStream.CopyToAsync(memoryStream);
			}
			catch (Exception ex)
			{
				memoryStream.Dispose();
				throw ex;
			}
			memoryStream.Position = 0;
			return memoryStream;
		}
		protected override Task<bool> ExistsAsyncImplementation()
		{
			FilePath.Dump();
			return Task.FromResult(Path.Exists(FilePath));
		}
		protected override async Task<bool> CanReadAsyncImplementation() 
		{
			if (!Path.Exists(FilePath))
				return false;
			try
			{
				using FileStream fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
				await fileStream.ReadAsync(new byte[1], 0, 1);
				return true;
			}
			catch (UnauthorizedAccessException) { }
			catch (SecurityException) { }
			catch (IOException) { }
			
			return false;
		}
		protected override async Task<bool> CanWriteAsyncImplementation()
		{
			if (!System.IO.Directory.Exists(Directory))
				return false;

			if (Path.Exists(FilePath))
			{
				try
				{
					using FileStream fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Write, FileShare.None);
					await Task.CompletedTask; // No write operation, just to keep the method async
					return true;
				}
				catch (UnauthorizedAccessException) { }
				catch (SecurityException) { }
				catch (IOException) { }
			}
			else
			{
				try
				{
					using FileStream fileStream = new FileStream(FilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
					await Task.CompletedTask; // No write operation, just to keep the method async
					return true;
				}
				catch (UnauthorizedAccessException) { }
				catch (SecurityException) { }
				catch (IOException) { }
				finally
				{
					if (Path.Exists(FilePath))
					{
						System.IO.File.Delete(FilePath);
					}
				}
			}

			return false;
		}

	}

	public interface DatabaseDataStore : IDataStore
	{
		public string Server { get; set; }
		public string Database { get; set; }
		// authentication
	}
}


namespace DataStoreTests
{

	public abstract class DataStoreTests
	{
		public abstract StoreType StoreType { get; }
		public abstract IDataStore GetNewConfigurableDataStore();
		public abstract void ConstructorTest();
		public abstract void DataStoreStaticConstructorTest();
		public abstract void ExistsTest();
		public abstract void CanReadTest();
		public abstract void CanWriteTest();
		public abstract void GetStreamTest();


		public void RunAllTests()
		{
			ConstructorTest();
			DataStoreStaticConstructorTest();
			ExistsTest();
			CanReadTest();
			CanWriteTest();
			GetStreamTest();
		}
	}

	public class RawInputTests : DataStoreTests
	{
		public override StoreType StoreType => StoreType.RawInput;
		public override IDataStore GetNewConfigurableDataStore()
		{
			return new DataStore.RawInput()
			{
				Input = "Hello, World",
				Encoding = Encoding.Unicode
			};
		}

		public override void ConstructorTest()
		{
			var s = (DataStore.RawInput)GetNewConfigurableDataStore();
			Debug.Assert(s.Input != null);
			Debug.Assert(s.Encoding == Encoding.Unicode);
		}
		
		public override void DataStoreStaticConstructorTest()
		{
			var store = DataStore.New(StoreType);
			var s = (DataStore.RawInput)store;
			Debug.Assert(s.Input is null);
			Debug.Assert(s.Encoding is null);
		}


		public override void ExistsTest()
		{
			var s1 = (DataStore.RawInput)DataStore.New(StoreType);
			Debug.Assert(!s1.Exists());

			var s2 = (DataStore.RawInput)GetNewConfigurableDataStore();
			Debug.Assert(s2.Exists());
		}
		public override void CanReadTest()
		{
			var s1 = (DataStore.RawInput)DataStore.New(StoreType);
			Debug.Assert(!s1.CanRead());

			var s2 = (DataStore.RawInput)GetNewConfigurableDataStore();
			Debug.Assert(s2.CanRead());
		}
		public override void CanWriteTest()
		{
			var s1 = (DataStore.RawInput)DataStore.New(StoreType);
			Debug.Assert(!s1.CanWrite());

			var s2 = (DataStore.RawInput)GetNewConfigurableDataStore();
			Debug.Assert(!s2.CanWrite());
		}
		public override void GetStreamTest()
		{
			var s1 = (DataStore.RawInput)GetNewConfigurableDataStore();
			using (var stream = s1.GetStream())
			{
				string str = stream.ToString();
				Debug.Equals(str, "Hello, World");
			}
			
		}

	}


	public class FileTests : DataStoreTests
	{
		public override StoreType StoreType => StoreType.File;
		
		public FileTests()
		{
			
			TmpDir = Path.Join(Directory.GetCurrentDirectory(), Guid.NewGuid().ToString());
			TestFileName = "testfile.csv";
			
			var n = Environment.NewLine;
			TestFileContent = $"Col1,Col2,Col3{n}A,12,90{n}B,1,3{n}";
			
			WriteTestFile();
		}
		
		private string TmpDir { get; }
		private string TestFileName { get; }
		private string TestFileContent { get; }
		private string GetTestFilePath() => Path.Join(TmpDir, TestFileName);

		public void Dispose()
		{
			DeleteFile();
			GC.SuppressFinalize(this);
		}
		
		public void WriteTestFile()
		{			 
			Directory.CreateDirectory(TmpDir);
			using (FileStream fileStream = new FileStream(GetTestFilePath(), FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
			using (StreamWriter writer = new StreamWriter(fileStream, Encoding.UTF8))
			{
				writer.Write(TestFileContent);
			}
		}
		
		private void DeleteFile()
		{
			if (File.Exists(GetTestFilePath())) { File.Delete(GetTestFilePath()); }
			if (Directory.Exists(TmpDir)) { Directory.Delete(TmpDir); }
		}

		~FileTests()
		{
			DeleteFile();
		}
		

		
		
		public override IFileDataStore GetNewConfigurableDataStore()
		{
			return (IFileDataStore) new DataStore.File()
			{
				Directory = TmpDir,
				FileName = TestFileName
			};
		}

		public override void ConstructorTest()
		{
			var s = GetNewConfigurableDataStore();
			Debug.Equals(s.Directory, TmpDir);
			Debug.Equals(s.FileName, TestFileName);
		}

		public override void DataStoreStaticConstructorTest()
		{
			var store = DataStore.New(StoreType);
			var s = (DataStore.File)store;
			Debug.Assert(s.Directory is null);
			Debug.Assert(s.FileName is null);
		}


		public override void ExistsTest()
		{
			var s1 = (DataStore.File)DataStore.New(StoreType);
			Debug.Assert(!s1.Exists());

			var s2 = GetNewConfigurableDataStore();
			Debug.Assert(s2.Exists());
		}
		public override void CanReadTest()
		{
			var s1 = (DataStore.File)DataStore.New(StoreType);
			Debug.Assert(!s1.CanRead());

			var s2 = GetNewConfigurableDataStore();
			Debug.Assert(s2.CanRead());
		}
		public override void CanWriteTest()
		{
			// Check can't write to file if directory dne
			var s1 = (DataStore.File)DataStore.New(StoreType);
			Debug.Assert(!s1.CanWrite());

			// Check can write to file if does exist
			var s2 = GetNewConfigurableDataStore();
			Debug.Assert(s2.CanWrite());
		}
		public override void GetStreamTest()
		{
			var s1 = GetNewConfigurableDataStore();
			using (var stream = s1.GetStream())
			{
				string str = stream.ToString();
				Debug.Equals(str, TestFileContent);
			}

		}

	}
}
/*
/// <summary>
/// IDataFormat specifies info necessary for translating data from a data store into a data tree (instructions for reading source)
/// </summary>
public interface IDataFormat
{
	public DmsEnum.DataFormatType DataFormatType { get; }
}
public interface IDsvDataFormat: IDataFormat
{
	public bool HasHeaders { get; set; }
	public char Delimitter { get; set; }
	public char Escaper { get; set; }
	public string LineBreak { get; set; }
}
public interface IJsonDataFormat : IDataFormat
{
	public int MaxDepth { get; set; }
	public bool IncludeComments { get; set; }
}


/// <summary>
/// Data query returns a data tree
/// </summary>
public interface IDataSource
{
	public IDataStore Store { get; }
	public IDataFormat Format { get; }

}

namespace SourceData
{

	// Data format abstract implementation
	public abstract partial class DataSourceFormat : IDataStoreFormat
	{
		public abstract SourceFormatType SourceFormatType { get; }
	}
	// Data format concrete implementations (for all FormatType enum)
	public abstract partial class DataSourceFormat : IDataStoreFormat
	{
		public class Dsv : DataSourceFormat
		{
			public override DmsEnum.SourceFormatType SourceFormatType => DmsEnum.SourceFormatType.Dsv;

			// Configurable
			public bool HasHeaders { get; set; }
			public char Delimitter { get; set; }
			public char Escaper { get; set; }
			public string LineBreak { get; set; }
		}

		public class Json : DataSourceFormat
		{
			public override DmsEnum.SourceFormatType SourceFormatType => DmsEnum.SourceFormatType.Json;

			// Configurable
			public JsonDocumentOptions JsonDocumentOptions { get; set; }
		}
		
		//public class Xml : DataFormat
		//{
		//	public override DmsEnum.FormatType FormatType => DmsEnum.FormatType.Xml;
		//}
	}

	
	// Data source reader abstract implementation
	public abstract partial class DataReader : IDataReader
	{
		public IDataStore Source { get; set; }
		public IDataFormat Format { get; set; }

		public IDataTree GetTree()
		{
			try 
			{
				using (var stream = Source.GetStream())
				{
					return GetTreeImplementation(stream);
				}
			}
			catch (Exception ex) { throw new DataReaderException("Error when attempting to read data data source stream to data tree.", ex); }
		}

		/// <summary>
		/// Source-specific, defines stream-reading implementation
		/// </summary>
		protected abstract IDataTree GetTreeImplementation(Stream stream);
	}
	
	
	/// <summary>
	/// Delimitter-Separated Values
	/// </summary>
	public class Dsv : DataFormat
	{
		// Mostly conforms to https://www.rfc-editor.org/rfc/rfc4180
		public bool HasHeaders { get; set; }
		public char Delimitter { get; set; }
		public char Escaper { get; set; }
		public string LineBreak { get; set; }

		public override DmsEnum.FormatType DataFormat => DmsEnum.FormatType.Dsv;

		public override void ValidateFormat()
		{
			throw new NotImplementedException();
		}

		protected override ITreeGraph<Guid, object> GetTreeImplementation(IDataStore source)
		{
			var tree = new TreeGraph<Guid, object>(new GuidFactory());

			using (var stream = source.GetStream())
			using (var reader = new StreamReader(stream))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					var newRow = tree.CreateAndRegisterNode();
					tree.SetBranch(tree.Root, newRow);

					foreach (var nodeVal in ParseDelimittedStringLine(line))
					{
						var newNode = tree.CreateAndRegisterNode();
						tree.SetBranch(newRow, newNode);
						newNode.SetValue(nodeVal);
					}
				}
			}
			return tree;
		}

		private IEnumerable<string> ParseDelimittedStringLine(string input)
		{
			var state = new ParserState()
			{
				ValueChars = new List<char>(),
				InValue = false,
				Escaped = false,
				PrevChar = null
			};

			// Enumerate characters, yielding a string whenever a new delimitter is encounterd
			for (int i = 0; i < input.Length; i++)
			{
				char c = input[i];

				// Parse through, aggregate chars, write to list when delimitter is encountered (unless escaped)
				if (c == Delimitter && !state.Escaped)
				{
					if (state.ValueChars.Any())
					{
						yield return string.Join("", state.ValueChars);
						state.ValueChars.Clear();
					}
					else
					{
						yield return null;
					}
				}
				else
				{
					if (c == Escaper && state.Escaped)
					{
						state.Escaped = false;
					}
					else if (c == Escaper && !state.Escaped)
					{
						state.Escaped = true;

						// If prev char was the escaper, that means that it's a double quote string (so add escaper char to values list and resume escape)
						if (state.PrevChar == Escaper)
							state.ValueChars.Add(c);
					}
					else
					{
						state.ValueChars.Add(c);
					}
				}

				// If there are no more characters then return whatever is held in values at the moment
				if (i + 1 == input.Length)
					if (state.ValueChars.Any())
						yield return string.Join("", state.ValueChars);
					else
						yield return null;

				state.PrevChar = c;
				//char c = str[i];

			}
		}

		private class ParserState
		{
			public List<char> ValueChars { get; set; }
			public bool InValue { get; set; }
			public bool Escaped { get; set; }
			public char? PrevChar { get; set; }
		}

	}


	/// <summary>
	/// Json
	/// </summary>
	public class Json 
	{

		public override DmsEnum.FormatType DataFormat => DmsEnum.FormatType.Json;
		public JsonDocumentOptions JsonDocumentOptions { get; set; }

		public override void ValidateFormat()
		{
			throw new NotImplementedException();
		}

		protected override ITreeGraph<Guid, object> GetTreeImplementation(IDataStore source)
		{
			var tree = new TreeGraph<Guid, object>(new GuidFactory());

			using (var stream = source.GetStream())
			using (JsonDocument doc = JsonDocument.Parse(stream, JsonDocumentOptions))
			{
				ParseRecursive(tree, tree.Root.Id, doc.RootElement);
			}
			return tree;
		}

		private void ParseRecursive(ITreeGraph<Guid, object> tree, Guid currentNodeId, JsonElement element)
		{
			if (element.ValueKind == JsonValueKind.Object)
			{
				ParseJsonObject(tree, currentNodeId, element);
			}
			else if (element.ValueKind == JsonValueKind.Array)
			{
				ParseJsonArray(tree, currentNodeId, element);
			}
			else
			{
				ParseJsonValue(tree, currentNodeId, element);
			}

		}

		private void ParseJsonObject(ITreeGraph<Guid, object> tree, Guid currentNodeId, JsonElement element)
		{
			var record = tree.CreateAndRegisterNode();
			tree.SetBranch(currentNodeId, record.Id);

			foreach (JsonProperty prop in element.EnumerateObject())
			{
				var field = tree.CreateAndRegisterNode();
				tree.SetBranch(record, field);

				var label = tree.CreateAndRegisterNode();
				tree.SetBranch(field, label);
				label.SetValue(prop.Name);

				ParseRecursive(tree, field.Id, prop.Value);
			}
		}

		private void ParseJsonArray(ITreeGraph<Guid, object> tree, Guid currentNodeId, JsonElement element)
		{
			var list = tree.CreateAndRegisterNode();
			tree.SetBranch(currentNodeId, list.Id);

			foreach (JsonElement item in element.EnumerateArray())
			{
				ParseRecursive(tree, list.Id, item);
			}
		}

		private void ParseJsonValue(ITreeGraph<Guid, object> tree, Guid currentNodeId, JsonElement element)
		{
			if (!JsonValueKindValue.Contains(element.ValueKind))
				throw new InvalidDataException("Element is expected to be a value but does not have expected JsonValueKind.");

			var value = tree.CreateAndRegisterNode();
			tree.SetBranch(currentNodeId, value.Id);
			if (element.ValueKind == JsonValueKind.String)
				value.SetValue(element.ToString());
			else if (element.ValueKind == JsonValueKind.Number)
			{
				if (element.TryGetInt64(out Int64 i64))
					value.SetValue(i64);
				else if (element.TryGetDouble(out double doub))
					value.SetValue(doub);
				else if (element.TryGetDecimal(out Decimal dec))
					value.SetValue(dec);
				else
					throw new InvalidOperationException($"Can't parse value ({element.ToString()} as numeric for JsonValueKind {element.ValueKind}.");
			}
			else if (element.ValueKind == JsonValueKind.True)
				value.SetValue(true);
			else if (element.ValueKind == JsonValueKind.False)
				value.SetValue(false);
			else if (element.ValueKind == JsonValueKind.Null)
				value.SetValue(null);
			else if (element.ValueKind == JsonValueKind.Object || element.ValueKind == JsonValueKind.Array)
				throw new InvalidOperationException($"Can't parse json value for JsonValueKind {element.ValueKind}.");
			else
				throw new InvalidOperationException($"Unexpected JsonValueKind encountered ({element.ValueKind}).");

			//value.SetValue(element.ToString());
			//value.SetValue(element.GetBytesFromBase64());

			//value.SetValue(element.ToString());

		}

		private static HashSet<JsonValueKind> JsonValueKindValue = new HashSet<JsonValueKind>()
			{
				{ JsonValueKind.Null        },
				{ JsonValueKind.Undefined   },
				{ JsonValueKind.String      },
				{ JsonValueKind.Number      },
				{ JsonValueKind.True        },
				{ JsonValueKind.False       },
				//{ JsonValueKind.Object      },
				//{ JsonValueKind.Array       },
			};
	}


	/// <summary>
	/// Get whatever the requested data is and read it as a stream. 
	/// Before getting data, validate configuration and ability to connect to source
	/// </summary>
	public abstract class DataReaderBase : IDataReader
	{
		/// <summary>
		/// Each enum value in DataSource has a corresponding class which conforms to the IDataStore interface
		/// </summary>
		public abstract DmsEnum.SourceType Source { get; }

		/// <summary>
		/// GetStream() returns a stream and implements generalized error-handling
		/// </summary>
		public Stream GetStream()
		{
			// Add this back when configurable object is in a better spot
			//try { ValidateConfiguration(); }
			//catch (Exception ex) { throw new DataReaderException("Error when attempting to validate data source configuration.", ex); }

			try { ValidateCanRead(); }
			catch (Exception ex) { throw new DataReaderException("Unable to read data source.", ex); }

			try { return GetStreamImplementation(); }
			catch (Exception ex) { throw new DataReaderException("Error when attempting to read data stream from data source.", ex); }
		}

		/// <summary>
		/// Source-specific, checks if source is accessible and throw descriptive error if inaccessible 
		/// </summary>
		public abstract void ValidateCanRead();

		/// <summary>
		/// Source-specific, defines stream-reading implementation
		/// </summary>
		protected abstract Stream GetStreamImplementation();

		/// <summary>
		/// Source-specific, defines stream-reading implementation
		/// </summary>
		protected abstract IDataTree GetTreeImplementation();
	}


	/// <summary>
	/// Data source is a user-specified input string
	/// </summary>
	public class RawInput : DataReaderBase, IDataReader
	{
		// Config
		public string Text { get; set; }
		public Encoding TextEncoding { get; set; }

		// Data source implementation
		public override DmsEnum.SourceType Source => DmsEnum.SourceType.RawInput;

		public override void ValidateSource()
		{
			if (string.IsNullOrEmpty(Text))
				throw new DataReaderException("Can't read from source because InputText value is null or empty.");
		}
		protected override Stream GetStreamImplementation() => new MemoryStream(TextEncoding.GetBytes(Text));

		protected override IDataTree GetTreeImplementation(IDataFormat source)
		{

		}
	}

	/// <summary>
	/// Data source is a file
	/// </summary>
	public class File : DataReaderBase, IDataReader
	{
		// Config
		public string Directory { get; set; }
		public string FileName { get; set; }

		// Data source implementation	
		public override DmsEnum.SourceType Source => DmsEnum.SourceType.File;

		public override void ValidateSource()
		{
			if (!System.IO.Directory.Exists(Directory))
				throw new DataReaderException($"Specified directory [{Directory}] does not exist.");
			if (!Path.Exists(GetFilePath()))
				throw new DataReaderException($"Specified file [{FileName}] does not exist in directory [{Directory}].");
		}
		protected override Stream GetStreamImplementation() => System.IO.File.Open(GetFilePath(), FileMode.Open);

		private string GetFilePath() => Path.Combine(Directory, FileName);
	}


	public class DataStoreException : Exception
	{
		public DataStoreException() : base() { }
		public DataStoreException(string message) : base(message) { }
		public DataStoreException(string message, Exception innerException) : base(message, innerException) { }
	}
	public class DataFormatException : Exception
	{
		public DataFormatException() : base() { }
		public DataFormatException(string message) : base(message) { }
		public DataFormatException(string message, Exception innerException) : base(message, innerException) { }
	}
	public class DataReaderException : Exception
	{
		public DataReaderException() : base() { }
		public DataReaderException(string message) : base(message) { }
		public DataReaderException(string message, Exception innerException) : base(message, innerException) { }
	}


}


/// <summary>
/// Streams are translated to data tree
/// </summary>
public interface IDataTreeBuilder
{
	public IDataReader SourceReader { get; }
	public IDataTree GetTree();
}
*/
#endregion

/// <summary>
/// Data represents any data -- this is for tracking a dataset in memory, not suitable for bulk operations
/// Ultimately, would like to extend it to "configurable data"
/// </summary>
/// 
/// 
#region Data Graph


// interface IDataContainer?
public interface IDataTreeManager
{
	public ITreeGraph<IData> Tree { get; }
	public IData RootItem { get; }
	public ITreeNode<IData>[] GetUnlinkedDataItems();
	
	public IDataValue CreateNewValue();
	public IDataField CreateNewField();
	public IDataList CreateNewList();
	//public IDataRecord CreateNewRecord();
	//public IDataset CreateNewDataset();
	
	public void DeleteDataNode(IData data);

	public void AddFieldDataValue(IDataField field, IData dataValue);
	public void AddListDataItem(IDataList list, IData data);
	//public void AddRecordField(IDataRecord record, IDataField dataField);
	//public void AddListItemDataAt(IDataList list, IData dataValue, int position);
}


/// <summary>
/// IData is "abstract" interface, where data graph is implemented as ITreeGraph<IData>
/// </summary>
public interface IData : IEnumerable<IData>, IEquatable<IData>
{
	public DmsEnum.StructureType Structure { get; }
	public Guid NodeId { get; }
}

/// <summary>
/// IData is "abstract" interface, where data graph is implemented as ITreeGraph<IData>
/// </summary>
public interface IDataContainer
{
	public IData RootData { get; set; }
	public IList LooseData { get; }

	public ITreeGraph<IData> Tree { get; }


	public IDataValue NewValue();
	public IDataField NewField();
	public IDataList NewList();

	public void SetFieldData(IDataField field, IData data);
	public void SetListItem(IDataList list, IData data);
	public void SetListItemFirst(IDataList list, IData data);
	public void SetListItemAt(IDataList list, IData data, int position);
	public void SetListItemBefore(IDataList list, IData data, IData beforeData);
	public void SetListItemAfter(IDataList list, IData data, IData afterData);

	//public IDataRecord NewRecord();
	//public IDataset NewDataset();
}

public interface IDataValue : IData
{
	public object RawValue { get; set; }
	public DmsEnum.ValueType DmsType { get; }

	public void AddValue();
	public void AddValue(object value);
	public void ClearValue();
	public void SetValue(object value);
}
public interface IDataField : IData
{
	public string Label { get; set; } 
	public IData FieldData { get; }

	public void AddFieldData();
	public void AddFieldData(IData data);
	public void ClearFieldData();
	public void SetFieldData(IData data);
}
public interface IDataList : IData
{
	public IList<IData> DataItems { get; set; }

	public void SetListItem(IData data);
	public void SetListItemFirst(IData data);
	public void SetListItemAt(IData data, int position);
	public void SetListItemBefore(IData data, IData beforeData);
	public void SetListItemAfter(IData data, IData afterData);
}

/*
public interface IDataRecord : IData
{
	public IEnumerable<IDataField> Fields { get; set; }

	public IDataField GetField(string label);
	public IDataField GetField(int position);
	public IDataField GetField(Guid id);

	public IData GetFieldValue(string label);
	public IData GetFieldValue(int position);
	public IData GetFieldValue(Guid id);

	public void AddField(IDataField field);
	public void DeleteField(IDataField field);
	public void MoveFieldTo(IDataField field, int position);

	//public IEnumerable<string> GetFlatLabels() { get; }
	//public IEnumerable<string> GetFlatPositions() { get; }
	//public IEnumerable<string> GetFlatValues() { get; }
}
public interface IDataset : IData
{
	public new DmsEnum.StructureType Structure => DmsEnum.StructureType.Dataset;

	public IEnumerable<IDataRecord> Records { get; }
	public IEnumerable<string> GetFieldLabels();
	public IDictionary<string, IEnumerable<IData>> GetFieldValues();
	public IEnumerable<string> GetFlatLabels();

	public void AddRecord(IDataRecord record);
	public void DeleteRecord(IDataRecord record);
	public void DeleteRecordAt(IDataRecord record, int position);
}
*/




namespace Data
{

	//public static class DataTreeNodeExtensions
	//{
	//	public static IDataValue CreateDataValue(this ITreeNode<IData> node) 
	//	{
	//		
	//	}
	//}
	

	public class DataContainer : IDataContainer
	{
		public IData RootData { get => Tree.Root.GetValue(); set => throw new NotImplementedException(); }
		public IList LooseData => throw new NotImplementedException();

		public ITreeGraph<IData> Tree { get; }
		
		public DataContainer()
		{
			Tree = new TreeGraph<IData>();
		}
		
		public IDataField NewField() => throw new NotImplementedException();
		public IDataList NewList() => throw new NotImplementedException();
		public IDataValue NewValue() => throw new NotImplementedException();
		public void SetFieldData(IDataField field, IData data) => throw new NotImplementedException();
		public void SetListItem(IDataList list, IData data) => throw new NotImplementedException();
		public void SetListItemAfter(IDataList list, IData data, IData afterData) => throw new NotImplementedException();
		public void SetListItemAt(IDataList list, IData data, int position) => throw new NotImplementedException();
		public void SetListItemBefore(IDataList list, IData data, IData beforeData) => throw new NotImplementedException();
		public void SetListItemFirst(IDataList list, IData data) => throw new NotImplementedException();
	}
	/// <summary>
	/// When creating data, if no graph is specified a new graph is created and the data is the root node
	/// </summary>
	public abstract partial class BaseDataNode : IData
	{
		public abstract StructureType Structure { get; }
		public ITreeGraph<IData> Tree { get; }
		public ITreeNode<IData> Node { get; }



		public Guid NodeId => throw new NotImplementedException();

		public abstract IEnumerator<IData> GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();



		public bool Equals(IData other)
		{
			if (Structure != other.Structure) return false;
			if (Structure == DmsEnum.StructureType.Value) return ((IDataValue)this).Equals((IDataValue)other);
			if (Structure == DmsEnum.StructureType.Field) return ((IDataField)this).Equals((IDataField)other);
			if (Structure == DmsEnum.StructureType.List) return ((IDataList)this).Equals((IDataList)other);
			//if (Structure == DmsEnum.StructureType.Record) return ((IDataRecord)this).Equals((IDataRecord)other);
			//if (Structure == DmsEnum.StructureType.Dataset) return ((IDataset)this).Equals((IDataset)other);
			throw new NotImplementedException($"No type comparison logic defined for structures {Structure} and {other.Structure}.");
		}

		// if no graph is specified, create a new graph and build from the root node.
		protected internal BaseDataNode()
		{
			Tree = new TreeGraph<IData>();
			Node = Tree.Root;
		}
		protected internal BaseDataNode(ITreeNode<IData> dataNode) 
		{
			if (dataNode.HasValue)
				throw new InvalidOperationException("Cannot create data object for specified node because it already has a value.");

			Node = dataNode;
			Node.SetValue(this);
		}

	}


	public class Value : BaseDataNode, IDataValue
	{
		// Value structure
		public override StructureType Structure => StructureType.Value;

		// DataValue
		public object RawValue
		{
			get => _rawValue;
			set => _rawValue = TryApplyValueTypeMap(value);
		}
		protected internal object _rawValue { get; set; }
		
		// DmsValueType
		public DmsEnum.ValueType DmsType
		{
			get => _dmsValueType ?? DmsEnum.ValueType.None;
			set => _dmsValueType = value;
		}
		protected internal DmsEnum.ValueType? _dmsValueType { get; set; }

		public void AddValue()
		{
			throw new NotImplementedException();
		}

		public void AddValue(object value)
		{
			throw new NotImplementedException();
		}

		public void ClearValue()
		{
			throw new NotImplementedException();
		}

		public void SetValue(object value)
        {
            throw new NotImplementedException();
        }
		
		// Enumeration
		public override IEnumerator<IData> GetEnumerator() => throw new DataGraphException("Data object cannot be enumerated because the structure type is Value.");

		// Equatability
		public bool Equals(IDataValue other)
		{
			var val1 = this.RawValue;
			var val2 = other.RawValue;

			if (val1 is null && val2 is null) return true;
			if (val1 is null || val2 is null) return false;
			if (AreEquatableType(val1, val2)) return val1.Equals(val2);
			return false;
		}


		// Constructors

		// Can be directly instantiated (will create new TreeGraph<IData>)
		public Value() : base() { }

		// Can be instantiated from an existing tree node without a value specified
		public Value(ITreeNode<IData> dataNode) : base(dataNode) { }

		private object ToDump() => new
		{
			Path = Node.GetPath(),
			Structure,
			DmsType,
			RawValue,
		};

		/// <summary>
		/// use reflection to determine whether the two object types are equatable
		/// </summary>
		private bool AreEquatableType(object obj1, object obj2)
		{
			Type type1 = obj1.GetType();
			Type type2 = obj2.GetType();
			try
			{
				if (type1.Equals(type2))
					return true;

				Type iEquatable = typeof(IEquatable<>).MakeGenericType(type1);

				if (!iEquatable.IsAssignableFrom(type1))
					return false;

				return true;
			}
			catch
			{
				return false;
			}
		}


		// sets _dmsValueType and returns value
		protected internal object TryApplyValueTypeMap(object value)
		{
			try
			{
				// set underlying _dmsValueType based on object value
				if (value is null)
					_dmsValueType = DmsEnum.ValueType.Null;
				else if (GetSupportedTypeMap().TryGetValue(value.GetType(), out DmsEnum.ValueType mappedValueType))
					_dmsValueType = mappedValueType;
				else
					_dmsValueType = DmsEnum.ValueType.Unknown;

				return value;
			}
			catch (Exception ex)
			{
				// if there is error overwrite object with error and set type to error 
				_dmsValueType = DmsEnum.ValueType.Error;
				return new DataValueException("Error setting dms value type for specific value.", ex, value);
			}
		}

		protected internal static Dictionary<Type, DmsEnum.ValueType> GetSupportedTypeMap()
		{
			return new Dictionary<Type, DmsEnum.ValueType>() {
				{ typeof(char),     DmsEnum.ValueType.String },
				{ typeof(string),   DmsEnum.ValueType.String },
				{ typeof(byte),     DmsEnum.ValueType.Number },
				{ typeof(sbyte),    DmsEnum.ValueType.Number },
				{ typeof(int),      DmsEnum.ValueType.Number },
				{ typeof(uint),     DmsEnum.ValueType.Number },
				{ typeof(short),    DmsEnum.ValueType.Number },
				{ typeof(ushort),   DmsEnum.ValueType.Number },
				{ typeof(long),     DmsEnum.ValueType.Number },
				{ typeof(ulong),    DmsEnum.ValueType.Number },
				{ typeof(decimal),  DmsEnum.ValueType.Number},
				{ typeof(double),   DmsEnum.ValueType.Number },
				{ typeof(float),    DmsEnum.ValueType.Number },
				{ typeof(bool),     DmsEnum.ValueType.Boolean },
				{ typeof(DateOnly), DmsEnum.ValueType.DateTime },
				{ typeof(DateTime), DmsEnum.ValueType.DateTime },
			};
		}

    }


	// when setting value, handle differently depending on type (IData vs object)
//	public class Field : BaseDataNode, IDataField
//	{
//		// Field structure
//		public override StructureType Structure => StructureType.Field;
//
//		public string Label { get; set; }   // consider logic to check if container has other fields w/ same name in dataset
//		public IData DataValue => Node.HasBranch ? Node.GetFirstBranch().GetValue() : null;
//
//		public IData DataValue => Node.HasBranch ? Node.GetFirstBranch().GetValue() : null;
//
//
//
//		protected internal ITreeNode<IData> GetDataValueNode()
//		{
//			try
//			{
//				return Node.GetFirstBranch();
//			}
//			catch (Exception ex)
//			{
//				throw new TreeGraphException("Unable to get branch node representing data value from underlying tree graph.", ex);
//			}
//		}
//
//
//		public override IEnumerator<IData> GetEnumerator()
//		{
//			if (DataValue.Structure == DmsEnum.StructureType.Value 
//				|| DataValue.Structure == DmsEnum.StructureType.Field)
//			{
//				yield return DataValue;
//				yield break;
//			}
//			
//			foreach (var dataItem in DataValue.AsEnumerable<IData>())
//				yield return dataItem;
//		}
//		
//		
//		// Constructors
//
//		// Can be directly instantiated
//		public Field() : base() 
//		{ 
//			Tree.Root.SetValue(this);
//			var valueNode = Node.Tree.CreateBranch(Node);
//		}
//
//		// Can be instantiated from an existing tree node without a value specified
//		public Field(ITreeNode<IData> dataNode) : base(dataNode) 
//		{ 
//			
//		}
//
//
//		private object ToDump() => new
//		{
//			Path = Node.GetPath(),
//			Structure = this.Structure,
//			Label,
//			DataValue = this.DataValue,
//		};
//
//
//
//		public void ValidateNode()
//		{
//			if (Node.GetBranches().Length != 1)
//				throw new TreeGraphException($"Field object node on underlying data graph has {Node.GetBranches().Length} branch node(s) and is expected to have only 1.");
//		}
//
//	}


	//
	//
	//
	//
	//	public class List : BaseDataNode, IDataList
	//	{
	//		private object ToDump() => new
	//		{
	//			Path = Node.GetPath(),
	//			Structure,
	//			Count = DataItems.Count,
	//			DataItems,
	//		};
	//		public new StructureType Structure => DmsEnum.StructureType.List;
	//
	//		private string _label { get; set; }
	//		private IData _dataValue { get; set; }
	//
	//		public IList<IData> DataItems => Node.GetBranches().Select(n => n.GetValue()).ToList();
	//
	//		public void AddValue(object value)
	//		{
	//			var node = Node.Tree.CreateBranch(Id);
	//			//node.SetValue(new DataValue(Node, value));
	//		}
	//		public void AddDataItem(IData data) => Node.Tree.SetBranch(Id, data.Id);
	//		public void AddDataItemAt(IData data, int position) => Node.Tree.SetBranchAt(Id, data.Id, position);
	//
	//		public void DeleteDataItem(IData data) => DeleteDataItem(data.Id);
	//		public void DeleteDataItem(Guid id)
	//		{
	//			Node.Tree.RemoveBranch(Id, id);
	//			Node.Tree.DeleteNode(id);
	//		}
	//		public void DeleteDataItemAt(int position)
	//		{
	//			var delete = Node.GetBranches()[position];
	//			DeleteDataItem(delete.Id);
	//		}
	//		
	//		
	//		public override IEnumerator<IData> GetEnumerator()
	//		{
	//			if (_dataValue is null)
	//				throw new DataFieldException("Can't enumerate field because data value is null.");
	//			if (_dataValue.Structure == DmsEnum.StructureType.Value)
	//				yield return _dataValue;
	//			else
	//				foreach (var dataItem in _dataValue.AsEnumerable())
	//					yield return dataItem;
	//		}
	//
	//		public DataList(IDataNode node) : base(node) { }

	// Constructor
	//public DataField(ITreeNode<IData> node) : base(node)
	//{ }
	//public DataField(ITreeNode<IData> node, string label) : base(node)
	//{
	//	SetLabel(label);
	//}
	//public DataField(ITreeNode<IData> node, string label, IData dataValue) : base(node)
	//{
	//	SetLabel(label);
	//	SetFieldData(dataValue);
	//}


	public class DataGraphException : Exception
	{
		public DataGraphException() : base() { }
		public DataGraphException(string message) : base(message) { }
		public DataGraphException(string message, Exception innerException) : base(message, innerException) { }
	}

	public class DataValueException : Exception
	{
		public object UnderlyingValue { get; }
		public Type UnderlyingValueType { get; }
		public DataValueException() : base() { }
		public DataValueException(string message) : base(message) { }
		public DataValueException(string message, Exception innerException) : base(message, innerException) { }
		public DataValueException(string message, Exception innerException, object underlyingValue) : base(message, innerException)
		{
			UnderlyingValue = underlyingValue;
			UnderlyingValueType = underlyingValue?.GetType();
		}
	}
	public class DataFieldException : Exception
	{
		public object UnderlyingValue { get; }
		public DataFieldException() : base() { }
		public DataFieldException(string message) : base(message) { }
		public DataFieldException(string message, Exception innerException) : base(message, innerException) { }
	}

}


//	public class DataRecord : Data, IDataRecord
//	{
//
//	}
//
//	public class Dataset : Data, IDataset
//	{
//
//	}






#endregion




#region TESTS


namespace TreeTests
{
	
	public class TreeGraphTests<T>
	{
		public ITreeGraph<T> GetNewTree()
		{
			var tree = new TreeGraph<T>();
			tree.Validate();
			return tree;
		}

		public ITreeGraph<T> GetExampleTree1()
		{
			var tree = new TreeGraph<T>();
			var node1 = tree.CreateBranch(tree.Root);
			var node1a = tree.CreateBranch(node1);
			var node1b = tree.CreateBranch(node1);
			var node2 = tree.CreateBranch(tree.Root);
			var node3 = tree.CreateBranch(tree.Root);
			var node3a = tree.CreateBranch(node3);
			var node3b = tree.CreateBranch(node3);
			tree.Validate();
			return tree;
		}
		// TREE & NODE CHECKING METHODS
		
		// Aggregates tree checking methods
		public void CheckTree(
			ITreeGraph<T> tree
			, int expectedNodeCount
			, int expectedHeight
			)
		{
			CheckTreeNodesCount(tree, expectedNodeCount);
			CheckTreeHeight(tree, expectedHeight);
		}

		// Aggregates tree node checking methods
		public void CheckNode(
			ITreeNode<T> node
			, int expectedDepth
			, bool expectedLeaf
			, int[] expectedPathArray
			, ITreeNode<T> expectedStem
			, ITreeNode<T> expectedPrevPeer
			, ITreeNode<T> expectedNextPeer
			, ITreeNode<T> expectedFirstBranch
			, ITreeNode<T> expectedLastBranch
			)
		{
			CheckNodeDepth(node, expectedDepth);
			CheckNodeLeafStatus(node, expectedLeaf);
			CheckNodePathAndPosition(node, expectedPathArray);
			CheckNodeStemId(node, expectedStem);
			CheckNodePrevNextPeerIds(node, expectedPrevPeer, expectedNextPeer);
			CheckNodeFirstLastBranchIds(node, expectedFirstBranch, expectedLastBranch);
		}
		
		
		// Test utility methods
		
		// Used to check tree
		protected void CheckTreeNodesCount(ITreeGraph<T> tree, int expectedNodeCount)
		{
			Debug.Assert(tree != null);
			Debug.Assert(tree.GetNodeCount() == expectedNodeCount);
		}
		protected void CheckTreeHeight(ITreeGraph<T> tree, int expectedHeight)
		{
			Debug.Assert(tree != null);
			Debug.Assert(tree.GetHeight() == expectedHeight);
		}
		
		// Used to check node
		protected void CheckNodeDepth(ITreeNode<T> node, int expectedDepth)
		{
			Debug.Assert(node != null);
			Debug.Assert(node.GetDepth() == expectedDepth);
		}
		protected void CheckNodeLeafStatus(ITreeNode<T> node, bool expectedLeaf)
		{
			Debug.Assert(node != null);
			// If node is supposed to be a leaf, then it should not have any branches
			Debug.Assert(expectedLeaf != node.HasBranch);
			Debug.Assert(expectedLeaf != node.GetBranches().Any());
		}
		protected void CheckNodePathAndPosition(ITreeNode<T> node, int[] expectedPathArray)
		{
			Debug.Assert(node != null);
			Debug.Assert(node.Position >= 0);

			// check is root node and root path (empty array)
			var path = node.GetPath();
			if (node.Position == 0 && !node.HasStem)
			{
				Debug.Assert(expectedPathArray.Count() == 0);
				Debug.Assert(!node.GetPath().Any());
				return;
			}
				
			Debug.Assert(node.Position == expectedPathArray.Last());
			
			Debug.Assert(path.Length == expectedPathArray?.Length);

			for (int i = 0; i < expectedPathArray.Length; i++)
				Debug.Assert(path[i] == expectedPathArray[i]);
		}
		protected void CheckNodeStemId(ITreeNode<T> node, ITreeNode<T> stem)
		{
			Debug.Assert(node != null);
			if (stem is null)
			{
				Debug.Assert(!node.HasStem);
				Debug.Assert(node.GetStem() is null);
			}
			else
			{
				Debug.Assert(node.HasStem);
				Debug.Assert(node.GetStem() != null);
				Debug.Assert(node.GetStem().Id.Equals(stem.Id));
			}
		}
		protected void CheckNodePrevNextPeerIds(ITreeNode<T> node, ITreeNode<T> prevPeer, ITreeNode<T> nextPeer)
		{
			Debug.Assert(node != null);
			if (prevPeer is null)
			{
				Debug.Assert(!node.HasPrevPeer);
				Debug.Assert(node.GetPrevPeer() is null);
			}
			else
			{
				Debug.Assert(node.HasPrevPeer);
				Debug.Assert(node.GetPrevPeer() != null);
				Debug.Assert(node.GetPrevPeer().Id.Equals(prevPeer.Id));
			}
			if (nextPeer is null)
			{
				Debug.Assert(!node.HasNextPeer);
				Debug.Assert(node.GetNextPeer() is null);
			}
			else
			{
				Debug.Assert(node.HasNextPeer);
				Debug.Assert(node.GetNextPeer() != null);
				Debug.Assert(node.GetNextPeer().Id.Equals(nextPeer.Id));
			}
		}
		protected void CheckNodeFirstLastBranchIds(ITreeNode<T> node, ITreeNode<T> firstBranch, ITreeNode<T> lastBranch)
		{
			Debug.Assert(node != null);
			Debug.Assert((firstBranch is null && lastBranch is null) || (firstBranch != null && lastBranch != null));
			if (firstBranch is null)
			{
				Debug.Assert(!node.HasBranch);
				Debug.Assert(!node.GetBranches().Any());
			}
			else
			{
				Debug.Assert(node.HasBranch);
				Debug.Assert(node.GetBranches().Any());
				Debug.Assert(node.GetFirstBranch().Id.Equals(firstBranch.Id));
				Debug.Assert(node.GetLastBranch().Id.Equals(lastBranch.Id));
			}
		}


		// TEST METHODS

		public void RunAllTests()
		{
			NewTreeTest();
			SetBranchTest();
			SetBranchesTest();
			CreateBranchTest();
			CreateBranchesTest();
			DeleteNodeTest();
			SwapTest();
			MoveBeforeTest();
			MoveAfterTest();
		}
		
		/// <summary>
		/// Test that tree has valid root after creation (root is only node, has path of base position, and has no stem/peers/branches)
		/// </summary>
		public void NewTreeTest()
		{
			var tree = GetNewTree();
			tree.Validate(); 
			Debug.Assert(tree.Root != null);
			CheckTree(tree, 1, 0);
			CheckNode(tree.Root, 0, true, new int[] {}, null, null, null, null, null);
		}

		/// <summary>
		/// Test "set branch" method works as expected
		/// </summary>
		public void SetBranchTest()
		{
			var tree = GetNewTree();
			var node = tree.CreateNode();
			Debug.Assert(tree.GetFloatingNodes().Count() == 1);
			
			tree.SetBranch(tree.Root, node);
			tree.Validate();
			
			CheckTree(tree, 2, 1);
			CheckNode(node, 1, true, new int[] { 0 }, tree.Root, null, null, null, null);
		}
		/// <summary>
		/// Test all "set branch" methods work as expected
		/// </summary>
		public void SetBranchesTest()
		{
			var tree = GetNewTree();
			var node1a = tree.CreateNode();
			var node1b = tree.CreateNode();
			var node1c = tree.CreateNode();
			var node1d = tree.CreateNode();
			var node1e = tree.CreateNode();
			var node1f = tree.CreateNode();

			Debug.Assert(tree.GetFloatingNodes().Count() == 6);

			// after these operations, branches should be positioned in alphabetical order
			tree.SetBranch(tree.Root, node1d);
			tree.SetBranchAt(tree.Root, node1c, 0);
			tree.SetPrevPeer(node1c, node1b);
			tree.SetNextPeer(node1d, node1e);
			tree.SetFirstBranch(tree.Root, node1a);
			tree.SetLastBranch(tree.Root, node1f);

			tree.Validate();

			CheckTree(tree, 7, 1);  
			
			CheckNode(node1a, 1, true, new int[] { 0 }, tree.Root, null, node1b, null, null);
			CheckNode(node1b, 1, true, new int[] { 1 }, tree.Root, node1a, node1c, null, null);
			CheckNode(node1c, 1, true, new int[] { 2 }, tree.Root, node1b, node1d, null, null);
			CheckNode(node1d, 1, true, new int[] { 3 }, tree.Root, node1c, node1e, null, null);
			CheckNode(node1e, 1, true, new int[] { 4 }, tree.Root, node1d, node1f, null, null);
			CheckNode(node1f, 1, true, new int[] { 5 }, tree.Root, node1e, null, null, null);
			
		}

		/// <summary>
		/// Test "create branch" method works as expected
		/// </summary>
		public void CreateBranchTest()
		{
			var tree = GetNewTree();

			var node = tree.CreateBranch(tree.Root);

			tree.Validate();

			CheckTree(tree, 2, 1);
			CheckNode(node, 1, true, new int[] { 0 }, tree.Root, null, null, null, null);
		}
		/// <summary>
		/// Test all "create branch" methods work as expected
		/// </summary>
		public void CreateBranchesTest()
		{
			var tree = GetNewTree();
			var node1d = tree.CreateBranch(tree.Root);
			var node1c = tree.CreateBranchAt(tree.Root, 0);
			var node1b = tree.CreatePrevPeer(node1c);
			var node1e = tree.CreateNextPeer(node1d);
			var node1f = tree.CreateBranch(tree.Root);
			var node1a = tree.CreateBranchAt(tree.Root, 0);
			
			tree.Validate();

			CheckTree(tree, 7, 1);

			CheckNode(node1a, 1, true, new int[] { 0 }, tree.Root, null, node1b, null, null);
			CheckNode(node1b, 1, true, new int[] { 1 }, tree.Root, node1a, node1c, null, null);
			CheckNode(node1c, 1, true, new int[] { 2 }, tree.Root, node1b, node1d, null, null);
			CheckNode(node1d, 1, true, new int[] { 3 }, tree.Root, node1c, node1e, null, null);
			CheckNode(node1e, 1, true, new int[] { 4 }, tree.Root, node1d, node1f, null, null);
			CheckNode(node1f, 1, true, new int[] { 5 }, tree.Root, node1e, null, null, null);
		}

		/// <summary>
		/// Test "delete node" method works as expected
		/// </summary>
		public void DeleteNodeTest()
		{
			var tree = GetNewTree();
			var node1 = tree.CreateBranch(tree.Root);

			CheckTree(tree, 2, 1);
			CheckNode(tree.Root, 0, false, new int[] { }, null, null, null, node1, node1);
			CheckNode(node1, 1, true, new int[] { 0 }, tree.Root, null, null, null, null);

			var node1a = tree.CreateBranch(node1);

			CheckTree(tree, 3, 2);


			CheckNode(tree.Root, 0, false, new int[] { }, null, null, null, node1, node1);
			CheckNode(node1, 1, false, new int[] { 0 }, tree.Root, null, null, node1a, node1a);
			CheckNode(node1a, 2, true, new int[] { 0, 0 }, node1, null, null, null, null);

			tree.DeleteNode(node1a);

			CheckTree(tree, 2, 1);
			CheckNode(tree.Root, 0, false, new int[] { }, null, null, null, node1, node1);
			CheckNode(node1, 1, true, new int[] { 0 }, tree.Root, null, null, null, null);

			tree.DeleteNode(node1);
			
			CheckTree(tree, 1, 0);
			CheckNode(tree.Root, 0, true, new int[] { }, null, null, null, null, null);
		}
		/// <summary>
		/// Test all "set branch" methods work as expected
		/// </summary>
		public void DeleteNodeWithValueTest()
		{
			var tree =  new TreeGraph<T>();
			var node = tree.CreateNode();

			CheckTree(tree, 2, 1);
			

			tree.DeleteNode(node);

			CheckTree(tree, 1, 1);
			CheckNode(tree.Root, 0, false, new int[] { }, null, null, null, null, null);
		}

		public void SwapTest()
		{
			var tree = GetNewTree();

			// ACTION - Add nodes with nested depth, then swap nodes
			var node1 = tree.CreateBranch(tree.Root);
			var node1a = tree.CreateBranch(node1);
			var node1b = tree.CreateBranch(node1);
			var node2 = tree.CreateBranch(tree.Root);
			var node3 = tree.CreateBranch(tree.Root);
			var node3a = tree.CreateBranch(node3);
			var node3b = tree.CreateBranch(node3);

			CheckTree(tree, 8, 2);
			CheckNode(node1a, 2, true, new int[] { 0, 0 }, node1, null, node1b, null, null);
			CheckNode(node3, 1, false, new int[] { 2 }, tree.Root, node2, null, node3a, node3b);

			tree.Swap(node1a, node3);
			
			CheckTree(tree, 8, 3);
			CheckNode(node1a, 1, true, new int[] { 2 }, tree.Root, node2, null, null, null);
			CheckNode(node3, 2, false, new int[] { 0, 0 }, node1, null, node1b, node3a, node3b);
		}

		public void MoveBeforeTest()
		{
			var tree = GetNewTree();

			// ACTION - Add nodes with nested depth, then swap nodes
			var node1 = tree.CreateBranch(tree.Root);
			var node1a = tree.CreateBranch(node1);
			var node1b = tree.CreateBranch(node1);
			var node2 = tree.CreateBranch(tree.Root);
			var node3 = tree.CreateBranch(tree.Root);
			var node3a = tree.CreateBranch(node3);
			var node3b = tree.CreateBranch(node3);

			tree.Validate();
			CheckTree(tree, 8, 2);
			CheckNode(node1a, 2, true, new int[] { 0, 0 }, node1, null, node1b, null, null);
			CheckNode(node3, 1, false, new int[] { 2 }, tree.Root, node2, null, node3a, node3b);

			tree.MoveBefore(node1a, node3);
			
			CheckTree(tree, 8, 3);
			CheckNode(node1a, 2, true, new int[] { 0, 1 }, node1, node3, node1b, null, null);
			CheckNode(node3, 2, false, new int[] { 0, 0 }, node1, null, node1a, node3a, node3b);
		}

		public void MoveAfterTest()
		{
			var tree = GetNewTree();

			// ACTION - Add nodes with nested depth, then swap nodes
			var node1 = tree.CreateBranch(tree.Root);
			var node1a = tree.CreateBranch(node1);
			var node1b = tree.CreateBranch(node1);
			var node2 = tree.CreateBranch(tree.Root);
			var node3 = tree.CreateBranch(tree.Root);
			var node3a = tree.CreateBranch(node3);
			var node3b = tree.CreateBranch(node3);

			tree.Validate();
			CheckTree(tree, 8, 2);
			CheckNode(node1a, 2, true, new int[] { 0, 0 }, node1, null, node1b, null, null);
			CheckNode(node3, 1, false, new int[] { 2 }, tree.Root, node2, null, node3a, node3b);

			tree.MoveAfter(node1a, node3);

			CheckTree(tree, 8, 3);
			CheckNode(node1a, 2, true, new int[] { 0, 0 }, node1, null, node3, null, null);
			CheckNode(node3, 2, false, new int[] { 0, 1 }, node1, node1a, node1b, node3a, node3b);
		}

	}


	public class DataTreeTests
	{
		public IDataTree GetNewTree()
		{
			var tree = new DataTree(DmsEnum.DataTreeLayoutType.Columnar);
			tree.Validate();
			return tree;
		}

		public IDataTree GetExampleTree1()
		{
			var tree = new DataTree(DmsEnum.DataTreeLayoutType.Columnar);
			var node1 = tree.CreateBranch(tree.Root);
			var node1a = tree.CreateBranch(node1);
			var node1b = tree.CreateBranch(node1);
			var node2 = tree.CreateBranch(tree.Root);
			var node3 = tree.CreateBranch(tree.Root);
			var node3a = tree.CreateBranch(node3);
			var node3b = tree.CreateBranch(node3);
			tree.Validate();
			return tree;
		}
		// TREE & NODE CHECKING METHODS

		// Aggregates tree checking methods
		public void CheckTree(
			IDataTree tree
			, int expectedNodeCount
			, int expectedHeight
			)
		{
			CheckTreeNodesCount(tree, expectedNodeCount);
			CheckTreeHeight(tree, expectedHeight);
		}

		// Aggregates tree node checking methods
		public void CheckNode(
			ITreeNode<object> node
			, int expectedDepth
			, bool expectedLeaf
			, int[] expectedPathArray
			, ITreeNode<object> expectedStem
			, ITreeNode<object> expectedPrevPeer
			, ITreeNode<object> expectedNextPeer
			, ITreeNode<object> expectedFirstBranch
			, ITreeNode<object> expectedLastBranch
			)
		{
			CheckNodeDepth(node, expectedDepth);
			CheckNodeLeafStatus(node, expectedLeaf);
			CheckNodePathAndPosition(node, expectedPathArray);
			CheckNodeStemId(node, expectedStem);
			CheckNodePrevNextPeerIds(node, expectedPrevPeer, expectedNextPeer);
			CheckNodeFirstLastBranchIds(node, expectedFirstBranch, expectedLastBranch);
		}


		// Test utility methods

		// Used to check tree
		protected void CheckTreeNodesCount(ITreeGraph<object> tree, int expectedNodeCount)
		{
			Debug.Assert(tree != null);
			Debug.Assert(tree.GetNodeCount() == expectedNodeCount);
		}
		protected void CheckTreeHeight(ITreeGraph<object> tree, int expectedHeight)
		{
			Debug.Assert(tree != null);
			Debug.Assert(tree.GetHeight() == expectedHeight);
		}

		// Used to check node
		protected void CheckNodeDepth(ITreeNode<object> node, int expectedDepth)
		{
			Debug.Assert(node != null);
			Debug.Assert(node.GetDepth() == expectedDepth);
		}
		protected void CheckNodeLeafStatus(ITreeNode<object> node, bool expectedLeaf)
		{
			Debug.Assert(node != null);
			// If node is supposed to be a leaf, then it should not have any branches
			Debug.Assert(expectedLeaf != node.HasBranch);
			Debug.Assert(expectedLeaf != node.GetBranches().Any());
		}
		protected void CheckNodePathAndPosition(ITreeNode<object> node, int[] expectedPathArray)
		{
			Debug.Assert(node != null);
			Debug.Assert(node.Position >= 0);

			// check is root node and root path (empty array)
			var path = node.GetPath();
			if (node.Position == 0 && !node.HasStem)
			{
				Debug.Assert(expectedPathArray.Count() == 0);
				Debug.Assert(!node.GetPath().Any());
				return;
			}

			Debug.Assert(node.Position == expectedPathArray.Last());

			Debug.Assert(path.Length == expectedPathArray?.Length);

			for (int i = 0; i < expectedPathArray.Length; i++)
				Debug.Assert(path[i] == expectedPathArray[i]);
		}
		protected void CheckNodeStemId(ITreeNode<object> node, ITreeNode<object> stem)
		{
			Debug.Assert(node != null);
			if (stem is null)
			{
				Debug.Assert(!node.HasStem);
				Debug.Assert(node.GetStem() is null);
			}
			else
			{
				Debug.Assert(node.HasStem);
				Debug.Assert(node.GetStem() != null);
				Debug.Assert(node.GetStem().Id.Equals(stem.Id));
			}
		}
		protected void CheckNodePrevNextPeerIds(ITreeNode<object> node, ITreeNode<object> prevPeer, ITreeNode<object> nextPeer)
		{
			Debug.Assert(node != null);
			if (prevPeer is null)
			{
				Debug.Assert(!node.HasPrevPeer);
				Debug.Assert(node.GetPrevPeer() is null);
			}
			else
			{
				Debug.Assert(node.HasPrevPeer);
				Debug.Assert(node.GetPrevPeer() != null);
				Debug.Assert(node.GetPrevPeer().Id.Equals(prevPeer.Id));
			}
			if (nextPeer is null)
			{
				Debug.Assert(!node.HasNextPeer);
				Debug.Assert(node.GetNextPeer() is null);
			}
			else
			{
				Debug.Assert(node.HasNextPeer);
				Debug.Assert(node.GetNextPeer() != null);
				Debug.Assert(node.GetNextPeer().Id.Equals(nextPeer.Id));
			}
		}
		protected void CheckNodeFirstLastBranchIds(ITreeNode<object> node, ITreeNode<object> firstBranch, ITreeNode<object> lastBranch)
		{
			Debug.Assert(node != null);
			Debug.Assert((firstBranch is null && lastBranch is null) || (firstBranch != null && lastBranch != null));
			if (firstBranch is null)
			{
				Debug.Assert(!node.HasBranch);
				Debug.Assert(!node.GetBranches().Any());
			}
			else
			{
				Debug.Assert(node.HasBranch);
				Debug.Assert(node.GetBranches().Any());
				Debug.Assert(node.GetFirstBranch().Id.Equals(firstBranch.Id));
				Debug.Assert(node.GetLastBranch().Id.Equals(lastBranch.Id));
			}
		}


		// TEST METHODS

		public void RunAllTests()
		{
			NewTreeTest();
			SetBranchTest();
			SetBranchesTest();
			CreateBranchTest();
			CreateBranchesTest();
			DeleteNodeTest();
			SwapTest();
			MoveBeforeTest();
			MoveAfterTest();
		}

		/// <summary>
		/// Test that tree has valid root after creation (root is only node, has path of base position, and has no stem/peers/branches)
		/// </summary>
		public void NewTreeTest()
		{
			var tree = GetNewTree();
			tree.Validate();
			Debug.Assert(tree.Root != null);
			CheckTree(tree, 1, 0);
			CheckNode(tree.Root, 0, true, new int[] { }, null, null, null, null, null);
		}

		/// <summary>
		/// Test "set branch" method works as expected
		/// </summary>
		public void SetBranchTest()
		{
			var tree = GetNewTree();
			var node = tree.CreateNode();
			Debug.Assert(tree.GetFloatingNodes().Count() == 1);

			tree.SetBranch(tree.Root, node);
			tree.Validate();

			CheckTree(tree, 2, 1);
			CheckNode(node, 1, true, new int[] { 0 }, tree.Root, null, null, null, null);
		}
		/// <summary>
		/// Test all "set branch" methods work as expected
		/// </summary>
		public void SetBranchesTest()
		{
			var tree = GetNewTree();
			var node1a = tree.CreateNode();
			var node1b = tree.CreateNode();
			var node1c = tree.CreateNode();
			var node1d = tree.CreateNode();
			var node1e = tree.CreateNode();
			var node1f = tree.CreateNode();

			Debug.Assert(tree.GetFloatingNodes().Count() == 6);

			// after these operations, branches should be positioned in alphabetical order
			tree.SetBranch(tree.Root, node1d);
			tree.SetBranchAt(tree.Root, node1c, 0);
			tree.SetPrevPeer(node1c, node1b);
			tree.SetNextPeer(node1d, node1e);
			tree.SetFirstBranch(tree.Root, node1a);
			tree.SetLastBranch(tree.Root, node1f);

			tree.Validate();

			CheckTree(tree, 7, 1);

			CheckNode(node1a, 1, true, new int[] { 0 }, tree.Root, null, node1b, null, null);
			CheckNode(node1b, 1, true, new int[] { 1 }, tree.Root, node1a, node1c, null, null);
			CheckNode(node1c, 1, true, new int[] { 2 }, tree.Root, node1b, node1d, null, null);
			CheckNode(node1d, 1, true, new int[] { 3 }, tree.Root, node1c, node1e, null, null);
			CheckNode(node1e, 1, true, new int[] { 4 }, tree.Root, node1d, node1f, null, null);
			CheckNode(node1f, 1, true, new int[] { 5 }, tree.Root, node1e, null, null, null);

		}

		/// <summary>
		/// Test "create branch" method works as expected
		/// </summary>
		public void CreateBranchTest()
		{
			var tree = GetNewTree();

			var node = tree.CreateBranch(tree.Root);

			tree.Validate();

			CheckTree(tree, 2, 1);
			CheckNode(node, 1, true, new int[] { 0 }, tree.Root, null, null, null, null);
		}
		/// <summary>
		/// Test all "create branch" methods work as expected
		/// </summary>
		public void CreateBranchesTest()
		{
			var tree = GetNewTree();
			var node1d = tree.CreateBranch(tree.Root);
			var node1c = tree.CreateBranchAt(tree.Root, 0);
			var node1b = tree.CreatePrevPeer(node1c);
			var node1e = tree.CreateNextPeer(node1d);
			var node1f = tree.CreateBranch(tree.Root);
			var node1a = tree.CreateBranchAt(tree.Root, 0);

			tree.Validate();

			CheckTree(tree, 7, 1);

			CheckNode(node1a, 1, true, new int[] { 0 }, tree.Root, null, node1b, null, null);
			CheckNode(node1b, 1, true, new int[] { 1 }, tree.Root, node1a, node1c, null, null);
			CheckNode(node1c, 1, true, new int[] { 2 }, tree.Root, node1b, node1d, null, null);
			CheckNode(node1d, 1, true, new int[] { 3 }, tree.Root, node1c, node1e, null, null);
			CheckNode(node1e, 1, true, new int[] { 4 }, tree.Root, node1d, node1f, null, null);
			CheckNode(node1f, 1, true, new int[] { 5 }, tree.Root, node1e, null, null, null);
		}

		/// <summary>
		/// Test "delete node" method works as expected
		/// </summary>
		public void DeleteNodeTest()
		{
			var tree = GetNewTree();
			var node1 = tree.CreateBranch(tree.Root);

			CheckTree(tree, 2, 1);
			CheckNode(tree.Root, 0, false, new int[] { }, null, null, null, node1, node1);
			CheckNode(node1, 1, true, new int[] { 0 }, tree.Root, null, null, null, null);

			var node1a = tree.CreateBranch(node1);

			CheckTree(tree, 3, 2);


			CheckNode(tree.Root, 0, false, new int[] { }, null, null, null, node1, node1);
			CheckNode(node1, 1, false, new int[] { 0 }, tree.Root, null, null, node1a, node1a);
			CheckNode(node1a, 2, true, new int[] { 0, 0 }, node1, null, null, null, null);

			tree.DeleteNode(node1a);

			CheckTree(tree, 2, 1);
			CheckNode(tree.Root, 0, false, new int[] { }, null, null, null, node1, node1);
			CheckNode(node1, 1, true, new int[] { 0 }, tree.Root, null, null, null, null);

			tree.DeleteNode(node1);

			CheckTree(tree, 1, 0);
			CheckNode(tree.Root, 0, true, new int[] { }, null, null, null, null, null);
		}
		/// <summary>
		/// Test all "set branch" methods work as expected
		/// </summary>
		public void DeleteNodeWithValueTest()
		{
			var tree = new DataTree(DmsEnum.DataTreeLayoutType.Columnar);
			var node = tree.CreateNode();

			CheckTree(tree, 2, 1);


			tree.DeleteNode(node);

			CheckTree(tree, 1, 1);
			CheckNode(tree.Root, 0, false, new int[] { }, null, null, null, null, null);
		}

		public void SwapTest()
		{
			var tree = GetNewTree();

			// ACTION - Add nodes with nested depth, then swap nodes
			var node1 = tree.CreateBranch(tree.Root);
			var node1a = tree.CreateBranch(node1);
			var node1b = tree.CreateBranch(node1);
			var node2 = tree.CreateBranch(tree.Root);
			var node3 = tree.CreateBranch(tree.Root);
			var node3a = tree.CreateBranch(node3);
			var node3b = tree.CreateBranch(node3);

			CheckTree(tree, 8, 2);
			CheckNode(node1a, 2, true, new int[] { 0, 0 }, node1, null, node1b, null, null);
			CheckNode(node3, 1, false, new int[] { 2 }, tree.Root, node2, null, node3a, node3b);

			tree.Swap(node1a, node3);

			CheckTree(tree, 8, 3);
			CheckNode(node1a, 1, true, new int[] { 2 }, tree.Root, node2, null, null, null);
			CheckNode(node3, 2, false, new int[] { 0, 0 }, node1, null, node1b, node3a, node3b);
		}

		public void MoveBeforeTest()
		{
			var tree = GetNewTree();

			// ACTION - Add nodes with nested depth, then swap nodes
			var node1 = tree.CreateBranch(tree.Root);
			var node1a = tree.CreateBranch(node1);
			var node1b = tree.CreateBranch(node1);
			var node2 = tree.CreateBranch(tree.Root);
			var node3 = tree.CreateBranch(tree.Root);
			var node3a = tree.CreateBranch(node3);
			var node3b = tree.CreateBranch(node3);

			tree.Validate();
			CheckTree(tree, 8, 2);
			CheckNode(node1a, 2, true, new int[] { 0, 0 }, node1, null, node1b, null, null);
			CheckNode(node3, 1, false, new int[] { 2 }, tree.Root, node2, null, node3a, node3b);

			tree.MoveBefore(node1a, node3);

			CheckTree(tree, 8, 3);
			CheckNode(node1a, 2, true, new int[] { 0, 1 }, node1, node3, node1b, null, null);
			CheckNode(node3, 2, false, new int[] { 0, 0 }, node1, null, node1a, node3a, node3b);
		}

		public void MoveAfterTest()
		{
			var tree = GetNewTree();

			// ACTION - Add nodes with nested depth, then swap nodes
			var node1 = tree.CreateBranch(tree.Root);
			var node1a = tree.CreateBranch(node1);
			var node1b = tree.CreateBranch(node1);
			var node2 = tree.CreateBranch(tree.Root);
			var node3 = tree.CreateBranch(tree.Root);
			var node3a = tree.CreateBranch(node3);
			var node3b = tree.CreateBranch(node3);

			tree.Validate();
			CheckTree(tree, 8, 2);
			CheckNode(node1a, 2, true, new int[] { 0, 0 }, node1, null, node1b, null, null);
			CheckNode(node3, 1, false, new int[] { 2 }, tree.Root, node2, null, node3a, node3b);

			tree.MoveAfter(node1a, node3);

			CheckTree(tree, 8, 3);
			CheckNode(node1a, 2, true, new int[] { 0, 0 }, node1, null, node3, null, null);
			CheckNode(node3, 2, false, new int[] { 0, 1 }, node1, node1a, node1b, node3a, node3b);
		}

	}

}



#endregion



/*


namespace DataSource
{

	/// <summary>
	/// Data source is a file
	/// </summary>
	public class File : DataReaderBase, IDataReader
	{
		// Config
		public string Directory { get; set; }
		public string FileName { get; set; }

		// Data source implementation	
		public override DmsEnum.SourceType SourceType => DmsEnum.SourceType.File;

		public override void ValidateSource()
		{
			if (!System.IO.Directory.Exists(Directory))
				throw new DataReaderException($"Specified directory [{Directory}] does not exist.");
			if (!Path.Exists(GetFilePath()))
				throw new DataReaderException($"Specified file [{FileName}] does not exist in directory [{Directory}].");
		}
		protected override Stream GetStreamImplementation() => System.IO.File.Open(GetFilePath(), FileMode.Open);

		private string GetFilePath() => Path.Combine(Directory, FileName);
	}
	
	public class DataReaderException : Exception
	{
		public DataReaderException() : base() { }
		public DataReaderException(string message) : base(message) { }
		public DataReaderException(string message, Exception innerException) : base(message, innerException) { }
	}

	/// <summary>
	/// Get whatever the requested data is and read it as a stream. 
	/// Before getting data, validate configuration and ability to connect to source
	/// </summary>
	public abstract class DataReaderBase : AbstractConfigurableObject, IDataReader
	{
		/// <summary>
		/// Each enum value in DataSource has a corresponding class which conforms to the IDataStore interface
		/// </summary>
		public abstract DmsEnum.SourceType SourceType { get; }

		/// <summary>
		/// GetStream() returns a stream and implements generalized error-handling
		/// </summary>
		public Stream GetStream()
		{
			// Add this back when configurable object is in a better spot
			//try { ValidateConfiguration(); }
			//catch (Exception ex) { throw new DataReaderException("Error when attempting to validate data source configuration.", ex); }

			try { ValidateCanRead(); }
			catch (Exception ex) { throw new DataReaderException("Unable to read data source.", ex); }

			try { return GetStreamImplementation(); }
			catch (Exception ex) { throw new DataReaderException("Error when attempting to read data stream from data source.", ex); }
		}

		/// <summary>
		/// GetTree() returns a data tree
		/// </summary>
		public IDataTree GetTree()
		{
			// Add this back when configurable object is in a better spot
			//try { ValidateConfiguration(); }
			//catch (Exception ex) { throw new DataReaderException("Error when attempting to validate data source configuration.", ex); }

			try { ValidateCanRead(); }
			catch (Exception ex) { throw new DataReaderException("Unable to read data source.", ex); }

			try { return GetTreeImplementation(); }
			catch (Exception ex) { throw new DataReaderException("Error when attempting to read data stream from data source.", ex); }
		}


		/// <summary>
		/// Source-specific, checks if source is accessible and throw descriptive error if inaccessible 
		/// </summary>
		public abstract void ValidateCanRead();

		/// <summary>
		/// Source-specific, defines stream-reading implementation
		/// </summary>
		protected abstract Stream GetStreamImplementation();

		/// <summary>
		/// Source-specific, defines stream-reading implementation
		/// </summary>
		protected abstract IDataTree GetTreeImplementation();
	}

	/// <summary>
	/// Data source is a user-specified input string
	/// </summary>
	public class RawInput : DataReaderBase, IDataReader
	{
		// Config
		public string Text { get; set; }
		public Encoding TextEncoding { get; set; }

		// Data source implementation
		public override DmsEnum.SourceType SourceType => DmsEnum.SourceType.RawInput;

		public override void ValidateSource()
		{
			if (string.IsNullOrEmpty(Text))
				throw new DataReaderException("Can't read from source because InputText value is null or empty.");
		}
		protected override Stream GetStreamImplementation() => new MemoryStream(TextEncoding.GetBytes(Text));

		protected override IDataTree GetTreeImplementation(IDataFormat source)
		{

		}
	}

	/// <summary>
	/// Data source is a file
	/// </summary>
	public class File : DataReaderBase, IDataReader
	{
		// Config
		public string Directory { get; set; }
		public string FileName { get; set; }

		// Data source implementation	
		public override DmsEnum.SourceType SourceType => DmsEnum.SourceType.File;

		public override void ValidateSource()
		{
			if (!System.IO.Directory.Exists(Directory))
				throw new DataReaderException($"Specified directory [{Directory}] does not exist.");
			if (!Path.Exists(GetFilePath()))
				throw new DataReaderException($"Specified file [{FileName}] does not exist in directory [{Directory}].");
		}
		protected override Stream GetStreamImplementation() => System.IO.File.Open(GetFilePath(), FileMode.Open);

		private string GetFilePath() => Path.Combine(Directory, FileName);
	}
}

/// <summary>
/// Data format specifies how data in a source is structured
/// </summary>
public interface IDataFormat : IConfigurableObject
{
	public DmsEnum.FormatType FormatType { get; }
}

/// <summary>
/// Data source reader returns data from data source as a stream
/// </summary>
public interface IDataReader
{
	public IDataStore Source { get; }
	public IDataFormat Format { get; }
	public Stream GetStream();
	public IDataTree GetTree();	// can enumerate trees if data too big
}

/// <summary>
/// Streams are translated to data tree
/// </summary>
public interface IDataTreeBuilder
{
	public IDataReader SourceReader { get; }
	public IDataTree GetTree();
}

#endregion


/// <summary>
/// A data format can be configured and prescribes how a source stream can be translated to an in-memory data graph
/// </summary>
#region Data Format Interface and Implementations
/// <summary>
/// Specifies the underlying format of a data source and provides logic for returning a data tree object
/// </summary>
public interface IDataFormat
{
	public DmsEnum.FormatType DataFormat { get; }
	public ITreeGraph<Guid, object> GetTree(IDataStore source);
	//public void ValidateFormat();
}
namespace DataFormat
{
	public class DataFormatException : Exception
	{
		public DataFormatException() : base() { }
		public DataFormatException(string message) : base(message) { }
		public DataFormatException(string message, Exception innerException) : base(message, innerException) { }
	}
	/// <summary>
	/// Data formats are configured to match a specific data source and are capabale of validating the source data's format and parsing it as a data tree.
	/// </summary>
	public abstract class DataFormatBase : IDataFormat
	{
		/// <summary>
		/// Each enum value in DataFormat has a corresponding class which conforms to the IDataTreeBuilder interface
		/// </summary>
		public abstract DmsEnum.FormatType DataFormat { get; }

		/// <summary>
		/// Reads a stream and returns a data tree
		/// </summary>
		public ITreeGraph<Guid, object> GetTree(IDataStore source)
		{
			//try { ValidateConfiguration(); }
			//catch (Exception ex) { throw new DataReaderException("Error when attempting to validate data tree builder configuration.", ex); }

			//try { ValidateFormat(); }
			//catch (Exception ex) { throw new DataReaderException("Error when attempting to validate data source.", ex); }

			try { return GetTreeImplementation(source); }
			catch (Exception ex) { throw new DataFormatException("Error when attempting to read data stream from data source using specified format.", ex); }
		}

		/// <summary>
		/// Format-specific, throws error if the source data format doesn't match configuration
		/// </summary>
		public abstract void ValidateFormat();

		/// <summary>
		/// Format-specific, returns a data tree from a data source
		/// </summary>
		protected abstract ITreeGraph<Guid, object> GetTreeImplementation(IDataStore source);
	}

	/// <summary>
	/// Delimitter-Separated Values
	/// </summary>
	public class Dsv : DataFormatBase
	{
		// Mostly conforms to https://www.rfc-editor.org/rfc/rfc4180
		public bool HasHeaders { get; set; }
		public char Delimitter { get; set; }
		public char Escaper { get; set; }
		public string LineBreak { get; set; }

		public override DmsEnum.FormatType DataFormat => DmsEnum.FormatType.Dsv;

		public override void ValidateFormat()
		{
			throw new NotImplementedException();
		}

		protected override ITreeGraph<Guid, object> GetTreeImplementation(IDataStore source)
		{
			var tree = new TreeGraph<Guid, object>(new GuidFactory());

			using (var stream = source.GetStream())
			using (var reader = new StreamReader(stream))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					var newRow = tree.CreateAndRegisterNode();
					tree.SetBranch(tree.Root, newRow);

					foreach (var nodeVal in ParseDelimittedStringLine(line))
					{
						var newNode = tree.CreateAndRegisterNode();
						tree.SetBranch(newRow, newNode);
						newNode.SetValue(nodeVal);
					}
				}
			}
			return tree;
		}

		private IEnumerable<string> ParseDelimittedStringLine(string input)
		{
			var state = new ParserState()
			{
				ValueChars = new List<char>(),
				InValue = false,
				Escaped = false,
				PrevChar = null
			};

			// Enumerate characters, yielding a string whenever a new delimitter is encounterd
			for (int i = 0; i < input.Length; i++)
			{
				char c = input[i];

				// Parse through, aggregate chars, write to list when delimitter is encountered (unless escaped)
				if (c == Delimitter && !state.Escaped)
				{
					if (state.ValueChars.Any())
					{
						yield return string.Join("", state.ValueChars);
						state.ValueChars.Clear();
					}
					else
					{
						yield return null;
					}
				}
				else
				{
					if (c == Escaper && state.Escaped)
					{
						state.Escaped = false;
					}
					else if (c == Escaper && !state.Escaped)
					{
						state.Escaped = true;

						// If prev char was the escaper, that means that it's a double quote string (so add escaper char to values list and resume escape)
						if (state.PrevChar == Escaper)
							state.ValueChars.Add(c);
					}
					else
					{
						state.ValueChars.Add(c);
					}
				}

				// If there are no more characters then return whatever is held in values at the moment
				if (i + 1 == input.Length)
					if (state.ValueChars.Any())
						yield return string.Join("", state.ValueChars);
					else
						yield return null;

				state.PrevChar = c;
				//char c = str[i];

			}
		}

		private class ParserState
		{
			public List<char> ValueChars { get; set; }
			public bool InValue { get; set; }
			public bool Escaped { get; set; }
			public char? PrevChar { get; set; }
		}

	}
	
	/// <summary>
	/// Json
	/// </summary>
	public class Json : DataFormatBase
	{

		public override DmsEnum.FormatType DataFormat => DmsEnum.FormatType.Json;
		public JsonDocumentOptions JsonDocumentOptions { get; set; }

		public override void ValidateFormat()
		{
			throw new NotImplementedException();
		}

		protected override ITreeGraph<Guid, object> GetTreeImplementation(IDataStore source)
		{
			var tree = new TreeGraph<Guid, object>(new GuidFactory());

			using (var stream = source.GetStream())
			using (JsonDocument doc = JsonDocument.Parse(stream, JsonDocumentOptions))
			{
				ParseRecursive(tree, tree.Root.Id, doc.RootElement);
			}
			return tree;
		}

		private void ParseRecursive(ITreeGraph<Guid, object> tree, Guid currentNodeId, JsonElement element)
		{
			if (element.ValueKind == JsonValueKind.Object)
			{
				ParseJsonObject(tree, currentNodeId, element);
			}
			else if (element.ValueKind == JsonValueKind.Array)
			{
				ParseJsonArray(tree, currentNodeId, element);
			}
			else
			{
				ParseJsonValue(tree, currentNodeId, element);
			}

		}

		private void ParseJsonObject(ITreeGraph<Guid, object> tree, Guid currentNodeId, JsonElement element)
		{
			var record = tree.CreateAndRegisterNode();
			tree.SetBranch(currentNodeId, record.Id);

			foreach (JsonProperty prop in element.EnumerateObject())
			{
				var field = tree.CreateAndRegisterNode();
				tree.SetBranch(record, field);

				var label = tree.CreateAndRegisterNode();
				tree.SetBranch(field, label);
				label.SetValue(prop.Name);

				ParseRecursive(tree, field.Id, prop.Value);
			}
		}

		private void ParseJsonArray(ITreeGraph<Guid, object> tree, Guid currentNodeId, JsonElement element)
		{
			var list = tree.CreateAndRegisterNode();
			tree.SetBranch(currentNodeId, list.Id);

			foreach (JsonElement item in element.EnumerateArray())
			{
				ParseRecursive(tree, list.Id, item);
			}
		}

		private void ParseJsonValue(ITreeGraph<Guid, object> tree, Guid currentNodeId, JsonElement element)
		{
			if (!JsonValueKindValue.Contains(element.ValueKind))
				throw new InvalidDataException("Element is expected to be a value but does not have expected JsonValueKind.");

			var value = tree.CreateAndRegisterNode();
			tree.SetBranch(currentNodeId, value.Id);
			if (element.ValueKind == JsonValueKind.String)
				value.SetValue(element.ToString());
			else if (element.ValueKind == JsonValueKind.Number)
			{
				if (element.TryGetInt64(out Int64 i64))
					value.SetValue(i64);
				else if (element.TryGetDouble(out double doub))
					value.SetValue(doub);
				else if (element.TryGetDecimal(out Decimal dec))
					value.SetValue(dec);
				else
					throw new InvalidOperationException($"Can't parse value ({element.ToString()} as numeric for JsonValueKind {element.ValueKind}.");
			}
			else if (element.ValueKind == JsonValueKind.True)
				value.SetValue(true);
			else if (element.ValueKind == JsonValueKind.False)
				value.SetValue(false);
			else if (element.ValueKind == JsonValueKind.Null)
				value.SetValue(null);
			else if (element.ValueKind == JsonValueKind.Object || element.ValueKind == JsonValueKind.Array)
				throw new InvalidOperationException($"Can't parse json value for JsonValueKind {element.ValueKind}.");
			else
				throw new InvalidOperationException($"Unexpected JsonValueKind encountered ({element.ValueKind}).");

			//value.SetValue(element.ToString());
			//value.SetValue(element.GetBytesFromBase64());

			//value.SetValue(element.ToString());

		}

		private static HashSet<JsonValueKind> JsonValueKindValue = new HashSet<JsonValueKind>()
			{
				{ JsonValueKind.Null        },
				{ JsonValueKind.Undefined   },
				{ JsonValueKind.String      },
				{ JsonValueKind.Number      },
				{ JsonValueKind.True        },
				{ JsonValueKind.False       },
				//{ JsonValueKind.Object      },
				//{ JsonValueKind.Array       },
			};
	}
}

#endregion


#region Data

// consider auto validation by adding schema header type item
public interface IData
{
	// these go private -- when you look at a tree, the roots you dont see
	public IDataNodeBuilder DataBuilder { get; }

	public DmsEnum.TreeFormatType TreeFormatType { get; }
	public ITreeGraph<Guid, object> DataTree { get; }
	public IDataNode Root { get; }

	//public ITreeGraph<Guid, object> Schema { get; }

	public Guid RootId { get; }
	public void Display();

	// Static constructors
	public IData NewValue();
	public IData NewField();
	public IData NewList();
	public IData NewRecord();
	public IData NewDataset();

	public T CreateData<T>() where T : IDataNode;
	public T CreateDataAt<T>(IDataNode node) where T : IDataNode;
	public T CreateDataAt<T>(Guid id) where T : IDataNode;



	public IDataValue CreateValue(object value);
	public IDataField CreateField(string label, IDataNode dataValue);
	public IDataList CreateList(IEnumerable<IDataNode> dataValues);
	public IDataRecord CreateRecord(IEnumerable<IDataRecord> dataFields);
	public IDataset CreateDataSet(IEnumerable<IDataset> dataFields);


	public IDataValue AddNewValue(IDataNode add_at);
	public IDataField AddNewField(IDataNode add_at);
	public IDataList AddNewList(IDataNode add_at);
	public IDataRecord AddNewRecord(IDataNode add_at);
	public IDataset AddNewDataset(IDataNode add_at);

	public IDataValue CreateValue();
	public IDataField CreateField();
	public IDataList CreateList();
	public IDataRecord CreateRecord();
	public IDataset CreateDataSet();



	public IDataValue CreateValueAt(Guid id);
	public IDataField CreateFieldAt(Guid id);
	public IDataList CreateListAt(Guid id);
	public IDataRecord CreateRecordAt(Guid id);
	public IDataset CreateDataSetAt(Guid id);

	public IDataValue CreateValueAt(Guid id, object value);
	public IDataField CreateFieldAt(Guid id, string label, IDataNode dataValue);
	public IDataList CreateListAt(Guid id, IEnumerable<IDataNode> dataValues);
	public IDataRecord CreateRecordAt(Guid id, IEnumerable<IDataRecord> dataFields);
	public IDataset CreateDataSetAt(Guid id, IEnumerable<IDataset> dataFields);
}

public interface IDataNodeBuilder
{
	public IData DataContainer { get; }
	public IDataValue CreateNewValue();
	public IDataField CreateNewField();
	public IDataList CreateNewList();
	public IDataRecord CreateNewRecord();
	public IDataset CreateNewDataset();
}
public interface IDataPath : IEquatable<IDataPath>, IComparable<IDataPath>
{
	public IList<int> Items { get; }
	public IDataPath AddStepPosition();
	public IDataPath GetBasePath();
}
public interface IDataNode : IEquatable<IDataNode>
{
	public IData Container { get; }
	public IDataPath Path { get; }
	public Guid Id { get; }
	public DmsEnum.StructureType Structure { get; }
}
// node type implementations
public interface IDataValue : IDataNode, IEquatable<IDataValue>
{
	public new DmsEnum.StructureType Structure => DmsEnum.StructureType.Value;
	public object Value { get; }
	public void SetValue(object value);
}
public interface IDataField : IDataNode, IEquatable<IDataField>
{
	public new DmsEnum.StructureType Structure => DmsEnum.StructureType.Field;
	public IDataNode FieldValue { get; }
	public void SetFieldValue(IDataNode data);
	public string Label { get; }
	public void SetLabel(string label);
}
public interface IDataList : IDataNode, IEquatable<IDataList>
{
	public new DmsEnum.StructureType Structure => DmsEnum.StructureType.List;
	public IList<IDataNode> Items { get; }
	public IDictionary<string, int> ItemPositions { get; }

	public IDataNode AddNewItem<T>() where T : IDataNode;

	public void AddItem(IDataNode data);
	public void DeleteItem(IDataNode data);
	public void DeleteItemAt(int position);
}
public interface IDataRecord : IDataNode, IEquatable<IDataRecord>
{
	public new DmsEnum.StructureType Structure => DmsEnum.StructureType.Record;
	public IEnumerable<IDataField> Fields { get; }
	public IEnumerable<string> FieldLabels { get; }
	public IDictionary<string, int> FieldPositions { get; }
	public IDictionary<string, IDataNode> FieldValues { get; }
	//public IEnumerable<string> FlatLabels { get; }
	//public IEnumerable<string> FlatPositions { get; }
	//public IEnumerable<string> FlatValues { get; }

	public IDataField AddNewField();

	public void AddField(IDataField field);
	public void DeleteField(IDataField field);
	public void MoveFieldTo(IDataField field, int position);
}

public interface IDataset : IDataNode, IEquatable<IDataset>
{
	public new DmsEnum.StructureType Structure => DmsEnum.StructureType.Dataset;
	public IEnumerable<IDataRecord> Records { get; }
	public IEnumerable<string> FieldLabels { get; }
	public IEnumerable<string> FlatLabels { get; }
	public IDictionary<string, IEnumerable<IDataNode>> FieldValues { get; }
	public void AddRecord(IDataRecord record);
	public void DeleteRecord(IDataRecord record);
}





/// <summary>
/// Data tree is used to 
/// </summary>

//public class DataTree //: IDataTree
//{
//	private object ToDump() => new { Root };
//	
//	public ITreeGraph<Guid, IDataNode> Graph { get; set; }
//	public IDataNode Root => Graph.Root.HasValue ? Graph.Root.GetValue() : null;
//
//	public IDataValue CreateValue()
//	{
//		var value = new DataValue(this, Graph.CreateAndRegisterNode().Id);
//		Graph.SetNodeValue(value.Id, value);
//		return value;
//	}
//	public IDataField CreateField()
//	{
//		var field = new DataField(this, Graph.CreateAndRegisterNode().Id);
//		Graph.SetNodeValue(field.Id, field);
//		return field;
//	}
//	public IDataList CreateList()
//	{
//		var list = new DataList(this, Graph.CreateAndRegisterNode().Id);
//		Graph.SetNodeValue(list.Id, list);
//		return list;
//	}
//	public IDataRecord CreateRecord() => throw new NotImplementedException(); //new DataRecord(this, Graph.CreateAndRegisterNode().Id);
//	public IDataset CreateDataSet() => throw new NotImplementedException(); //new Dataset(this, Graph.CreateAndRegisterNode().Id);
//
//
//	public IDataValue CreateValue(object value)
//	{
//		var data = CreateValue();
//		data.SetValue(value);
//		return data;
//	}
//	public IDataField CreateField(string label, IDataNode data)
//	{
//		//Graph.GetNode(data.Id).Position.Dump();
//		var field = CreateField();
//		//Graph.GetNode(data.Id).Position.Dump();
//		field.SetLabel(label);
//		//Graph.GetNode(data.Id).Position.Dump();
//		//Graph.GetNode(data.Id).Position.Dump();
//		////Graph.GetNode(field.Id).Dump();
//		////Graph.GetNode(data.Id).Dump();
//		Graph.SetBranch(field.Id, data.Id);
//		field.SetFieldValue(data);
//		return field;
//	}
//
//	public IDataList CreateList(IEnumerable<IDataNode> items)
//	{
//		var list = CreateList();
//		foreach (var item in items)
//			list.AddValue(item);
//		return list;
//	}
//	public IDataRecord CreateRecord(IEnumerable<IDataRecord> dataFields) => throw new NotImplementedException();
//	public IDataset CreateDataSet(IEnumerable<IDataset> dataFields) => throw new NotImplementedException();
//
//
//	public IDataValue CreateValueAt(Guid id)
//	{
//		ValidateNodeForCreateAt(id);
//		var value = new DataValue(this, id);
//		Graph.SetNodeValue(id, value);
//		return value;
//	}
//	public IDataField CreateFieldAt(Guid id)
//	{
//		ValidateNodeForCreateAt(id);
//		var field = new DataField(this, id);
//		Graph.SetNodeValue(id, field);
//		return field;
//	}
//	public IDataList CreateListAt(Guid id)
//	{
//		ValidateNodeForCreateAt(id);
//		return new DataList(this, id);
//	}
//	public IDataRecord CreateRecordAt(Guid id) => throw new NotImplementedException();
//	public IDataset CreateDataSetAt(Guid id) => throw new NotImplementedException();
//	
//	
//
//	public IDataValue CreateValueAt(Guid id, object value)
//	{
//		var data = CreateValueAt(id);
//		data.SetValue(value);
//		return data;
//	}
//	public IDataField CreateFieldAt(Guid id, string label, IDataNode data)
//	{
//		ValidateNodeForCreateAt(id);
//		var field = CreateFieldAt(id);
//		field.SetLabel(label);
//		field.SetFieldValue(data);
//		return field;
//	}
//	
//	public IDataList CreateListAt(Guid id, IEnumerable<IDataNode> items)
//	{
//		ValidateNodeForCreateAt(id);
//		var list = new DataList(this, id, items);
//		Graph.SetNodeValue(id, list);
//		return list;		
//	}
//	public IDataRecord CreateRecordAt(Guid id, IEnumerable<IDataRecord> dataFields) => throw new NotImplementedException();
//	public IDataset CreateDataSetAt(Guid id, IEnumerable<IDataset> dataFields) => throw new NotImplementedException();
//
//	public DataTree()
//	{
//		Graph = new TreeGraph<Guid, IDataNode>(new GuidFactory());
//	}
//
//	protected void ValidateNodeForCreate(Guid id)
//	{
//		ValidateNodePresent(id);
//		ValidateNodeHasNoValue(id);
//		ValidateNodeHasNoBranches(id);
//	}
//	protected void ValidateNodeForCreateAt(Guid id)
//	{
//		ValidateNodePresent(id);
//		ValidateNodeHasNoValue(id);
//	}
//	protected void ValidateNodePresent(Guid id)
//	{
//		if (!Graph.NodeMap.ContainsKey(id)) 
//			throw new TreeGraphException($"DataTree's underlying tree graph does not contain a node with the specified id ({id}).");
//	}
//	protected void ValidateNodeHasNoValue(Guid id)
//	{
//		if (Graph.NodeHasValue(id)) 
//			throw new TreeGraphException($"DataTree's underlying tree graph already has a value specified for node with the specified id ({id}).");
//	}
//	protected void ValidateNodeHasNoBranches(Guid id)
//	{
//		if (Graph.GetNode(id).BranchIds.Any()) 
//			throw new TreeGraphException($"Node with specified id ({id}) has {Graph.GetNode(id).BranchIds.Count()} branches.");
//	}
//}
//public abstract class DataNodeBase: IDataNode
//{
//	public virtual DmsEnum.StructureType Structure { get; }
//	public IDataTree Tree { get; }
//	public Guid Id { get; }
//
//	public bool Equals(IDataNode other)
//	{
//		if (Structure != other.Structure) return false;
//		if (Structure == DmsEnum.StructureType.Value) return ((IDataValue)this).Equals((IDataValue)other);
//		if (Structure == DmsEnum.StructureType.Field) return ((IDataField)this).Equals((IDataField)other);
//		if (Structure == DmsEnum.StructureType.List) return ((IDataList)this).Equals((IDataList)other);
//		if (Structure == DmsEnum.StructureType.Record) return ((IDataRecord)this).Equals((IDataRecord)other);
//		if (Structure == DmsEnum.StructureType.Dataset) return ((IDataset)this).Equals((IDataset)other);
//		throw new NotImplementedException($"No type comparison logic defined for structures {Structure} and {other.Structure}.");
//    }
//	
//	public DataNodeBase(IDataTree tree, Guid id)
//	{
//		Tree = tree;
//		Id = id;
//	}
//}
//
//public class DataValue : DataNodeBase, IDataValue
//{
//	private object ToDump() => new { Id, Structure, Value };
//	
//	public new DmsEnum.StructureType Structure => DmsEnum.StructureType.Value;
//	public object _value { get; set; }
//	public object Value => _value;
//	public void SetValue(object value) => _value = value;
//
//	public DataValue(IDataTree tree, Guid id) : base(tree, id) => new DataValue(tree, id, null);
//	public DataValue(IDataTree tree, Guid id, object value) : base(tree, id)
//	{
//		_value = value;
//	}
//	
//	public bool Equals(IDataValue other)
//	{
//		if (AreEquatableType(this, other))
//			return this.Equals(other);
//		return false;
//	}
//	
//	/// <summary>
//	/// use reflection to determine whether the two object types are equatable
//	/// </summary>
//	private bool AreEquatableType(object obj1, object obj2)
//	{
//		Type type1 = obj1.GetType();
//		Type type2 = obj2.GetType();
//		try
//		{
//			if (type1.Equals(type2))
//				return true;
//				
//			Type iEquatable = typeof(IEquatable<>).MakeGenericType(type1);
//
//			if (!iEquatable.IsAssignableFrom(type1))
//				return false;
//
//			return true;
//		}
//		catch 
//		{
//			return false;
//		}
//	}
//}
//
//public class DataField : DataNodeBase, IDataField
//{
//	private object ToDump() => new { Id, Structure, Label, FieldValue };
//
//	public new DmsEnum.StructureType Structure => DmsEnum.StructureType.Field;
//
//	public IDataNode FieldValue => Tree.Graph.GetNodeValue(_valueId);
//	public void SetFieldValue(IDataNode data)
//	{
//		Tree.Graph.SetBranch(Id, data.Id);
//		_valueId = data.Id;
//	}
//	
//	public string Label => _label;
//	public void SetLabel(string label) => _label = label;
//	
//	private string _label { get; set; }
//	private Guid _valueId { get; set; }
//
//	public DataField(IDataTree tree, Guid id) : base(tree, id)
//	{
//		_label = null;
//	}
//	public DataField(IDataTree tree, Guid id, string label, IDataNode data) : base(tree, id)
//	{
//		_label = label;
//		_valueId = data.Id;
//		Tree.Graph.SetBranch(Id, data.Id);
//	}
//	
//	public bool Equals(IDataField other)
//	{
//		return (string.Equals(Label, other.Label, StringComparison.OrdinalIgnoreCase) && ((IDataNode)this).Equals((IDataNode)other));
//	}
//}
//
//public class DataList : DataNodeBase, IDataList
//{
//	public new DmsEnum.StructureType Structure => DmsEnum.StructureType.List;
//	public IList<IDataNode> Values => Tree.Graph.GetNode(Id).BranchIds.Select(b => Tree.Graph.GetNodeValue(b)).ToList();
//
//
//	public DataList(IDataTree tree, Guid id) : base(tree, id) { }
//	public DataList(IDataTree tree, Guid id, IEnumerable<IDataNode> items) : base(tree, id)
//	{
//		foreach (var item in items)
//			AddValue(item);
//	}
//	public void AddValue(IDataNode data) => Tree.Graph.SetBranch(Id, data.Id);
//
//	public void DeleteValue(IDataNode data) => Tree.Graph.DeleteNode(data.Id);
//
//	public void DeleteValue(int position) 
//	{
//		if (!Tree.Graph.GetNode(Id).HasStem) throw new InvalidOperationException("Cannot delete a node without a stem (might be root).");
//		else
//		{
//			var deleteId = Tree.Graph.GetNode(Tree.Graph.GetNode(Id).StemId)
//							.BranchIds.Where(b => Tree.Graph.PositionMap[b] == position)
//							.First();
//			Tree.Graph.DeleteNode(deleteId);
//		}
//	}
//
//	public bool Equals(IDataList other)
//	{
//		int count = Values.Count();
//		if (count != other.Values.Count())
//			return false;
//		else
//			for (int i = 0; i < count; i++)
//				if (!(Values[i].Equals(other.Values[i])))
//					return false;
//		return false;
//	}
//
//}

#endregion


*/

//#region Data Schema
///// <summary>
///// Data schema is a specialized tree 
///// </summary>
//public interface IDataSchema: IEquatable<IDataSchema>
//{
//
//}
///// <summary>
///// Data schema nodes are specialized for the specific tree 
///// </summary>
//public interface IDataSchemaNode: IEquatable<IDataSchemaNode>
//{
//	public DataStructure StructureType { get; }
//}
//public interface IDataSchemaValue : IDataSchemaNode
//{
//	public new DataStructure StructureType => DataStructure.Value;
//	public DataValue DataValueType { get; }
//}
//public interface IDataSchemaField : IDataSchemaValue
//{
//	public new DataStructure StructureType => DataStructure.Field;
//	public string Label { get; }
//}
//public interface IDataSchemaList : IDataSchemaNode
//{
//	public new DataStructure StructureType => DataStructure.List;
//	public string Label { get; }
//}
//public interface IDataSchemaRecord : IDataSchemaNode
//{
//	public new DataStructure StructureType => DataStructure.Record;
//	public IEnumerable<IDataSchemaField> Fields { get; }
//}
//public interface IDataSchemaDataset : IDataSchemaRecord
//{
//	public new DataStructure StructureType => DataStructure.Dataset;
//}
//
//
//#endregion
//
//
//#region Configurable Objects (Abstract)
///// <summary>
///// IConfigurableObjects are objects which could be configured through a UI
///// </summary>
//public interface IConfigurableObject
//{
//	/// <summary>
//	/// Confirms object has been configured correctly (uses custom attributes defined in AbstractConfigurableObject).
//	/// </summary>
//	public void ValidateConfiguration();
//
//	/// <summary>
//	/// Returns a list of all configurable props for the current object
//	/// </summary>
//	public PropertyInfo[] GetConfigurableProps();
//
//	/// <summary>
//	/// Returns the default value of a configurable prop if DefaultAttribute is applied
//	/// </summary>
//	public object GetConfigurablePropDefaultValue(PropertyInfo prop);
//
//	/// <summary>
//	/// Returns all the allowed values for a configurable prop AllowedAttribute is applied
//	/// </summary>
//	public object[] GetConfigurablePropAllowedValues(PropertyInfo prop);
//}
///// <summary>
///// AbstractConfigurableObject is the base class for objects that implement IConfigurableObject. The "configurability" of object fields is 
///// determined based on application of custom attributes (which are defined in this class).
///// </summary>
//public abstract class AbstractConfigurableObject : IConfigurableObject
//{
//	//////// Custom Attributes ////////
//	/// <summary>
//	/// ConfigurableAttribute - tagged property would be included in any configuration UI
//	/// </summary>
//	[AttributeUsage(AttributeTargets.Property)]
//	public class ConfigurableAttribute : Attribute { }
//
//	/// <summary>
//	/// RequiredAttribute - object will error on ValidateConfiguration() if no value is specified for tagged property
//	/// </summary>
//	[AttributeUsage(AttributeTargets.Property)]
//	public class RequiredAttribute : Attribute { }
//
//	/// <summary>
//	/// DefaultAttribute(Value) - will apply this value if none other is specified
//	/// </summary>
//	[AttributeUsage(AttributeTargets.Property)]
//	public class DefaultAttribute : Attribute
//	{
//		public object Value { get; }
//		public DefaultAttribute(object value)
//		{
//			Value = value;
//		}
//	}
//
//	/// <summary>
//	/// AllowedAttribute(Value[]) - will error on ValidateConfiguration() if specified value is not present
//	/// </summary>
//	[AttributeUsage(AttributeTargets.Property)]
//	public class AllowedAttribute : Attribute
//	{
//		public object[] Values { get; set; }
//		public AllowedAttribute(params object[] allowedValues) { Values = allowedValues; }
//	}
//
//
//	//////// IConfigurableObject ////////
//	public void ValidateConfiguration()
//	{
//		var configurableFields = GetFields<ConfigurableAttribute>();
//
//		// Validate configurable fields w/ DefaultAttribute 
//		foreach (PropertyInfo prop in GetFields<DefaultAttribute>())
//		{
//			// Confirm ConfigurableAttribute is applied
//			if (!configurableFields.Contains(prop))
//				throw new InvalidDataException($"Property named [{prop.Name}] on configurable object of type [{this.GetType().Name}] is tagged with DefaultValue attribute, but not with ConfigurableAttribute.");
//		}
//
//		// Validate configurable fields w/ RequiredAttribute  
//		foreach (PropertyInfo prop in GetFields<RequiredAttribute>())
//		{
//			// Confirm ConfigurableAttribute is applied
//			if (!configurableFields.Contains(prop))
//				throw new InvalidDataException($"Property named [{prop.Name}] on configurable object of type [{this.GetType().Name}] is tagged with RequiredAttribute, but not with ConfigurableAttribute.");
//
//			// Confirm field has value specified
//			if (prop.GetValue(this) == null)
//				throw new ConfigurableObjectException($"Configurable object instance of type [{this.GetType().Name}] does not have a value specified for required field [{prop.Name}].");
//		}
//
//		// Validate configurable fields w/ AllowedAttribute 
//		foreach (PropertyInfo prop in GetFields<AllowedAttribute>())
//		{
//			// Confirm ConfigurableAttribute is applied
//			if (!configurableFields.Contains(prop))
//				throw new InvalidDataException($"Property named [{prop.Name}] on configurable object of type [{this.GetType().Name}] is tagged with AllowedAttribute, but not with ConfigurableAttribute.");
//
//			// Confirm field's specified value is null or included in list of allowed values
//			if (!GetConfigurablePropAllowedValues(prop).Contains(prop.GetValue(this)))
//				throw new ConfigurableObjectException($"The specified value for configurable property [{prop.Name}] on configurable object [{this.GetType().Name}] is not allowed. "
//					+ $"Specified value is: [{prop.GetValue(this)}]. Allowed values are: [{string.Join(", ", GetConfigurablePropAllowedValues(prop))}].");
//		}
//
//	}
//	public PropertyInfo[] GetConfigurableProps() => GetFields<ConfigurableAttribute>().ToArray();
//	public object GetConfigurablePropDefaultValue(PropertyInfo prop)
//	{
//		try
//		{	
//			DefaultAttribute fieldDefault = (DefaultAttribute)prop.GetCustomAttribute(typeof(DefaultAttribute));
//			return fieldDefault.Value;
//		}
//		catch (Exception ex)
//		{
//			throw new ConfigurableObjectException($"Unable to get DefaultValue attribute and/or default value for configurable property [{prop.Name}].", ex);
//		}
//	}
//	public object[] GetConfigurablePropAllowedValues(PropertyInfo prop)
//	{
//		try
//		{
//			AllowedAttribute allowed = (AllowedAttribute)prop.GetCustomAttribute(typeof(AllowedAttribute));
//			return allowed.Values;
//		}
//		catch (Exception ex)
//		{
//			throw new ConfigurableObjectException($"Unable to get AllowedAttribute and/or allowed values for configurable property [{prop.Name}].", ex);
//		}
//	}
//
//
//
//	//////// Constructor ////////
//	public AbstractConfigurableObject() { ApplyDefaults(); }
//
//
//	//////// Internal Code ////////
//	/// <summary>
//	/// Default values are applied immediately after instantiation (so they can be overwritten later)
//	/// </summary>
//	private void ApplyDefaults()
//	{
//		// If field is null and has non-null default value specified, set field value equal to specified default
//		foreach (PropertyInfo prop in GetFields<DefaultAttribute>())
//			prop.SetValue(this, GetConfigurablePropDefaultValue(prop));
//	}
//
//	/// <summary>
//	/// Get fields with custom attribute of type T applied
//	/// </summary>
//	protected IEnumerable<PropertyInfo> GetFields<T>() where T : Attribute
//	{
//		return this.GetType().GetProperties().Where(p => p.GetCustomAttribute(typeof(T)) != null);
//	}
//}
//public class ConfigurableObjectException : Exception
//{
//	public ConfigurableObjectException() : base() { }
//	public ConfigurableObjectException(string message) : base(message) { }
//	public ConfigurableObjectException(string message, Exception innerException) : base(message, innerException) { }
//}
//#endregion
//
//
//#region DataSources
///// <summary>
///// Configurable data sources all expose GetStream()
///// </summary>
//public interface IDataStore: IConfigurableObject
//{
//	public DmsEnum.DataSource SourceType { get; }
//	public Stream GetStream();
//	public void ValidateSource();
//}
//
///// <summary>
///// DataSource namespace stores base class and actual implementations of IDataStore.
///// </summary>
//namespace DataSource
//{
//	public abstract class Base : AbstractConfigurableObject, IDataStore
//	{
//		/// <summary>
//		/// Each enum value in DataSource has a corresponding class which conforms to the IDataStore interface
//		/// </summary>
//		public abstract DmsEnum.DataSource SourceType { get; }
//
//		/// <summary>
//		/// Get whatever the requested data is and read it as a stream. 
//		/// Before getting data, validate configuration and ability to connect to source
//		/// </summary>
//		public Stream GetStream()
//		{
//			try { ValidateConfiguration(); }
//			catch (Exception ex) { throw new DataReaderException("Error when attempting to validate data source configuration.", ex); }
//
//			try { ValidateSource(); }
//			catch (Exception ex) { throw new DataReaderException("Error when attempting to validate data source.", ex); }
//
//			try { return GetStreamImplementation(); }
//			catch (Exception ex) { throw new DataReaderException("Error when attempting to read data stream from data source.", ex); }
//		}
//
//		/// <summary>
//		/// Throws error if the source is inaccessible 
//		/// </summary>
//		public abstract void ValidateSource();
//
//		/// <summary>
//		/// Stream reading implementations defined in derived classes
//		/// </summary>
//		protected abstract Stream GetStreamImplementation();
//	}
//
//
//	//////// Configurable Data Source Implementations ////////
//
//	/// <summary>
//	/// Reads a user-specified input string
//	/// </summary>
//	public class RawInput : Base, IDataStore
//	{
//		// Config
//
//		[Configurable]
//		[Required]
//		public string InputText { get; set; }
//
//		[Configurable]
//		[Required]
//		public Encoding InputTextEncoding { get; set; }
//
//
//		// Data source implementation
//		public override DmsEnum.DataSource SourceType => DmsEnum.DataSource.RawInput;
//		
//		public override void ValidateSource()
//		{
//			if (string.IsNullOrEmpty(InputText))
//				throw new DataReaderException("Can't read from source because InputText value is null or empty.");
//		}
//
//		protected override Stream GetStreamImplementation() => new MemoryStream(InputTextEncoding.GetBytes(InputText));
//	}
//
//	/// <summary>
//	/// Reads a file
//	/// </summary>
//	public class File : Base, IDataStore
//	{
//		// Config
//
//		[Configurable]
//		[Required]
//		public string Directory { get; set; }
//
//		[Configurable]
//		[Required]
//		public string FileName { get; set; }
//
//
//		// Data source implementation
//
//		public override DmsEnum.DataSource SourceType => DmsEnum.DataSource.File;
//
//		public override void ValidateSource()
//		{
//			if (!System.IO.Directory.Exists(Directory))
//				throw new DataReaderException($"Specified directory [{Directory}] does not exist.");
//			if (!Path.Exists(GetFilePath()))
//				throw new DataReaderException($"Specified file [{FileName}] does not exist in directory [{Directory}].");
//		}
//
//		protected override Stream GetStreamImplementation() => System.IO.File.Open(GetFilePath(), FileMode.Open);
//
//		private string GetFilePath() => Path.Combine(Directory, FileName);
//	}
//
//}
//public class DataReaderException : Exception
//{
//	public DataReaderException() : base() { }
//	public DataReaderException(string message) : base(message) { }
//	public DataReaderException(string message, Exception innerException) : base(message, innerException) { }
//}
//#endregion


//#region DataTreeBuilders
///// <summary>
///// Reads a data source to a data tree object
///// </summary>
//public interface IDataTreeBuilder<T>
//{
//	public DmsEnum.DataFormat DataFormat { get; }
//	public IDataTree<T> Build(IDataStore source);
//	public void ValidateFormat();
//}
//namespace DataTreeBuilder
//{
//	public abstract class Base : AbstractConfigurableObject, IDataTreeBuilder<T>
//	{
//		/// <summary>
//		/// Each enum value in DataFormat has a corresponding class which conforms to the IDataTreeBuilder interface
//		/// </summary>
//		public abstract DmsEnum.DataFormat DataFormat { get; }
//
//
//		/// <summary>
//		/// Reads a stream and returns a data tree
//		/// </summary>
//		public IDataTree<T> Build(IDataStore source)
//		{
//			try { ValidateConfiguration(); }
//			catch (Exception ex) { throw new DataReaderException("Error when attempting to validate data tree builder configuration.", ex); }
//
//			//try { ValidateFormat(); }
//			//catch (Exception ex) { throw new DataReaderException("Error when attempting to validate data source.", ex); }
//
//			try { return BuildImplementation(source); }
//			catch (Exception ex) { throw new DataReaderException("Error when attempting to read data stream from data source.", ex); }
//		}
//
//		/// <summary>
//		/// Throws error if the format doesn't match expected
//		/// </summary>
//		public abstract void ValidateFormat();
//
//		/// <summary>
//		/// Stream reading implementations defined in derived classes
//		/// </summary>
//		protected abstract IDataTree<T> BuildImplementation(IDataStore source);
//	}
//
//	/// <summary>
//	/// DSV = Delimitter-Separated Values
//	/// </summary>
//	public class FromDsv : Base
//	{
//		// Mostly conforms to https://www.rfc-editor.org/rfc/rfc4180
//
//		[Configurable]
//		[Required]
//		[Default(true)]
//		public bool HasHeaders { get; set; }
//
//		[Configurable]
//		[Required]
//		[Default(',')]
//		public char Delimitter { get; set; }
//
//		[Configurable]
//		[Required]
//		[Default('"')]
//		public char Escaper { get; set; }
//
//		// Sets to Environment.NewLine if nothing specified
//		[Configurable]
//		public string LineBreak { get; set; }
//
//
//		public FromDsv() : base()
//		{
//			LineBreak = LineBreak ?? Environment.NewLine;
//		}
//
//		public override DmsEnum.DataFormat DataFormat => DmsEnum.DataFormat.Dsv;
//
//		public override void ValidateFormat()
//		{
//			throw new NotImplementedException();
//		}
//		
//		protected override IDataTree BuildImplementation(IDataStore source)
//		{
//			var tree = new DataTree();
//
//			using (var stream = source.GetStream())
//			using (var reader = new StreamReader(stream))
//			{
//				string line;
//				while ((line = reader.ReadLine()) != null)
//				{
//					var recordNode = tree.CreateNode();
//					tree.Root.SetBranch(recordNode.Id);
//
//					foreach (var nodeVal in ParseDelimittedStringLine(line))
//					{
//						var valueNode = tree.CreateNode();
//						recordNode.SetBranch(valueNode.Id);
//
//						valueNode.SetValue(nodeVal);
//					}
//				}
//			}
//			return tree;
//		}
//		
//		private class ParserState
//		{
//			public List<char> ValueChars { get; set; }
//			public bool InValue { get; set; }
//			public bool Escaped { get; set; }
//			public char? PrevChar { get; set; }
//		}
//
//		private IEnumerable<string> ParseDelimittedStringLine(string input)
//		{
//			var state = new ParserState()
//			{
//				ValueChars = new List<char>(),
//				InValue = false,
//				Escaped = false,
//				PrevChar = null
//			};
//
//			// Enumerate characters, yielding a string whenever a new delimitter is encounterd
//			for (int i = 0; i < input.Length; i++)
//			{
//				char c = input[i];
//
//				// Parse through, aggregate chars, write to list when delimitter is encountered (unless escaped)
//				if (c == Delimitter && !state.Escaped)
//				{
//					if (state.ValueChars.Any())
//					{
//						yield return string.Join("", state.ValueChars);
//						state.ValueChars.Clear();
//					}
//					else
//					{
//						yield return null;
//					}
//				}
//				else
//				{
//					if (c == Escaper && state.Escaped)
//					{
//						state.Escaped = false;
//					}
//					else if (c == Escaper && !state.Escaped)
//					{
//						state.Escaped = true;
//
//						// If prev char was the escaper, that means that it's a double quote string (so add escaper char to values list and resume escape)
//						if (state.PrevChar == Escaper)
//							state.ValueChars.Add(c);
//					}
//					else
//					{
//						state.ValueChars.Add(c);
//					}
//				}
//
//				// If there are no more characters then return whatever is held in values at the moment
//				if (i + 1 == input.Length)
//					if (state.ValueChars.Any())
//						yield return string.Join("", state.ValueChars);
//					else
//						yield return null;
//
//				state.PrevChar = c;
//				//char c = str[i];
//
//			}
//		}
//
//
//    }
//
//	public class FromJson : Base
//	{
//
//		[Configurable]
//		[Required]
//		public JsonDocumentOptions JsonDocumentOptions { get; set; }
//
//
//		public override DmsEnum.DataFormat DataFormat => DmsEnum.DataFormat.Json;
//
//		public override void ValidateFormat()
//		{
//			throw new NotImplementedException();
//		}
//		
//		protected override IDataTree BuildImplementation(IDataStore source)
//		{
//			var tree = new DataTree();
//
//			using (var stream = source.GetStream())
//			using (JsonDocument doc = JsonDocument.Parse(stream, JsonDocumentOptions))
//			{
//				doc.Dump();
//				ParseRecursive(tree, tree.Root.Id, doc.RootElement);
//			}
//			return tree;
//		}
//
//
//		private void ParseRecursive(IDataTree tree, Guid currentNodeId, JsonElement element)
//		{
//			if (element.ValueKind == JsonValueKind.Object)
//			{
//				ParseJsonObject(tree, currentNodeId, element);
//			}
//			else if (element.ValueKind == JsonValueKind.Array)
//			{
//				ParseJsonArray(tree, currentNodeId, element);
//			}
//			else
//			{
//				ParseJsonValue(tree, currentNodeId, element);
//			}
//
//		}
//
//		private void ParseJsonObject(IDataTree tree, Guid currentNodeId, JsonElement element)
//		{
//			var record = tree.CreateNode();
//			tree.NodeMap[currentNodeId].SetBranch(record.Id);
//
//			foreach (JsonProperty prop in element.EnumerateObject())
//			{
//				var field = tree.CreateNode();
//				record.SetBranch(field.Id);
//
//				var label = tree.CreateNode();
//				field.SetBranch(label.Id);
//				label.SetValue(prop.Name);
//				
//				ParseRecursive(tree, field.Id, prop.Value);
//			}
//		}
//
//		private void ParseJsonArray(IDataTree tree, Guid currentNodeId, JsonElement element)
//		{
//			var list = tree.CreateNode();
//			tree.NodeMap[currentNodeId].SetBranch(list.Id);
//
//			foreach (JsonElement item in element.EnumerateArray())
//			{
//				ParseRecursive(tree, list.Id, item);
//			}
//		}
//
//		private void ParseJsonValue(IDataTree tree, Guid currentNodeId, JsonElement element)
//		{
//			if (!JsonValueKindValue.Contains(element.ValueKind))
//				throw new InvalidDataException("Element is expected to be a value but does not have expected JsonValueKind.");
//				
//			var value = tree.CreateNode();
//			tree.NodeMap[currentNodeId].SetBranch(value.Id);
//			value.SetValue(element.ToString());
//
//		}
//
//		private HashSet<JsonValueKind> JsonValueKindValue = new HashSet<JsonValueKind>()
//		{
//			{ JsonValueKind.Null        },
//			{ JsonValueKind.Undefined   },
//			{ JsonValueKind.String      },
//			{ JsonValueKind.Number      },
//			{ JsonValueKind.True        },
//			{ JsonValueKind.False       },
//			//{ JsonValueKind.Object      },
//			//{ JsonValueKind.Array       },
//		};
//	}
//}
//public class DataTreeBuilderException : Exception
//{
//	public DataTreeBuilderException() : base() { }
//	public DataTreeBuilderException(string message) : base(message) { }
//	public DataTreeBuilderException(string message, Exception innerException) : base(message, innerException) { }
//}
//#endregion
//






//
//public interface IDataValueNode<T> : IDataNode<T> where T : IEquatable<T>
//{
//	public dmsEnum.DataValue DataValueEnum { get; }
//	public object? Value { get; }
//}
//public interface IDataFieldNode<T> : IDataNode<T> where T : IEquatable<T>
//{
//	public IDataValueNode<T> Label { get; }
//	public IDataNode<T> FieldValue { get; }
//}
//public interface IDataListNode<T> : IDataNode<T> where T : IEquatable<T>
//{
//	public IList<IDataNode<T>> Nodes { get; }
//}
//public interface IDataRecordNode<T> : IDataListNode<T> where T : IEquatable<T>
//{
//	public new IList<IDataFieldNode<T>> Nodes { get; }
//	public IDictionary<string, IDataNode<T>> Fields { get; }
//}
//public interface IDatasetNode<T> : IDataListNode<T> where T : IEquatable<T>
//{
//	public new IList<IDataRecordNode<T>> Nodes { get; }
//	public IDictionary<string, IEnumerable<IDataNode<T>>> FieldValues { get; }
//}
//



//
//public class DataValueNode : DataNode, IDataValueNode<Guid>
//{
//	public dmsEnum.DataValue DataValueEnum { get; }
//	public object Value { get; }
//
//	public DataValueNode(IDataTree tree): base(tree) 
//	{ }
//}
//
//public class DataFieldNode : DataNode, IDataFieldNode<Guid>
//{
//	public IDataValueNode<Guid> Label { get; }
//	public IDataNode FieldValue { get; }
//
//	public DataFieldNode(IDataValueNode<Guid> fieldLabel, IDataNode fieldValue) : base()
//	{
//		Label = fieldLabel;
//		FieldValue = fieldValue;
//	}
//}
//public class DataListNode : DataNode, IDataListNode<Guid>
//{
//	public IList<IDataNode> Nodes => _nodes;
//	protected IList<IDataNode> _nodes { get; set; }
//
//	public DataListNode() {}
//	public DataListNode(IList<IDataNode> nodes) : base()
//	{
//		_nodes = nodes;
//	}
//}
//public class DataRecordNode : DataListNode, IDataRecordNode<Guid>
//{
//	public new IList<IDataFieldNode<Guid>> Nodes => _nodes;
//	protected new IList<IDataFieldNode<Guid>> _nodes { get; set; }
//
//	public IDictionary<string, IDataNode> Fields => (IDictionary<string, IDataNode>)_fields;
//	private IDictionary<string, IDataNode> _fields { get; set; }
//
//	public DataRecordNode(IList<IDataFieldNode<Guid>> nodes) : base()
//	{
//		_nodes = nodes;			
//		
//		_fields = new Dictionary<string, IDataNode>();
//		if (_nodes.Any())
//		{
//			foreach (var field in _nodes)
//				_fields.Add(field.Label.Value.ToString(), field);
//		}
//	}
//}
//public class DatasetNode : DataListNode, IDatasetNode<Guid>
//{
//	public new IList<IDataRecordNode<Guid>> Nodes => _nodes;
//	protected new IList<IDataRecordNode<Guid>> _nodes { get; set; }
//
//	public IDictionary<string, IEnumerable<IDataNode>> FieldValues => throw new NotImplementedException();
//	private IDictionary<string, IEnumerable<IDataNode>> _fieldValues { get; set; }
//
//	public DatasetNode(IList<IDataRecordNode<Guid>> nodes) : base()
//	{
//		_nodes = nodes;
//		_fieldValues = new Dictionary<string, IEnumerable<IDataNode>>();
//		if (_nodes.Any())
//		{
//			//foreach (var field in _nodes)
//			//{
//			//	_fieldValues.Add(field.Value.ToString(), field);
//			//}
//		}
//	}
//}

