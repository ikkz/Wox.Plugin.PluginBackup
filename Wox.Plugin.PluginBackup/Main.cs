using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Wox.Plugin.PluginBackup
{
    public static class State
    {
        private static string _plugin_name;
        private static string _plugin_path;
        private static string _wox_path;
        private static string _desktop_path;

        public static void Init()
        {
            _plugin_name = "Wox.Plugin.PluginBackup";

            UriBuilder uri = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase);
            _plugin_path = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
            _wox_path = _plugin_path.Remove(_plugin_path.LastIndexOf('\\'));
            _wox_path = _wox_path.Remove(_wox_path.LastIndexOf('\\'));

            _desktop_path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        }
        public static string PluginName() { return _plugin_name; }

        public static string ThisPluginDir() { return _plugin_path; }

        public static string WoxDir() { return _wox_path; }

        public static string WoxPluginsDir() { return _wox_path + "\\Plugins"; }

        public static string WoxSettingsDir() { return _wox_path + "\\Settings"; }

        public static string DesktopDir() { return _desktop_path; }

        public static string TempDir() { return ThisPluginDir() + "\\Wox.PluginBackup"; }

        public static string BackupFilePath() { return ThisPluginDir() + "\\Wox.wpb"; }
    }

    public class Main : IPlugin
    {
        public void Init(PluginInitContext context)
        {
            State.Init();
        }

        public List<Result> Query(Query query)
        {
            return null;
        }


        public void Backup(string path)
        {
            if (Directory.Exists(State.TempDir()))
            {
                Directory.Delete(State.TempDir(), true);
            }
            if (Directory.Exists(State.BackupFilePath()))
            {
                Directory.Delete(State.BackupFilePath(), true);
            }
            Directory.CreateDirectory(State.TempDir());

            DirectoryInfo plugins_dir_info = new DirectoryInfo(State.WoxPluginsDir());
            foreach (DirectoryInfo plugin_dir in plugins_dir_info.GetDirectories())
            {
                if (!plugin_dir.Name.StartsWith(State.PluginName()))
                {
                    //copy to dest;
                }
            }
        }

        public void Restore(string path)
        {

        }
    }
}
