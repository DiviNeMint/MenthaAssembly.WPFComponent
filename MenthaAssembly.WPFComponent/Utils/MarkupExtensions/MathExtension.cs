using MenthaAssembly.Expressions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Expression = System.Linq.Expressions.Expression;

namespace MenthaAssembly.MarkupExtensions
{
    [ContentProperty("Parameters")]
    public class MathExtension : MarkupExtension
    {
        public string Formula { set; get; }

        public List<MathParameter> Parameters { get; } = new List<MathParameter>();

        public MathExtension()
        {

        }
        public MathExtension(string Formula)
        {
            this.Formula = Formula;
        }

        public override object ProvideValue(IServiceProvider Provider)
        {
            if (!MathExpression.TryParse(Formula, out ExpressionBlock Block))
                return null;

            List<ParameterExpression> ParamExprs = new List<ParameterExpression>();
            List<BindingBase> ParamBindings = new List<BindingBase>();
            foreach (MathParameter Param in Parameters)
            {
                ParamExprs.Add(Expression.Parameter(typeof(object), Param.Name));
                ParamBindings.Add(Param.Value);
            }

            List<string> Properies = new List<string>();
            foreach (ExpressionRoute Route in EnumRoute(Block).Where(i => !ParamExprs.Any(p => p.Name == i.Contexts[0].Name) &&
                                                                          !i.TryImplement(null, null, ParamExprs, out _) &&
                                                                          !i.TryParseType(null, ParamExprs, out _)))
            {
                IExpressionRoute Path = Route.Contexts[0];
                if (Path.Type == ExpressionObjectType.Member)
                    Properies.Add(Path.Name);
            }

            if (Properies.Count > 0)
            {
                if (Provider.GetService(typeof(IProvideValueTarget)) is not IProvideValueTarget ValueTarget ||
                    ValueTarget.TargetObject is not DependencyObject Item)
                    return this;

                Type ItemType = Item.GetType();
                foreach (PropertyInfo Property in Properies.Select(i => ItemType.GetProperty(i)))
                {
                    ParamExprs.Add(Expression.Parameter(Property.PropertyType, Property.Name));
                    ParamBindings.Add(new Binding(Property.Name) { Source = Item });
                }
            }

            Expression FormulaExpression = Block.Implement(null, ParamExprs);
            Delegate Function = Expression.Lambda(FormulaExpression, ParamExprs)
                                          .Compile();

            MultiBinding Multi = new MultiBinding { Converter = new MathConverter(Function) };
            foreach (BindingBase Param in ParamBindings)
                Multi.Bindings.Add(Param);

            return Multi.ProvideValue(Provider);
        }

        private IEnumerable<ExpressionRoute> EnumRoute(ExpressionBlock Block)
        {
            foreach (IExpressionObject Context in Block.Contexts)
            {
                if (Context is ExpressionBlock Children)
                {
                    foreach (ExpressionRoute Route in EnumRoute(Children))
                        yield return Route;
                }

                else if (Context is ExpressionRoute Route)
                    yield return Route;
            }
        }

        private class MathConverter : IMultiValueConverter
        {
            private readonly Delegate Function;

            public MathConverter(Delegate Function)
            {
                this.Function = Function;
            }

            public object Convert(object[] Values, Type TargetType, object Parameter, CultureInfo Culture)
                => Function.DynamicInvoke(Values);

            public object[] ConvertBack(object Value, Type[] TargetTypes, object Parameter, CultureInfo Culture)
                => throw new NotSupportedException();

        }

    }
}