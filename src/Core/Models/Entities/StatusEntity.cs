namespace CrmBack.Core.Models.Entities;

public record StatusEntity(
    int status_id = 0,
    string name = "-",
    bool is_deleted = false
);