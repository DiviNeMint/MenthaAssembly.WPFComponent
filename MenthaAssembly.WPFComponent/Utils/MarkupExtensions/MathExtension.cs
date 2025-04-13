using MenthaAssembly.Expressions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Expression = System.Linq.Expressions.Expression;

namespace MenthaAssembly.MarkupExtensions
{
    [ContentProperty(nameof(Parameters))]
    public class MathExtension : MarkupExtension
    {
        public string Formula { set; get; }

        public List<MathParameter> Parameters { get; } = [];

        public MathExtension()
        {

        }
        public MathExtension(string Formula)
        {
            this.Formula = Formula;
        }

        public override object ProvideValue(IServiceProvider Provider)
        {
            if (Provider.GetService(typeof(IProvideValueTarget)) is not IProvideValueTarget ValueTarget ||
                ValueTarget.TargetObject is not DependencyObject Item)
                return this;

            if (!ExpressionBlock.TryParse(Formula, out ExpressionBlock Block))
                return null;

            List<ParameterExpression> ParamExprs = [];
            List<BindingBase> ParamBindings = [];
            foreach (MathParameter Param in Parameters)
            {
                ParamExprs.Add(Expression.Parameter(Param.Type ?? typeof(double), Param.Name));
                ParamBindings.Add(Param.Value);
            }

            Expression FormulaExpression = Block.Implement(ExpressionMode.Math, Expression.Constant(Item), ParamExprs);
            Delegate Function = Expression.Lambda(FormulaExpression, ParamExprs)
                                          .Compile();

            MultiBinding Multi = new() { Converter = new MathConverter(Function) };
            foreach (BindingBase Param in ParamBindings)
                Multi.Bindings.Add(Param);

            return Multi.ProvideValue(Provider);
        }

        private class MathConverter(Delegate Function) : IMultiValueConverter
        {
            public object Convert(object[] Values, Type TargetType, object Parameter, CultureInfo Culture)
                => Function.DynamicInvoke(Values);

            public object[] ConvertBack(object Value, Type[] TargetTypes, object Parameter, CultureInfo Culture)
                => throw new NotSupportedException();

        }

    }
}