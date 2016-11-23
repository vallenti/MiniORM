namespace CustomORM.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    class IdAttribute : Attribute
    {
    }
}
