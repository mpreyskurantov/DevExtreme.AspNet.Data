namespace DevExtreme.AspNet.Data {

    /// <summary>
    /// A class with properties that specify data processing settings.
    /// </summary>
    public class DataSourceLoadOptionsBase {
        /// <summary>
        /// A group expression.
        /// </summary>
        public GroupingInfo[] Group { get; set; }

        /// <summary>
        /// A flag that indicates whether the LINQ provider should execute grouping.
        /// If set to false, the entire dataset is loaded and grouped in memory.
        /// </summary>
        public bool? RemoteGrouping { get; set; }

        public bool AllowAsyncOverSync { get; set; }
    }

}
