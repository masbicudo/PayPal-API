using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Specialized;

namespace PayPal
{
    public static class EnumHelper
    {
        /// <summary>
        /// Gets an attribute on an enum field value.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute you want to retrieve.</typeparam>
        /// <param name="enumValue">The enum value.</param>
        /// <returns>The attribute of type TAttribute that exists on the enumValue declaration.</returns>
        public static TAttribute GetAttributeOfType<TAttribute>(this Enum enumValue) where TAttribute : Attribute
        {
            var memberInfo = enumValue.GetType().GetMember(enumValue.ToString()).Single();
            var result = (TAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(TAttribute));
            return result;
        }

        /// <summary>
        /// Gets an attribute on an enum field value, using a lamda expression.
        /// This is useful, when multiple fileds of the have the same value.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute you want to retrieve.</typeparam>
        /// <param name="enumValue">The enum value.</param>
        /// <returns>The attribute of type TAttribute that exists on the enumValue declaration.</returns>
        public static TAttribute GetAttributeOfType<TAttribute, TEnum>(string fieldName) where TAttribute : Attribute
        {
            var memberInfo = typeof(TEnum).GetMember(fieldName).Single();
            var result = (TAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(TAttribute));
            return result;
        }
    }

    public static class EnumerableExtensions
    {
        public static T Slice<T>(this List<T> list, int index)
        {
            var result = list[index];
            list.RemoveAt(index);
            return result;
        }

        public static IEnumerable<T> ContinueWith<T>(this IEnumerable<T> e, T item)
        {
            foreach (var each in e)
                yield return each;
            yield return item;
        }
        public static IEnumerable<T> ContinueWith<T>(this IEnumerable<T> e0, IEnumerable<T> e1)
        {
            foreach (var each in e0)
                yield return each;
            foreach (var each in e1)
                yield return each;
        }
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> e0, Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            foreach (var each in e0)
                action(each);
            return e0;
        }
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> e0, Action<T, int> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            int i = 0;
            foreach (var each in e0)
                action(each, i++);
            return e0;
        }
    }

    public static class ExpressionHelper
    {
        public static PropertyInfo PropertyInfoOf<TOut>(Expression<Func<TOut>> propExpr)
        {
            var body = ((LambdaExpression)propExpr).Body;
            var member = body as MemberExpression ?? ((UnaryExpression)body).Operand as MemberExpression;
            var pi1 = (PropertyInfo)member.Member;
            return pi1;
        }
    }
}
