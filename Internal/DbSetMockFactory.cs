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
    static class DbSetMockFactory
    {
        /// <summary>
        /// Create a DbSetMock from a given ObservableCollection of elements
        /// </summary>
        /// <param name="entityType">The entity type of the DbSet</param>
        /// <param name="collection">An instance of the underlying ObservableCollection</param>
        /// <returns></returns>
        public static IDbSetMock Create(Type entityType, object collection)
        {
            return (IDbSetMock)Activator.CreateInstance(typeof(DbSetMock<>).MakeGenericType(entityType), collection);
        }

    }
}
