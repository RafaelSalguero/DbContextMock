using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace Tonic
{
    /// <summary>
    /// Intercepts DbSet properties on a context
    /// </summary>
    class DbContextProxyInterceptor : IInterceptor
    {
        public DbContextProxyInterceptor(Dictionary<string , object> Sets)
        {
            this.PropertyValues = Sets;
        }
        readonly Dictionary<string, object> PropertyValues;

        public void Intercept(IInvocation invocation)
        {
            //Substitute DbSet properties and the Set method:
            bool isPropertyGet = invocation.Method.IsSpecialName && invocation.Method.Name.StartsWith("get_");
            bool isDbSetMethod = invocation.Method.Name == nameof(DbContext.Set);

            if ((isPropertyGet || isDbSetMethod) && invocation.Method.ReturnType.IsGenericType && invocation.Method.ReturnType.GetGenericTypeDefinition() == typeof(DbSet<>))
            {
                //Return the substitute value
                var EntityType = invocation.Method.ReturnType.GetGenericArguments()[0];
                var typeName = EntityType.FullName;

                if (!PropertyValues.ContainsKey(typeName))
                {
                    throw new ArgumentException($"DbSet mock could not find the property for the type {typeName}");
                }
                invocation.ReturnValue = PropertyValues[typeName];
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
        public static T Create<T>(Dictionary<string, object> Properties)
            where T : DbContext
        {
            var Interceptor = new DbContextProxyInterceptor(Properties);
            return (T)generator.CreateClassProxy(typeof(T), Interceptor);
        }
    }

}
