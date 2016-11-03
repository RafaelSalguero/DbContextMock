using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Tonic.Internal;

namespace Tonic
{
    /// <summary>
    /// Intercepts DbSet properties on a context
    /// </summary>
    class DbContextProxyInterceptor : IInterceptor
    {
        public DbContextProxyInterceptor(InMemoryMockDatabase database)
        {
            this.database = database;
        }

        readonly InMemoryMockDatabase database;
        private Dictionary<Type, object> setValues = new Dictionary<Type, object>();
        public void Intercept(IInvocation invocation)
        {
            //Substitute DbSet properties and the Set method:
            bool isPropertyGet = invocation.Method.IsSpecialName && invocation.Method.Name.StartsWith("get_");
            bool isDbSetMethod = invocation.Method.Name == nameof(DbContext.Set);

            if ((isPropertyGet || isDbSetMethod) && invocation.Method.ReturnType.IsGenericType && invocation.Method.ReturnType.GetGenericTypeDefinition() == typeof(DbSet<>))
            {
                //Return the substitute value
                object returnValue;
                var entityType = invocation.Method.ReturnType.GetGenericArguments()[0];

                if (!setValues.TryGetValue(entityType, out returnValue))
                {
                    var collection = database.Set(entityType);
                    returnValue = DbSetMockFactory.Create(entityType, collection);
                    setValues.Add(entityType, returnValue);
                }

                invocation.ReturnValue = returnValue;
            }
            else
            {
                //Continue with normal execution
                invocation.Proceed();
            }

        }

    }

    /// <summary>
    /// Create dynamic proxy instances of DbContext types
    /// </summary>
    class DbContextFactory
    {
        [ThreadStatic]
        private static ProxyGenerator _generator;
        private static ProxyGenerator generator
        {
            get
            {
                if (_generator == null)
                    _generator = new ProxyGenerator();
                return _generator;
            }
        }

        /// <summary>
        /// Create an instance of the given DbContext type, substituting DbSet properties
        /// </summary>
        /// <typeparam name="T">The DbContext type</typeparam>
        /// <param name="Properties">A dictionary where the keys are the entity types full names and the objects are the substitute DbSet mocks</param>
        /// <returns></returns>
        public static T Create<T>(InMemoryMockDatabase database)
            where T : DbContext
        {
            //Check that all DbSet properties are marked as virtual:
            var DbSetNotVirtualProperties =
                typeof(T).GetProperties()
                .Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>) && x.CanRead)
                .Where(x => !x.GetGetMethod().IsVirtual);

            if (DbSetNotVirtualProperties.Any())
            {
                var names = DbSetNotVirtualProperties.Select(x => x.Name).Aggregate("", (a, b) => a == "" ? b : a + ", " + b);
                throw new ArgumentException($"There are some DbSet properties on type {typeof(T)} that are not virtual and thus the DbContextInterceptor is not able to mock it: {names}");
            }
            var Interceptor = new DbContextProxyInterceptor(database);
            return (T)generator.CreateClassProxy(typeof(T), Interceptor);
        }
    }

}
