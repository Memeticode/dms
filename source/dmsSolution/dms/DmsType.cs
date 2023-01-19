using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dms;

public class DmsType : IDmsType
{
    public StructureType StructureType { get; }
    public ValueType ValueType { get; }
    public override string ToString() => $"{StructureType}<{ValueType}>";
    public DmsType(StructureType structureType, ValueType valueType)
    {
        StructureType = structureType;
        ValueType = valueType;
        Validate();
    }

    public bool IsPrimitive => ValueType.CheckIsValue() && (StructureType.CheckIsValue() || StructureType.CheckIsField());
    public bool IsEnumerable => StructureType.CheckIsEnumerable() || ValueType.CheckIsEnumerable();
    public bool IsLabeled => StructureType.CheckIsLabeled();


    public bool IsValue => StructureType.CheckIsValue() && ValueType.CheckIsValue();
    public bool IsField => StructureType.CheckIsField();
    public bool IsList => StructureType.CheckIsList();
    public bool IsRecord => StructureType.CheckIsRecord();
    public bool IsDataset => StructureType.CheckIsDataset();
    
    public bool Equals(IDmsType? dmsType)
    {
        if ( dmsType is null )
            return false;
        else if (!StructureType.Equals(dmsType.StructureType))
            return false;
        else if (!ValueType.Equals(dmsType.ValueType))
            return false;
        else
            return true;
    }

    public void Validate()
    {
        if (StructureType.CheckIsValue())
            if (!ValueType.CheckIsPrimitive())
                throw new DmsTypeInvalidException(this, $"StructureType is Value but ValueType ({ValueType}) is not a primitive type.");

        if (StructureType.CheckIsList())
            if (ValueType.CheckIsField())
                throw new DmsTypeInvalidException(this, $"StructureType is List and ValueType is Field -- StructureType should be Record.");
            else if (ValueType.CheckIsRecord())
                throw new DmsTypeInvalidException(this, $"StructureType is List and ValueType is Record -- StructureType should be Dataset.");

        if (StructureType.CheckIsRecord())
            if (!ValueType.CheckIsField())
                throw new DmsTypeInvalidException(this, $"StructureType is Record but ValueType ({ValueType}) is not Field.");

        if (StructureType.CheckIsDataset())
            if (!ValueType.CheckIsRecord())
                throw new DmsTypeInvalidException(this, $"StructureType is Dataset but ValueType ({ValueType}) is not Record.");

    }

    // Exceptions
    public class DmsTypeInvalidException : Exception
    {
        public DmsTypeInvalidException(DmsType type, string message) : base($"DmsType {type} is invalid. {message}") { }
        public DmsTypeInvalidException(DmsType type, string message, Exception e) : base($"DmsType {type} is invalid. {message}", e) { }
    }

}

