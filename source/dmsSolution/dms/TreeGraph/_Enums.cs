
namespace dms.TreeGraph;

/// <summary>
/// Tree Node Type
/// </summary>
public enum TreeNodeType
{
    Root,   // is root node
    Stem,   // has branches
    Leaf,   // is not root and does not have branches
    Floating, // is not root but has no stem
}
