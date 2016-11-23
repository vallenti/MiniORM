namespace _12AddNewEntity
{
    using System;
    using CustomORM.Core;
    using CustomORM.DBConnection;
    using CustomORM.Entities;
    using System.Text;
    using System.Linq;
    class BookEntity
    {
        static void Main(string[] args)
        {
            ConnectionStringBuilder db = new ConnectionStringBuilder("MinionsDB");
            DbContext em = new EntityManager(db.ConnectionString, true);

            int titleLength = int.Parse(Console.ReadLine());
            var books = em.FindAll<Book>("LEN(Title) > "+titleLength).ToList();

            foreach (var book in books)
            {
                book.Title = TrimSymbols(book.Title, titleLength);
                em.Persist(book);
            }

            var booksWithTitleLength = em.FindAll<Book>("LEN(Title) = " + titleLength).ToList();
            Console.WriteLine("{0} books now have title with length of {1}", booksWithTitleLength.Count, titleLength);
        }

        private static string TrimSymbols(string text, int length)
        {
            StringBuilder trimmedWord = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                trimmedWord.Append(text[i]);
            }
            return trimmedWord.ToString();
        }
    }
}
