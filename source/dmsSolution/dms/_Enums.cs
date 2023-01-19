namespace dms;


/// /// <summary>
/// Describes data structure interface
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
/// Describes data value type (can be a data structure)
/// </summary>
public enum ValueType
{
	Null,   // "Any"
	Unknown,
	Error,
	String,
	Integer,
	Decimal,
	Scientific,
	Boolean,
	Date,
	DateTime,
	Field,
	List,
	Record,
	Dataset,
}

/// <summary>
/// StructureType helper methods to describe type characteristics and map to corresponding ValueType if exists
/// </summary>
public static class StructureTypeExtensions
{
	public static bool CheckIsValue(this StructureType structureType) => structureType.Equals(StructureType.Value);
	public static bool CheckIsField(this StructureType structureType) => structureType.Equals(StructureType.Field);
	public static bool CheckIsList(this StructureType structureType) => structureType.Equals(StructureType.List);
	public static bool CheckIsRecord(this StructureType structureType) => structureType.Equals(StructureType.Record);
	public static bool CheckIsDataset(this StructureType structureType) => structureType.Equals(StructureType.Dataset);
	public static bool CheckIsEnumerable(this StructureType structureType) => EnumerableStructureTypes.Contains(structureType);
	public static bool CheckIsLabeled(this StructureType structureType) => LabeledStructureTypes.Contains(structureType);

	// StructureType -> ValueType Conversion
	public static bool IsValueType(this StructureType structureType) => ValueTypeMap.ContainsKey(structureType);
	public static ValueType ToValueType(this StructureType structureType)
	{
		if (ValueTypeMap.TryGetValue(structureType, out ValueType valueType))
			return valueType;
		else
			throw new InvalidOperationException($"StructureType enum {structureType} does not have a corresponding ValueType enum!");
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
	private static Dictionary<StructureType, ValueType> ValueTypeMap = new Dictionary<StructureType, ValueType>()
	{
		{ StructureType.Field, ValueType.Field },
		{ StructureType.List, ValueType.List },
		{ StructureType.Record, ValueType.Record },
		{ StructureType.Dataset, ValueType.Dataset },
	};


}

/// <summary>
/// ValueType helper methods to describe type characteristics and map to corresponding StructureType if exists
/// </summary>
public static class ValueTypeEnumExtensions
{
	public static bool CheckIsPrimitive(this ValueType valueType) => PrimitiveValueTypes.Contains(valueType);


	// Enum Value Checking
	public static bool CheckIsValue(this ValueType valueType) => !StructureTypeMap.ContainsKey(valueType);
	public static bool CheckIsField(this ValueType valueType) => valueType.Equals(ValueType.Field);
	//public static bool CheckIsList(this ValueType valueType) => valueType.Equals(ValueType.List);
	public static bool CheckIsRecord(this ValueType valueType) => valueType.Equals(ValueType.Record);
	//public static bool CheckIsDataset(this ValueType valueType) => valueType.Equals(ValueType.Dataset);

	// Interface info
	public static bool CheckIsEnumerable(this ValueType valueType) => EnumerableValueTypes.Contains(valueType);

	// ValueType -> StructureType Conversion
	public static StructureType ToStructureType(this ValueType valueType)
	{
		if (StructureTypeMap.TryGetValue(valueType, out StructureType structureType))
			return structureType;
		else
			throw new InvalidOperationException($"ValueType enum {valueType} does not have a corresponding StructureType enum!");
	}

	// Mapping (private)
	private static HashSet<ValueType> PrimitiveValueTypes = new HashSet<ValueType>()
	{
		ValueType.Null,   // "Any"
		ValueType.Unknown,
		ValueType.Error,
		ValueType.String,
		ValueType.Integer,
		ValueType.Decimal,
		ValueType.Scientific,
		ValueType.Boolean,
		ValueType.Date,
		ValueType.DateTime,
	};
	private static HashSet<ValueType> EnumerableValueTypes = new HashSet<ValueType>()
	{
		// does not include field (field is treated as value since the field name is secondary)
		ValueType.List,
		ValueType.Record,
		ValueType.Dataset
	};
	private static Dictionary<ValueType, StructureType> StructureTypeMap = new Dictionary<ValueType, StructureType>()
	{
		{ ValueType.Field, StructureType.Field },
		{ ValueType.List, StructureType.List },
		{ ValueType.Record, StructureType.Record },
		{ ValueType.Dataset, StructureType.Dataset},
	};
}
