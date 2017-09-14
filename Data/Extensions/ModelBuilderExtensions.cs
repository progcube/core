using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Progcube.Core.Models.Entities.Mappings;

namespace Progcube.Core.Data.Extensions
{
    public static class ModelBuilderExtensions
    {
        /// <summary>
        /// Scan the given assembly and register all its IEntityMappingConfiguration implementations against the ModelBuilder.
        /// </summary>
        public static void RegisterEntityMappings(this ModelBuilder modelBuilder, Assembly assembly)
        {
            var mappingTypes = assembly.GetMappingTypes(typeof(IEntityMappingConfiguration<>));
            foreach (var config in mappingTypes.Select(Activator.CreateInstance).Cast<IEntityMappingConfiguration>())
            {
                config.Map(modelBuilder);
            }
        }

        private static IEnumerable<Type> GetMappingTypes(this Assembly assembly, Type mappingInterface)
        {
            return assembly.GetTypes().Where(t => !t.IsAbstract && t.GetInterfaces().Any(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == mappingInterface));
        }
    }
}