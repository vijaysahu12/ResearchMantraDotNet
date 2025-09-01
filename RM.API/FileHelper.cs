using System.IO;

namespace RM.API
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
            catch (System.Exception)
            {
            }
        }
    }
}
