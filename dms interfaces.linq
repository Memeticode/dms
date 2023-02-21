<Query Kind="Program">
  <Namespace>System.Runtime.InteropServices.ComTypes</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>DmsTypeEnums</Namespace>
</Query>

void Main()
{
	//var tester = new TreeGraphTests();
	//tester.RunAllTests();

	//IDataSource Source = new RawInput()
	//{
	//	Text = $"A,B,C{Environment.NewLine}1,2,3{Environment.NewLine}\"1a\",2,3",
	//	TextEncoding = Encoding.UTF8
	//};
	//IDataFormat Format = new DelimitterSeparatedValues()
	//{
	//	HasHeaders = true,
	//	Delimitter = ',',
	//	Escaper = '"',
	//	LineBreak = Environment.NewLine
	//};

	IDataFormat Format = new Json()
	{
		JsonDocumentOptions = new JsonDocumentOptions()
		{
			AllowTrailingCommas = true,
			CommentHandling = JsonCommentHandling.Skip,
			MaxDepth = 64
		},
	};
	IDataSource Source = new File()
	{
		Directory = @"C:\Users\Work\Projects\dms\SampleData\JSON",
		FileName = "sampleRecord.json",
	};

	var tree = Format.GetTree(Source);

	//tree.Dump();
	//tree.Height.Dump();
	foreach (var node in tree.EnumerateNodes())
	{
		$"{new string('-', node.Depth)}{(node.HasValue ? node.GetValue()?.ToString() : "")}".Dump();
		//$"{new string('-', node.Depth)}{node.Id} {(node.HasValue ? node.GetValue()?.ToString() : "")}".Dump();
	}
	//"----".Dump();


}

/*
Core abstractions:

Any data from data sources (i.e. file, database, etc.) is represented as a graph

	- Data Tree: a tree graph comprised of data nodes.
		- Root node is always a structure node
		- Leaf nodes are always value nodes

	- Data Node: there are 2 types of data node, structure and value
		- Structure: contains other data nodes (types specified by DataStructure enum)
		- Value: does not contain other data nodes, instead specifies a reference to a value, which can be null (types specified by DataType enum)


*/

namespace DmsTypeEnums
{
	/// <summary>
	/// Specifies DMS enum types
	/// </summary>
	public enum TypeIndex
	{
		DataStructure,
		DataValue,
		DataSource,
		DataFormat,
	}

	/// <summary>
	/// Data structure type (describes DataNode types)
	/// </summary>
	public enum DataStructure
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
	public enum DataValue
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
	/// Data sources which can be read from
	/// </summary>
	public enum DataSource
	{
		RawInput,   // i.e. hard-coded string
		File,
		Database,
		Api,
	}

	/// <summary>
	/// Data storage formats used by data source
	/// </summary>
	public enum DataFormat
	{
		Dsv,    // delimitted string values
		Json,
		Xml,
		Sql,
		PDBx_mmCIF,
	}


	/// <summary>
	/// Extensions class maps string values to corresponding enum, may be used for configuring (i.e. specifying list of values)
	/// </summary>
	public static class DmsEnumExtensions
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
		private static readonly Dictionary<string, DataStructure> DataStructureMap = DmsEnumExtensions.GenerateEnumNameValueMap<DataStructure>();
		private static readonly Dictionary<string, DataValue> DataValueMap = DmsEnumExtensions.GenerateEnumNameValueMap<DataValue>();
		private static readonly Dictionary<string, DataSource> DataSourceMap = DmsEnumExtensions.GenerateEnumNameValueMap<DataSource>();
		private static readonly Dictionary<string, DataFormat> DataFormatMap = DmsEnumExtensions.GenerateEnumNameValueMap<DataFormat>();


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
/// Tree graph is used as underlying structure data from different sources can be mapped to for dataset
/// </summary>
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

	public void DeleteNode(T id);   // can only delete node if it has no branches
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
public class TreeGraphException : Exception
{
	public TreeGraphException() : base() { }
	public TreeGraphException(string message) : base(message) { }
	public TreeGraphException(string message, Exception innerException) : base(message, innerException) { }
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



#region DataSource Interfaces
public interface IDataSource
{
	public DmsTypeEnums.DataSource SourceType { get; }
	public Stream GetStream();
	public void ValidateSource();
}

/// <summary>
/// Get whatever the requested data is and read it as a stream. 
/// Before getting data, validate configuration and ability to connect to source
/// </summary>
public abstract class DataSourceBase : IDataSource
{
	/// <summary>
	/// Each enum value in DataSource has a corresponding class which conforms to the IDataSource interface
	/// </summary>
	public abstract DmsTypeEnums.DataSource SourceType { get; }
	
	public Stream GetStream()
	{
		//try { ValidateConfiguration(); }
		//catch (Exception ex) { throw new DataSourceException("Error when attempting to validate data source configuration.", ex); }
		
		try { ValidateSource(); }
		catch (Exception ex) { throw new DataSourceException("Error when attempting to validate data source.", ex); }

		try { return GetStreamImplementation(); }
		catch (Exception ex) { throw new DataSourceException("Error when attempting to read data stream from data source.", ex); }
	}

	/// <summary>
	/// Throws error if the source is inaccessible 
	/// </summary>
	public abstract void ValidateSource();

	/// <summary>
	/// Stream reading implementations defined in derived classes
	/// </summary>
	protected abstract Stream GetStreamImplementation();
}
public class DataSourceException : Exception
{
	public DataSourceException() : base() { }
	public DataSourceException(string message) : base(message) { }
	public DataSourceException(string message, Exception innerException) : base(message, innerException) { }
}


/// <summary>
/// Reads a user-specified input string
/// </summary>
public class RawInput : DataSourceBase, IDataSource
{
	// Config
	public string Text { get; set; }
	public Encoding TextEncoding { get; set; }


	// Data source implementation
	public override DmsTypeEnums.DataSource SourceType => DmsTypeEnums.DataSource.RawInput;

	public override void ValidateSource()
	{
		if (string.IsNullOrEmpty(Text))
			throw new DataSourceException("Can't read from source because InputText value is null or empty.");
	}
	protected override Stream GetStreamImplementation() => new MemoryStream(TextEncoding.GetBytes(Text));
}
/// <summary>
/// Reads a file
/// </summary>
public class File : DataSourceBase, IDataSource
{

	public override DmsTypeEnums.DataSource SourceType => DmsTypeEnums.DataSource.File;
	
	// Config
	public string Directory { get; set; }
	public string FileName { get; set; }

	// Data source implementation	
	public override void ValidateSource()
	{
		if (!System.IO.Directory.Exists(Directory))
			throw new DataSourceException($"Specified directory [{Directory}] does not exist.");
		if (!Path.Exists(GetFilePath()))
			throw new DataSourceException($"Specified file [{FileName}] does not exist in directory [{Directory}].");
	}
	protected override Stream GetStreamImplementation() => System.IO.File.Open(GetFilePath(), FileMode.Open);

	private string GetFilePath() => Path.Combine(Directory, FileName);
}


public interface IDataFormat
{
	public DmsTypeEnums.DataFormat DataFormat { get; }
	public ITreeGraph<Guid, object> GetTree(IDataSource source);
	//public void ValidateFormat();
}

public abstract class DataFormatBase : IDataFormat
{
	/// <summary>
	/// Each enum value in DataFormat has a corresponding class which conforms to the IDataTreeBuilder interface
	/// </summary>
	public abstract DmsTypeEnums.DataFormat DataFormat { get; }


	/// <summary>
	/// Reads a stream and returns a data tree
	/// </summary>
	public ITreeGraph<Guid, object> GetTree(IDataSource source)
	{
		//try { ValidateConfiguration(); }
		//catch (Exception ex) { throw new DataSourceException("Error when attempting to validate data tree builder configuration.", ex); }

		//try { ValidateFormat(); }
		//catch (Exception ex) { throw new DataSourceException("Error when attempting to validate data source.", ex); }

		try { return GetTreeImplementation(source); }
		catch (Exception ex) { throw new DataSourceException("Error when attempting to read data stream from data source.", ex); }
	}

	/// <summary>
	/// Throws error if the format doesn't match expected
	/// </summary>
	public abstract void ValidateFormat();

	/// <summary>
	/// Stream reading implementations defined in derived classes
	/// </summary>
	protected abstract ITreeGraph<Guid, object> GetTreeImplementation(IDataSource source);
}


/// <summary>
/// DSV = Delimitter-Separated Values
/// </summary>
public class DelimitterSeparatedValues : DataFormatBase
{
	// Mostly conforms to https://www.rfc-editor.org/rfc/rfc4180
	public bool HasHeaders { get; set; }
	public char Delimitter { get; set; }
	public char Escaper { get; set; }
	public string LineBreak { get; set; }


	public DelimitterSeparatedValues() : base()
	{ }

	public override DmsTypeEnums.DataFormat DataFormat => DmsTypeEnums.DataFormat.Dsv;

	public override void ValidateFormat()
	{
		throw new NotImplementedException();
	}

	protected override ITreeGraph<Guid, object> GetTreeImplementation(IDataSource source)
	{
		var tree = new TreeGraph<Guid, object>(new GuidFactory());

		using (var stream = source.GetStream())
		using (var reader = new StreamReader(stream))
		{
			string line;
			while ((line = reader.ReadLine()) != null)
			{
				var newRow = tree.CreateAndRegisterNode();
				tree.AddBranch(tree.Root, newRow);

				foreach (var nodeVal in ParseDelimittedStringLine(line))
				{
					var newNode = tree.CreateAndRegisterNode();
					tree.AddBranch(newRow, newNode);
					newNode.SetValue(nodeVal);
				}
			}
		}
		return tree;
	}

	private class ParserState
	{
		public List<char> ValueChars { get; set; }
		public bool InValue { get; set; }
		public bool Escaped { get; set; }
		public char? PrevChar { get; set; }
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

}

public class Json: DataFormatBase
{

	public override DmsTypeEnums.DataFormat DataFormat => DmsTypeEnums.DataFormat.Json;
	public JsonDocumentOptions JsonDocumentOptions { get; set; }

	public override void ValidateFormat()
	{
		throw new NotImplementedException();
	}

	protected override ITreeGraph<Guid, object> GetTreeImplementation(IDataSource source)
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
		tree.AddBranch(currentNodeId, record.Id);

		foreach (JsonProperty prop in element.EnumerateObject())
		{
			var field = tree.CreateAndRegisterNode();
			tree.AddBranch(record, field);

			var label = tree.CreateAndRegisterNode();
			tree.AddBranch(field, label);
			label.SetValue(prop.Name);

			ParseRecursive(tree, field.Id, prop.Value);
		}
	}

	private void ParseJsonArray(ITreeGraph<Guid, object> tree, Guid currentNodeId, JsonElement element)
	{
		var list = tree.CreateAndRegisterNode();
		tree.AddBranch(currentNodeId, list.Id);

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
		tree.AddBranch(currentNodeId, value.Id);
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
#endregion

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
//public interface IDataSource: IConfigurableObject
//{
//	public DmsTypeEnums.DataSource SourceType { get; }
//	public Stream GetStream();
//	public void ValidateSource();
//}
//
///// <summary>
///// DataSource namespace stores base class and actual implementations of IDataSource.
///// </summary>
//namespace DataSource
//{
//	public abstract class Base : AbstractConfigurableObject, IDataSource
//	{
//		/// <summary>
//		/// Each enum value in DataSource has a corresponding class which conforms to the IDataSource interface
//		/// </summary>
//		public abstract DmsTypeEnums.DataSource SourceType { get; }
//
//		/// <summary>
//		/// Get whatever the requested data is and read it as a stream. 
//		/// Before getting data, validate configuration and ability to connect to source
//		/// </summary>
//		public Stream GetStream()
//		{
//			try { ValidateConfiguration(); }
//			catch (Exception ex) { throw new DataSourceException("Error when attempting to validate data source configuration.", ex); }
//
//			try { ValidateSource(); }
//			catch (Exception ex) { throw new DataSourceException("Error when attempting to validate data source.", ex); }
//
//			try { return GetStreamImplementation(); }
//			catch (Exception ex) { throw new DataSourceException("Error when attempting to read data stream from data source.", ex); }
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
//	public class RawInput : Base, IDataSource
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
//		public override DmsTypeEnums.DataSource SourceType => DmsTypeEnums.DataSource.RawInput;
//		
//		public override void ValidateSource()
//		{
//			if (string.IsNullOrEmpty(InputText))
//				throw new DataSourceException("Can't read from source because InputText value is null or empty.");
//		}
//
//		protected override Stream GetStreamImplementation() => new MemoryStream(InputTextEncoding.GetBytes(InputText));
//	}
//
//	/// <summary>
//	/// Reads a file
//	/// </summary>
//	public class File : Base, IDataSource
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
//		public override DmsTypeEnums.DataSource SourceType => DmsTypeEnums.DataSource.File;
//
//		public override void ValidateSource()
//		{
//			if (!System.IO.Directory.Exists(Directory))
//				throw new DataSourceException($"Specified directory [{Directory}] does not exist.");
//			if (!Path.Exists(GetFilePath()))
//				throw new DataSourceException($"Specified file [{FileName}] does not exist in directory [{Directory}].");
//		}
//
//		protected override Stream GetStreamImplementation() => System.IO.File.Open(GetFilePath(), FileMode.Open);
//
//		private string GetFilePath() => Path.Combine(Directory, FileName);
//	}
//
//}
//public class DataSourceException : Exception
//{
//	public DataSourceException() : base() { }
//	public DataSourceException(string message) : base(message) { }
//	public DataSourceException(string message, Exception innerException) : base(message, innerException) { }
//}
//#endregion


//#region DataTreeBuilders
///// <summary>
///// Reads a data source to a data tree object
///// </summary>
//public interface IDataTreeBuilder<T>
//{
//	public DmsTypeEnums.DataFormat DataFormat { get; }
//	public IDataTree<T> Build(IDataSource source);
//	public void ValidateFormat();
//}
//namespace DataTreeBuilder
//{
//	public abstract class Base : AbstractConfigurableObject, IDataTreeBuilder<T>
//	{
//		/// <summary>
//		/// Each enum value in DataFormat has a corresponding class which conforms to the IDataTreeBuilder interface
//		/// </summary>
//		public abstract DmsTypeEnums.DataFormat DataFormat { get; }
//
//
//		/// <summary>
//		/// Reads a stream and returns a data tree
//		/// </summary>
//		public IDataTree<T> Build(IDataSource source)
//		{
//			try { ValidateConfiguration(); }
//			catch (Exception ex) { throw new DataSourceException("Error when attempting to validate data tree builder configuration.", ex); }
//
//			//try { ValidateFormat(); }
//			//catch (Exception ex) { throw new DataSourceException("Error when attempting to validate data source.", ex); }
//
//			try { return BuildImplementation(source); }
//			catch (Exception ex) { throw new DataSourceException("Error when attempting to read data stream from data source.", ex); }
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
//		protected abstract IDataTree<T> BuildImplementation(IDataSource source);
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
//		public override DmsTypeEnums.DataFormat DataFormat => DmsTypeEnums.DataFormat.Dsv;
//
//		public override void ValidateFormat()
//		{
//			throw new NotImplementedException();
//		}
//		
//		protected override IDataTree BuildImplementation(IDataSource source)
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
//					tree.Root.AddBranch(recordNode.Id);
//
//					foreach (var nodeVal in ParseDelimittedStringLine(line))
//					{
//						var valueNode = tree.CreateNode();
//						recordNode.AddBranch(valueNode.Id);
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
//		public override DmsTypeEnums.DataFormat DataFormat => DmsTypeEnums.DataFormat.Json;
//
//		public override void ValidateFormat()
//		{
//			throw new NotImplementedException();
//		}
//		
//		protected override IDataTree BuildImplementation(IDataSource source)
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
//			tree.NodeMap[currentNodeId].AddBranch(record.Id);
//
//			foreach (JsonProperty prop in element.EnumerateObject())
//			{
//				var field = tree.CreateNode();
//				record.AddBranch(field.Id);
//
//				var label = tree.CreateNode();
//				field.AddBranch(label.Id);
//				label.SetValue(prop.Name);
//				
//				ParseRecursive(tree, field.Id, prop.Value);
//			}
//		}
//
//		private void ParseJsonArray(IDataTree tree, Guid currentNodeId, JsonElement element)
//		{
//			var list = tree.CreateNode();
//			tree.NodeMap[currentNodeId].AddBranch(list.Id);
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
//			tree.NodeMap[currentNodeId].AddBranch(value.Id);
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


/*

public interface IDataTree<T> where T : IEquatable<T>
{
    public IDictionary<T, IDataNode<T>> NodeMap { get; }
    public IDictionary<T, object> ValueMap { get; }

    public IDictionary<T, T> StemIdMap { get; }
    public IDictionary<T, HashSet<T>> BranchIdsMap { get; }
    public IDictionary<T, uint> PositionMap { get; }
    public IDictionary<T, string> NodePathMap { get; }
    public IDictionary<string, T> PathNodeMap { get; }

    public IDataNode<T> Root { get; }

}


public class DataNode<T>: IDataNode<T> where T : IEquatable<T>
{
    public IDataNodeTypeEnum TypeEnum { get; }
    public T Id { get; }

    public DataNode(IDataNodeTypeEnum type, T id)
    {
        TypeEnum = type;
        Id = id;
    }
}
public class DataTree<T>: IDataTree<T> where T : IEquatable<T>
{
    public IDictionary<T, IDataNode<T>> NodeMap { get; set; }
    public IDictionary<T, object> ValueMap { get; set; }

    public IDictionary<T, T> StemIdMap { get; set; }
    public IDictionary<T, HashSet<T>> BranchIdsMap { get; set; }
    public IDictionary<T, uint> PositionMap { get; set; }
    public IDictionary<T, string> NodePathMap { get; set; }
    public IDictionary<string, T> PathNodeMap { get; set; }

    public IDataNode<T> Root { get; set; }
}

*/
