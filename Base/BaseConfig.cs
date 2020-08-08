using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ClussPro.Base
{
    internal class BaseConfig
    {
        private static bool _forceLoad = true;

        private static string _sqlProviderPath;
        internal static string SQLProviderPath
        {
            get { Load(); return _sqlProviderPath; }
        }

        private static void Load()
        {
            if (!_forceLoad)
            {
                return;
            }

            _forceLoad = false;

            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            if (!File.Exists("BaseConfig.xml"))
            {
                using (FileStream stream = new FileStream("BaseConfig.xml", FileMode.Create))
                {
                    serializer.Serialize(stream, new Config());
                    stream.Flush();
                }

                _forceLoad = true;
                throw new FileNotFoundException("BaseConfig.xml was not found.  A new one has been created - enter required configuration and relaunch");
            }

            Config config;
            using (FileStream stream = new FileStream("BaseConfig.xml", FileMode.Open))
            {
                config = (Config)serializer.Deserialize(stream);
            }

            _sqlProviderPath = config.SQLProvider;
        }
    }

    [XmlRoot(ElementName = "BaseConfig")]
    public class Config
    {
        public string SQLProvider { get; set; } = "";
    }
}
