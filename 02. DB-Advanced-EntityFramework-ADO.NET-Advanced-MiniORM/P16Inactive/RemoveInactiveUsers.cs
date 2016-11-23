namespace P16Inactive
{
    using CustomORM.Core;
    using CustomORM.DBConnection;
    using CustomORM.Entities;
    using System;

    class RemoveInactiveUsers
    {
        static void Main(string[] args)
        {
            ConnectionStringBuilder db = new ConnectionStringBuilder("MinionsDB");
            DbContext em = new EntityManager(db.ConnectionString, true);

            string username = Console.ReadLine(); 
            User user = em.FindFirst<User>(string.Format("[Username] = '{0}'", username));

            string relativeTime = GetRelativeTime(user.LastLoginTime);
            Console.WriteLine($"User {user.Username} was last online {relativeTime}");

            if (!user.IsActive)
            {
                Console.WriteLine("Would you like to delete that user? (y/n)");
                string answer = Console.ReadLine();
                if(answer.ToLower() == "y")
                {
                    em.Delete<User>(user);
                    Console.WriteLine($"User {username} was successfully deleted from the database");
                }
                else
                {
                    Console.WriteLine($"User {username} was not deleted from the database");
                }
            }
        }

        private static string GetRelativeTime(DateTime lastLoginTime)
        {
            const int Second = 1;
            const int Minute = 60 * Second;
            const int Hour = 60 * Minute;
            const int Day = 24 * Hour;
            const int Month = 30 * Day;

            var ts = DateTime.Now - lastLoginTime;
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < 1 * Second)
                return "less than a second";

            if (delta < 1 * Minute)
                return "less than a minute";

            if (delta < Hour)
                return ts.Minutes + " minutes ago";

            if (delta < Day)
                return ts.Hours + " hours ago";

            if (delta < Month)
                return ts.Days + " days ago";

            if (delta < 12 * Month)
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "one month ago" : months + " months ago";
            }
            else
            {
                return "more than a year";
            }
        }
    }
}
