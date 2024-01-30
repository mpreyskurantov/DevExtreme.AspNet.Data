using DevExtreme.AspNet.Data.RemoteGrouping;

using System;
using System.Linq;
using System.Linq.Expressions;

namespace DevExtreme.AspNet.Data {

    class DataSourceExpressionBuilder {
        Expression Expr;
        readonly DataSourceLoadContext Context;

        public DataSourceExpressionBuilder(Expression expr, DataSourceLoadContext context) {
            Expr = expr;
            Context = context;
        }

        public Expression BuildLoadGroupsExpr(bool paginate, bool suppressGroups = false, bool suppressTotals = false) {
            AddRemoteGrouping(suppressGroups, suppressTotals);
            return Expr;
        }

        void AddRemoteGrouping(bool suppressGroups, bool suppressTotals) {
            var compiler = new RemoteGroupExpressionCompiler(
                GetItemType(), Context.GuardNulls, Context.CreateAnonTypeNewTweaks(),
                suppressGroups ? null : Context.Group
            );
            Expr = compiler.Compile(Expr);
        }

        Type[] GetQueryableGenericArguments() {
            const string queryable1 = "IQueryable`1";
            var type = Expr.Type;

            if(type.IsInterface && type.Name == queryable1)
                return type.GenericTypeArguments;

            return type.GetInterface(queryable1).GenericTypeArguments;
        }

        Type GetItemType()
            => GetQueryableGenericArguments().First();

    }

}
