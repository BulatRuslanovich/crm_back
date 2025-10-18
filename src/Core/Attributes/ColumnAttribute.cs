namespace CrmBack.Core.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class ColumnAttribute(string? columnName = null) : Attribute
{
    public string? ColumnName { get; } = columnName;
    public bool IsInsertable { get; set; } = true;
    public bool IsUpdatable { get; set; } = true;
    public bool IsKey { get; set; } = false;
}