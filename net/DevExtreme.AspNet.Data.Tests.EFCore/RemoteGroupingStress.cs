#if !EFCORE1
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using Xunit;

namespace DevExtreme.AspNet.Data.Tests.EFCore {

    public class RemoteGroupingStress {

        [Table(nameof(RemoteGroupingStress) + "_" + nameof(DataItem))]
        public class DataItem : RemoteGroupingStressHelper.IEntity {
            public int ID { get; set; }
            public int Num { get; set; }
            public int? NullNum { get; set; }
            public DateTime Date { get; set; }
            public DateTime? NullDate { get; set; }
#if NET6_0_OR_GREATER && EFCORE8
            public DateOnly DateO { get; set; }
            public DateOnly? NullDateO { get; set; }
#endif
        }

        [Fact]
        public async Task Scenario() {
            await TestDbContext.ExecAsync(context => {
                var dbSet = context.Set<DataItem>();

                dbSet.Add(new DataItem());
                context.SaveChanges();

                //https://stackoverflow.com/questions/73884768
                RemoteGroupingStressHelper.Run(dbSet);
            });
        }

    }

}
#endif
