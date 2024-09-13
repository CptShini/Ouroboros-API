namespace OuroborosLibrary.Queries.QueryHelpers
{
    internal static class QueryStringBuilder
    {
        /// <summary>
        /// Constructs a query string with a query name, an input for it, and it's default value.
        /// </summary>
        /// <param name="inputName">The query parameter name.</param>
        /// <param name="input">The input to give to the query.</param>
        /// <param name="defaultValue">The default value of the query; if input == this value, function returns "".</param>
        /// <returns>Example: GetQueryString("countries", "dk", "") => "&countries=dk". Returns "" if input == defaultValue.</returns>
        internal static string GetQueryString(string inputName, string input, string defaultValue) => (input == defaultValue) ? "" : $"&{inputName}={input}";

        /// <summary>
        /// Constructs a query string with a query name, an input for it, and it's default value.
        /// </summary>
        /// <param name="inputName">The query parameter name.</param>
        /// <param name="input">The input to give to the query.</param>
        /// <param name="defaultValue">The default value of the query; if input == this value, function returns "".</param>
        /// <returns>Example: GetQueryString("page", 4, 0) => "&page=4". Returns "" if input == defaultValue.</returns>
        internal static string GetQueryString(string inputName, int input, int defaultValue) => (input == defaultValue) ? "" : $"&{inputName}={input}";
    }
}