namespace CustomORM.Core
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Reflection;
    using System.Text;
    using Attributes;

    public class EntityManager : DbContext
    {
        private SqlConnection connection;
        private string connectionString;
        private bool isCodeFirst;
        //private ISet<object> persistedEntities;
        //private IDictionary<Type, IDictionary<long, object>> objectsByTableAndId; //TODO optimize fetching objects

        public EntityManager(string connectionString, bool isCodeFirst)
        {
            this.connectionString = connectionString;
            this.isCodeFirst = isCodeFirst;
            //this.persistedEntities = new HashSet<object>();
            //this.SyncSetWithDb();
        }

        public bool Persist(object entity)
        {
            if (entity == null)
            {
                return false;
            }

            if (isCodeFirst && !CheckIfTableExists(entity.GetType()))
            {
                this.CreateTable(entity.GetType());
            }

            FieldInfo primary = this.GetId(entity.GetType());
            object value = primary.GetValue(entity);

            if (value == null || (int)value <= 0)
            {
                return this.Insert(entity, primary);
            }

            return this.Update(entity, primary);
        }

        public T FindById<T>(int id)
        {
            T result = default(T);

            using (this.connection = new SqlConnection(this.connectionString))
            {
                SqlCommand getEntityCommand = new SqlCommand(
                    $"SELECT * FROM {this.GetTableName(typeof(T))} WHERE Id = @Id", 
                    connection);
                getEntityCommand.Parameters.AddWithValue("@Id", id);

                connection.Open();
                using (SqlDataReader reader = getEntityCommand.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        throw new InvalidOperationException("No enitty was found with id " + id);
                    }

                    reader.Read();
                    result = CreateEntity<T>(reader);
                }
            }

            return result;
        }

        public IEnumerable<T> FindAll<T>(string where)
        {
            StringBuilder cmdSQL = new StringBuilder($"SELECT * FROM {this.GetTableName(typeof(T))} ");
            if (where != null)
            {
                cmdSQL.Append($" WHERE {where.Trim()}");
            }
            List<T> result = new List<T>();

            using (this.connection = new SqlConnection(this.connectionString))
            {
                SqlCommand cmd = new SqlCommand(cmdSQL.ToString(), this.connection);
                connection.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        T entity = CreateEntity<T>(reader);
                        result.Add(entity);
                    }
                }
            }

            return result;
        }

        public IEnumerable<T> FindAll<T>()
        {
            return this.FindAll<T>(null);
        }

        public T FindFirst<T>()
        {
            return this.FindFirst<T>(null);
        }

        public T FindFirst<T>(string where)
        {

            string query = $"SELECT TOP 1 * FROM {this.GetTableName(typeof(T))}";
            if (where != null)
            {
                query += $" WHERE {where}";
            }
            T result = default(T);

            using (this.connection = new SqlConnection(this.connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, this.connection);
                connection.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result = CreateEntity<T>(reader);
                    }
                }
            }

            return result;
        }

        private T CreateEntity<T>(SqlDataReader reader)
        {
            //Retrieve data from the current row of the reader
            object[] originalValues = new object[reader.FieldCount];
            reader.GetValues(originalValues);

            //Prepare arrays of types and values required for the constructor of the object
            object[] values = new object[reader.FieldCount - 1];
            Array.Copy(originalValues, 1, values, 0, reader.FieldCount - 1);
            Type[] types = new Type[values.Length];
            for (int i = 0; i < types.Length; i++)
            {
                types[i] = values[i].GetType();
            }

            //Create new instance of the object and set all its fields with values form DB
            T entity = (T)typeof(T).GetConstructor(types).Invoke(values);
            typeof(T).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(x => x.IsDefined(typeof(IdAttribute)))
                .SetValue(entity, originalValues[0]);

            return entity;
        }

        private bool Insert<T>(T entity, FieldInfo primary)
        {
            string insertSQL = PrepareInsertStatement(entity, primary);
            int rowsAffected = 0;
            using (this.connection = new SqlConnection(this.connectionString))
            {
                SqlCommand insertCommand = new SqlCommand(insertSQL, this.connection);
                connection.Open();
                rowsAffected = insertCommand.ExecuteNonQuery();
            }

            //Set ID of the newly created object
            using (this.connection = new SqlConnection(this.connectionString))
            {
                SqlCommand newIdCommand = new SqlCommand(
                    $"SELECT Max(Id) from {this.GetTableName(entity.GetType())}",
                    this.connection);
                connection.Open();
                int id = (int)newIdCommand.ExecuteScalar();
                this.GetId(entity.GetType()).SetValue(entity, id);
                //this.SyncSetWithDb();
                return rowsAffected > 0;
            }
        }

        private string PrepareInsertStatement<T>(T entity, FieldInfo primary)
        {
            StringBuilder columnNames = new StringBuilder();
            StringBuilder values = new StringBuilder();
            FieldInfo[] fields = entity.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                if (field == primary)
                {
                    continue;
                }

                columnNames.Append($"[{this.GetFieldName(field)}], ");

                if (field.FieldType == typeof(DateTime))
                {
                    DateTime date = (DateTime)field.GetValue(entity);
                    values.Append($"'{date.ToString("yyyy-MM-dd HH:mm:ss")}', ");
                }
                else
                {
                    values.Append($"'{field.GetValue(entity).ToString()}', ");
                }
            }

            columnNames.Remove(columnNames.Length - 2, 2);
            values.Remove(values.Length - 2, 2);

            string insertSQL = $"INSERT INTO {this.GetTableName(entity.GetType())} " +
                                $"({columnNames})" +
                                $" VALUES ({values})";
            return insertSQL;
        }

        private bool Update<T>(T entity, FieldInfo primary)
        {
            string updateSQL = PrepareUpdateStatement(entity, primary);
           
            using (this.connection = new SqlConnection(this.connectionString))
            {
                SqlCommand updateCommand = new SqlCommand(updateSQL, this.connection);
                connection.Open();
                int rowsAffected = updateCommand.ExecuteNonQuery();
                //this.SyncSetWithDb();
                return rowsAffected > 0;
            }  
        }

        private string PrepareUpdateStatement<T>(T entity, FieldInfo primary)
        {
            FieldInfo[] fields = entity.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

            StringBuilder query = new StringBuilder("UPDATE " + this.GetTableName(entity.GetType()) + " SET ");
            StringBuilder where = new StringBuilder(" WHERE ");
            foreach (var field in fields)
            {
                if (field == primary)
                {
                    where.Append($"[{this.GetFieldName(field)}] = ");
                    where.Append($"'{field.GetValue(entity).ToString()}'");
                    continue;
                }

                if (field.GetValue(entity) != null)
                {
                    query.Append($"[{this.GetFieldName(field)}] = ");
                    string currentValue = string.Empty;
                    if (field.FieldType == typeof(DateTime))
                    {
                        DateTime date = (DateTime)field.GetValue(entity);
                        currentValue += date.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                    {
                        currentValue = field.GetValue(entity).ToString();
                    }
                    
                    query.Append($"'{currentValue}', ");
                }
            }

            query.Remove(query.Length - 2, 2);
            query.Append(where);
            return query.ToString();
        }

        private FieldInfo GetId(Type entity)
        {
            var field = entity.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(x => x.IsDefined(typeof(IdAttribute)))
                .FirstOrDefault();
            if(field == null)
            {
                throw new InvalidOperationException("Cannot operate with entity without primary key");
            }
            return field;
        }

        private string GetTableName(Type entity)
        {
            if (!entity.IsDefined(typeof(EntityAttribute)))
            {
                throw new InvalidOperationException("Cannot get table name for entity that has no Entity attribute");
            }

            string tableName = entity.GetCustomAttribute<EntityAttribute>().TableName;
            if(tableName != string.Empty)
            {
                return entity.GetCustomAttribute<EntityAttribute>().TableName;
            }

            return entity.Name;
        }

        private string GetFieldName(FieldInfo field)
        {
            if(!field.IsDefined(typeof(ColumnAttribute)) && !field.IsDefined(typeof(IdAttribute)))
            {
                throw new InvalidOperationException("Cannot get name of field that is not marked with Column or Id attribute");
            }

            if (field.IsDefined(typeof(ColumnAttribute)))
            {
                string fieldName = field.GetCustomAttribute<ColumnAttribute>().Name;
                if(fieldName != string.Empty)
                {
                    return fieldName;
                }

                return field.Name;
            }

            return field.Name;
        }
        
        private void CreateTable(Type table)
        {
            string createSQL = this.PrepareCreateTableStatement(table);
            
            using (this.connection = new SqlConnection(this.connectionString))
            {
                SqlCommand createCommand = new SqlCommand(createSQL, this.connection);
                this.connection.Open();
                createCommand.ExecuteNonQuery();
            }
        }

        private string PrepareCreateTableStatement(Type table)
        {
            StringBuilder createSQL = new StringBuilder($"CREATE TABLE {this.GetTableName(table)} (");
            createSQL.Append("Id INT IDENTITY PRIMARY KEY, ");

            string[] columnNames = table.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(f => f.IsDefined(typeof(ColumnAttribute)))
                .Select(f => this.GetFieldName(f))
                .ToArray();

            FieldInfo[] fields = table.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(f => f.IsDefined(typeof(ColumnAttribute)))
                .ToArray();

            for (int i = 0; i < fields.Length; i++)
            {
                createSQL.Append($"{columnNames[i]} {this.ToDbType(fields[i].FieldType)}, ");
            }

            createSQL.Remove(createSQL.Length - 2, 2);
            createSQL.Append(")");
            return createSQL.ToString();
        }

        private bool CheckIfTableExists(Type table)
        {
            using (this.connection = new SqlConnection(this.connectionString))
            {
                SqlCommand tablesCountCommand = new SqlCommand(
                    $"SELECT COUNT(name) FROM sys.sysobjects WHERE [Name] = '{this.GetTableName(table)}' AND [xtype] = 'U'",
                    this.connection);

                this.connection.Open();
                int tablesCount = (int)tablesCountCommand.ExecuteScalar();
                return tablesCount > 0;
            }
        }

        private string ToDbType(Type fieldType)
        {
            switch (fieldType.Name)
            {
                case "Int32": return "INT";
                case "String": return "VARCHAR(50)";
                case "DateTime": return "DATETIME";
                case "Boolean": return "BIT";
                case "Decimal": return "DECIMAL(18,2)";
                default:
                    throw new InvalidOperationException();
            }
        }

        public void Delete<T>(object entity)
        {
            FieldInfo primary = this.GetId(entity.GetType());
            int entityId = (int)primary.GetValue(entity);
            DeleteById<T>(entityId);
        }

        public void DeleteById<T>(int id)
        {
            string deleteSql = $"DELETE FROM {this.GetTableName(typeof(T))} WHERE Id = {id}";
            using (this.connection = new SqlConnection(this.connectionString))
            {
                SqlCommand deleteCommand = new SqlCommand(deleteSql, this.connection);
                this.connection.Open();
                deleteCommand.ExecuteNonQuery();
            }
            
        }

        //private void SyncSetWithDb()
        //{
        //Type[] entityTypes = Assembly.GetExecutingAssembly()
        //    .GetTypes()
        //    .Where(t => t.IsDefined(typeof(EntityAttribute)))
        //    .ToArray();
        //foreach (var type in entityTypes)
        //{
        //    var elements = Find<Book>(); //??? generic trouble?
        //    foreach (var element in elements)
        //    {
        //        persistedEntities.Add(element);
        //    }
        //}



        //TODO: SELECT all records from all tables and enter them in the hashset
        ////TODO Should be better way to get
        //foreach (var item in Find<Book>())
        //{
        //    persistedEntities.Add(item);
        //}
        //foreach (var item in Find<User>())
        //{
        //    persistedEntities.Add(item);
        //}
        // }
    }
}

