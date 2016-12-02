using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.Internal
{
    static class PredicateBuilder
    {
        static Expression PredicateEqualBody(Type ElementType, IEnumerable<Tuple<string, object>> PropertyValues, ParameterExpression Instance)
        {
            Expression REx = null;
            foreach (var P in PropertyValues)
            {
                var Property = Expression.Property(Instance, P.Item1);

                var PropertyType = ((PropertyInfo)Property.Member).PropertyType;
                Expression Value = Expression.Constant(P.Item2);

                if ((Nullable.GetUnderlyingType(PropertyType) != null) && Value == null || (Nullable.GetUnderlyingType(Value.GetType()) == null))
                {
                    Value = Expression.Convert(Value, PropertyType);
                }

                var Eq = Expression.Equal(Property, Value);
                if (REx == null)
                    REx = Eq;
                else
                    REx = Expression.And(REx, Eq);
            }

            return REx;
        }
        /// <summary>
        /// Returns an expression that test each property equality to a given value. The given predicate can be used with the RLinq.Where method
        /// </summary>
        /// <param name="ElementType"></param>
        /// <param name="PropertyValues">A collection that matches the property and the values to test</param>
        /// <returns></returns>
        public static Expression PredicateEqual(Type ElementType, IEnumerable<Tuple<string, object>> PropertyValues)
        {
            var Instance = Expression.Parameter(ElementType);
            var REx = PredicateEqualBody(ElementType, PropertyValues, Instance);
            var Lambda = Expression.Lambda(REx, Instance);
            return Lambda;
        }

        /// <summary>
        /// Returns an expression that test each property equality to a given value. The given predicate can be used with the native Where method
        /// </summary>
        /// <returns></returns>
        public static Expression<Func<T, bool>> PredicateEqual<T>(IEnumerable<Tuple<string, object>> PropertyValues)
        {
            var ElementType = typeof(T);
            var Instance = Expression.Parameter(ElementType);
            var REx = PredicateEqualBody(ElementType, PropertyValues, Instance);
            var Lambda = Expression.Lambda<Func<T, bool>>(REx, Instance);
            return Lambda;
        }
    }
}
