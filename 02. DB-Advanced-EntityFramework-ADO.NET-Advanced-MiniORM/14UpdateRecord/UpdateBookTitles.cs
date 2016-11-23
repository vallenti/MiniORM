namespace _14UpdateRecord
{
    using System;
    using CustomORM.Core;
    using CustomORM.DBConnection;
    using CustomORM.Entities;
    class UpdateBookTitles
    {
        static void Main()
        {
            ConnectionStringBuilder db = new ConnectionStringBuilder("MinionsDB");
            DbContext em = new EntityManager(db.ConnectionString, true);

            int year = int.Parse(Console.ReadLine());
            var books = em.FindAll<Book>("YEAR(PublishedOn) > " + year);
            foreach (var book in books)
            {
                book.Title = book.Title.ToUpper();
                em.Persist(book);
            }

        }
    }
}
