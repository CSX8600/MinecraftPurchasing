using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ClussPro.SqlServerProvider
{
    internal static class MSSqlServerProviderConfig
    {
        private static bool _forceLoad = true;

        private static string _connectionString;
        internal static string ConnectionString
        {
            get { Load(); return _connectionString; }
        }

        private static void Load()
        {
            if (!_forceLoad)
            {
                return;
            }

            _forceLoad = false;

            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            if (!File.Exists("MSSqlServerProviderConfig.xml"))
            {
                using (FileStream stream = new FileStream("MSSqlServerProviderConfig.xml", FileMode.Create))
                {
                    serializer.Serialize(stream, new Config());
                    stream.Flush();
                }

                _forceLoad = true;
                throw new FileNotFoundException("MSSqlServerProviderConfig.xml was not found.  A new one has been created - enter required configuration and relaunch");
            }

            Config config;
            using (FileStream stream = new FileStream("MSSqlServerProviderConfig.xml", FileMode.Open))
            {
                config = (Config)serializer.Deserialize(stream);
            }

            _connectionString = config.ConnectionString;
        }
    }

    [XmlRoot("MSSqlServerProviderConfig")]
    public class Config
    {
        public string ConnectionString { get; set; } = "";
    }
}
