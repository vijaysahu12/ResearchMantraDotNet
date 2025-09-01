namespace RM.MService.Helpers
{
    public static class FileHelper
    {
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
    }
}
