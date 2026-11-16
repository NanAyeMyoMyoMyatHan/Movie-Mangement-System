using System;

namespace DotNetProject.Database.AppDbModels;

public partial class RolePermission
{
    public int RoleId { get; set; }

    public int PermissionId { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public virtual Role Role { get; set; } = null!;

    public virtual Permission Permission { get; set; } = null!;
}
