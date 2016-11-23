namespace CustomORM
{
    using System;
    using Core;
    using DBConnection;
    using Entities;

    class TestORM
    {
        static void Main()
        {
            //Setup db connection and entity manager
            ConnectionStringBuilder db = new ConnectionStringBuilder("MinionsDB");
            DbContext em = new EntityManager(db.ConnectionString, true);

            //Crate new entity book
            Book book = new Book("Harry Potter","JK Rowling", new DateTime(1997, 06, 26), "English", true, 1.0m);
            Book book2 = new Book("Harry Potter 2", "JK Rowling", new DateTime(1998, 06, 26), "Bulgarian", false, 1.9m);
            Book book3 = new Book("Harry Potter 3", "JK Rowling", new DateTime(1999, 06, 26), "English", true, 9.2m);
            em.Persist(book);
            em.Persist(book2);
            em.Persist(book3);

            //Get list of all books
            var books = em.FindAll<Book>();
            Console.WriteLine("--All books--");
            Console.WriteLine(string.Join("\n", books));

            //get book by id
            var bookId3 = em.FindById<Book>(3);

            //update properites of that book
            bookId3.Title = "Harry Potter and the Prisoner of Azkaban";
            bookId3.PublishedOn = new DateTime(1999, 8, 7);
            em.Persist(bookId3);

            //get list of all books again
            Console.WriteLine("--Updated All books--");
            var updatedBooks = em.FindAll<Book>();
            Console.WriteLine(string.Join("\n", updatedBooks));

            //Get list of entities by condition LEN(Title) > 20
            var booksFiltered = em.FindAll<Book>("LEN(Title) > 20");
            Console.WriteLine("--Books filtered--");
            Console.WriteLine(string.Join("\n", booksFiltered));

            var user = new User("pesho", "1234", 20, new DateTime(2016, 10, 10), DateTime.Now, true);
            em.Persist(user);

            Console.WriteLine("-----------------");
            Console.WriteLine("BEFORE: "+em.FindById<User>(1));
            user.Age = 25;
            em.Persist(user);
            Console.WriteLine("AFTER: "+em.FindById<User>(1));
        }
    }
}