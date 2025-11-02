using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrmBack.Core.Models.Entities;

public abstract class BaseEntity
{
    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;
}