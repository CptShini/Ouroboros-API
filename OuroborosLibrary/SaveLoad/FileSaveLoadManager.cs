namespace OuroborosLibrary.SaveLoad
{
    internal static class FileSaveLoadManager
    {
        /// <summary>
        /// Saves a given string at a given path.
        /// </summary>
        /// <param name="path">The full path at which to save the contents.</param>
        /// <param name="contents">The actual string contents to be saved.</param>
        internal static void SaveFile(string path, string contents)
        {
            using StreamWriter sw = File.CreateText(path);
            sw.WriteLine(contents);
        }

        /// <summary>
        /// Loads data from a text file at a given path.
        /// </summary>
        /// <param name="path">The full path from which to load the contents.</param>
        /// <param name="contents">The data loaded from the file.</param>
        /// <returns>True, if the file at the given path was found. Otherwise false.</returns>
        internal static bool LoadFile(string path, out string contents)
        {
            bool fileExists = File.Exists(path);

            contents = fileExists ? File.ReadAllText(path) : "";

            return fileExists;
        }

        /// <summary>
        /// Deletes the file at the given path.
        /// </summary>
        /// <param name="path">The full path from which to delete a file.</param>
        internal static void DeleteFile(string path) => File.Delete(path);
    }
}