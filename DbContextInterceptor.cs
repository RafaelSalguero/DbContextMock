using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;

namespace Tonic
{
    class ProxyInterceptor : IInterceptor
    {
        public ProxyInterceptor(Dictionary<Type, object> Sets)
        {
            this.PropertyValues = Sets;
        }
        readonly Dictionary<Type, object> PropertyValues;

        public void Intercept(IInvocation invocation)
        {
            //Substitute DbSet properties and the Set method:
            bool isPropertyGet = invocation.Method.IsSpecialName && invocation.Method.Name.StartsWith("get_");
            bool isSetMethod = invocation.Method.Name == nameof(DbContext.Set);

            if ((isPropertyGet || isSetMethod) && invocation.Method.ReturnType.IsGenericType && invocation.Method.ReturnType.GetGenericTypeDefinition() == typeof(DbSet<>))
            {
                //Return the substitute value
                var EntityType = invocation.Method.ReturnType.GetGenericArguments()[0];
                invocation.ReturnValue = PropertyValues[EntityType];
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
        /// <param name="Properties">A dictionary where the keys are the entity types and the objects are the substitute DbSet mocks</param>
        /// <returns></returns>
        public static T Create<T>(Dictionary<Type, object> Properties)
            where T : DbContext
        {
            var Interceptor = new ProxyInterceptor(Properties);
            return (T)generator.CreateClassProxy(typeof(T), Interceptor);
        }
    }

}
