using DevExtreme.AspNet.Data.Types;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DevExtreme.AspNet.Data.RemoteGrouping {

    class RemoteGroupExpressionCompiler : ExpressionCompiler {
        AnonTypeNewTweaks _anonTypeNewTweaks;
        IEnumerable<GroupingInfo> _grouping;

        public RemoteGroupExpressionCompiler(Type itemType, bool guardNulls, AnonTypeNewTweaks anonTypeNewTweaks, IEnumerable<GroupingInfo> grouping)
            : base(itemType, guardNulls) {
            _anonTypeNewTweaks = anonTypeNewTweaks;
            _grouping = grouping;
        }

        public Expression Compile(Expression target) {
            var groupByParam = CreateItemParam();
            var groupKeyExprList = new List<Expression>();
            var descendingList = new List<bool>();

            if(_grouping != null) {
                foreach(var i in _grouping) {
                    var selectorExpr = CompileAccessorExpression(groupByParam, i.Selector, liftToNullable: true);

                    groupKeyExprList.Add(selectorExpr);
                    descendingList.Add(i.Desc);
                }
            }

            var groupKeyTypeFacade = new AnonTypeFacade(groupKeyExprList);
            var groupKeyLambda = Expression.Lambda(groupKeyTypeFacade.CreateNewExpression(_anonTypeNewTweaks), groupByParam);
            var groupingType = typeof(IGrouping<,>).MakeGenericType(groupKeyLambda.ReturnType, ItemType);

            target = Expression.Call(typeof(Queryable), nameof(Queryable.GroupBy), new[] { ItemType, groupKeyLambda.ReturnType }, target, Expression.Quote(groupKeyLambda));

            for(var i = 0; i < groupKeyExprList.Count; i++) {
                var orderParam = Expression.Parameter(groupingType, "g");
                var orderAccessor = groupKeyTypeFacade.CreateMemberAccessor(
                    Expression.Property(orderParam, "Key"),
                    i
                );

                target = Expression.Call(
                    typeof(Queryable),
                    Utils.GetSortMethod(i == 0, descendingList[i]),
                    new[] { groupingType, orderAccessor.Type },
                    target,
                    Expression.Quote(Expression.Lambda(orderAccessor, orderParam))
                );
            }

            return MakeAggregatingProjection(target, groupingType, groupKeyTypeFacade);
        }

        Expression MakeAggregatingProjection(Expression target, Type groupingType, AnonTypeFacade groupKeyTypeFacade) {
            var param = Expression.Parameter(groupingType, "g");
            var groupCount = groupKeyTypeFacade.MemberCount;

            var projectionExprList = new List<Expression> {
                Expression.Call(typeof(Enumerable), nameof(Enumerable.Count), new[] { ItemType }, param)
            };

            for(var i = 0; i < groupCount; i++)
                projectionExprList.Add(groupKeyTypeFacade.CreateMemberAccessor(Expression.Property(param, "Key"), i));

            var projectionTypeFacade = new AnonTypeFacade(projectionExprList);
            var projectionLambda = Expression.Lambda(projectionTypeFacade.CreateNewExpression(_anonTypeNewTweaks), param);

            return Expression.Call(typeof(Queryable), nameof(Queryable.Select), new[] { param.Type, projectionLambda.ReturnType }, target, Expression.Quote(projectionLambda));
        }
    }

}
