namespace RM.CommonService
{
    public class GetRelativeTimeSincePost
    {
        public static string GetRelativeTimeSincePosted(DateTime postTime)
        {
            var now = DateTime.UtcNow;
            var timespan = now - postTime;

            int seconds = (int)timespan.TotalSeconds;
            int minutes = (int)Math.Round(seconds / 60.0);
            int hours = (int)Math.Round(minutes / 60.0);
            int days = (int)Math.Round(hours / 24.0);
            int weeks = (int)Math.Round(days / 7.0);
            int months = (int)Math.Round(days / 30.0);
            int years = (int)Math.Round(days / 365.0);

            if (seconds < 60)
            {
                return minutes > 1 ? $"{minutes} minutes " : "just now";
            }
            else if (minutes < 60)
            {
                return $"{minutes} minute{(minutes > 1 ? "s" : "")} ago";
            }
            else if (hours < 24)
            {
                return $"{hours} hour{(hours > 1 ? "s" : "")} ago";
            }
            else if (days < 7)
            {
                return $"{days} day{(days > 1 ? "s" : "")} ago";
            }
            else if (weeks < 5)
            {
                return $"{weeks} week{(weeks > 1 ? "s" : "")} ago";
            }
            else if (months < 12)
            {
                return $"{months} month{(months > 1 ? "s" : "")} ago";
            }
            else
            {
                return $"{years} year{(years > 1 ? "s" : "")} ago";

            }
        }

        public static string GetRelativeTimeSincePostedForCommentsAndReply(DateTime postTime)
        {
            var now = DateTime.UtcNow; // Use UTC time for consistency
            var timespan = now - postTime;

            int seconds = (int)timespan.TotalSeconds;
            int minutes = (int)Math.Round(seconds / 60.0);
            int hours = (int)Math.Round(minutes / 60.0);
            int days = (int)Math.Round(hours / 24.0);
            int weeks = (int)Math.Round(days / 7.0);
            int months = (int)Math.Round(days / 30.0);
            int years = (int)Math.Round(days / 365.0);

            if (seconds < 60)
            {
                return minutes > 1 ? $"{minutes} minutes " : $"{seconds}s";
            }
            else if (minutes < 60)
            {
                return $"{minutes}m";
            }
            else if (hours < 24)
            {
                return $"{hours}h";
            }
            else if (days < 7)
            {
                return $"{days}d";
            }
            else if (weeks < 5)
            {
                return $"{weeks}w";
            }
            else if (months < 12)
            {
                return $"{months}m";
            }
            else
            {
                return $"{years}y";
            }

        }

        private static double RoundToNearestHalf(double value)
        {
            return Math.Round(value * 2) / 2;
        }
    }
}
