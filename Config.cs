using Microsoft.Extensions.Configuration;
using MyCouch;
using System;
using System.IO;

namespace Authentication
{
    public static class Config
    {
        public static string DateFormat { get;  }
        public static string CouchDbUsername { get; }
        public static string CouchDbPassword { get; }
        public static string CouchDbAddress { get;  }
        public static string UserDbName { get; }
        public static MyCouchClient UserDBClient { get; }

        /// <summary>
        /// Get configuarion variables for the authentication service
        /// These are configured in the appsettings.json file of the main project
        /// </summary>
        static Config()
        {
            string environmentPath = Path.Combine(Directory.GetCurrentDirectory(), $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json");
            string genericPath = Path.Combine(Directory.GetCurrentDirectory(), $"appsettings.json");

            ConfigurationBuilder configBuilder = new ConfigurationBuilder();

            configBuilder.AddJsonFile(environmentPath, false);
            configBuilder.AddJsonFile(genericPath, false);
            IConfigurationRoot config = configBuilder.Build();
            DateFormat = config.GetSection("DateFormat").Value;

            /*
             *  The address should be changed before deployment, harder to break into if you dont know where to look.
            */
            CouchDbAddress = config.GetConnectionString("CouchDbAddress");
            UserDbName = config.GetSection("UserDbName").Value;

            /* 
              * These should be changed before deployment. 
              *  It's generally considered to be a bad idea to have your authentication user credentials available on a public repo.
              *  Can't think why...
            */
            CouchDbUsername = config.GetSection("Credentials").GetSection("CouchDbUsername").Value;
            CouchDbPassword = config.GetSection("Credentials").GetSection("CouchDbPassword").Value;

            /*
              * We access the connection trhough here to avoid overloading the server by having too many concurrent connections
              * if we were to use a new connection for each request it takes up rescources on the server we're requesting from
            */
            DbConnectionInfo connection = new DbConnectionInfo(CouchDbAddress, UserDbName)
            {
                BasicAuth = new MyCouch.Net.BasicAuthString(CouchDbUsername, CouchDbPassword)
            };

            UserDBClient = new MyCouchClient(connection);
        }
    }
}
