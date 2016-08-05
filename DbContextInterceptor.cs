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
            bool isGet = invocation.Method.IsSpecialName && invocation.Method.Name.StartsWith("get_");
            var propType = (isGet) ? invocation.Method.Name.Substring("get_".Length) : null;

            if (invocation.Method.ReturnType.IsGenericType && invocation.Method.ReturnType.GetGenericTypeDefinition() == typeof(DbSet<>))
            {
                var EntityType = invocation.Method.ReturnType.GetGenericArguments()[0];
                invocation.ReturnValue = PropertyValues[EntityType];
            }
            else
            {
                invocation.Proceed();
            }

        }

    }

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

        public static T Create<T>(Dictionary<Type, object> Properties)
        {
            var Interceptor = new ProxyInterceptor(Properties);
            return (T)generator.CreateClassProxy(typeof(T), Interceptor);
        }
    }

}
