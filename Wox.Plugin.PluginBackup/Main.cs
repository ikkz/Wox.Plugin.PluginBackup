using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Wox.Plugin.PluginBackup
{
    //defines strings of wox's paths
    public static class GlobalState
    {
        private static string _plugin_name = "Wox.Plugin.PluginBackup";
        private static string _plugin_path;
        private static string _wox_path;
        private static string _desktop_path;
        private static string _backup_file_name = "Wox.Plugins.zip";

        public static void Init()
        {
            //get wox's root path by this dll's path
            UriBuilder uri = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase);
            _plugin_path = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
            _wox_path = _plugin_path.Remove(_plugin_path.LastIndexOf(Path.DirectorySeparatorChar));
            _wox_path = _wox_path.Remove(_wox_path.LastIndexOf(Path.DirectorySeparatorChar));

            //get desktop's directory(where plugins backup file will be save)
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
            List<Result> results = new List<Result>();
            const string ico = "Images\\pic.png";
            string path = "";
            if (query.RawQuery.IndexOf(' ') != -1)
            {
                path = query.RawQuery.Substring(query.RawQuery.IndexOf(' '));
            }
            path = path.Length == 0 ? GlobalState.DesktopDir() + Path.DirectorySeparatorChar + GlobalState.BackupFileName() : path;
            results.Add(new Result
            {
                Title = "backup to:",
                SubTitle = path,
                IcoPath = ico,
                Action = e =>
                {
                    Backup(path);
                    return true;
                }
            });
            results.Add(new Result
            {
                Title = "restore from:",
                SubTitle = path,
                IcoPath = ico,
                Action = e =>
                {
                    Restore(path);
                    return true;
                }
            });
            return results;
        }

        public void Backup(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                }
                catch (IOException)
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
                bool existed = false;
                foreach (string name in existed_plugins)
                {
                    if (entry.FileName.Contains(name))
                        existed = true;
                }
                if (!existed)
                {
                    try
                    {
                        entry.Extract(GlobalState.WoxDir());
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
            //kill wox directly because when you exit wox normally it will rewrite its setting files
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}
