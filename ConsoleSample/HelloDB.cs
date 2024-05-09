using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NuoDb.Data.Client;
using System.Data.Common;

namespace ConsoleSample
{
    class HelloDB : IDisposable
    {
        private NuoDbConnection connection;

        /*
         * Creates an instance of HelloDB connected to the database with
         * the given name on the localhost. This example class uses the
         * default testing name & password.
         */
        public HelloDB(string dbName)
        {
            NuoDbConnectionStringBuilder builder = new NuoDbConnectionStringBuilder();
            builder.Server = "localhost";
            builder.Database = dbName;
            builder.User = "dba";
            builder.Password = "dba";
            builder.Schema = "hello";

            this.connection = new NuoDbConnection(builder.ConnectionString);
            this.connection.Open();
        }


        #region IDisposable Members

        public void Dispose()
        {
            if (this.connection != null)
                this.connection.Close();
        }

        #endregion

        public void CreateTable()
        {
            try
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "create table names (id bigint generated always as identity primary key, name string)";
                    command.ExecuteNonQuery();
                    Console.Out.WriteLine("The table 'NAMES' was created.");
                }
            }
            catch (NuoDbSqlException)
            {
                Console.Out.WriteLine("The table 'NAMES' already exists, re-using it.");
            }
        }

        public void AddNames()
        {
            try
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "insert into names (name) values (?),(?),(?),(?),(?),(?),(?),(?),(?),(?),(?),(?),(?),(?),(?)";
                    command.Prepare();

                    /* batch insert a set of names */
                    for (int i = 0; i < 15; i++) 
                    {
                        string name = String.Format("Fred # {0}", i+1);
                        command.Parameters[i].Value = name;
                    }
                    // This is how to retrieve the generated key. Even if it's an update statement, we request the DbReader, that will contain 
                    // only the generated column value
                    using (DbDataReader reader = command.ExecuteReader())
                    {
                        string columnName = reader.GetName(0);
                        while (reader.Read())
                        {
                            Console.Out.WriteLine("New id={0} for column {1}", reader.GetInt64(0), columnName);
                        }
                    }
                }
            }
            catch (NuoDbSqlException e)
            {
                throw e;
            }

        }

        public string GetName(int id)
        {
            try
            {
                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText = "select name from names where id=?";
                    command.Parameters.Add(id);

                    using (DbDataReader reader = command.ExecuteReader())
                    {
                        if(reader.Read())
                            return reader.GetString(0);
                    }
                }
                return null;
            }
            catch (NuoDbSqlException e)
            {
                throw e;
            }

        }
    }
}
