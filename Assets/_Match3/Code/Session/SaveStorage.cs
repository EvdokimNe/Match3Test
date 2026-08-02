using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Match3.Session
{
    public sealed class SaveStorage
    {
        private const string FileName = "match3_save.json";

        private static readonly JsonSerializerSettings Settings = new()
        {
            Formatting = Formatting.Indented,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
        };

        private readonly string _path = Path.Combine(Application.persistentDataPath, FileName);

        public bool TryLoad(out SaveData data)
        {
            data = null;

            if (!File.Exists(_path))
                return false;

            try
            {
                data = JsonConvert.DeserializeObject<SaveData>(File.ReadAllText(_path), Settings);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Match3] Сейв не прочитан ({_path}): {exception.Message}");
                return false;
            }

            return data != null;
        }

        public void Save(SaveData data)
        {
            try
            {
                File.WriteAllText(_path, JsonConvert.SerializeObject(data, Settings));
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Match3] Сейв не записан ({_path}): {exception.Message}");
            }
        }

        public void Delete()
        {
            if (File.Exists(_path))
                File.Delete(_path);
        }
    }
}
