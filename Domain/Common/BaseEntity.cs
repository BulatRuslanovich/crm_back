using System.ComponentModel.DataAnnotations.Schema;

namespace CrmBack.Domain.Common;

public abstract class BaseEntity
{
    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;
}
