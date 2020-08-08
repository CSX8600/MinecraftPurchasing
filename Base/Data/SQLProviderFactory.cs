using ClussPro.Base.Data.Query;
using System;
using System.Linq;
using System.Reflection;

namespace ClussPro.Base.Data
{
    public class SQLProviderFactory
    {
        private ISQLProvider _provider;

        private static SQLProviderFactory _instance;
        private static SQLProviderFactory GetInstance()
        {
            if (_instance == null)
            {
                _instance = new SQLProviderFactory();
            }

            return _instance;
        }

        private SQLProviderFactory()
        {
            Assembly providerAssembly = Assembly.LoadFrom(BaseConfig.SQLProviderPath);

            Type sqlProviderType = typeof(ISQLProvider);
            Type sqlProvider = providerAssembly.GetTypes().FirstOrDefault(t => t != sqlProviderType && sqlProviderType.IsAssignableFrom(t));
            if (sqlProvider == null)
            {
                throw new EntryPointNotFoundException("Could not find a SQL Provider in the specified assembly");
            }

            _provider = (ISQLProvider)Activator.CreateInstance(sqlProvider);
        }

        public static ISelectQuery GetSelectQuery()
        {
            return GetInstance()._provider.GetSelectQuery();
        }

        public static IUpdateQuery GetUpdateQuery()
        {
            return GetInstance()._provider.GetUpdateQuery();
        }

        public static IDeleteQuery GetDeleteQuery()
        {
            return GetInstance()._provider.GetDeleteQuery();
        }

        public static IInsertQuery GetInsertQuery()
        {
            return GetInstance()._provider.GetInsertQuery();
        }

        public static ITransaction GenerateTransaction()
        {
            return GetInstance()._provider.GenerateTransaction();
        }

        public static ICreateSchema GetCreateSchemaQuery()
        {
            return GetInstance()._provider.GetCreateSchemaQuery();
        }

        public static ICreateTable GetCreateTableQuery()
        {
            return GetInstance()._provider.GetCreateTableQuery();
        }

        public static IAlterTable GetAlterTableQuery()
        {
            return GetInstance()._provider.GetAlterTableQuery();
        }

        public static IDropSchema GetDropSchemaQuery()
        {
            return GetInstance()._provider.GetDropSchemaQuery();
        }

        public static IDropTable GetDropTableQuery()
        {
            return GetInstance()._provider.GetDropTableQuery();
        }
    }
}
