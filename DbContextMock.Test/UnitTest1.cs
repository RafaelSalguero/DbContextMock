using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbContextMock.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void SetTest()
        {
            var Id = Guid.NewGuid();
            using (var C = Tonic.DbContextMock.Persistent<Db>(Id))
            {
                var Customer = new Customer
                {
                    Name = "Rafa",
                    Id = 10,
                    Email = "rafaelsalgueroiturrios@gmail.comn"
                };

                //Add two customers
                C.Customer.Add(Customer);
                C.Set<Customer>().Add(Customer);

                C.SaveChanges();
            }

            //New instance should be empty
            using (var C = Tonic.DbContextMock.Persistent<Db>(Id))
            {
                Assert.AreEqual(2, C.Customer.Count());
                Assert.AreEqual(2, C.Set<Customer>().Count());
                //The set instance should be the same:
                Assert.AreEqual(C.Set<Customer>(), C.Customer);
                Assert.AreEqual(C.Set<Customer>(), C.Set<Customer>());
                Assert.AreEqual(C.Customer, C.Customer);

            }
        }


        [TestMethod]
        public async Task AsyncTest()
        {
            using (var C = Tonic.DbContextMock.Transient<Db>())
            {
                C.Customer.Add(new Customer
                {
                    Name = "Rafa",
                    Id = 5,
                    Email = "rafaelsalgueroiturrios@gmail.comn"
                });

                C.Customer.Add(new Customer
                {
                    Name = "Jose",
                    Id = 6,
                    Email = "jose@gmail.comn"
                });


                var List = await C.Customer.ToListAsync();

                Assert.AreEqual(2, List.Count);
                Assert.AreEqual(5, List[0].Id);

                var First = await C.Customer.FirstAsync();
                Assert.AreEqual("Rafa", First.Name);
            }
        }


        [TestMethod]
        public async Task AsyncQueryableTest()
        {
            using (var C = Tonic.DbContextMock.Transient<Db>())
            {
                C.Customer.Add(new Customer
                {
                    Name = "Rafa",
                    Id = 5,
                    Email = "rafaelsalgueroiturrios@gmail.comn"
                });

                C.Customer.Add(new Customer
                {
                    Name = "Jose",
                    Id = 6,
                    Email = "jose@gmail.comn"
                });


                var List = await C.Customer.Where(x => x.Name == "Rafa").Select(x => x.Id).ToListAsync();

                Assert.AreEqual(1, List.Count);
                Assert.AreEqual(5, List[0]);
            }
        }

        [TestMethod]
        public void TransientTest()
        {
            using (var C = Tonic.DbContextMock.Transient<Db>())
            {
                var Customer = new Customer
                {
                    Name = "Rafa",
                    Id = 10,
                    Email = "rafaelsalgueroiturrios@gmail.comn"
                };

                C.Customer.Add(Customer);
            }

            //New instance should be empty
            using (var C = Tonic.DbContextMock.Transient<Db>())
            {
                Assert.IsFalse(C.Customer.Any());
            }
        }

        [TestMethod]
        public void QueryTest()
        {
            var DbId = Guid.NewGuid();

            using (var C = Tonic.DbContextMock.Persistent<Db>(DbId))
            {
                C.Customer.Add(new Customer
                {
                    Name = "Rafa",
                    Id = 1,
                    Email = "rafaelsalgueroiturrios@gmail.comn"
                });

                C.Customer.Add(new Customer
                {
                    Name = "Jose",
                    Id = 2,
                    Email = "jose@gmail.comn"
                });
            }

            using (var C = Tonic.DbContextMock.Persistent<Db>(DbId))
            {
                Assert.AreEqual("Jose", C.Customer.Where(x => x.Id == 2).Select(x => x.Name).FirstOrDefault());
            }
        }

        [TestMethod]
        public void PersistentTest()
        {
            var DbId = Guid.NewGuid();

            var X = Tonic.DbContextMock.Persistent<Db>(DbId);

            var Customer = new Customer
            {
                Name = "Rafa",
                Id = 10,
                Email = "rafaelsalgueroiturrios@gmail.comn"
            };

            X.Customer.Add(Customer);

            var Ret = X.Customer.ToList();
            Assert.AreEqual(Customer, Ret[0]);

            //Create a new context with the same database id, the data should be the same
            var Y = Tonic.DbContextMock.Persistent<Db>(DbId);

            Assert.AreEqual(Customer, Y.Customer.First());
        }

        [TestMethod]
        public async Task FindTest()
        {
            using (var c = Tonic.DbContextMock.Transient<Db>())
            {
                var Customer = new Customer
                {
                    Name = "Rafa",
                    Id = 10,
                    Email = "rafaelsalgueroiturrios@gmail.comn"
                };
                c.Customer.Add(Customer);

                Assert.IsNull(c.Customer.Find(1));
                Assert.IsNull(c.Product.Find(1));

                Assert.IsNotNull(c.Customer.Find(10));
                Assert.AreEqual("Rafa", (await c.Customer.FindAsync(10)).Name);
            }
        }
    }
}
