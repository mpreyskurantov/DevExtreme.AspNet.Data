using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Data.Types;

using System.Collections.Generic;
using System.Linq;

namespace DevExtreme.AspNet.Data.RemoteGrouping {

    class RemoteGroupTransformer {
        public static RemoteGroupingResult Run(IEnumerable<AnonType> flatGroups, int groupCount) {
            List<Group> hierGroups = null;

            if(groupCount > 0) {
                hierGroups = new GroupHelper<AnonType>(AnonTypeAccessor.Instance).Group(
                    flatGroups,
                    Enumerable.Range(0, groupCount).Select(i => new GroupingInfo { Selector = AnonType.IndexToField(1 + i) }).ToArray()
                );
            }

            return new RemoteGroupingResult {
                Groups = hierGroups
            };
        }
    }

}
