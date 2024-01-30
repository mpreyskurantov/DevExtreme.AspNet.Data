using DevExtreme.AspNet.Data.RemoteGrouping;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Data.Types;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace DevExtreme.AspNet.Data {

    class DataSourceLoaderImpl<S> {
        readonly IQueryable Source;
        readonly DataSourceLoadContext Context;
        readonly Func<Expression, ExpressionExecutor> CreateExecutor;

#if DEBUG
        readonly Action<Expression> ExpressionWatcher;
        readonly bool UseEnumerableOnce;
#endif

        public DataSourceLoaderImpl(IQueryable source, DataSourceLoadOptionsBase options, CancellationToken cancellationToken, bool sync) {
            var providerInfo = new QueryProviderInfo(source.Provider);

            Source = source;
            Context = new DataSourceLoadContext(options, providerInfo);
            CreateExecutor = expr => new ExpressionExecutor(Source.Provider, expr, providerInfo, cancellationToken, sync, options.AllowAsyncOverSync);
        }

        DataSourceExpressionBuilder CreateBuilder() => new DataSourceExpressionBuilder(Source.Expression, Context);

        public async Task<LoadResult> LoadAsync() {
            var result = new LoadResult();

            if(Context.UseRemoteGrouping && Context.ShouldEmptyGroups) {
                var remotePaging = Context.Group.Count == 1;
                var groupingResult = await ExecRemoteGroupingAsync(remotePaging, false, remotePaging);

                EmptyGroups(groupingResult.Groups, Context.Group.Count);

                result.data = groupingResult.Groups;
            }

            return result;
        }

        async Task<RemoteGroupingResult> ExecRemoteGroupingAsync(bool remotePaging, bool suppressGroups, bool suppressTotals) {
            return RemoteGroupTransformer.Run(
                await ExecExprAnonAsync(CreateBuilder().BuildLoadGroupsExpr(remotePaging, suppressGroups, suppressTotals)),
                !suppressGroups ? Context.Group.Count : 0
            );
        }

        async Task<IEnumerable<R>> ExecExprAsync<R>(Expression expr) {
#if DEBUG
            ExpressionWatcher?.Invoke(expr);
#endif

            var executor = CreateExecutor(expr);

            if(Context.RequireQueryableChainBreak)
                executor.BreakQueryableChain();

            var result = await executor.ToEnumerableAsync<R>();

#if DEBUG
            if(UseEnumerableOnce)
                result = new EnumerableOnce<R>(result);
#endif

            return result;
        }

        async Task<IEnumerable<AnonType>> ExecExprAnonAsync(Expression expr) {
            return (await ExecExprAsync<object>(expr))
                .Select(i => i is AnonType anon ? anon : new DynamicClassAdapter(i));
        }

        static void EmptyGroups(IEnumerable groups, int level) {
            foreach(Group g in groups) {
                if(level < 2) {

                    if(g.items[0] is AnonType remoteGroup) {
                        g.count = (int)remoteGroup[0];
                    } else {
                        g.count = g.items.Count;
                    }

                    g.items = null;
                } else {
                    EmptyGroups(g.items, level - 1);
                }
            }
        }
    }

}
