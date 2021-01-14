﻿using Guardian.Utilities;
using System.IO;
using System.Text;

namespace Guardian.Features.Properties
{
    class PropertyManager : FeatureManager<Property>
    {
        private string DataPath = Mod.RootDir + "\\GameSettings.txt";

        // Master Client
        public Property<bool> LaughingTitans = new Property<bool>("MC_TitansLaughOnDeath", new string[0], false);
        public Property<bool> EndlessTitans = new Property<bool>("MC_EndlessTitans", new string[0], false);
        public Property<bool> InfiniteRoom = new Property<bool>("MC_InfiniteRoom", new string[0], false);

        // Player
        public Property<bool> AlternateIdle = new Property<bool>("Player_AHSSIdle", new string[0], false);
        public Property<bool> AlternateBurst = new Property<bool>("Player_CrossBurst", new string[0], false);

        // Chat
        public Property<string> JoinMessage = new Property<string>("Chat_JoinMessage", new string[0], string.Empty);
        public Property<string> ChatName = new Property<string>("Chat_UserName", new string[0], string.Empty);
        public Property<bool> BoldName = new Property<bool>("Chat_BoldName", new string[0], false);
        public Property<bool> ItalicName = new Property<bool>("Chat_ItalicName", new string[0], false);
        public Property<string> TextColor = new Property<string>("Chat_TextColor", new string[0], string.Empty);
        public Property<string> TextPrefix = new Property<string>("Chat_TextPrefix", new string[0], string.Empty);
        public Property<string> TextSuffix = new Property<string>("Chat_TextSuffix", new string[0], string.Empty);
        public Property<bool> BoldText = new Property<bool>("Chat_BoldText", new string[0], false);
        public Property<bool> ItalicText = new Property<bool>("Chat_ItalicText", new string[0], false);

        // Visual
        public Property<bool> LegacyTimeFormat = new Property<bool>("Visual_LegacyTimeFormat", new string[0], true);

        // Logging
        public Property<bool> ShowLog = new Property<bool>("Log_ShowLog", new string[0], true);
        public Property<bool> LogInfo = new Property<bool>("Log_ShowGeneric", new string[0], true);
        public Property<bool> LogWarnings = new Property<bool>("Log_ShowWarnings", new string[0], true);
        public Property<bool> LogErrors = new Property<bool>("Log_ShowErrors", new string[0], true);

        public override void Load()
        {
            // Gameplay
            base.Add(LaughingTitans);
            base.Add(EndlessTitans);
            base.Add(InfiniteRoom);

            // Player
            base.Add(AlternateIdle);
            base.Add(AlternateBurst);

            // Chat
            base.Add(JoinMessage);
            base.Add(ChatName);
            base.Add(TextColor);
            base.Add(BoldName);
            base.Add(ItalicName);
            base.Add(TextPrefix);
            base.Add(TextSuffix);
            base.Add(BoldText);
            base.Add(ItalicText);

            // Visual
            base.Add(LegacyTimeFormat);

            // Logging
            base.Add(ShowLog);
            base.Add(LogInfo);
            base.Add(LogWarnings);
            base.Add(LogErrors);

            LoadFromFile();
            Save();
        }

        public void LoadFromFile()
        {
            GameHelper.TryCreateFile(DataPath, false);

            foreach (string line in File.ReadAllLines(DataPath))
            {
                string[] data = line.Split(new char[] { '=' }, 2);
                Property property = Find(data[0]);

                if (property != null)
                {
                    if (property.Value is bool)
                    {
                        if (bool.TryParse(data[1], out bool result))
                        {
                            ((Property<bool>)property).Value = result;
                        }
                    }
                    else if (property.Value is int)
                    {
                        if (int.TryParse(data[1], out int result))
                        {
                            ((Property<int>)property).Value = result;
                        }
                    }
                    else if (property.Value is float)
                    {
                        if (float.TryParse(data[1], out float result))
                        {
                            ((Property<float>)property).Value = result;
                        }
                    }
                    else if (property.Value is string)
                    {
                        ((Property<string>)property).Value = data[1];
                    }
                }
            }
        }

        public override void Save()
        {
            GameHelper.TryCreateFile(DataPath, false);

            StringBuilder builder = new StringBuilder();
            base.Elements.ForEach(property => builder.AppendLine($"{property.Name}={property.Value}"));

            File.WriteAllText(DataPath, builder.ToString());
        }
    }
}
