using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Progcube.Core.Models.Entities.Mappings
{
    public interface IEntityMappingConfiguration
    {
        void Map(ModelBuilder builder);
    }

    public interface IEntityMappingConfiguration<T> : IEntityMappingConfiguration where T : class
    {
        void Map(EntityTypeBuilder<T> builder);
    }
}