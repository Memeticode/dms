namespace dms;


/// <summary>
/// Tree Node Type
/// </summary>
public enum TreeNodeType
{
	Root,	// is root node
	Stem,	// has branches
	Leaf,	// is not root and does not have branches
	Floating, // is not root but has no stem
}


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


