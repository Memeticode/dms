namespace dms;


/// <summary>
/// Describes data object's structure and type
/// </summary>
public interface IDmsType : IEquatable<IDmsType>
{
	public StructureType StructureType { get; }
	public ValueType ValueType { get; }
	public void Validate();


	// Descriptive properties
	public bool IsValue { get; }
	public bool IsField { get; }
	public bool IsList { get; }
	public bool IsRecord { get; }
	public bool IsDataset { get; }
	public bool IsPrimitive { get; }
	public bool IsEnumerable { get; }
	public bool IsLabeled { get; }
}


/// <summary>
/// Maps IData interface to underlying type interface
/// </summary>
public interface IDmsDataAdapter
{
	public bool IsSuitableFor(IData data);
	public IData GetValue(IData data);
	public IEnumerable<IData> GetValues(IData data);
	public IData GetLabel(IData data);
	public IEnumerable<IData> GetLabels(IData data);
	public IData GetValueByPosition(IData data, int position);
	public IData GetValueByLabel(IData data, object? label);
	public IData GetValuesByLabel(IData data, object? label);
}



/// <summary>
/// General interface which wraps an object and exposes a generic interface for accessing the data
/// </summary>
public interface IData : IEnumerable<IData> //, IEquatable<IData>
{
	// Dms Type (determines used to determine what methods work on the IData object) and factory
	public IDmsType DmsType { get; }
	//public IDmsDataAdapter Adapter { get; }
	//public IDmsDataFactory Factory { get; }

	// Data value and underlying type
	public object? Value { get; }
	public Type? SourceType { get; }
	
	/// <summary>
	///  Returns truw if underlying value is null
	/// </summary>
	public bool IsNull { get; }

	/// <summary>
	/// Returns count of enumerable data items or 1 if is primitive value.
	/// </summary>
	public int Count { get; }


	// DATA ACCESS METHODS
	public IData GetValue();
	public IEnumerable<IData> GetValues();
	public IData GetLabel();
	public IEnumerable<IData> GetLabels();
	public IData GetValueByPosition(int position);
	public IData GetValueByLabel(object? label);
	public IData GetValuesByLabel(object? label);


	/// <summary>
	/// Gets the underlying object.
	/// Valid for all data objects with following type-specific modificaations:
	/// StructureType Field - return the field value's underlying value.
	/// </summary>
	public bool TryGetValue(out IData? value);

	/// <summary>
	/// Gets the values of an enumerable data object.
	/// Valid for all data objects with following type-specific modificaations:
	/// StructureType Field - return the field value's underlying value if ValueType is enumerable
	/// StructureType Record - return the field values?
	/// </summary>
	public bool TryGetValues(out IEnumerable<IData>? values);

	/// <summary>
	/// Gets the label of a field.
	/// Only valid for all data objects with StructureType Field.
	/// </summary>
	public bool TryGetLabel(out IData? label);

	/// <summary>
	/// Valid for StructureType = Record or Dataset only
	/// </summary>
	public bool TryGetLabels(object? label, out IEnumerable<IData>? values);

	/// <summary>
	/// Gets the values of an enumerable data object.
	/// Valid for all data objects with following type-specific modificaations:
	/// StructureType Field - return the field value's underlying value if ValueType is enumerable
	/// StructureType Record - return the field values?
	/// </summary>
	public bool TryGetValueByPosition(int position, out IData? value);

	/// <summary>
	/// Valid for StructureType = Record only
	/// </summary>
	public bool TryGetValueByLabel(object? label, out IData? value);

	/// <summary>
	/// Columnar access
	/// Valid for StructureType = Dataset only
	/// </summary>
	public bool TryGetValuesByLabel(object? label, out IData? values);
}



public interface IDmsDataFactory
{
	public IEnumerable<IDmsDataParser> Parsers { get; set; }
	public void AddParser(IDmsDataParser parser);

	public IData GetAsDmsData(object? value);
	public IData GetAsDmsData(object? value, IDmsDataParser parser);
	public IData GetAsDmsData(object? value, IDmsDataParser parser, bool use_specified_parser_only);
	public IData GetAsDmsData(object? value, IEnumerable<IDmsDataParser> parsers, bool use_specified_parsers_only);
}



/// <summary>
/// Parsing interface
/// </summary>
public interface IParse<PT, RT>
{
	public bool CanParse(PT type);
	public RT? Parse(PT type);
	public bool TryParse(PT type, out RT parsed);
}
public abstract class Parser<PT, RT>
{
	public string ParserClassName => this.GetType().FullName;
	public abstract bool CanParse(PT type); 
	public abstract RT Parse(PT type);
	public bool TryParse(PT type, out RT parsed)
	{
		if (this.CanParse(type))
        {
			RT res = this.Parse(type);
			if (!(res is null))
            {
				parsed = res;
				return true;
			}
		}
		parsed = default;
		return false;
	}
}



public interface IDmsDataParser
{
	public bool CanParse(Type type);
	public IData Parse(Type type);
	public bool TryParse(Type type, out IData parsed);

}
public abstract class DmsDataParserBase
{

}

public interface IDmsDataAdapterParser
{
	public bool CanParse(Type type);
	public IDmsType Parse(Type type);
	public bool TryParse(Type type, out IDmsType parsed);
}

