namespace RM.CommonServices.Helpers
{
    public static class FileHelper
    {
        public static string ReadFile(string fileNameWithPath)
        {
            try
            {
                if (File.Exists(fileNameWithPath))
                {
                    string text = File.ReadAllText(fileNameWithPath);
                    return text;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static void WriteToFile(string fileName, string text)
        {
            try
            {
                using StreamWriter sw = File.AppendText(fileName + ".txt");
                sw.WriteLine(text);
            }
            catch (Exception)
            {
            }
        }

        public static string ReadFileAndEncodeToBase64(string path)
        {
            try
            {
                byte[] fileBytes = File.ReadAllBytes(path);
                string encodedFile = Convert.ToBase64String(fileBytes);
                return encodedFile;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static void ReplaceFileContent(string filePath, string content)
        {
            try
            {
                // Check if the file exists
                if (File.Exists(filePath))
                {
                    // File exists, overwrite the content
                    using (StreamWriter sw = new StreamWriter(filePath, false)) // 'false' indicates overwrite mode
                    {
                        sw.WriteLine(content);
                    }
                }
                else
                {
                    // File doesn't exist, create it and write content
                    using (StreamWriter sw = new StreamWriter(filePath)) // Automatically creates the file if it doesn't exist
                    {
                        sw.WriteLine(content);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., file path issues, permissions)
                Console.WriteLine($"An error occurred while writing to the file: {ex.Message}");
            }
        }
    }
}
