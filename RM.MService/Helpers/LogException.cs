namespace RM.MService.Helpers
{
    public class LogException
    {
        public static void LogExceptionToFile(Exception ex)
        {
            string logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "error_log.txt");

            // Create or append to the log file
            using StreamWriter writer = File.AppendText(logFilePath);
            writer.WriteLine($"[Error Occurred at {DateTime.UtcNow}]");
            writer.WriteLine($"Message: {ex.Message}");
            writer.WriteLine($"Stack Trace: {ex.StackTrace}");
            writer.WriteLine(new string('-', 50)); // Separator
        }
    }
}
