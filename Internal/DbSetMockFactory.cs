using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.Internal
{
    /// <summary>
    /// Create a DbSetMock from a given ObservableCollection
    /// </summary>
    public static class DbSetMockFactory
    {
        //var DbSetProperties = typeof(T).GetProperties()
        //  .Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

        //var ProxyProperties = new Dictionary<string, object>();
        //    foreach (var DbSet in DbSetProperties)
        //    {
        //        if (!DbSet.GetGetMethod().IsVirtual)
        //            throw new ArgumentException($"Make your DbContext property '{DbSet.Name}' virtual in order to be able to mock it");

        //var EntityType = DbSet.PropertyType.GetGenericArguments()[0];

        ////Get the database list:
        //object SetList = database.Set(EntityType);
        //object SetMock = Activator.CreateInstance(typeof(DbSetMock<>).MakeGenericType(EntityType), SetList);

        //ProxyProperties.Add(EntityType.FullName, SetMock);
        //    }

        /// <summary>
        /// Create a DbSetMock from a given ObservableCollection of elements
        /// </summary>
        /// <param name="entityType">The entity type of the DbSet</param>
        /// <param name="collection">An instance of the underlying ObservableCollection</param>
        /// <returns></returns>
        public static object Create(Type entityType, object collection)
        {
            return Activator.CreateInstance(typeof(DbSetMock<>).MakeGenericType(entityType), collection);
        }

    }
}
