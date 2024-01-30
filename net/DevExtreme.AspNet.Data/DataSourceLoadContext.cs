using DevExtreme.AspNet.Data.Types;

using System.Collections.Generic;
using System.Linq;

namespace DevExtreme.AspNet.Data {

    partial class DataSourceLoadContext {
        readonly DataSourceLoadOptionsBase _options;
        readonly QueryProviderInfo _providerInfo;

        public DataSourceLoadContext(DataSourceLoadOptionsBase options, QueryProviderInfo providerInfo) {
            _options = options;
            _providerInfo = providerInfo;
        }

        public bool GuardNulls {
            get {
                return _providerInfo.IsLinqToObjects;
            }
        }

        public bool RequireQueryableChainBreak {
            get {
                return false;
            }
        }

        public AnonTypeNewTweaks CreateAnonTypeNewTweaks() => new AnonTypeNewTweaks {
            AllowEmpty = !_providerInfo.IsL2S && !_providerInfo.IsMongoDB,
            AllowUnusedMembers = !_providerInfo.IsL2S
        };
    }

    // Grouping
    partial class DataSourceLoadContext {
        bool?
            _shouldEmptyGroups,
            _useRemoteGrouping;

        public IReadOnlyList<GroupingInfo> Group => _options.Group;

        public bool ShouldEmptyGroups {
            get {
                if(!_shouldEmptyGroups.HasValue)
                    _shouldEmptyGroups = !Group.Last().GetIsExpanded();
                return _shouldEmptyGroups.Value;
            }
        }

        public bool UseRemoteGrouping {
            get {

                bool ShouldUseRemoteGrouping() {
                    if(_providerInfo.IsLinqToObjects)
                        return false;

                    return true;
                }

                if(!_useRemoteGrouping.HasValue)
                    _useRemoteGrouping = _options.RemoteGrouping ?? ShouldUseRemoteGrouping();

                return _useRemoteGrouping.Value;
            }
        }
    }
}
