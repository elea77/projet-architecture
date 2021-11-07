using System;
using System.Linq;
using System.Linq.Expressions;

namespace Archi.Library.Extensions
{
    public static class IQueryableExtensions
    {
        public static IQueryable<TModel> filter<TModel>(this IQueryable<TModel> query, string key, string value)
        {
            Expression finalExpression = Expression.Constant(true);
            var type = typeof(TModel);
            var property = type.GetProperty(key);// recupererle type
            ParameterExpression pe = Expression.Parameter(typeof(TModel), "string"); // recuperer le type de la data
            MemberExpression me = Expression.Property(pe, key);

            string replacx(string replacx, char[] characs)
            {
                foreach (char charac in characs)
                {
                    replacx = replacx.Replace(charac, ' ');
                }
                return replacx;
            }


            var op = value.Split(",");//rechercher l'operateur
            if (op.Length > 1)
            {
                Expression expression = null;
                if (value.Contains("]") && property.PropertyType == typeof(DateTime) || value.Contains("]") && property.PropertyType == typeof(int))
                {
                    if (value.Contains("[,"))// inferieur 
                    {
                        value = replacx(value, new char[] { '[', ',', ']' });
                        ConstantExpression constant = Expression.Constant(Convert.ChangeType(value, property.PropertyType));
                        finalExpression = Expression.LessThanOrEqual(me, constant);
                    }
                    else if (value.Contains(",]"))// superieure 
                    {
                        value = replacx(value, new char[] { '[', ',', ']' });
                        ConstantExpression constant = Expression.Constant(Convert.ChangeType(value, property.PropertyType));
                        finalExpression = Expression.GreaterThanOrEqual(me, constant);
                    }
                    else // fourchette 
                    {
                        Expression expression2 = null;
                        value = replacx(value, new char[] { '[', ']' });
                        var x = value.Split(",");
                        ConstantExpression constant1 = Expression.Constant(Convert.ChangeType(x[0], property.PropertyType));
                        ConstantExpression constant2 = Expression.Constant(Convert.ChangeType(x[1], property.PropertyType));
                        expression = Expression.GreaterThanOrEqual(me, constant1);
                        expression2 = Expression.LessThanOrEqual(me, constant2);
                        finalExpression = Expression.And(finalExpression, expression);
                        finalExpression = Expression.And(finalExpression, expression2);
                    }
                }
                else 
                {
                    foreach (var x in op)
                    {
                        ConstantExpression constant = Expression.Constant(Convert.ChangeType(x, property.PropertyType));
                        expression = Expression.Equal(me, constant);
                        finalExpression = Expression.Or(finalExpression, expression);
                    }
                }

            }
            else 
            {
                var w = Convert.ChangeType(value, property.PropertyType);
                ConstantExpression constant = Expression.Constant(w); //créer une valeur pour comparer avec me type
                finalExpression = Expression.Equal(me, constant); // Equal exemple mail==mail@mail.COM
                // c'est comme si je faisais -> Pizza.Name = "Pizza6";
            }


            var ExpressionTree = Expression.Lambda<Func<TModel, bool>>(finalExpression, new[] { pe });

            return query.Where(ExpressionTree);

        }

    }
}
