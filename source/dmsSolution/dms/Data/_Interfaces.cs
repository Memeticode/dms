namespace dms.Data;



/// <summary>
/// Data exists within a data context
/// </summary>
public interface IDataContext: IReadOnlyDictionary<string, IData>
{
    public IData CreateData(DataValueType dataValueType);
    public IData AddNewData(DataValueType dataValueType, string label);
    public IData AddNewDataAt(DataValueType dataValueType, IDataReference reference);

}

/// <summary>
/// Data reference points to data within a data context
/// </summary>
public interface IDataReference : IEquatable<IDataReference>
{
    public string ContextLabel { get; }
    public ITreePath Path { get; }
    public string String { get; }
}

public interface IData: IEquatable<IData>
{
    public IDataContext Context { get; }
    public DataStructureType StructureType { get; }
}

public interface IDataValue : IData
{
    public object? GetValue();
    public void SetValue(object? value);
    //public DataValueType ValueType { get; }
}

public interface IDataField : IData
{
    public string Name { get; }
    public IData GetFieldData();
    public void SetDataValue(IDataValue dataValue);
}
public interface IDataList : IData, IList<IData>
{ }
public interface IDataRecord : IData, IDictionary<string, IData> 
{
    public IEnumerable<IDataField> Fields { get; }
}
public interface IDataset : IData, IEnumerable<IData>
{
    public IEnumerable<IDataRecord> Records { get; }
}


public interface ISchema: IEquatable<ISchema>
{
    public DataStructureType DataType { get; }
    public ISchemaRuleset Ruleset { get; }
}
public interface ISchemaRuleset : IEquatable<ISchemaRuleset>, IEnumerable<ISchemaRule> { }
public interface ISchemaRule : IEquatable<ISchemaRule>
{
    public bool CanEvaluate(IData data);
    public bool Passes(IData data);
}



public interface ITransformation 
{
    public Task<IData> ApplyAsync(IData data);
}

//public interface IDataset
//{
//    public IDataSource Source {  get; }
//    //public IDataSpec Spec { get; }

//    public IEnumerable<IDataRecord> Records { get; }

//}

/// <summary>
/// IDataSource implementations contains all info necessary for confirming store exists and reads underlying data to a stream
/// </summary>
public interface IDataSource
{
    public SourceType SourceType { get; }

    public bool Exists();
    public bool CanRead();
    public bool CanWrite();
    public Stream GetStream();

    public Task<bool> ExistsAsync();
    public Task<bool> CanReadAsync();
    public Task<bool> CanWriteAsync();
    public Task<Stream> GetStreamAsync();


    // Configurable data store implementation interfaces
    public interface IRawInput : IDataSource
    {
        public string Input { get; set; }
        public Encoding Encoding { get; set; }
    }
    public interface IFile : IDataSource
    {
        public string Directory { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; }
    }
    public interface IDatabase : IDataSource
    {
        public string Server { get; set; }
        public string Database { get; set; }
    }
}


/// <summary>
/// IDataSpec specifies info necessary for reading store data stream as a dataset
/// </summary>
//public interface IDataSpec
//{
//    public SpecType SpecType { get; }
//    public DatasetType DatasetType { get; }

//    public void Validate(IDataSource source);
//    public IDataTree GetTree(IDataStore store);
//    public Task<IDataTree> GetTreeAsync(IDataStore store);


//    // Configurable data spec implementation interfaces
//    public interface IDsvDataSpec : IDataSpec
//    {
//        public bool HasHeaders { get; set; }
//        public char Delimitter { get; set; }
//        public char Escaper { get; set; }
//        public string LineBreak { get; set; }
//    }
//    public interface IJsonDataSpec : IDataSpec
//    {
//        public int MaxDepth { get; set; }
//        public bool IncludeComments { get; set; }
//    }

//}

