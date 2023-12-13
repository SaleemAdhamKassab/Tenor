using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Tenor.Services.KpisService.ViewModels.KpiModels;

namespace Infrastructure.Helpers
{
    public static partial class ExpressionUtils
    {
        public static Expression<Func<T, bool>> BuildPredicate<T>(string propertyName, string comparison, string value)
        {

            var parameter = Expression.Parameter(typeof(T), "x");

            var left = propertyName.Split('.').Aggregate((Expression)parameter, Expression.Property);

            var body1 = MakeComparison(left, comparison, value);

            return Expression.Lambda<Func<T, bool>>(body1, parameter);

        }

        public static Expression<Func<T, bool>> BuildFieldPredicate<T>(string propertyName, string comparison, string value)
        {

            var parameter = Expression.Parameter(typeof(T), "x.KpiFileds");

            var left = "FieldName".Split('.').Aggregate((Expression)parameter, Expression.Property);

            var body1 = MakeComparison(left, comparison, propertyName);
            //-------------------------------------------+-----------------
            var parameter2 = Expression.Parameter(typeof(T), "x.KpiFileds");

            var left2 = "Value".Split('.').Aggregate((Expression)parameter2, Expression.Property);

            var body2 = MakeComparison(left2, comparison, value);

            //--------------------------------------------------

            var andExpress = Expression.And(body1, body2);

            return Expression.Lambda<Func<T, bool>>(andExpress, parameter);

        }
        public static Expression<Func<T, bool>> BuildPredicate<T>(List<Filter> filter)
        {
            try
            {
                object collection =null;
                var parameter = Expression.Parameter(typeof(T), "x");
                Expression body2 = null;
                Expression body1 = null;
                Expression bodytemp1 = null;
                Expression bodytemp2 = null;
                Expression binExp = null;
                Expression body = parameter;

                foreach (var item in filter)
                {
                    bodytemp1 = null;
                    bodytemp2 = null;
                    binExp = null;
                    body = parameter;
                    foreach (var subMember in item.key.Split('.'))
                    {
                        body = Expression.PropertyOrField(body, subMember);

                    }


                        object val;
                        try
                        {


                        val = Convert.ChangeType(item.values, body.Type);
                        if(val==null)
                        {
                            val = Convert.ToString(item.values);

                        }


                    }
                        catch (Exception)
                        {
                            val = Int32.Parse(item.values.ToString());
                        }

                        Expression bodytemp;
                        try
                        {
                            if (val == "List")
                            {
                                bodytemp = Expression.NotEqual(body, Expression.Constant(val, body.Type));

                            }

                            else if (item.key == "key")

                            {
                                ConstantExpression constant = Expression.Constant(val, body.Type);
                                Expression left = Expression.Call(body, typeof(string).GetMethod("ToLower", System.Type.EmptyTypes));
                                Expression right = Expression.Call(constant, typeof(string).GetMethod("ToLower", System.Type.EmptyTypes));
                                bodytemp = Expression.Call(left, "StartsWith", null, right);

                            }


                            else
                            {
                                bodytemp = Expression.Equal(body, Expression.Constant(val, body.Type));

                            }



                        }
                        catch (Exception e)
                        {
                            val = float.Parse(item.values.ToString());
                            bodytemp = Expression.Equal(body, Expression.Constant(val, body.Type));
                        }
                        //bodytemp = Expression.Equal(body, Expression.Constant(val, body.Type));
                        if (bodytemp1 == null)
                        {
                            bodytemp1 = bodytemp;
                        }
                        else
                        {
                            bodytemp2 = bodytemp;
                            binExp = Expression.Or(bodytemp1, bodytemp2);
                            bodytemp1 = binExp;
                        }
                    

                    if (body1 == null)
                    {
                        body1 = bodytemp1;
                        binExp = body1;
                    }
                    else
                    {
                        body2 = bodytemp1;
                        binExp = Expression.And(body1, body2);
                        body1 = binExp;
                    }
                }

                var x = Expression.Lambda<Func<T, bool>>(binExp, parameter);
                return Expression.Lambda<Func<T, bool>>(binExp, parameter);
            }
            catch (Exception e)
            {
                return null;
            }

        }
        public static Expression MakeComparison(Expression left, string comparison, string value)
        {
            switch (comparison)
            {
                case "==":
                    return MakeBinary(ExpressionType.Equal, left, value);
                case "!=":
                    return MakeBinary(ExpressionType.NotEqual, left, value);
                case ">":
                    return MakeBinary(ExpressionType.GreaterThan, left, value);
                case ">=":
                    return MakeBinary(ExpressionType.GreaterThanOrEqual, left, value);
                case "<":
                    return MakeBinary(ExpressionType.LessThan, left, value);
                case "<=":
                    return MakeBinary(ExpressionType.LessThanOrEqual, left, value);
                case "Contains":
                case "StartsWith":
                case "EndsWith":
                    return Expression.Call(MakeString(left), comparison, Type.EmptyTypes, Expression.Constant(value, typeof(string)));
                default:
                    throw new NotSupportedException($"Invalid comparison operator '{comparison}'.");
            }
        }
        private static Expression MakeString(Expression source)
        {
            return source.Type == typeof(string) ? source : Expression.Call(source, "ToString", Type.EmptyTypes);
        }
        private static Expression MakeBinary(ExpressionType type, Expression left, string value)
        {
            object typedValue = value;
            if (left.Type != typeof(string))
            {
                if (string.IsNullOrEmpty(value))
                {
                    typedValue = null;
                    if (Nullable.GetUnderlyingType(left.Type) == null)
                        left = Expression.Convert(left, typeof(Nullable<>).MakeGenericType(left.Type));
                }
                else
                {
                    var valueType = Nullable.GetUnderlyingType(left.Type) ?? left.Type;
                    typedValue = valueType.IsEnum ? Enum.Parse(valueType, value) :
                        valueType == typeof(Guid) ? Guid.Parse(value) :
                        Convert.ChangeType(value, valueType);
                }
            }
            var right = Expression.Constant(typedValue, left.Type);
            return Expression.MakeBinary(type, left, right);
        }
    }
}
