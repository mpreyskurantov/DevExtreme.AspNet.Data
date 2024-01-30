using Newtonsoft.Json;

using System;

namespace DevExtreme.AspNet.Data.Helpers {

    /// <summary>
    /// A parser for the data processing settings.
    /// </summary>
    public static class DataSourceLoadOptionsParser {
        public const string
            KEY_GROUP = "group";

        /// <summary>
        /// Converts the string representations of the data processing settings to equivalent values of appropriate types.
        /// </summary>
        /// <param name="loadOptions">An object that will contain converted values.</param>
        /// <param name="valueSource">A function that accepts names of the data source options (such as "filter", "sort", etc.) and returns corresponding values.</param>
        public static void Parse(DataSourceLoadOptionsBase loadOptions, Func<string, string> valueSource) {
            var group = valueSource(KEY_GROUP);

            if(!String.IsNullOrEmpty(group))
                loadOptions.Group = JsonConvert.DeserializeObject<GroupingInfo[]>(group);
        }
    }

}
