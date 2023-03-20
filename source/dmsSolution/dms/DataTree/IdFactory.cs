namespace dms.DataTree;



public class IdFactory
{

    protected internal uint _maxId = uint.MaxValue;
    protected internal uint _lastId { get; set; }

    public uint NewId()
    {
        _lastId += 1;
        return _lastId;
    }
    public void Reset()
    {
        _lastId = default(uint);
    }
    public IdFactory()
    {
        _lastId = default(uint);
    }
}


