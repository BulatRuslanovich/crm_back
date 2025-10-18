namespace CrmBack.Core.Models.Entities;

public record StatusEntity(
    int status_id,
    string name,
    bool is_deleted
);