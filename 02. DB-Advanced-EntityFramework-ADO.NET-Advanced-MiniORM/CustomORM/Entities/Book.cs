namespace CustomORM.Entities
{
    using System;
    using Attributes;

    [Entity(TableName = "Books")]
    public class Book
    {
        [Id]
        private int id;
        
        [Column(Name = "Title")]
        private string title;

        [Column(Name = "Author")]
        private string author;

        [Column(Name = "PublishedOn")]
        private DateTime publishedOn;

        [Column(Name = "Language")]
        private string language;

        [Column(Name = "IsHardCovered")]
        private bool isHardCovered;

        [Column(Name = "Rating")]
        private decimal rating;

        public Book(string title, string author, DateTime publishedOn, string language, bool isHardCovered, decimal rating)
        {
            this.Title = title;
            this.Author = author;
            this.PublishedOn = publishedOn;
            this.Language = language;
            this.IsHardCovered = isHardCovered;
            this.Rating = rating;
        }

        public int Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        public string Title
        {
            get { return this.title; }
            set { this.title = value; }
        }

        public string Author
        {
            get { return this.author; }
            set { this.author = value; }
        }

        public DateTime PublishedOn
        {
            get { return this.publishedOn; }
            set { this.publishedOn = value; }
        }

        public string Language
        {
            get { return this.language; }
            set { this.language = value; }
        }

        public bool IsHardCovered
        {
            get { return this.isHardCovered; }
            set { this.isHardCovered = value; }
        }

        public decimal Rating
        {
            get { return this.rating; }
            set { this.rating = value; }
        }

        public override string ToString()
        {
            return $"{Id} {Title} {PublishedOn}";
        }

    }
}
