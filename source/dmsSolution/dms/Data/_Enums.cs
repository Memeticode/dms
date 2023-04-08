using System.Diagnostics.Tracing;

namespace dms.Data;

/// <summary>
/// Data types (structures)
/// </summary>
public enum DataStructureType
{
    Value,
    Field,
    List,
    Record,
    Dataset
}

/// <summary>
/// Data value types
/// </summary>
public enum DataValueType
{
    None,       // no value associated
    Null,       // empty value associated
    String,
    Integer,
    Decimal,
    Scientific,
    Boolean,
    Date,
    DateTime,
    Unknown,    // type is unspecified
    Error,      // program error when attempting to determine type
}


/// <summary>
/// Data store which can be read from or theoretically written to
/// </summary>
public enum SourceType
{
    RawInput,   // i.e. hard-coded string
    File,
    Database,
    Api,
}

/// <summary>
/// Data specification (guide for reading/writing data used by data source)
/// </summary>
public enum SpecType
{
    Dsv,    // delimitted string values
    Json,
    Xml,
    PDBx_mmCIF,
    Table,
    Query,
}


/// <summary>
/// Dataset can store data in a variety of formats
/// </summary>
public enum DatasetType
{
    Tabular,
    Hierarchal,
    Columnar
}
