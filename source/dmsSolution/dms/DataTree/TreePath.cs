using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace dms.DataTree;


public class TreePathException : Exception
{
    public TreePathException() : base() { }
    public TreePathException(string message) : base(message) { }
    public TreePathException(string message, Exception innerException) : base(message, innerException) { }
}
public class TreePath : ITreePath
{
    // Static methods & properties
    protected internal static char _seperator => '.';

    // Instance backing fields
    protected internal List<uint> _list { get; set; }

    // ITreePath implementation
    public IReadOnlyCollection<uint> List => new ReadOnlyCollection<uint>(_list);
    public string String => Length == 0 ? "" : string.Join(_seperator, _list);
    public uint Length { get; }

    public uint this[uint pathIdx] => _list[(int)pathIdx];
    public uint this[int pathIdx] => _list[pathIdx];

    public bool Equals(ITreePath? other)
    {
        if (other == null) return false;

        if (Length != other.Length) return false;
        for (int i = 0; i < Length; i++)
            if (this[i] != other[i]) return false;
        return true;
    }
    public int CompareTo(ITreePath? other)
    {
        // -1  if this is less
        // 0   if are equal
        // 1   if this is greater

        if (other == null) return 1;

        uint len = Math.Min(Length, other.Length);
        for (int i = 0; i < Length; i++)
        {
            if (this[i] == other[i]) continue;
            if (this[i] < other[i]) return -1;
            if (this[i] > other[i]) return 1;
        }

        if (Length == other.Length) return 0;
        if (Length < other.Length) return -1;
        if (Length > other.Length) return 1;

        throw new TreePathException($"Unable to compare specified paths {this.String} and {other.String}.");
    }

    public ITreePath GetBasePath()
    {
        if (Length == 0) 
            throw new TreePathException("Cannot get base path because specified path is a root path.");
        if (Length == 1)
            return new TreePath();
        return new TreePath(_list.GetRange(0, (int)Length - 1));
    }
    public ITreePath GetBranchPath(uint position) => new TreePath(List.Append(position));
    public ITreePath GetBranchPath(int position) => new TreePath(List.Append((uint)position));

    public bool IsBasePathOf(ITreePath other)
    {
        if (other == null) return false;
        if (Length + 1 != other.Length) return false;
        return Equals(other.GetBasePath());
    }
    public bool IsAncestorOf(ITreePath other)
    {
        if (other == null) return false;
        if (Length < other.Length)
        {
            for (int i = 0; i < Length; i++)
                if (this[i] != other[i])
                    return false;
            return true;
        }
        return false;
    }
    public bool IsDescendantOf(ITreePath other)
    {
        if (other == null) return false;
        if (Length > other.Length)
        {
            for (int i = 0; i < other.Length; i++)
                if (this[i] != other[i])
                    return false;
            return true;
        }
        return false;
    }


    // Constructors
    public TreePath()
    {
        Length = 0;
        _list = new List<uint>();
    }
    public TreePath(IEnumerable<int> pathPositions)
    {
        Length = 0;
        _list = new List<uint>();
        foreach (var pathPosition in pathPositions)
        {
            if (pathPosition < 0)
                throw new TreePathException($"Unable to create path. Specified path position {pathPosition} at enumerable index {Length} is negative.");
            Length += 1;
            _list.Add((uint)pathPosition);
        }
    }
    public TreePath(IEnumerable<uint> pathPositions)
    {
        Length = 0;
        _list = new List<uint>();
        foreach (var pathPosition in pathPositions)
        {
            Length += 1;
            _list.Add(pathPosition);
        }
    }
    public TreePath(string pathString)
    {
        _list = ParseStringAsPathList(pathString);
        Length = (uint)_list.Count();
    }


    protected internal List<uint> ParseStringAsPathList(string pathString)
    {
        if (string.IsNullOrEmpty(pathString))
            return new List<uint>();

        var strArr = pathString.Split(_seperator, StringSplitOptions.None);
        var pathPositions = new List<uint>();

        foreach (var str in strArr)
        {
            if (uint.TryParse(str, out uint pathPosition))
                pathPositions.Add(pathPosition);
            else
                throw new ArgumentException($"The input string {str} is not a valid unsigned integer.");
        }
        return pathPositions;
    }


}