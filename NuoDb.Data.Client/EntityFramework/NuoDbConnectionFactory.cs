/****************************************************************************
	Author: Jiri Cincura (jiri@cincura.net)
****************************************************************************/

using System;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace NuoDb.Data.Client.EntityFramework
{
	public class NuoDbConnectionFactory : IDbConnectionFactory
	{
		public DbConnection CreateConnection(string nameOrConnectionString)
		{
			if (nameOrConnectionString == null)
				throw new ArgumentNullException("nameOrConnectionString cannot be null.");

			if (nameOrConnectionString.Contains('='))
			{
				return new NuoDbConnection(nameOrConnectionString);
			}
			else
			{
				var configuration = ConfigurationManager.ConnectionStrings[nameOrConnectionString];
				if (configuration == null)
					throw new ArgumentException("Specified connection string name cannot be found.");
				return new NuoDbConnection(configuration.ConnectionString);
			}
		}
	}
}
