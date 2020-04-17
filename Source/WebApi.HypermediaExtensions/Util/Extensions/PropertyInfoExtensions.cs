using System;
using System.Linq.Expressions;
using System.Reflection;
using FunicularSwitch;
using WebApi.HypermediaExtensions.Hypermedia;

namespace WebApi.HypermediaExtensions.Util.Extensions
{
    public static class PropertyInfoExtensions
    {
        public static Func<object, TOut> GetValueGetter<TOut>(this PropertyInfo propertyInfo)
        {
                var parameterExpression = Expression.Parameter(typeof(object), "i");
                var instance = Expression.Convert(parameterExpression, propertyInfo.DeclaringType);
                var property = Expression.Property(instance, propertyInfo);
                var convert = Expression.TypeAs(property, typeof(TOut));
                return Expression.Lambda<Func<object, TOut>>(convert, parameterExpression).Compile(); ;
        }

        public static Action<T, object> GetValueSetter<T>(this PropertyInfo propertyInfo)
        {
            if (typeof(T) != propertyInfo.DeclaringType)
            {
                throw new ArgumentException();
            }

            var instance = Expression.Parameter(propertyInfo.DeclaringType, "i");
            var argument = Expression.Parameter(typeof(object), "a");
            var setterCall = Expression.Call(
                instance,
                propertyInfo.GetSetMethod(),
                Expression.Convert(argument, propertyInfo.PropertyType));
            return (Action<T, object>)Expression.Lambda(setterCall, instance, argument).Compile();
        }
    }
}