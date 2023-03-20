using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dms;

/*
public class Data : IData
{
    private object? _value { get; }
    private IDmsType _dmsType { get; }
    private IDmsDataAdapter _adapter { get; }
    private IDmsDataFactory _factory { get; }

    public IDmsType DmsType => _dmsType;

    public IDmsDataAdapter Adapter => _adapter;
    public object? Value => _value;
    public Type? SourceType => _value?.GetType();


    public bool IsNull => _value is null;

    public int Count => throw new NotImplementedException();



    public IData GetValue()
    {
        if (!_dmsType.IsField)
            throw new InvalidOperationException($"Data has DmsType {_dmsType}. In order to call GetValue(), DmsType must have StructureType of Field.");
        else
            return _adapter.GetValue(this);
    }
    public IEnumerable<IData> GetValues() 
    {
        if (_dmsType.IsEnumerable)
            throw new InvalidOperationException($"Data has DmsType {_dmsType}. In order to call GetValue(), DmsType must have StructureType of Field.");
        else
            return _adapter.GetValues(this);
    }
    public IData GetLabel() => _adapter.GetLabel(this);
    public IEnumerable<IData> GetLabels() => _adapter.GetLabels(this);
    public IData GetValueByPosition(int position) => throw new NotImplementedException();
    public IData GetValueByLabel(object? label) => throw new NotImplementedException();
    public IData GetValuesByLabel(object? label) => throw new NotImplementedException();



    public bool TryGetValue(out IData? value)
    {
        if (IsNull)
        {
            value = null;
            return false;
        }


    }
    public bool TryGetValues(out IEnumerable<IData>? values)
    {
        throw new NotImplementedException();
    }

    public bool TryGetLabel(out IData? label)
    {
        throw new NotImplementedException();
    }

    public bool TryGetLabels(object? label, out IEnumerable<IData>? values)
    {
        throw new NotImplementedException();
    }


    public bool TryGetValueByLabel(object? label, out IData? value)
    {
        throw new NotImplementedException();
    }

    public bool TryGetValueByPosition(int position, out IData? value)
    {
        throw new NotImplementedException();
    }


    public bool TryGetValuesByLabel(object? label, out IData? values)
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
    public IEnumerator<IData> GetEnumerator()
    {
        throw new NotImplementedException();
    }
}

*/