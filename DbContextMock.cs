using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tonic
{
    public static class DbContextMock
    {


        static DbContextMock()
        {

        }

        /// <summary>
        /// Create an in-memory DbContext mock that will not persists its state between instances
        /// </summary>
        /// <typeparam name="T">Your DbContext type</typeparam>
        /// <returns></returns>
        public static T Transient<T>()
             where T : DbContext
        {
            return Create<T>(null);
        }

        /// <summary>
        /// Create an in-memory DbContext mock that will persist its state between instances
        /// </summary>
        /// <typeparam name="T">Your DbContext type</typeparam>
        /// <param name="DatabaseId">The Id of the internal in-memory database. Use the same Id for access the persisted data</param>
        /// <returns></returns>
        public static T Persistent<T>(Guid DatabaseId)
            where T : DbContext
        {
            return Create<T>(DatabaseId);
        }

        /// <summary>
        /// Create an in-memory typed DbContext
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="DatabaseId">The Id of the in-memory database. Use the same Id between calls for persisting state between DbContext instances. Use null for not persisting state at all</param>
        /// <returns></returns>
        private static T Create<T>(Guid? DatabaseId)
            where T : DbContext
        {

            var database = DatabaseId.HasValue ? InMemoryMockDatabase.GetDatabase(DatabaseId.Value) : new InMemoryMockDatabase();

            var DbSetProperties = typeof(T).GetProperties()
            .Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

            var ProxyProperties = new Dictionary<Type, object>();
            foreach (var DbSet in DbSetProperties)
            {
                if (!DbSet.GetGetMethod().IsVirtual)
                    throw new ArgumentException($"Make your DbContext property '{DbSet.Name}' virtual in order to be able to mock it");

                var EntityType = DbSet.PropertyType.GetGenericArguments()[0];

                //Get the database list:
                object SetList = database.Set(EntityType);
                object SetMock = Activator.CreateInstance(typeof(DbSetMock<>).MakeGenericType(EntityType), SetList);

                ProxyProperties.Add(EntityType, SetMock);
            }

            return DbContextFactory.Create<T>(ProxyProperties);
        }
    }
}
