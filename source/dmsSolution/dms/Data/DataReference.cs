using System.IO;
using System.Xml.Linq;

namespace dms.Data;

public class DataReference : IDataReference
{
    protected internal string? _contextLabel { get; set; }
    public string ContextLabel
    {
        get => _contextLabel ?? "{None}";
        set
        {
            if (!value.All(c => char.IsLetterOrDigit(c)))
                throw new ArgumentException("Context label can only contain alphanumeric characters.");
            _contextLabel = value;
        }
    }
    public ITreePath? Path { get; set; }

    public string String => $"{ContextLabel}:{(Path is null ? "root" : Path.ToString())}";
    public override string ToString() => String;
    public bool Equals(IDataReference? other)
    {
        if (other == null) return false;
        return this.ToString() == other.ToString();
    }


    public DataReference(string referenceString)
    {
        try
        {
            if (referenceString.Contains(':'))
            {
                var parts = referenceString.Split(':', 2);
                var contextLabel = parts[0];
                var pathString = parts[1];
                var path = new TreePath(pathString);
                ContextLabel = contextLabel;
                Path = path;
            }
            else
            {
                ContextLabel = referenceString;
            }
        }
        catch
        {
            throw new DataReferenceException($"Unable to create data reference from reference string {{{referenceString}}}.");
        }
    }
    public DataReference(string contextLabel, TreePath path)
    {
        ContextLabel = contextLabel;
        Path = path;
    }
    public DataReference(string contextLabel, string pathString)
    {
        var path = new TreePath(pathString);
        ContextLabel = contextLabel;
        Path = path;
    }

}

public class DataReferenceException : Exception
{
    public DataReferenceException() : base() { }
    public DataReferenceException(string message) : base(message) { }
    public DataReferenceException(string message, Exception innerException) : base(message, innerException) { }
}
