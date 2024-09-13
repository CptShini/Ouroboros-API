namespace OuroborosLibrary.SaveLoad
{
    internal static class Base64FileNameConverter
    {
        /// <summary>
        /// Encodes plaintext input into base64; taking filename friendliness into account.
        /// </summary>
        /// <param name="plainText">The plaintext to be encoded.</param>
        /// <returns>The base64 encoded input.</returns>
        internal static string Base64Encode(string plainText)
        {
            byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            string encodedText = Convert.ToBase64String(plainTextBytes);
            string fileNameFriendlyText = encodedText.Replace('/', '-');
            return fileNameFriendlyText;
        }

        /// <summary>
        /// Decodes base64 input into plaintext; taking filename friendliness into account.
        /// </summary>
        /// <param name="base64EncodedData">The base64 data to be decoded.</param>
        /// <returns>The plaintext decoded output.</returns>
        internal static string Base64Decode(string base64EncodedData)
        {
            string RevertedFileNameFriendliness = base64EncodedData.Replace('-', '/');
            byte[] base64EncodedBytes = Convert.FromBase64String(RevertedFileNameFriendliness);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}