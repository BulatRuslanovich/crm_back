namespace CrmBack.Core.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class TableAttribute(string tableName) : Attribute
{
    public string TableName { get; } = tableName;
}