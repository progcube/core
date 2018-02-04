using System;

namespace Progcube.Core.Models.Entities
{
    /// <summary>
    /// An entity that inherits from class will automatically support soft deletion
    /// and have its timestamps updated upon creation and modification.
    /// <seealso cref="Progcube.Core.Data.BaseDbContext"/>
    /// </summary>
    public abstract class BaseEntity
    {
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public DateTime? DateDeleted { get; set; }
    }
}