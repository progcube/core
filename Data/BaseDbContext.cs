using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Progcube.Core.Models.Entities;

namespace Progcube.Core.Data
{
    public abstract class BaseDbContext : DbContext
    {
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        private void UpdateTimestamps()
        {
            // Update the timestamp for all entities inheriting from BaseEntity
            var entities = ChangeTracker.Entries().Where(x => x.Entity is BaseEntity && (x.State == EntityState.Added || x.State == EntityState.Modified));
            foreach (var entity in entities)
            {
                var baseEntity = ((BaseEntity)entity.Entity);
                if (entity.State == EntityState.Added)
                {
                    baseEntity.DateCreated = DateTime.UtcNow;
                }

                baseEntity.DateModified = DateTime.UtcNow;
            }
        }
    }
}