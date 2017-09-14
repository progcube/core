using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Progcube.Core.Models.Entities.Mappings
{
    /// <summary>
    /// Allows configuration to be performed for an entity type in a model.
    /// </summary>
    public abstract class EntityMappingConfiguration<T> : IEntityMappingConfiguration<T> where T : class
    {
        public abstract void Map(EntityTypeBuilder<T> map);

        public void Map(ModelBuilder map)
        {
            Map(map.Entity<T>());
        }
    }
}