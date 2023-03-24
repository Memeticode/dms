namespace dms.DataTree;



public class IdFactory
{

    protected internal uint _maxId = uint.MaxValue;
    protected internal uint _nextId { get; set; }

    public uint NewId()
    {
        uint newId = _nextId;
        _nextId++;
        return newId;
    }
    public void Reset()
    {
        _nextId = default;
    }
    public IdFactory()
    {
        _nextId = default;
    }
}


