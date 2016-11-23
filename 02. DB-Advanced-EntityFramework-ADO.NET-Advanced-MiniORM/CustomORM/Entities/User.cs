namespace CustomORM.Entities
{
    using Attributes;
    using System;

    [Entity(TableName = "Users")]
    public class User
    {
        [Id]
        private int id;

        [Column(Name = "Username")]
        private string username;

        [Column(Name = "Password")]
        private string password;

        [Column(Name = "Age")]
        private int age;

        [Column(Name = "RegistrationDate")]
        private DateTime registrationDate;

        [Column(Name = "LastLoginTime")]
        private DateTime lastLoginTime;

        [Column(Name = "IsActive")]
        private bool isActive;

        public User(string username, string password, int age, DateTime registrationDate, DateTime lastLoginTime, bool isActive)
        {
            this.Username = username;
            this.Password = password;
            this.Age = age;
            this.RegistrationDate = registrationDate;
            this.LastLoginTime = lastLoginTime;
            this.IsActive = isActive;
        }

        public int Id
        {
            get{ return id; }
            set{ this.id = value; }
        }

        public string Username
        {
            get { return username; }
            set { this.username = value; }
        }

        public string Password
        {
            get { return password; }
            set { this.password = value; }
        }

        public int Age
        {
            get { return age; }
            set { this.age = value; }
        }

        public DateTime RegistrationDate
        {
            get { return this.registrationDate; }
            set { this.registrationDate = value; }
        }

        public DateTime LastLoginTime
        {
            get { return this.lastLoginTime; }
            set { this.lastLoginTime = value; }
        }

        public bool IsActive
        {
            get { return this.isActive; }
            set { this.isActive = value; }
        }

        public override string ToString()
        {
            return $"{Id} {Username} {Password} {Age} {RegistrationDate}";
        }
    }
}
