using Newtonsoft.Json;
using System.IO;

namespace GitHelper.Common.Helper
{
    public static class LocalCacheHelper
    {
        private static readonly string AppFolder = Path.Combine(
           Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GitHelper"
        );

        static LocalCacheHelper()
        {
            Directory.CreateDirectory(AppFolder);
        }

        private static string GetCachePath(string key) => Path.Combine(AppFolder, $"{key}.json");

        public static bool HasCacheExisted(string key)
        {
            return File.Exists(GetCachePath(key));
        }

        public static T? GetCache<T>(string key)
        {
            string path = GetCachePath(key);
            if (!File.Exists(path))
            {
                return default;
            }

            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static async Task SaveCacheAsync(object obj, string key)
        {
            await File.WriteAllTextAsync(GetCachePath(key), JsonConvert.SerializeObject(obj));
        }

    }
}
