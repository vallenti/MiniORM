namespace CustomORM.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    class EntityAttribute : Attribute
    {
        public EntityAttribute() { }
  
        public string TableName { get; set; }
    }
}
