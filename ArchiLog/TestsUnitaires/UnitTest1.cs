using Archi.api.Data;
using Archi.api.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Archi.api.Controllers;
using Microsoft.EntityFrameworkCore;


namespace TestProject1
{
    [TestClass]
    public class UnitTest1
    {
        private readonly ArchiDbContext repository;

        public UnitTest1()
        {
            repository = MockDbContext.GetDbContext();
        }

        


        [TestMethod]
        public void GetIdTest()
        {
            var controller = new PizzasController(repository); //récupère les méthodes de PizzasController avec les données du mock
            var pizza = controller.GetPizza(1).Result.Value; //Utilise la méthode GetPizza, result car méthode async, récupère sa valeur avec value
            Assert.IsNotNull(pizza); // vérifie si la nouvelle pizza est nulle
            var result = controller.PostPizza(new Pizza { ID = 20 }).Result; // utilise la méthode PostPizza du controller et on créé une nouvelle pizza
            var pizza2 = repository.Pizzas.SingleOrDefaultAsync(x => x.ID == 20).Result; // on va récupérer les valeurs de la base de données mock avec l'Id qui est égal à deux
            Assert.IsNotNull(pizza); //On vérifie que la pizza n'est pas nulle



            //var controllerCustomer = new CustomersController(repository); //récupère les méthodes de CustomersController avec les données du mock

        }
    }
}