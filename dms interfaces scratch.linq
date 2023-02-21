<Query Kind="Program">
  <Namespace>System.Runtime.InteropServices.ComTypes</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>DmsTypeEnums</Namespace>
</Query>

void Main()
{
	//
	//	IDataSource Source = new DataSource.RawInput()
	//	{
	//		InputText = "A,B,C\r\n1,2,3\r\n\"1a\",2,3",
	//		InputTextEncoding = Encoding.UTF8
	//	};
	//	IDataTreeBuilder TreeBuilder = new DataTreeBuilder.FromDsv()
	//	{
	//		//HasHeaders = true,
	//		//Delimitter = ',',
	//		//Escaper = '"',
	//		LineBreak = Environment.NewLine,
	//	};
	//	
	//	var tg = TreeBuilder.Build(Source);
	//	tg.Dump();



	//	IDataSource Source = new DataSource.File()
	//	{
	//		Directory = @"C:\Users\Work\Projects\dms\SampleData\JSON",
	//		FileName = "sampleRecord.json",
	//	};
	//	
	//	IDataTreeBuilder TreeBuilder = new DataTreeBuilder.FromJson()
	//	{
	//		JsonDocumentOptions = new JsonDocumentOptions()
	//		{
	//			AllowTrailingCommas = true,
	//			CommentHandling = JsonCommentHandling.Skip,
	//			MaxDepth = 64
	//		},
	//	};
	//
	//	var tg = TreeBuilder.Build(Source);
	//	tg.Dump();


	IDataSource Source = new DataSource.File()
	{
		Directory = @"C:\Users\Work\Projects\dms\SampleData\JSON",
		FileName = "sampleRecord.json",
	};

	IDataTreeBuilder TreeBuilder = new DataTreeBuilder.FromJson()
	{
		JsonDocumentOptions = new JsonDocumentOptions()
		{
			AllowTrailingCommas = true,
			CommentHandling = JsonCommentHandling.Skip,
			MaxDepth = 64
		},
	};
	

	var tg = TreeBuilder.Build(Source);
	tg.Dump();



}

/*
Core abstractions:

Any data from data sources (i.e. file, database, etc.) is represented as a graph

	- Data Tree: a tree graph comprised of data nodes.
		- Root node is always a structure node
		- Leaf nodes are always value nodes

	- Data Node: there are 2 types of data node, structure and value
		- Structure: contains other data nodes (types specified by DataNodeStructure enum)
		- Value: does not contain other data nodes, instead specifies a reference to a value, which can be null (types specified by DataType enum)


*/

namespace DmsTypeEnums
{
	/// <summary>
	/// Specifies DMS enum types
	/// </summary>
	public enum TypeIndex
	{
		DataNodeStructure,
		DataValue,
		DataSource,
		DataFormat,
	}

	/// <summary>
	/// Data structure type (describes DataNode types)
	/// </summary>
	public enum DataNodeStructure
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
		private static readonly Dictionary<string, DataNodeStructure> DataNodeStructureMap = DmsEnumExtensions.GenerateEnumNameValueMap<DataNodeStructure>();
		private static readonly Dictionary<string, DataValue> DataValueMap = DmsEnumExtensions.GenerateEnumNameValueMap<DataValue>();
		private static readonly Dictionary<string, DataSource> DataSourceMap = DmsEnumExtensions.GenerateEnumNameValueMap<DataSource>();
		private static readonly Dictionary<string, DataFormat> DataFormatMap = DmsEnumExtensions.GenerateEnumNameValueMap<DataFormat>();


		//private static readonly Dictionary<DmsEnumType, Dictionary<string, Enum>> DmsEnumMap
		//	= new Dictionary<DmsEnumType, Dictionary<string, Enum>>()
		//	{
		//		{ DmsEnumType.DataNodeStructure,    DataNodeStructureMap },
		//		{ DmsEnumType.DataValue,        DataValueMap },
		//		{ DmsEnumType.DataSource,   DataSourceMap },
		//		{ DmsEnumType.DataFormat,       DataFormatMap },
		//	};

	}
}


#region DataNode & DataTree
/// <summary>
/// IDataTree is a universal data model (all data can be represented as collection of nodes in a tree graph)
/// Each node is assigned a unique id
/// </summary>
public interface IDataTree
{
	public IDataNode Root { get; }
	public IDictionary<Guid, IDataNode> NodeMap { get; }
	public IDictionary<Guid, Guid> StemMap { get; }
	public IDictionary<Guid, HashSet<Guid>> BranchMap { get; }
	public IDictionary<Guid, object> ValueMap { get; }

	public int NodeCount { get; }
	public int ValueCount { get; }

	public IDataNode CreateNode();
	public void AddNode(IDataNode node);
	public void DeleteNode(Guid id);

	public void AddStemBranchNode(Guid stemId, Guid branchId);
	public void RemoveStemBranchNode(Guid stemId, Guid branchId);

	public bool NodeHasValue(Guid id);
	public object GetNodeValue(Guid id);
	public void SetNodeValue(Guid id, object value);
	public void DeleteNodeValue(Guid id);
}
public class DataTree : IDataTree
{
	public IDataNode Root { get; }
	public IDictionary<Guid, IDataNode> NodeMap => _nodeMap;
	public IDictionary<Guid, Guid> StemMap => _stemMap;
	public IDictionary<Guid, HashSet<Guid>> BranchMap => _branchMap;
	public IDictionary<Guid, object> ValueMap => _valueMap;

	public int NodeCount => NodeMap.Count();
	public int ValueCount => ValueMap.Count();

	private IDictionary<Guid, IDataNode> _nodeMap { get; set; }
	private IDictionary<Guid, Guid> _stemMap { get; set; }
	private IDictionary<Guid, HashSet<Guid>> _branchMap { get; set; }
	private IDictionary<Guid, object> _valueMap { get; set; }


	public IDataNode CreateNode()
	{
		var node = new DataNode(this);
		AddNode(node);
		return node;
	}

	public void AddNode(IDataNode node)
	{
		if (_nodeMap.ContainsKey(node.Id)) throw new InvalidOperationException($"Tree already has node with id {node.Id}.");
		_nodeMap.Add(node.Id, node);
	}
	public void DeleteNode(Guid id)
	{
		if (!_nodeMap.ContainsKey(id)) throw new InvalidOperationException($"Tree does not contain node with specified id ({id}).");
		if (_stemMap.ContainsKey(id)) throw new InvalidOperationException($"Node {id} has a value specified ({_stemMap[id]}) and cannot be deleted.");
		if (_stemMap.ContainsKey(id)) throw new InvalidOperationException($"Node {id} has a stem node specified ({_stemMap[id]}) and cannot be deleted.");
		if (_branchMap.ContainsKey(id))
			if (_branchMap[id].Any()) throw new InvalidOperationException($"Node {id} has {_branchMap[id].Count()} items and cannot be deleted.");
			else
				_branchMap.Remove(id);

		_nodeMap.Remove(id);
	}

	public void AddStemBranchNode(Guid stemId, Guid branchId)
	{
		if (!_nodeMap.ContainsKey(stemId)) throw new InvalidOperationException($"Stem id {stemId} not found in tree nodes.");
		if (!_nodeMap.ContainsKey(branchId)) throw new InvalidOperationException($"Branch id {branchId} not found in tree nodes.");
		if (_stemMap.ContainsKey(branchId)) throw new InvalidOperationException($"Branch id {branchId} already has a stem node associated ({_stemMap[branchId]}).");
		if (_branchMap.TryGetValue(stemId, out HashSet<Guid> branches))
			if (branches.Contains(branchId)) throw new InvalidOperationException($"Node {branchId} is already branch of node {stemId}.");
			else
			{
				_branchMap[stemId].Add(branchId);
				_stemMap[branchId] = stemId;
			}
		else
		{
			_branchMap.Add(stemId, new HashSet<Guid>() { branchId });
			_stemMap[branchId] = stemId;
		}
	}
	public void RemoveStemBranchNode(Guid stemId, Guid branchId)
	{
		if (!_nodeMap.ContainsKey(stemId)) throw new InvalidOperationException($"Stem id {stemId} not found in tree nodes.");
		if (!_nodeMap.ContainsKey(branchId)) throw new InvalidOperationException($"Branch id {branchId} not found in tree nodes.");

		if (!_stemMap.TryGetValue(branchId, out Guid specifiedStemId))
			throw new InvalidOperationException($"Branch id {branchId} does not have a stem id specified.");
		if (stemId != specifiedStemId)
			throw new InvalidOperationException($"Branch id {branchId}'s specified stem id ({_stemMap[branchId]}) does not match input stem id ({_stemMap[branchId]}).");

		if (_branchMap.TryGetValue(stemId, out HashSet<Guid> branches))
			if (!branches.Contains(branchId)) throw new InvalidOperationException($"Node {branchId} is already branch of node {stemId}.");

		_stemMap.Remove(branchId);

		_branchMap[stemId].Remove(branchId);
		if (!_branchMap[stemId].Any())
			_branchMap.Remove(stemId);

	}


	public bool NodeHasValue(Guid id)
	{
		if (!_nodeMap.ContainsKey(id)) throw new InvalidOperationException($"Tree does not contain node with specified id ({id}).");
		return _valueMap.ContainsKey(id);
	}
	public object GetNodeValue(Guid id)
	{
		if (!NodeHasValue(id)) throw new InvalidOperationException($"Node {id} does not have a value specified.");
		return _valueMap[id];
	}
	public void SetNodeValue(Guid id, object value)
	{
		if (!_nodeMap.ContainsKey(id)) throw new InvalidOperationException($"Tree does not contain node with specified id ({id}).");
		_valueMap[id] = value;
	}
	public void DeleteNodeValue(Guid id)
	{
		if (!NodeHasValue(id)) throw new InvalidOperationException($"Node {id} does not have a value specified.");
		_valueMap.Remove(id);
	}


	public DataTree()
	{
		_nodeMap = new Dictionary<Guid, IDataNode>();
		_stemMap = new Dictionary<Guid, Guid>();
		_branchMap = new Dictionary<Guid, HashSet<Guid>>();
		_valueMap = new Dictionary<Guid, object>();

		var root = CreateNode();
		Root = root;
	}

	private object ToDump() => new
	{
		NodeCount,
		ValueCount,
		Root,
	};
}

/// <summary>
/// Tree nodes are contained within a data graph and assigned a unique id 
/// Nodes can optionally store values and relationships
/// </summary>
public interface IDataNode : IEquatable<IDataNode>
{
	public Guid Id { get; }
	public Guid? StemId { get; }
	public Guid[] BranchIds { get; }

	public void AddBranch(Guid branchId);
	public void DeleteBranch(Guid branchId);

	public bool HasValue();
	public object GetValue();
	public void SetValue(object value);
	public void DeleteValue();
}
public class DataNode : IDataNode
{
	private IDataTree Tree { get; }
	public Guid Id { get; }
	public Guid? StemId => Tree.StemMap.ContainsKey(this.Id) ? Tree.StemMap[this.Id] : null;
	public Guid[] BranchIds => Tree.BranchMap.ContainsKey(this.Id) ? Tree.BranchMap[this.Id].ToArray() : new Guid[] { };

	public bool Equals(IDataNode other) => Id == other.Id;

	public DataNode(IDataTree tree)
	{
		Id = Guid.NewGuid();
		Tree = tree;
	}

	public void AddBranch(Guid branchId) => Tree.AddStemBranchNode(this.Id, branchId);
	public void DeleteBranch(Guid branchId) => Tree.RemoveStemBranchNode(this.Id, branchId);

	public bool HasValue() => Tree.NodeHasValue(this.Id);
	public object GetValue() => Tree.GetNodeValue(this.Id);
	public void SetValue(object value) => Tree.SetNodeValue(this.Id, value);
	public void DeleteValue() => Tree.DeleteNodeValue(this.Id);

	private object ToDump() => new
	{
		Id = string.Concat(Id.ToString().Take(7)),
		Value = HasValue() ? GetValue() : null,
		ValueType = HasValue() ? GetValue()?.GetType() : null,
		BranchIds = BranchIds.Any() ? BranchIds.Select(bi => Tree.NodeMap[bi]) : null,
		//Value = HasValue() ? GetValue() : null,
	};
}

#endregion



#region Data Schema
/// <summary>
/// Data schema is a specialized tree 
/// </summary>
public interface IDataSchema: IEquatable<IDataSchema>
{

}
/// <summary>
/// Data schema nodes are specialized for the specific tree 
/// </summary>
public interface IDataSchemaNode: IEquatable<IDataSchemaNode>
{
	public DataNodeStructure StructureType { get; }
}
public interface IDataSchemaValue : IDataSchemaNode
{
	public new DataNodeStructure StructureType => DataNodeStructure.Value;
	public DataValue DataValueType { get; }
}
public interface IDataSchemaField : IDataSchemaValue
{
	public new DataNodeStructure StructureType => DataNodeStructure.Field;
	public string Label { get; }
}
public interface IDataSchemaList : IDataSchemaNode
{
	public new DataNodeStructure StructureType => DataNodeStructure.List;
	public string Label { get; }
}
public interface IDataSchemaRecord : IDataSchemaNode
{
	public new DataNodeStructure StructureType => DataNodeStructure.Record;
	public IEnumerable<IDataSchemaField> Fields { get; }
}
public interface IDataSchemaDataset : IDataSchemaRecord
{
	public new DataNodeStructure StructureType => DataNodeStructure.Dataset;
}


#endregion


#region Configurable Objects (Abstract)
/// <summary>
/// IConfigurableObjects are objects which could be configured through a UI
/// </summary>
public interface IConfigurableObject
{
	/// <summary>
	/// Confirms object has been configured correctly (uses custom attributes defined in AbstractConfigurableObject).
	/// </summary>
	public void ValidateConfiguration();

	/// <summary>
	/// Returns a list of all configurable props for the current object
	/// </summary>
	public PropertyInfo[] GetConfigurableProps();

	/// <summary>
	/// Returns the default value of a configurable prop if DefaultAttribute is applied
	/// </summary>
	public object GetConfigurablePropDefaultValue(PropertyInfo prop);

	/// <summary>
	/// Returns all the allowed values for a configurable prop AllowedAttribute is applied
	/// </summary>
	public object[] GetConfigurablePropAllowedValues(PropertyInfo prop);
}
/// <summary>
/// AbstractConfigurableObject is the base class for objects that implement IConfigurableObject. The "configurability" of object fields is 
/// determined based on application of custom attributes (which are defined in this class).
/// </summary>
public abstract class AbstractConfigurableObject : IConfigurableObject
{
	//////// Custom Attributes ////////
	/// <summary>
	/// ConfigurableAttribute - tagged property would be included in any configuration UI
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class ConfigurableAttribute : Attribute { }

	/// <summary>
	/// RequiredAttribute - object will error on ValidateConfiguration() if no value is specified for tagged property
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class RequiredAttribute : Attribute { }

	/// <summary>
	/// DefaultAttribute(Value) - will apply this value if none other is specified
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class DefaultAttribute : Attribute
	{
		public object Value { get; }
		public DefaultAttribute(object value)
		{
			Value = value;
		}
	}

	/// <summary>
	/// AllowedAttribute(Value[]) - will error on ValidateConfiguration() if specified value is not present
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class AllowedAttribute : Attribute
	{
		public object[] Values { get; set; }
		public AllowedAttribute(params object[] allowedValues) { Values = allowedValues; }
	}


	//////// IConfigurableObject ////////
	public void ValidateConfiguration()
	{
		var configurableFields = GetFields<ConfigurableAttribute>();

		// Validate configurable fields w/ DefaultAttribute 
		foreach (PropertyInfo prop in GetFields<DefaultAttribute>())
		{
			// Confirm ConfigurableAttribute is applied
			if (!configurableFields.Contains(prop))
				throw new InvalidDataException($"Property named [{prop.Name}] on configurable object of type [{this.GetType().Name}] is tagged with DefaultValue attribute, but not with ConfigurableAttribute.");
		}

		// Validate configurable fields w/ RequiredAttribute  
		foreach (PropertyInfo prop in GetFields<RequiredAttribute>())
		{
			// Confirm ConfigurableAttribute is applied
			if (!configurableFields.Contains(prop))
				throw new InvalidDataException($"Property named [{prop.Name}] on configurable object of type [{this.GetType().Name}] is tagged with RequiredAttribute, but not with ConfigurableAttribute.");

			// Confirm field has value specified
			if (prop.GetValue(this) == null)
				throw new ConfigurableObjectException($"Configurable object instance of type [{this.GetType().Name}] does not have a value specified for required field [{prop.Name}].");
		}

		// Validate configurable fields w/ AllowedAttribute 
		foreach (PropertyInfo prop in GetFields<AllowedAttribute>())
		{
			// Confirm ConfigurableAttribute is applied
			if (!configurableFields.Contains(prop))
				throw new InvalidDataException($"Property named [{prop.Name}] on configurable object of type [{this.GetType().Name}] is tagged with AllowedAttribute, but not with ConfigurableAttribute.");

			// Confirm field's specified value is null or included in list of allowed values
			if (!GetConfigurablePropAllowedValues(prop).Contains(prop.GetValue(this)))
				throw new ConfigurableObjectException($"The specified value for configurable property [{prop.Name}] on configurable object [{this.GetType().Name}] is not allowed. "
					+ $"Specified value is: [{prop.GetValue(this)}]. Allowed values are: [{string.Join(", ", GetConfigurablePropAllowedValues(prop))}].");
		}

	}
	public PropertyInfo[] GetConfigurableProps() => GetFields<ConfigurableAttribute>().ToArray();
	public object GetConfigurablePropDefaultValue(PropertyInfo prop)
	{
		try
		{	
			DefaultAttribute fieldDefault = (DefaultAttribute)prop.GetCustomAttribute(typeof(DefaultAttribute));
			return fieldDefault.Value;
		}
		catch (Exception ex)
		{
			throw new ConfigurableObjectException($"Unable to get DefaultValue attribute and/or default value for configurable property [{prop.Name}].", ex);
		}
	}
	public object[] GetConfigurablePropAllowedValues(PropertyInfo prop)
	{
		try
		{
			AllowedAttribute allowed = (AllowedAttribute)prop.GetCustomAttribute(typeof(AllowedAttribute));
			return allowed.Values;
		}
		catch (Exception ex)
		{
			throw new ConfigurableObjectException($"Unable to get AllowedAttribute and/or allowed values for configurable property [{prop.Name}].", ex);
		}
	}



	//////// Constructor ////////
	public AbstractConfigurableObject() { ApplyDefaults(); }


	//////// Internal Code ////////
	/// <summary>
	/// Default values are applied immediately after instantiation (so they can be overwritten later)
	/// </summary>
	private void ApplyDefaults()
	{
		// If field is null and has non-null default value specified, set field value equal to specified default
		foreach (PropertyInfo prop in GetFields<DefaultAttribute>())
			prop.SetValue(this, GetConfigurablePropDefaultValue(prop));
	}

	/// <summary>
	/// Get fields with custom attribute of type T applied
	/// </summary>
	protected IEnumerable<PropertyInfo> GetFields<T>() where T : Attribute
	{
		return this.GetType().GetProperties().Where(p => p.GetCustomAttribute(typeof(T)) != null);
	}
}
public class ConfigurableObjectException : Exception
{
	public ConfigurableObjectException() : base() { }
	public ConfigurableObjectException(string message) : base(message) { }
	public ConfigurableObjectException(string message, Exception innerException) : base(message, innerException) { }
}
#endregion


#region DataSources
/// <summary>
/// Configurable data sources all expose GetStream()
/// </summary>
public interface IDataSource: IConfigurableObject
{
	public DmsTypeEnums.DataSource SourceType { get; }
	public Stream GetStream();
	public void ValidateSource();
}

/// <summary>
/// DataSource namespace stores base class and actual implementations of IDataSource.
/// </summary>
namespace DataSource
{
	public abstract class Base : AbstractConfigurableObject, IDataSource
	{
		/// <summary>
		/// Each enum value in DataSource has a corresponding class which conforms to the IDataSource interface
		/// </summary>
		public abstract DmsTypeEnums.DataSource SourceType { get; }

		/// <summary>
		/// Get whatever the requested data is and read it as a stream. 
		/// Before getting data, validate configuration and ability to connect to source
		/// </summary>
		public Stream GetStream()
		{
			try { ValidateConfiguration(); }
			catch (Exception ex) { throw new DataSourceException("Error when attempting to validate data source configuration.", ex); }

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


	//////// Configurable Data Source Implementations ////////

	/// <summary>
	/// Reads a user-specified input string
	/// </summary>
	public class RawInput : Base, IDataSource
	{
		// Config

		[Configurable]
		[Required]
		public string InputText { get; set; }

		[Configurable]
		[Required]
		public Encoding InputTextEncoding { get; set; }


		// Data source implementation
		public override DmsTypeEnums.DataSource SourceType => DmsTypeEnums.DataSource.RawInput;
		
		public override void ValidateSource()
		{
			if (string.IsNullOrEmpty(InputText))
				throw new DataSourceException("Can't read from source because InputText value is null or empty.");
		}

		protected override Stream GetStreamImplementation() => new MemoryStream(InputTextEncoding.GetBytes(InputText));
	}

	/// <summary>
	/// Reads a file
	/// </summary>
	public class File : Base, IDataSource
	{
		// Config

		[Configurable]
		[Required]
		public string Directory { get; set; }

		[Configurable]
		[Required]
		public string FileName { get; set; }


		// Data source implementation

		public override DmsTypeEnums.DataSource SourceType => DmsTypeEnums.DataSource.File;

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

}
public class DataSourceException : Exception
{
	public DataSourceException() : base() { }
	public DataSourceException(string message) : base(message) { }
	public DataSourceException(string message, Exception innerException) : base(message, innerException) { }
}
#endregion


#region DataTreeBuilders
/// <summary>
/// Reads a data source to a data tree object
/// </summary>
public interface IDataTreeBuilder
{
	public DmsTypeEnums.DataFormat DataFormat { get; }
	public IDataTree Build(IDataSource source);
	public void ValidateFormat();
}
namespace DataTreeBuilder
{
	public abstract class Base : AbstractConfigurableObject, IDataTreeBuilder
	{
		/// <summary>
		/// Each enum value in DataFormat has a corresponding class which conforms to the IDataTreeBuilder interface
		/// </summary>
		public abstract DmsTypeEnums.DataFormat DataFormat { get; }


		/// <summary>
		/// Reads a stream and returns a data tree
		/// </summary>
		public IDataTree Build(IDataSource source)
		{
			try { ValidateConfiguration(); }
			catch (Exception ex) { throw new DataSourceException("Error when attempting to validate data tree builder configuration.", ex); }

			//try { ValidateFormat(); }
			//catch (Exception ex) { throw new DataSourceException("Error when attempting to validate data source.", ex); }

			try { return BuildImplementation(source); }
			catch (Exception ex) { throw new DataSourceException("Error when attempting to read data stream from data source.", ex); }
		}

		/// <summary>
		/// Throws error if the format doesn't match expected
		/// </summary>
		public abstract void ValidateFormat();

		/// <summary>
		/// Stream reading implementations defined in derived classes
		/// </summary>
		protected abstract IDataTree BuildImplementation(IDataSource source);
	}

	/// <summary>
	/// DSV = Delimitter-Separated Values
	/// </summary>
	public class FromDsv : Base
	{
		// Mostly conforms to https://www.rfc-editor.org/rfc/rfc4180

		[Configurable]
		[Required]
		[Default(true)]
		public bool HasHeaders { get; set; }

		[Configurable]
		[Required]
		[Default(',')]
		public char Delimitter { get; set; }

		[Configurable]
		[Required]
		[Default('"')]
		public char Escaper { get; set; }

		// Sets to Environment.NewLine if nothing specified
		[Configurable]
		public string LineBreak { get; set; }


		public FromDsv() : base()
		{
			LineBreak = LineBreak ?? Environment.NewLine;
		}

		public override DmsTypeEnums.DataFormat DataFormat => DmsTypeEnums.DataFormat.Dsv;

		public override void ValidateFormat()
		{
			throw new NotImplementedException();
		}
		
		protected override IDataTree BuildImplementation(IDataSource source)
		{
			var tree = new DataTree();

			using (var stream = source.GetStream())
			using (var reader = new StreamReader(stream))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					var recordNode = tree.CreateNode();
					tree.Root.AddBranch(recordNode.Id);

					foreach (var nodeVal in ParseDelimittedStringLine(line))
					{
						var valueNode = tree.CreateNode();
						recordNode.AddBranch(valueNode.Id);

						valueNode.SetValue(nodeVal);
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

	public class FromJson : Base
	{

		[Configurable]
		[Required]
		public JsonDocumentOptions JsonDocumentOptions { get; set; }


		public override DmsTypeEnums.DataFormat DataFormat => DmsTypeEnums.DataFormat.Json;

		public override void ValidateFormat()
		{
			throw new NotImplementedException();
		}
		
		protected override IDataTree BuildImplementation(IDataSource source)
		{
			var tree = new DataTree();

			using (var stream = source.GetStream())
			using (JsonDocument doc = JsonDocument.Parse(stream, JsonDocumentOptions))
			{
				ParseRecursive(tree, tree.Root.Id, doc.RootElement);
			}
			return tree;
		}


		private void ParseRecursive(IDataTree tree, Guid currentNodeId, JsonElement element)
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

		private void ParseJsonObject(IDataTree tree, Guid currentNodeId, JsonElement element)
		{
			var record = tree.CreateNode();
			tree.NodeMap[currentNodeId].AddBranch(record.Id);

			foreach (JsonProperty prop in element.EnumerateObject())
			{
				var field = tree.CreateNode();
				record.AddBranch(field.Id);

				var label = tree.CreateNode();
				field.AddBranch(label.Id);
				label.SetValue(prop.Name);
				
				ParseRecursive(tree, field.Id, prop.Value);
			}
		}

		private void ParseJsonArray(IDataTree tree, Guid currentNodeId, JsonElement element)
		{
			var list = tree.CreateNode();
			tree.NodeMap[currentNodeId].AddBranch(list.Id);

			foreach (JsonElement item in element.EnumerateArray())
			{
				ParseRecursive(tree, list.Id, item);
			}
		}

		private void ParseJsonValue(IDataTree tree, Guid currentNodeId, JsonElement element)
		{
			if (!JsonValueKindValue.Contains(element.ValueKind))
				throw new InvalidDataException("Element is expected to be a value but does not have expected JsonValueKind.");
				
			var value = tree.CreateNode();
			tree.NodeMap[currentNodeId].AddBranch(value.Id);
			//value.SetValue(element.ToString());
			if (element.ValueKind == JsonValueKind.String)
				value.SetValue(element.GetString());
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
public class DataTreeBuilderException : Exception
{
	public DataTreeBuilderException() : base() { }
	public DataTreeBuilderException(string message) : base(message) { }
	public DataTreeBuilderException(string message, Exception innerException) : base(message, innerException) { }
}
#endregion







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
