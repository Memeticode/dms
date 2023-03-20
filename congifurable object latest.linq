<Query Kind="Program">
  <Namespace>DmsEnum</Namespace>
  <Namespace>System.Runtime.InteropServices.ComTypes</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>TreeGraph</Namespace>
</Query>

void Main()
{
	var to = new ConfigurationTests.TestConfigurableObject();
	
	foreach(var pi in to.GetType().GetProperties())
	{
		var cp = new Configuration.ConfigurableProperty(pi);
		cp.Dump();
		"DEFAULT:".Dump();
		if (cp.HasDefault) cp.GetDefaultValue().Dump();
		else "{NONE}".Dump();

		"DEFAULT:".Dump();
		if (cp.HasDefault) cp.GetDefaultValue().Dump();
		else "{NONE}".Dump();
		
		//pi.Dump();
		//pi.CustomAttributes.Dump();
		//pi.CustomAttributes.Where(ca => ca.AttributeType == typeof(Configuration.ConfigurableAttribute)).Any().Dump();
	}
	
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

/// <summary>
/// Configurable Objects are objects which can be configured through a UI
/// Application objects that user may interact should be configurable
/// </summary>
#region Configurable Objects


/// <summary>
/// Data value types (describes DataValue types)
/// </summary>
public enum ValueTypeEnum
{
	String,
		Character,
	Number,
		Integer,
			UnsignedInteger,
		Decimal,
		Scientific,
	Boolean,
	DateTime,
		Date,
		Time
}

public static class ValueTypeEnumExtensions
{
	public static ValueTypeEnum GetParentType(this ValueTypeEnum e) => SubTypesMap.Where(kv => kv.Value.Contains(e)).Select(kv => kv.Key).First();
	public static HashSet<ValueTypeEnum> GetSubTypes(this ValueTypeEnum e) => SubTypesMap.ContainsKey(e) ? SubTypesMap[e] : new HashSet<ValueTypeEnum>();
	public static Type GetSystemType(this ValueTypeEnum e) => SystemTypeMap[e];
	
	private static readonly HashSet<ValueTypeEnum> TopLevelTypes
	= new HashSet<ValueTypeEnum>()
	{
		ValueTypeEnum.String,
		ValueTypeEnum.Character,
		ValueTypeEnum.Number,
		ValueTypeEnum.Boolean,
		ValueTypeEnum.DateTime,
	};

	private static readonly Dictionary<ValueTypeEnum, HashSet<ValueTypeEnum>> SubTypesMap
	= new Dictionary<ValueTypeEnum, HashSet<ValueTypeEnum>>()
	{
		{ ValueTypeEnum.String,    new HashSet<ValueTypeEnum>() { ValueTypeEnum.Character } },
		{ ValueTypeEnum.Number,    new HashSet<ValueTypeEnum>() { ValueTypeEnum.Integer, ValueTypeEnum.Decimal, ValueTypeEnum.Scientific } },
		{ ValueTypeEnum.Integer,    new HashSet<ValueTypeEnum>() { ValueTypeEnum.UnsignedInteger } },
		{ ValueTypeEnum.Boolean,   new HashSet<ValueTypeEnum>() } ,
		{ ValueTypeEnum.DateTime,  new HashSet<ValueTypeEnum>() { ValueTypeEnum.Date, ValueTypeEnum.Time } },
	};

	private static readonly Dictionary<ValueTypeEnum, Type> SystemTypeMap
	= new Dictionary<ValueTypeEnum, Type>()
	{
		{ ValueTypeEnum.String,    typeof(string) },
		{ ValueTypeEnum.Character, typeof(char) },
		{ ValueTypeEnum.Number,    typeof(decimal) },
		{ ValueTypeEnum.Integer,   typeof(int) },
		{ ValueTypeEnum.UnsignedInteger, typeof(uint) },
		{ ValueTypeEnum.Decimal,    typeof(decimal) },
		{ ValueTypeEnum.Scientific,	typeof(double) },
		{ ValueTypeEnum.Boolean,    typeof(bool) },
		{ ValueTypeEnum.DateTime,   typeof(DateTime) },
		{ ValueTypeEnum.Date,   	typeof(DateOnly) },
		{ ValueTypeEnum.Time,   	typeof(TimeOnly) },
	};
}

public class DataType
{
	public ValueTypeEnum ValueTypeEnum { get; }
	public Type SystemType => ValueTypeEnum.GetSystemType();
	public void Validate(object value) 
	{
		if (!SystemType.IsAssignableFrom(value.GetType()))
			throw new InvalidDataException($"Specified value type {value.GetType().Name} is not assignable to value type enum {ValueTypeEnum}'s underlying system type {SystemType.Name}.");
	}
}
namespace DataType
{
	public class AbstractDataType<T>
	{
	}
	public class String: IDataType
	{
		public ValueTypeEnum ValueTypeEnum => ValueTypeEnum.String;
		public ValueSubTypeEnum? ValueSubTypeEnum => null;
		private string _value { get; set; }
		
	}
}
/// <summary>
/// Extensions class maps string values to corresponding enum, may be used for configuring (i.e. specifying list of values)
/// </summary>
public static class ValueTypeEnumExtensions
{
	public static HashSet<ValueSubTypeEnum> GetDefinedSubTypes(this ValueTypeEnum e) => ValueSubTypesMap.ContainsKey(e) ? ValueSubTypesMap[e] : new HashSet<ValueSubTypeEnum>();

	private static readonly Dictionary<ValueTypeEnum, HashSet<ValueSubTypeEnum>> ValueSubTypesMap
	= new Dictionary<ValueTypeEnum, HashSet<ValueSubTypeEnum>>()
	{
		{ ValueTypeEnum.String,    new HashSet<ValueSubTypeEnum>() { ValueSubTypeEnum.Character } },
		{ ValueTypeEnum.Number,    new HashSet<ValueSubTypeEnum>() { ValueSubTypeEnum.Integer, ValueSubTypeEnum.Decimal, ValueSubTypeEnum.Scientific } },
		{ ValueTypeEnum.Boolean,   new HashSet<ValueSubTypeEnum>() } ,
		{ ValueTypeEnum.DateTime,  new HashSet<ValueSubTypeEnum>() { ValueSubTypeEnum.Date, ValueSubTypeEnum.Time } },
	};

}


public class ConfigurableValue
{
	public DmsEnum.ValueType ValueType { get; }

	private object _value { get; set; }

	public object GetValue() => _value;
	public void SetValue(object value) => _value = value;

	public static HashSet<DmsEnum.ValueType> ConfigurableValueTypes
		= new HashSet<DmsEnum.ValueType>() {
			DmsEnum.ValueType.String,
			DmsEnum.ValueType.Number,
			DmsEnum.ValueType.Boolean,
			DmsEnum.ValueType.DateTime,
			DmsEnum.ValueType.Unknown,    // type is unspecified
			DmsEnum.ValueType.Error,      // program error when attempting to determine type
		};
}
public class ConfigurablePropertyInfo : IEquatable<ConfigurablePropertyInfo>
{
	public PropertyInfo UnderlyingProperty { get; }

	public string Name { get; }
	public string Description { get; }
	public int Priority { get; }

	public bool IsRequired { get; }
	public bool IsAdvanced { get; }
	
	public bool HasDefault { get; }

	public bool Equals(ConfigurablePropertyInfo other)
	{
		throw new NotImplementedException();
	}

	//public bool HasGenericValidations { get; }
	//public bool HasCustomValidations { get; }

	public object GetDefaultValue();
	//public object[] GetAllowedValues();
}

/// <summary>
/// Configuration which can be used to create an object
/// </summary>
public class ObjectConfiguration
{
	public Type ConfigurableType { get; }
	public IList<ConfigurablePropertyInfo> Properties { get; }
	public IDictionary<ConfigurablePropertyInfo, object> PropertyValues { get; }

	public void SetPropertyValue(ConfigurablePropertyInfo property, object value) => PropertyValues[property] = value;
	public object GetPropertyValue(ConfigurablePropertyInfo property) => PropertyValues.ContainsKey(property) ? PropertyValues[property] : null;
	
	public ObjectConfiguration(Type configureType)
	{
		
	}
	
}

public interface IConfigurationSpecAttribute { }

// ConfigurationAttributes

/// <summary>
/// Configurable is used to mark a property as configurable, set description and configuration priority
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ConfigurableAttribute : Attribute, IConfigurationSpecAttribute
{
	public string Description { get; }
	public int Priority { get; }

	public ConfigurableAttribute() => new ConfigurableAttribute(null, default(int));
	public ConfigurableAttribute(string description) => new ConfigurableAttribute(description, default(int));
	public ConfigurableAttribute(int priority) => new ConfigurableAttribute(null, priority);
	public ConfigurableAttribute(string description, int priority)
	{
		Description = description;
		Priority = priority;
	}
}

/// <summary>
/// Required - field is mandatory, configuration not valid if field is missing or has null value
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class RequiredAttribute : Attribute, IConfigurationSpecAttribute { }

/// <summary>
/// Advanced - field is hidden in the gui (can be expanded)
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class AdvancedAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Property)]
public class DefaultAttribute : Attribute
{
	public object Value { get; }
	public DefaultAttribute(object value)
	{
		Value = value;
	}
}

public class ObjectConfigurationBuilder
{

	/// <summary>
	/// Get properties with custom attribute of type T applied
	/// </summary>
	private IEnumerable<PropertyInfo> GetPropertiesWithAttribute<T>(object obj) where T : Attribute, IConfigurationSpecAttribute
	{
		return obj.GetType().GetProperties().Where(p => p.GetCustomAttribute(typeof(T)) != null);
	}
}



//public interface IConfigurableValue: IEquatable<IConfigurablePropertyValue>


/// <summary>
/// Interface for generating, validating, and applying configurations
/// </summary>
public interface IConfigurationManager
{
	/// <summary>
	/// Determines whether object is configurable
	/// </summary>
	public bool IsConfigurable(object obj);

	/// <summary>
	/// Returns default configuration
	/// </summary>
	//public IObjectConfiguration GetDefaultConfiguration(object obj);

	/// <summary>
	/// Throws error if specific config is not valid/compatible for given object
	/// </summary>
	//public void ValidateConfigurationFor(object obj, IObjectConfiguration config);

	/// <summary>
	/// Applies configuration, should succeed if ValidateObjectConfiguration succeeds
	/// </summary>
	//public void ApplyConfigurationTo(object obj, IObjectConfiguration config);
}
