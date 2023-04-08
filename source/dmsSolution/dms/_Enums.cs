namespace dms;

/// <summary>
/// Data Structure Type
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
/// Data value types (describes DataValue types)
/// </summary>
public enum ValueTypeDetail
{
    None,       // no value associated
    Null,       // empty value associated
    String,
    Integer,
	Decimal,
	Scientific,
    Boolean,
    DateTime,
    Unknown,    // type is unspecified
    Error,      // program error when attempting to determine type
}
