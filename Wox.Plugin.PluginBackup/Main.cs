using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Wox.Plugin.PluginBackup
{
    public static class GlobalState
    {
        private static string _plugin_name = "Wox.Plugin.PluginBackup";
        private static string _plugin_path;
        private static string _wox_path;
        private static string _desktop_path;
        private static string _backup_file_name = "Wox.Plugins.zip";

        public static void Init()
        {
            UriBuilder uri = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase);
            _plugin_path = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
            _wox_path = _plugin_path.Remove(_plugin_path.LastIndexOf(Path.DirectorySeparatorChar));
            _wox_path = _wox_path.Remove(_wox_path.LastIndexOf(Path.DirectorySeparatorChar));

            _desktop_path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        }
        public static string PluginName() { return _plugin_name; }

        public static string ThisPluginDir() { return _plugin_path; }

        public static string WoxDir() { return _wox_path; }

        public static string WoxPluginsDir() { return _wox_path + Path.DirectorySeparatorChar + "Plugins"; }

        public static string WoxSettingsDir() { return _wox_path + Path.DirectorySeparatorChar + "Settings"; }

        public static string DesktopDir() { return _desktop_path; }

        public static string BackupFileName() { return _backup_file_name; }
    }

    public class Main : IPlugin
    {
        public void Init(PluginInitContext context)
        {
            GlobalState.Init();
        }

        public List<Result> Query(Query query)
        {
            return null;
        }


        public void Backup(string path)
        {
            path = path.Length == 0 ? GlobalState.DesktopDir() + Path.DirectorySeparatorChar + GlobalState.BackupFileName() : path;
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                }
                catch (System.IO.IOException)
                {
                    return;
                }
            }
            ZipFile zip = new ZipFile();
            AddDirToZip(zip, GlobalState.WoxPluginsDir());
            AddDirToZip(zip, GlobalState.WoxSettingsDir());
            zip.Save(path);
        }

        private void AddDirToZip(ZipFile zip, string dir)
        {
            foreach (string file in Directory.GetFiles(dir))
                zip.AddFile(file, dir.Replace(GlobalState.WoxDir(), ""));
            foreach (string sub in Directory.GetDirectories(dir))
                AddDirToZip(zip, sub);
        }

        public void Restore(string path)
        {
            path = path.Length == 0 ? GlobalState.DesktopDir() + Path.DirectorySeparatorChar + GlobalState.BackupFileName() : path;
            ZipFile zip = ZipFile.Read(path);
            List<string> existed_plugins = new List<string>();
            foreach (string pd in Directory.GetDirectories(GlobalState.WoxPluginsDir()))
            {
                if (pd.Contains("-"))
                    existed_plugins.Add(pd.Remove(pd.IndexOf('-')).Replace(GlobalState.WoxPluginsDir() + Path.DirectorySeparatorChar, ""));
                else
                    existed_plugins.Add(pd.Replace(GlobalState.WoxPluginsDir() + Path.DirectorySeparatorChar, ""));
            }
            foreach (ZipEntry entry in zip)
            {
                string filename = entry.FileName.Remove(entry.FileName.IndexOf('-')).Remove(0, entry.FileName.IndexOf('/') + 1);
                if (!existed_plugins.Contains(filename))

                    entry.Extract(GlobalState.WoxDir());
            }
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}
