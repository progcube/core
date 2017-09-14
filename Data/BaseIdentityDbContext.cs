using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Progcube.Core.Models.Entities;

namespace Progcube.Core.Data
{
    /// <summary>
    /// Base DbContext that supports AspNet.Identity.
    /// </summary>
    public abstract class BaseIdentityDbContext<TUser, TRole> : IdentityDbContext<TUser, TRole, Guid>
        where TUser : IdentityUser<Guid>
        where TRole : IdentityRole<Guid>
    {
        public BaseIdentityDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.Entity<TUser>().ToTable("Users");
            builder.Entity<TRole>().ToTable("Roles");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        }

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