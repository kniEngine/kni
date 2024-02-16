// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml.Serialization;

namespace Content.Pipeline.Editor
{
    public class PipelineSettings
    {
        public List<string> ProjectHistory = new List<string>();
        public string StartupProject;
        public Microsoft.Xna.Framework.Point Size;
        public int HSeparator, VSeparator;
        public bool Maximized, FilterOutput;
    }

    public class PipelineSettingsMgr
    {
        private const string SettingsPath = "Settings.xml";
        private IsolatedStorageFile _isoStore;

        public static PipelineSettingsMgr Current { get; private set; }
        
        public static PipelineSettings Settings;

        static PipelineSettingsMgr()
        {
            Current = new PipelineSettingsMgr();
        }
        
        public PipelineSettingsMgr()
        {
            _isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);            
        }

        /// <summary>
        /// If the project already exists in history, it will be moved to the end.
        /// </summary>
        public void AddProjectHistory(string file)
        {
            string cleanFile = file.Trim();
            Settings.ProjectHistory.Remove(cleanFile);
            Settings.ProjectHistory.Add(cleanFile);
        }
        
        public void RemoveProjectHistory(string file)
        {
            string cleanFile = file.Trim();
            Settings.ProjectHistory.Remove(cleanFile);
        }

        public void Clear()
        {
            Settings.ProjectHistory.Clear();
            Settings.StartupProject = null;
            Save();
        }

        public void Save()
        {
            FileMode mode = FileMode.CreateNew;
            if (_isoStore.FileExists (SettingsPath)) 
				mode = FileMode.Truncate;

            using (Stream isoStream = new IsolatedStorageFileStream(SettingsPath, mode, _isoStore))
            using (TextWriter writer = new StreamWriter(isoStream))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PipelineSettings));
                serializer.Serialize(writer, Settings);
            }
        }

        public void Load()
		{
            if (_isoStore.FileExists(SettingsPath))
            {
                using (Stream isoStream = new IsolatedStorageFileStream(SettingsPath, FileMode.Open, _isoStore))
                using (TextReader reader = new StreamReader(isoStream))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(PipelineSettings));
                    Settings = (PipelineSettings)serializer.Deserialize(reader);
                }
            }
        }
    }
}
