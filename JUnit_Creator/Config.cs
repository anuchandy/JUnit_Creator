using Newtonsoft.Json;
using System;
using System.IO;

namespace JUnit_Creator
{
    public class Config
    {
        [JsonProperty(PropertyName = "dB")]
        public DBConfig DB { get; set; }

        [JsonProperty(PropertyName = "github")]
        public GithubConfig Github { get; set; }

        public static Config Create()
        {
            string configFilePath = Environment.GetEnvironmentVariable("A01_JUNIT_CONFIG");
            if (string.IsNullOrEmpty(configFilePath))
            {
                throw new ArgumentException("The environment variable A01_JUNIT_CONFIG is not set.");
            }
            //
            Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configFilePath));
            if (config == null)
            {
                throw new ArgumentException($"Invalid config fie {configFilePath} (is empty).");
            }
            if (config.DB == null)
            {
                throw new ArgumentException($"'dB' is not set in config fie {configFilePath}.");
            }
            if (string.IsNullOrEmpty(config.DB.Host) 
                || string.IsNullOrEmpty(config.DB.Database) 
                || string.IsNullOrEmpty(config.DB.UserName)
                || string.IsNullOrEmpty(config.DB.Password))
            {
                throw new ArgumentException($"'dB.host, dB.database, dB.userName and dB.password' must be set in config fie {configFilePath}.");
            }
            if (config.Github == null)
            {
                throw new ArgumentException($"Invalid config fie {configFilePath} (is empty).");
            }
            if (string.IsNullOrEmpty(config.Github.RepoLocalPath) || !Directory.Exists(config.Github.RepoLocalPath))
            {
                throw new ArgumentException($"'github.repoLocalPath must be set in config fie {configFilePath} and it must point to a existing directory.");
            }
            return config;
        }

        public class DBConfig
        {
            [JsonProperty(PropertyName = "host")]
            public string Host { get; set; }
            [JsonProperty(PropertyName = "database")]
            public string Database { get; set; }
            [JsonProperty(PropertyName = "userName")]
            public string UserName { get; set; }
            [JsonProperty(PropertyName = "password")]
            public string Password { get; set; }
        }

        public class GithubConfig
        {
            [JsonProperty(PropertyName = "repoLocalPath")]
            public string RepoLocalPath { get; set; }
        }
    }

    
}
