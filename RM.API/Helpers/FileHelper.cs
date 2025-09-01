using System;
using System.IO;

namespace RM.CommonServices.Helpers
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
    }
}
