<Query Kind="Program">
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Text.Json.Nodes</Namespace>
  <Namespace>System.Diagnostics.CodeAnalysis</Namespace>
  <RuntimeVersion>7.0</RuntimeVersion>
</Query>

void Main()
{
	
	var dir = Path.GetDirectoryName(Util.CurrentQueryPath);
	var subpath = @"C:\Users\Work\Projects\dms\SampleData\JSON";
	var fileName = "sampleRecord.json";

	var filePath = Path.Combine(dir, subpath, fileName).Dump();
	
	
	var jsonDocumentOptions = new JsonDocumentOptions() {
		AllowTrailingCommas = true,
		CommentHandling = JsonCommentHandling.Skip,
		MaxDepth = 64
	};
	using (FileStream s = File.Open(filePath, FileMode.Open))
	{
		JsonDocument doc = JsonDocument.Parse(s, jsonDocumentOptions);
		doc.Dump();
		var x = ParseSchemaFrom(doc.RootElement);
		x.Dump();
	}
	
	//string url = @"https://jsonplaceholder.typicode.com/users/1";
	//var client = new HttpClient();
	//var response = client.GetAsync(url).Result;
	//
	//using (JsonDocument doc = JsonDocument.Parse(response.Content.ReadAsStream()))
	//{
	//	doc.Dump();
	//	var x = ParseSchemaFrom(doc.RootElement);
	//	x.Dump();
	//}
}

public ISchema ParseSchemaFrom(JsonElement jsonElement)
{
	Dictionary<JsonValueKind, DataType> DataTypeMap = new Dictionary<JsonValueKind, DataType>()
	{
		{ JsonValueKind.Null,       DataType.Null },
		{ JsonValueKind.Undefined,  DataType.Unknown },
		{ JsonValueKind.String,     DataType.String },
		//{ JsonValueKind.Number,     DataType.Number },
		{ JsonValueKind.Number,     DataType.Decimal },
		{ JsonValueKind.True,       DataType.Boolean },
		{ JsonValueKind.False,      DataType.Boolean },
		{ JsonValueKind.Object,     DataType.Record },
		{ JsonValueKind.Array,      DataType.List },
	};
	
	if (!(DataTypeMap.ContainsKey(jsonElement.ValueKind)))
		throw new InvalidDataException($"No Dms Data Type specfied for JsonValueKind {jsonElement.ValueKind}.");

	DataType dt = jsonElement.ValueKind.ToDataType();
	if (dt.IsValue())
	{
		return new ValueSchema(dt);
	}
	else if (jsonElement.ValueKind.Equals(JsonValueKind.Object))
	{
		var fieldSchemas = new List<FieldSchema>();
		foreach (JsonProperty jsonProperty in jsonElement.EnumerateObject())
		{
			fieldSchemas.Add((FieldSchema)ParseSchemaFrom(jsonProperty));
		}
		return new RecordSchema(fieldSchemas);
	}
	else if (jsonElement.ValueKind.Equals(JsonValueKind.Array))
	{
		var vs = new List<ISchema>();
		foreach (JsonElement je in jsonElement.EnumerateArray())
		{
			ISchema elementSchema = ParseSchemaFrom(je);
			vs.Add(elementSchema);
			if (!(vs.Count.Equals(0)))
				if (!(vs[0].Equals(elementSchema)))
					return new ListSchema(DataType.Unknown);
		}

		if (vs[0].StructureType.IsRecord())
		{
			DatasetSchema ds = new DatasetSchema();
			foreach (ISchema rsch in vs)
				foreach (FieldSchema field in ((RecordSchema)rsch).Fields)
				{
					//field.Dump();
					ds.AddOrUpdateField(field); // should error if different type but w/e, need CONSOLIDATE
				}
			return ds;
		}
		else
		{
			return new ListSchema(vs[0].StructureType.ToDataType());
		}
		
	}
	else
	{
		throw new InvalidDataException($"Unexpected mapping for JsonElement to DataType! Mapped DataType is {dt}. JsonElement has JsonValueKind {jsonElement.ValueKind}.");
	}
}

public ISchema ParseSchemaFrom(JsonProperty obj)
{
	return new FieldSchema(obj.Name, ParseSchemaFrom(obj.Value));
}



/*
Data is a recursive hierarchal tree consisting of data nodes

Data nodes are tagged with two attributes: structure and type.

Structure descrives describes the interfaces available for traversing the tree.

Type describes the underlying data.

*/
/// /// <summary>
/// Describes data structure interface
/// </summary>
public enum StructureType
{
	Value,
	Field,	//  tuple
	List,
	Record,
	Dataset
}

/// <summary>
/// Describes data value type (can be a data structure)
/// </summary>
public enum DataType
{
	Null,   // "Any"
	Unknown,
	Error,
	String,
	//Number,
	Integer,
	Decimal,
	Scientific,
	Boolean,
	DateTime,
	Field,
	List,
	Record,
	Dataset
}


/// /// <summary>
/// Describes data source 
/// </summary>
public enum DataSourceType
{
	Json,
}

public static class DataSourceTypeExtensions
{
	public static IDataSchemaParser GetDefaultDataSchemaParser(this DataSourceType obj) => new JsonDataSchemaParser();
}

public interface IDmsObject
{
	/// <summary> 
	/// Determines whether dms object conforms to dms expectations and can be used for dms programming
	/// </summary>
	public void Validate();
}

[System.Flags]
public enum GraphNodeType : sbyte
{
	Internal = 1,
	Root = 2,
	Leaf = 4
}

public interface IGraphNode: IEnumerable<IGraphNode>
{
	public GraphNodeType GraphNodeType { get; }
	public IEnumerable<IGraphNode> SubNodes { get; }
	public int Depth { get; }
	public int Level { get; }

	public bool IsRoot => GraphNodeType.HasFlag(GraphNodeType.Root);
	public bool IsInternal => GraphNodeType.HasFlag(GraphNodeType.Internal);
	public bool IsLeaf => GraphNodeType.HasFlag(GraphNodeType.Leaf);

}

public interface IData 
{
	public object NativeValue { get; }
	public DataSourceType SourceType { get; }
	public IDataSchema DataSchema { get; }
	//public IData GetValue();
	//public IData GetValues();
	//public IData GetLabel();
	//public IData GetLabels();
}
public class Data : IData
{
	public object NativeValue { get; }
	public DataSourceType SourceType { get; }
	public IDataSchema DataSchema { get; }
	
	public GraphNodeType NodeType { get; }
	public int Depth { get; }
	public int Level { get; }

	//IEnumerable<IData>

	public Data(object value, DataSourceType sourceType, IDataSchema dataSchema, int depth = 0, int level = 0)
	{
		NativeValue = value;
		DataSchema = dataSchema;

		// Set graph node properties
		Depth = depth;
		Level = level;
		NodeType = GraphNodeType.Internal;
		if (Depth == 0)
			NodeType |= GraphNodeType.Root;
		if (DataSchema.IsEnumerable)
			NodeType |= GraphNodeType.Leaf;

	}

	public IEnumerator<IData> GetEnumerator()
	{
		if (!(NativeValue is IEnumerable))
			throw new InvalidOperationException("NativeValue of data object does not implement IEnumerable and cannot be enumerated!");
		
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		throw new NotImplementedException();
	}
	
}
public interface IDataSchema : IEquatable<IDataSchema>
{
	public StructureType StructureType { get; }
	public DataType DataType { get; }

	public bool IsEnumerable => (StructureType.IsEnumerable() || DataType.IsEnumerable());
	public bool IsLabeled => (StructureType.IsLabeled() || DataType.IsLabeled());

}
public class DataSchema : IDataSchema
{
	public StructureType StructureType { get; }
	public DataType DataType { get; }

	public DataSchema(StructureType structureType, DataType dataType)
	{
		StructureType = structureType;
		DataType = dataType;
	}
	public bool Equals(IDataSchema other) => (StructureType == other.StructureType && DataType == other.DataType);
}

public interface IDataSchemaParser
{
	public bool CanParse(object obj);
	public IDataSchema Parse(object obj);
}
public class JsonDataSchemaParser : IDataSchemaParser
{
	public bool CanParse(object obj)
	{
		if (obj is System.Text.Json.JsonElement
			|| obj is System.Text.Json.JsonProperty
			|| obj is System.Text.Json.JsonDocument)
			return true;
		return false;
	}

	public IDataSchema Parse(object obj)
	{
		throw new NotImplementedException();
	}

	private IDataSchema _Parse(JsonDocument obj)
	{
		throw new NotImplementedException();
	}
	private IDataSchema _Parse(JsonProperty obj)
	{
		throw new NotImplementedException();
	}
	private IDataSchema _Parse(JsonElement obj)
	{
		throw new NotImplementedException();
	}

}


public interface ISchema : IEquatable<ISchema>
{
	public StructureType StructureType { get; }
	public DataType DataType { get; }
}

public interface IStructure : ISchema
{
	public IEnumerable<ISchema> SubSchema { get; }
	public void AddSubSchema(ISchema subSchema);
	public void ConsolidateSubSchema(ISchema subSchema);
}

public abstract class SchemaBase : ISchema
{
	public abstract StructureType StructureType { get; }
	public abstract DataType DataType { get; }

	public bool Equals(ISchema other) => StructureType.Equals(other.StructureType) && DataType.Equals(other.DataType);
}

public abstract class SchemaStructureBase : SchemaBase
{

}



public class ValueSchema : SchemaBase, IDmsObject
{
	public override StructureType StructureType => StructureType.Value;
	public override DataType DataType => _dataType;
	private DataType _dataType { get; }


	public ValueSchema(DataType dataType)
	{
		this._dataType = dataType;
		Validate();
	}

	public void Validate()
	{
		if (!_dataType.IsValue())
			throw new DmsObjectValidationException(this, $"ValueSchema is expected to have DataType value that is not a structure, but the specified DataType is {DataType}.");
	}
}

public class FieldSchema : SchemaBase, IDmsObject
{
	public override StructureType StructureType => StructureType.Field;
	public override DataType DataType => _dataType;
	private DataType _dataType { get; }

	public string Label => _label;
	private string _label { get; }

	public ISchema ValueSchema => _valueSchema;
	private ISchema _valueSchema { get; }


	public int? Order { get; set; }

	public FieldSchema(string label, ISchema valueSchema)
	{
		this._label = label;
		this._valueSchema = valueSchema;
		this._dataType = _valueSchema.StructureType.IsValue() ? _valueSchema.DataType : _valueSchema.StructureType.ToDataType();
		Validate();
	}

	public void Validate()
	{
		// Check if label is data type -- only strings for now
		if (string.IsNullOrWhiteSpace(_label))
			throw new DmsObjectValidationException(this, $"FieldSchema Label is blank or all empty characters.");
	}
}

public class ListSchema : SchemaBase, IDmsObject
{
	public override StructureType StructureType => StructureType.List;
	public override DataType DataType => _dataType;
	private DataType _dataType { get; }

	public ISchema ValueSchema => _valueSchema;
	private ISchema _valueSchema { get; }

	public ListSchema(DataType dataType)
	{
		this._dataType = dataType;
		Validate();
	}
	public void Validate() { }
}

public class RecordSchema : SchemaBase, IDmsObject
{
	public override StructureType StructureType => StructureType.Record;
	public override DataType DataType => DataType.List;
	private DataType _dataType { get; }

	public IEnumerable<FieldSchema> Fields => _fields.Values;
	
	private Dictionary<string, FieldSchema> _fields { get; set; }
	public void AddOrUpdateField(FieldSchema field) => _fields[field.Label] = field;
	public void RemoveFieldIfExists(FieldSchema field) 
	{
		if (_fields.ContainsKey(field.Label))
			_fields.Remove(field.Label);
	}
	public RecordSchema(IEnumerable<FieldSchema> fields)
	{
		this._fields = new Dictionary<string, FieldSchema>();
		foreach (var f in fields)
			_fields.Add(f.Label, f);
		Validate();
	}
	public RecordSchema(DatasetSchema datasetSchema)
	{
		this._fields = new Dictionary<string, FieldSchema>();
		foreach (var f in datasetSchema.Fields)
			_fields.Add(f.Label, f);
		Validate();
	}
	public void Validate() { }
	
}

public class DatasetSchema: SchemaBase, IDmsObject//, IEnumerable<FieldSchema>
{
	public override StructureType StructureType => StructureType.List;
	public override DataType DataType => DataType.Record;
	
	// Dataset implementation
	public IEnumerable<FieldSchema> Fields => _GetFieldsInOrder();
	//public IEnumerator<FieldSchema> GetEnumerator() => Fields.GetEnumerator();
	//IEnumerator IEnumerable.GetEnumerator() => Fields.GetEnumerator();

	// Fields and ordering
	private Dictionary<string, FieldSchema> _fields { get; set; }
	private List<string> _indexFields { get; set; }
	private List<string> _fieldOrderStart { get; set; }
	private List<string> _fieldOrderEnd { get; set; }
	private IEnumerable<FieldSchema> _GetFieldsInOrder()
	{
		int order = 0;
		if (_fieldOrderStart.Count() > 0
			|| _fieldOrderEnd.Count() > 0)
		{
			foreach (var field in _fieldOrderStart.Select(f => _fields[f]))
				yield return field;

			foreach (var fieldName in _fields.Keys.Where(fieldName => (!_fieldOrderStart.Contains(fieldName)) && (!_fieldOrderEnd.Contains(fieldName))))
				yield return _fields[fieldName];
				
			foreach (var field in _fieldOrderEnd.Select(f => _fields[f]).Reverse())
				yield return field;
		}
		else
		{
			foreach (var field in _fields.Values)
				yield return field;
		}
	}

	// ADDING AND REMOVING FIELDS TO DATASET
	public void AddOrUpdateField(FieldSchema field)
	{
		if (_FieldExists(field.Label))
			_fields[field.Label] = field;
		else
			_fields.Add(field.Label, field);
	}
	public void RemoveFieldIfExists(FieldSchema field)
	{
		if (_FieldExists(field.Label))
			_fields.Remove(field.Label);
	}

	// ADDING AND REMOVING FIELDS TO DATASET INDEX
	public void AddFieldToIndex(string fieldName)
	{
		if (!_indexFields.Contains(fieldName))
			_indexFields.Append(fieldName);
	}
	public void RemoveFieldFromIndex(string fieldName)
	{
		if (_indexFields.Contains(fieldName))
			_indexFields.Remove(fieldName);
	}

	// FIELD ORDER MANIPULATION
	public void RemoveFieldOrder(string fieldName)
	{
		if (_fieldOrderStart.Contains(fieldName))
			_fieldOrderStart.Remove(fieldName);
		if (_fieldOrderEnd.Contains(fieldName))
			_fieldOrderEnd.Remove(fieldName);
	}
	public void MoveFieldToStart(string fieldName)
	{
		_ValidateFieldExists(fieldName);
		RemoveFieldOrder(fieldName);
		_fieldOrderEnd.Add(fieldName);
	}
	public void MoveFieldToEnd(string fieldName)
	{
		_ValidateFieldExists(fieldName);
		RemoveFieldOrder(fieldName);
		_fieldOrderStart.Add(fieldName);
	}
	public void MoveFieldBefore(string fieldName, string otherFieldName)
	{
		_ValidateFieldExists(fieldName);
		_ValidateFieldExists(otherFieldName);
		_ValidateFieldIsOrdered(otherFieldName);
		RemoveFieldOrder(fieldName);

		if (_fieldOrderStart.Contains(fieldName))
			_fieldOrderStart.Insert(_fieldOrderStart.IndexOf(otherFieldName), fieldName);
		else if (_fieldOrderEnd.Contains(fieldName))
			_fieldOrderEnd.Insert(_fieldOrderStart.IndexOf(otherFieldName)+1, fieldName);
	}
	public void MoveFieldAfter(string fieldName, string otherFieldName)
	{
		_ValidateFieldExists(fieldName);
		_ValidateFieldExists(otherFieldName);
		_ValidateFieldIsOrdered(otherFieldName);
		RemoveFieldOrder(fieldName);


		if (_fieldOrderStart.Contains(fieldName))
			_fieldOrderStart.Insert(_fieldOrderStart.IndexOf(otherFieldName) + 1, fieldName);
		else if (_fieldOrderEnd.Contains(fieldName))
			_fieldOrderEnd.Insert(_fieldOrderStart.IndexOf(otherFieldName), fieldName);
	}

	private bool _FieldExists(string fieldName) => _fields.ContainsKey(fieldName);
	private void _ValidateFieldExists(string fieldName)
	{
		if (!_FieldExists(fieldName))
			throw new InvalidDataException($"DatasetSchema does not contain a field named {fieldName}.");
	}
	private void _ValidateFieldIsIndexed(string fieldName)
	{
		if (!_indexFields.Contains(fieldName))
			throw new InvalidDataException($"DatasetSchema index fields does not include field {fieldName}.");
	}
	private void _ValidateFieldIsOrdered(string fieldName)
	{
		if (!_FieldExists(fieldName))
			throw new InvalidDataException($"DatasetSchema does not specify ordering for field {fieldName}.");
	}

	public DatasetSchema()
	{
		_fields = new Dictionary<string, FieldSchema>();
		_indexFields = new List<string>();
		_fieldOrderStart = new List<string>();
		_fieldOrderEnd = new List<string>();
		Validate();
	}
	public void Validate() { }
}



public interface IData : IEnumerable<IData> //, IEquatable<IData>
{
	// Data value and underlying type
	public object? Value { get; set; }

	//// Data value access and traversal methods

	// for value and field structure types
	public object? GetValue();
	public Type? GetDataType();

	// for field structure types
	public IData GetFieldValue();
	public IData GetFieldLabel();

	// for enumerable dms types
	public IEnumerable<IData> GetValues();

	// lists
	public IData GetValueByPosition(int position);

	// record
	public IEnumerable<IData> GetFieldValues();

	// dataset
	public IEnumerable<IData> GetFieldValues(object? label);

	// record and dataset
	public IEnumerable<IData> GetFieldLabels();
	public IData GetFieldValueByLabel(object? label);

}


// Exceptions
public class DmsObjectValidationException : Exception
{
	public DmsObjectValidationException(IDmsObject obj, string message) : base($"Dms object (class {obj.GetType().FullName}) is invalid. {message}") { }
	public DmsObjectValidationException(IDmsObject obj, string message, Exception e) : base($"Dms object (class {obj.GetType().FullName}) is invalid. {message}", e) { }
}


#region EnumExtensions

/// <summary>
/// StructureType helper methods to describe type characteristics and map to corresponding DataType if exists
/// </summary>
public static class StructureTypeExtensions
{
	public static bool IsValue(this StructureType structureType) => structureType.Equals(StructureType.Value);
	public static bool IsField(this StructureType structureType) => structureType.Equals(StructureType.Field);
	public static bool IsList(this StructureType structureType) => structureType.Equals(StructureType.List);
	public static bool IsRecord(this StructureType structureType) => structureType.Equals(StructureType.Record);
	public static bool IsDataset(this StructureType structureType) => structureType.Equals(StructureType.Dataset);
	public static bool IsEnumerable(this StructureType structureType) => EnumerableStructureTypes.Contains(structureType);
	public static bool IsLabeled(this StructureType structureType) => LabeledStructureTypes.Contains(structureType);

	// StructureType -> DataType Conversion
	public static bool IsDataType(this StructureType structureType) => DataTypeMap.ContainsKey(structureType);
	public static DataType ToDataType(this StructureType structureType)
	{
		if (DataTypeMap.TryGetValue(structureType, out DataType DataType))
			return DataType;
		else
			throw new InvalidOperationException($"StructureType enum {structureType} does not have a corresponding DataType enum!");
	}

	private static HashSet<StructureType> EnumerableStructureTypes = new HashSet<StructureType>()
	{
		StructureType.List,
		StructureType.Record,
		StructureType.Dataset,
	};
	private static HashSet<StructureType> LabeledStructureTypes = new HashSet<StructureType>()
	{
		StructureType.Record,
		StructureType.Dataset,
	};
	private static Dictionary<StructureType, DataType> DataTypeMap = new Dictionary<StructureType, DataType>()
	{
		{ StructureType.Field, DataType.Field },
		{ StructureType.List, DataType.List },
		{ StructureType.Record, DataType.Record },
		{ StructureType.Dataset, DataType.Dataset },
	};


}

/// <summary>
/// DataType helper methods to describe type characteristics and map to corresponding StructureType if exists
/// </summary>
public static class DataTypeEnumExtensions
{
	public static bool IsPrimitive(this DataType DataType) => PrimitiveDataTypes.Contains(DataType);


	// Enum Value Checking
	public static bool IsValue(this DataType DataType) => !StructureTypeMap.ContainsKey(DataType);
	public static bool IsField(this DataType DataType) => DataType.Equals(DataType.Field);
	//public static bool IsList(this DataType DataType) => DataType.Equals(DataType.List);
	public static bool IsRecord(this DataType DataType) => DataType.Equals(DataType.Record);
	//public static bool IsDataset(this DataType DataType) => DataType.Equals(DataType.Dataset);
	public static bool IsLabeled(this DataType dataType) => LabeledDataTypes.Contains(dataType);

	// Interface info
	public static bool IsEnumerable(this DataType DataType) => EnumerableDataTypes.Contains(DataType);

	// DataType -> StructureType Conversion
	public static StructureType ToStructureType(this DataType DataType)
	{
		if (StructureTypeMap.TryGetValue(DataType, out StructureType structureType))
			return structureType;
		else
			throw new InvalidOperationException($"DataType enum {DataType} does not have a corresponding StructureType enum!");
	}

	// Mapping (private)
	private static HashSet<DataType> PrimitiveDataTypes = new HashSet<DataType>()
	{
		DataType.Null,   // "Any"
		DataType.Unknown,
		DataType.Error,
		DataType.String,
		//DataType.Number,
		DataType.Integer,
		DataType.Decimal,
		DataType.Scientific,
		DataType.Boolean,
		//DataType.Date,
		DataType.DateTime,
	};
	private static HashSet<DataType> EnumerableDataTypes = new HashSet<DataType>()
	{
		// does not include field (field is treated as value since the field name is secondary)
		DataType.List,
		DataType.Record,
		DataType.Dataset
	};

	private static HashSet<DataType> LabeledDataTypes = new HashSet<DataType>()
	{
		DataType.Field,
		DataType.Record,
		DataType.Dataset,
	};
	private static Dictionary<DataType, StructureType> StructureTypeMap = new Dictionary<DataType, StructureType>()
	{
		{ DataType.Field, StructureType.Field },
		{ DataType.List, StructureType.List },
		{ DataType.Record, StructureType.Record },
		{ DataType.Dataset, StructureType.Dataset},
	};

}


#endregion


#region JsonMappingExtensions

public static class JsonValueKindExtensions
{
	// DataType -> StructureType Conversion
	public static DataType ToDataType(this JsonValueKind JsonValueKind)
	{
		if (DataTypeMap.TryGetValue(JsonValueKind, out DataType dataType))
			return dataType;
		else
			throw new InvalidOperationException($"JsonValueKind enum {JsonValueKind} does not have a corresponding DataType enum specified!");
	}

	private static Dictionary<JsonValueKind, DataType> DataTypeMap = new Dictionary<JsonValueKind, DataType>()
	{
		{ JsonValueKind.Array,      DataType.List },
		{ JsonValueKind.False,      DataType.Boolean },
		{ JsonValueKind.Null,       DataType.Null },
		{ JsonValueKind.Number,     DataType.Number },
		//{ JsonValueKind.Number,     DataType.Scientific },	??????????
		{ JsonValueKind.Object,     DataType.Record},
		{ JsonValueKind.String,     DataType.String},
		{ JsonValueKind.True,       DataType.Boolean},
		{ JsonValueKind.Undefined,  DataType.Unknown},
	};
}

#endregion