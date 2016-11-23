using CustomORM.Core;
using CustomORM.DBConnection;
using CustomORM.Entities;
using System;
using System.Linq;

namespace P15DeleteRec
{
    class DeleteRecord
    {
        static void Main(string[] args)
        {
            ConnectionStringBuilder db = new ConnectionStringBuilder("MinionsDB");
            DbContext em = new EntityManager(db.ConnectionString, true);

            var books = em.FindAll<Book>("[Rating] < 2").ToList();
            foreach (var book in books)
            {
                em.Delete<Book>(book);
            }

            Console.WriteLine("{0} books has been deleted from the database", books.Count);
        }
    }
}
