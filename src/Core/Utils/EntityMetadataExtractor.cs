using System.Reflection;
using CrmBack.Core.Utils.Attributes;

namespace CrmBack.Core.Utils;

public static class EntityMetadataExtractor
{
    public static (string tableName, string keyColumn, string[] columns, string[] insertColumns, string[] updateColumns)
        ExtractMetadata<TEntity>() where TEntity : class
    {
        var entityType = typeof(TEntity);

        var tableAttribute = entityType.GetCustomAttribute<TableAttribute>() ?? throw new InvalidOperationException($"Entity {entityType.Name} must have TableAttribute");

        var tableName = tableAttribute.TableName;

        var properties = entityType.GetProperties()
            .Where(p => p.GetCustomAttribute<ColumnAttribute>() != null)
            .ToList();

        var constructor = entityType.GetConstructors()
            .FirstOrDefault(c => c.GetParameters().Length > 0);

        var parameters = constructor?.GetParameters()
            .Where(p => p.GetCustomAttribute<ColumnAttribute>() != null)
            .ToList() ?? [];

        var columns = new List<string>();
        var insertColumns = new List<string>();
        var updateColumns = new List<string>();
        string? keyColumn = null;

        foreach (var property in properties)
        {
            ProcessMember(property.GetCustomAttribute<ColumnAttribute>()!,
                         property.Name,
                         ref columns, ref insertColumns, ref updateColumns, ref keyColumn);
        }

        foreach (var parameter in parameters)
        {
            ProcessMember(parameter.GetCustomAttribute<ColumnAttribute>()!,
                         parameter.Name!,
                         ref columns, ref insertColumns, ref updateColumns, ref keyColumn);
        }

        if (string.IsNullOrEmpty(keyColumn))
        {
            throw new InvalidOperationException($"Entity {entityType.Name} must have a property/parameter marked with IsKey = true");
        }

        return (tableName, keyColumn, columns.ToArray(), insertColumns.ToArray(), updateColumns.ToArray());
    }

    private static void ProcessMember(ColumnAttribute columnAttribute, string memberName,
        ref List<string> columns, ref List<string> insertColumns, ref List<string> updateColumns, ref string? keyColumn)
    {
        var columnName = columnAttribute.ColumnName ?? ConvertToSnakeCase(memberName);

        columns.Add(columnName);

        if (columnAttribute.IsKey)
        {
            keyColumn = columnName;
        }

        if (columnAttribute.IsInsertable && columnAttribute.IsKey == false)
        {
            insertColumns.Add(columnName);
        }

        if (columnAttribute.IsUpdatable && columnAttribute.IsKey == false)
        {
            updateColumns.Add(columnName);
        }
    }

    private static string ConvertToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var result = new System.Text.StringBuilder();
        result.Append(char.ToLower(input[0]));

        for (int i = 1; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]))
            {
                result.Append('_');
                result.Append(char.ToLower(input[i]));
            }
            else
            {
                result.Append(input[i]);
            }
        }

        return result.ToString();
    }
}
