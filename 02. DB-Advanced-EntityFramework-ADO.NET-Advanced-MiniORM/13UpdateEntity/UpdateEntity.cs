using CustomORM.Core;
using CustomORM.DBConnection;
using CustomORM.Entities;
using System;
using System.Linq;

namespace _13UpdateEntity
{
    class UpdateEntity
    {
        static void Main(string[] args)
        {
            ConnectionStringBuilder db = new ConnectionStringBuilder("MinionsDB");
            DbContext em = new EntityManager(db.ConnectionString, true);

            var books = em.FindAll<Book>()
                .OrderByDescending(b => b.PublishedOn)
                .ThenBy(b => b.Rating)
                .Take(3)
                .Select(b => new
                {
                    b.Title,
                    b.Author, 
                    b.Rating
                })
                .OrderByDescending(b => b.Rating)
                .ThenBy(b => b.Title)
                .ToList();

            foreach (var book in books)
            {
                Console.WriteLine("{0} ({1}) - {2}/10", book.Title, book.Author, book.Rating);
            }
        }
    }
}
