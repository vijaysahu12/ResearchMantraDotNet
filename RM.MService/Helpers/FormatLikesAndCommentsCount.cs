namespace RM.MobileAPI.Helpers
{
    public class FormatLikesAndCommentsCount
    {
        public static string FormatCount(long count)
        {
            if (count < 1000)
            {
                return count.ToString();
            }
            else if (count < 1000000)
            {
                return $"{count / 1000.0:F1}K";
            }
            else
            {
                return $"{count / 1000000.0:F1}M";
            }
        }
    }
}
