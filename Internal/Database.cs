using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonic
{
    /// <summary>
    /// Store the DbContext mock state
    /// </summary>
      class InMemoryMockDatabase
    {
        private Dictionary<Type, object> Lists = new Dictionary<Type, object>();

        private static Dictionary<Guid, InMemoryMockDatabase> databases = new Dictionary<Guid, InMemoryMockDatabase>();
        /// <summary>
        /// Gets an initialized database by its Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static InMemoryMockDatabase GetDatabase(Guid Id)
        {
            InMemoryMockDatabase result;
            if (!databases.TryGetValue(Id, out result))
            {
                result = new InMemoryMockDatabase();
                databases.Add(Id, result);
            }
            return result;
        }

        /// <summary>
        /// Gets the ObservableCollection that mocks a database table
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <returns></returns>
        public ObservableCollection<T> Set<T>()
        {
            return (ObservableCollection<T>)Set(typeof(T));
        }

        /// <summary>
        /// Returns the generic list containing the table data
        /// </summary>
        /// <param name="EntityType"></param>
        /// <returns></returns>
        public object Set(Type EntityType)
        {
            object result;
            if (!Lists.TryGetValue(EntityType, out result))
            {
                result = Activator.CreateInstance(typeof(ObservableCollection<>).MakeGenericType(EntityType));
                Lists.Add(EntityType, result);
            }
            return result;
        }
    }
}
