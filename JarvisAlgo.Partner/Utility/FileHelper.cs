
using System;
using System.IO;

namespace JarvisAlgo.Partner.Utility
{
    public static class FileHelper
    {
        public static string fileName = "liveTrading";
        private static string fileExtension = ".csv";
        public static void WriteToFile(string fileName, string text)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(fileName + ".txt"))
                {
                    sw.WriteLine(text);
                }
            }
            catch (Exception)
            {
            }
        }

        public static void WriteToFile(string filePath, string fileName, string content)
        {
            try
            {

                using (StreamWriter sw = File.AppendText(filePath))
                {
                    sw.WriteLine(content);
                }
            }
            catch (Exception)
            {
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

        public static void ReplaceTheFileContent(string fileName, string newText)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    // Read the existing content
                    string existingContent = File.ReadAllText(fileName);
                    // Replace the existing content with the new text
                    existingContent = newText;
                    // Write the modified content back to the file
                    File.WriteAllText(fileName, existingContent);
                }
                else
                {
                    using StreamWriter sw = File.AppendText(fileName);
                    sw.WriteLine(newText);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
            }
        }
        public static int ReadFile(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    string text = File.ReadAllText(fileName);
                    return Convert.ToInt32(text);
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception e)
            {
                return 0;
            }
        }

    }
}
