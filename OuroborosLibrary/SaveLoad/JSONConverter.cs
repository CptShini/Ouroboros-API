using Newtonsoft.Json;

namespace OuroborosLibrary.SaveLoad
{
    public static class JSONConverter
    {
        private static readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };

        /// <summary>
        /// Converts given object of type T into a JSON string.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="contents">The object to convert to a JSON string.</param>
        /// <returns>The converted object as a string.</returns>
        public static string SerializeToJSON<T>(T contents) => JsonConvert.SerializeObject(contents, _serializerSettings);

        /// <summary>
        /// Converts a JSON formatted string into an object of type T.
        /// </summary>
        /// <typeparam name="T">The type of object to parse the string into.</typeparam>
        /// <param name="contents">The JSON formatted string to be converted into object of type T.</param>
        /// <returns>An object of type T.</returns>
        public static T DeserializeString<T>(string contents) => JsonConvert.DeserializeObject<T>(contents, _serializerSettings)!;
    }
}