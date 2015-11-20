using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Patchwork.Attributes;

namespace W2PWMod
{
    [NewType]
    public static class Options
    {
        private const string _DEFAULTS = "W2PWModDefaults.ini";
        private const string _OPTIONS = "W2PWMod.ini";

        private static readonly Dictionary<string, string> _values;
        
        static Options()
        {
            _values = LoadFiles();
        } 

        public static bool GetBool(string name)
        {
            string value;
            return _values.TryGetValue(name, out value) && !value.Equals("0", StringComparison.OrdinalIgnoreCase) && !value.Equals("false", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetFullPath(string file)
        {
            var folderUri = Assembly.GetExecutingAssembly().CodeBase;
            if (folderUri.StartsWith("file:///"))
            {
                var folder = Path.GetDirectoryName(Path.GetFullPath(folderUri.Substring(8).Replace('/', '\\')));
                return Path.Combine(folder, file);
            }

            return file;
        }

        private static void LoadFile(string file, Dictionary<string, string> values)
        {
            var path = GetFullPath(file);
            if (File.Exists(path))
            {
                var equals = new char[1];
                equals[0] = '=';

                foreach (var line in File.ReadAllLines(path))
                {
                    // Remove anything starting with a semicolon
                    var comment = line.IndexOf(';');
                    var trimmedLine = ((comment >= 0) ? line.Remove(comment) : line).Trim();
                    var parts = trimmedLine.Split(equals);

                    if (parts.Length == 2)
                    {
                        var name = parts[0].Trim();
                        var value = parts[1].Trim();
                        Helper.W2ModDebug.Log("found ini setting '{0}'='{1}'", name, value);
                        if (name.Length > 0 && value.Length > 0)
                        {
                            values[name] = value;
                        }
                    }
                }
            }
        }

        private static Dictionary<string, string> LoadFiles()
        {
            var result = new Dictionary<string, string>();
            LoadFile(_DEFAULTS, result);
            LoadFile(_OPTIONS, result);
            return result;
        } 
    }
}
