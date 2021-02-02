using ApplicationSettingsNS;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TM_SimplePendant
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ApplicationSettings_Serializer.ApplicationSettings Settings { get; set; }
#if DEBUG
        public static string SettingsFileRootDir { get; set; } = System.IO.Directory.GetCurrentDirectory();
#else        
        public static string SettingsFileRootDir { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
#endif
        public static string SettingsFileAppDir { get; set; } = "\\42Nexus\\TM_SimpleRemote\\";
        public static string SettingsFileName { get; set; } = "appsettings.xml";
        public static string SettingsFilePath { get; set; } = SettingsFileRootDir + SettingsFileAppDir + SettingsFileName;

        public static string Path { get; set; } = System.IO.Directory.GetCurrentDirectory();
        public App()
        {
            if (!Directory.Exists(SettingsFileRootDir + SettingsFileAppDir))
            {
                try
                {
                    Directory.CreateDirectory(SettingsFileRootDir + SettingsFileAppDir);
                }
                catch (Exception)
                {
                }
            }
            try
            {
                Settings = ApplicationSettings_Serializer.Load(SettingsFilePath);
            }
            catch (Exception)
            {
                Settings = new ApplicationSettings_Serializer.ApplicationSettings();
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            //GetData d = new GetData();
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                ApplicationSettings_Serializer.Save(SettingsFilePath, Settings);
            }
            catch (Exception)
            {
            }

            base.OnExit(e);
        }
    }
}
