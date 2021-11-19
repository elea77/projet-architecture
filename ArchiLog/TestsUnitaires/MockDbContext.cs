using Archi.api.Data;
using Archi.api.Models;
using Microsoft.EntityFrameworkCore;

namespace TestProject1
{
    public class MockDbContext : ArchiDbContext
    {
        public MockDbContext(DbContextOptions options) : base(options)
        {

        }
        public static MockDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder().UseInMemoryDatabase("dbmok").Options;//détermine les options de connexion pour al base en mémoire
            // DbContextOptionsBuilder initialise une nouvelle instance de Microsoft.EntityFrameworkCore.DbContextOptionsBuilder (classe sans aucune option définie)
            // UseInMemoryDatabase configure le contexte pour se connecter à une base de données en mémoire nommée.
            var db = new MockDbContext(options);
            db.Pizzas.Add(new Pizza { ID = 1, Active = true, Name = "Pizza1", Price = 10 }); //Ajoute des pizzas à la base de données mock
            db.Pizzas.Add(new Pizza { ID = 2, Active = true, Name = "Pizza2", Price = 15 });
            db.Pizzas.Add(new Pizza { ID = 3, Active = false, Name = "Pizza3", Price = 15 });
            db.Pizzas.Add(new Pizza { ID = 4, Active = true, Name = "Pizza4", Price = 15 });
            db.Pizzas.Add(new Pizza { ID = 5, Active = true, Name = "Pizza5", Price = 15 });
            db.SaveChanges();
            return db;
        }
    }
}