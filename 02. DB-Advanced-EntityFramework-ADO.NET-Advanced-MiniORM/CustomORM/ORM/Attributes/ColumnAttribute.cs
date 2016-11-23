namespace CustomORM.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    class ColumnAttribute : Attribute
    {
        public ColumnAttribute() { }

        public string Name { get; set; }
    }
}
