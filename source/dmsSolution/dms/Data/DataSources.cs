using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dms.Data;

public class DataSourceException : Exception
{
    public DataSourceException() : base() { }
    public DataSourceException(string message) : base(message) { }
    public DataSourceException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Base data store implementation adds error handling wrappers around interface methods and defines abstract implementation methods
/// </summary>
public abstract partial class DataSource : IDataSource
{
    // IDataSource Interface implementation
    public abstract SourceType SourceType { get; }

    public Stream GetStream()
    {
        try
        {
            return GetStreamImplementation();
        }
        catch (Exception ex)
        {
            throw new DataSourceException("Error when attempting to stream data from data store.", ex);
        }
    }
    public bool Exists()
    {
        try
        {
            return ExistsImplementation();
        }
        catch (Exception ex)
        {
            throw new DataSourceException("Error when attempting to check data store exists.", ex);
        }
    }
    public bool CanRead()
    {
        try
        {
            return CanReadImplementation();
        }
        catch (Exception ex)
        {
            throw new DataSourceException("Error when attempting to check whether data store can be read from.", ex);
        }
    }
    public bool CanWrite()
    {
        try
        {
            return CanWriteImplementation();
        }
        catch (Exception ex)
        {
            throw new DataSourceException("Error when attempting to check whether data store can be written to.", ex);
        }
    }

    public async Task<Stream> GetStreamAsync()
    {
        try
        {
            return await GetStreamAsyncImplementation();
        }
        catch (Exception ex)
        {
            throw new DataSourceException("Error when attempting to asynchronously stream data from data store.", ex);
        }
    }
    public async Task<bool> ExistsAsync()
    {
        try
        {
            return await ExistsAsyncImplementation();
        }
        catch (Exception ex)
        {
            throw new DataSourceException("Error when attempting to asynchronously check data store exists.", ex);
        }
    }
    public async Task<bool> CanReadAsync()
    {
        try
        {
            return await CanReadAsyncImplementation();
        }
        catch (Exception ex)
        {
            throw new DataSourceException("Error when attempting to asynchronously check whether data store can be read from.", ex);
        }
    }
    public async Task<bool> CanWriteAsync()
    {
        try
        {
            return await CanWriteAsyncImplementation();
        }
        catch (Exception ex)
        {
            throw new DataSourceException("Error when attempting to asynchronously check whether data store can be written to.", ex);
        }
    }

    // Implement these in concrete classes
    protected abstract Stream GetStreamImplementation();
    protected abstract bool ExistsImplementation();
    protected abstract bool CanReadImplementation();
    protected abstract bool CanWriteImplementation();

    // Optional implementationsvm
    protected virtual Task<Stream> GetStreamAsyncImplementation() => Task.FromResult(GetStreamImplementation());
    protected virtual Task<bool> ExistsAsyncImplementation() => Task.FromResult(ExistsImplementation());
    protected virtual Task<bool> CanReadAsyncImplementation() => Task.FromResult(CanReadImplementation());
    protected virtual Task<bool> CanWriteAsyncImplementation() => Task.FromResult(CanWriteImplementation());
}

public abstract partial class DataSource
{

    /// <summary>
    /// User-specified raw input
    /// </summary>
    public class RawInput : DataSource, IDataSource.IRawInput
    {
        public string Input { get; set; }
        public Encoding Encoding { get; set; }

        public override SourceType SourceType => SourceType.RawInput;

        protected override Stream GetStreamImplementation() => new MemoryStream(this.Encoding.GetBytes(Input));
        protected override bool ExistsImplementation() => !String.IsNullOrWhiteSpace(Input);
        protected override bool CanReadImplementation() => !String.IsNullOrWhiteSpace(Input);
        protected override bool CanWriteImplementation() => false;

        protected override Task<Stream> GetStreamAsyncImplementation() => Task.FromResult(GetStreamImplementation());
        protected override Task<bool> ExistsAsyncImplementation() => Task.FromResult(ExistsImplementation());
        protected override Task<bool> CanReadAsyncImplementation() => Task.FromResult(CanReadImplementation());
        protected override Task<bool> CanWriteAsyncImplementation() => Task.FromResult(CanWriteImplementation());
    }
}
