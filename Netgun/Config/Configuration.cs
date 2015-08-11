using Netgun.Config.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

namespace Netgun.Config
{
    public class Configuration
    {
        private const string ConfigFile = "config.json";

        private Configuration()
        {
            RecentConnections = new List<SavedConnection>();
            SavedConnections = new List<SavedConnection>();
        }

        /// <summary>
        /// Load configuration from the default file.
        /// </summary>
        public static void Load()
        {
            if(File.Exists(ConfigFile))
            {
                using (var stream = new StreamReader(File.OpenRead(ConfigFile)))
                {
                    Current = JsonConvert.DeserializeObject<Configuration>(stream.ReadToEnd());
                }
            }
            else
            {
                Current = new Configuration();
                Save();
            }
        }

        /// <summary>
        /// Save configuration to the default file.
        /// </summary>
        public static void Save()
        {
            using (var stream = new StreamWriter(File.Open(ConfigFile, FileMode.Create)))
            {
                stream.Write(JsonConvert.SerializeObject(Current));
            }
        }

        /// <summary>
        /// Add a connection, maintaining max 5 recent.
        /// </summary>
        /// <param name="conn">Connection to be added.</param>
        public static void AddRecentConnection(SavedConnection conn)
        {
            var match = Current.RecentConnections.FirstOrDefault(c => c.ConnectionString == conn.ConnectionString && c.Name == conn.Name);

            var index = 0;
            if (match != null)
            { 
                index = Current.RecentConnections.IndexOf(match);
                Current.RecentConnections.Remove(match);
            }

            Current.RecentConnections.Insert(index, conn);
            Current.RecentConnections.Take(5).ToList();
            Save();
        }

        public static Configuration Current { get; private set; }

        public List<SavedConnection> RecentConnections { get; private set; }

        public List<SavedConnection> SavedConnections { get; private set; }
    }
}
