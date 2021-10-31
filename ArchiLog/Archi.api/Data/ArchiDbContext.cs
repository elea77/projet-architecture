using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Archi.api.Models;
using Archi.Library.Models;
using Microsoft.EntityFrameworkCore;

namespace Archi.api.Data
{
    public class ArchiDbContext : DbContext
    {
        public ArchiDbContext(DbContextOptions options):base(options)
        {

        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            ChangeAddedState();
            ChangeDeleteState();

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ChangeAddedState();
            ChangeDeleteState();

            return base.SaveChangesAsync(cancellationToken);
        }

        private void ChangeAddedState() {
            var entites = ChangeTracker.Entries().Where(x => x.State == EntityState.Deleted);
            foreach (var item in entites)
            {
                item.State = EntityState.Modified;
                if (item.Entity is ModelBase)
                {
                    ((ModelBase)item.Entity).Active = true;
                }
            }
        }

        private void ChangeDeleteState() 
        {
            var entites = ChangeTracker.Entries().Where(x => x.State == EntityState.Deleted);
            foreach (var item in entites)
            {
                item.State = EntityState.Modified;
                if (item.Entity is ModelBase)
                {
                    ((ModelBase)item.Entity).Active = false;
                }
            }
        }
        public DbSet<Customer> Customers { get; set; }

        public DbSet<Pizza> Pizzas { get; set; }
    }
}
