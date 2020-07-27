using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using vgy.me.Models;

namespace vgy.me
{
    public class ConfigurationService
    {
        public string Userkey
        {
            get => _configuration.Userkey;
            set
            {
                _configuration.Userkey = value;
                SaveConfiguration();
            }
        }

        public List<UploadedFile> UploadedFiles => _configuration.UploadedFiles;

        private readonly string _configFilePath;
        private readonly Configuration _configuration;

        public ConfigurationService()
        {
            _configFilePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".vgy.me.json");

            _configuration = File.Exists(_configFilePath)
                ? JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(_configFilePath))
                : new Configuration {UploadedFiles = new List<UploadedFile>()};
        }

        public void AddUploadedFile(UploadedFile uploadedFile)
        {
            _configuration.UploadedFiles.Add(uploadedFile);
            SaveConfiguration();
        }

        public void DeleteUploadedFile(UploadedFile uploadedFile)
        {
            _configuration.UploadedFiles.Remove(uploadedFile);
            SaveConfiguration();
        }

        public bool DeleteConfig()
        {
            try
            {
                File.Delete(_configFilePath);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        private void SaveConfiguration()
        {
            File.WriteAllText(_configFilePath, JsonConvert.SerializeObject(_configuration));
        }
    }
}