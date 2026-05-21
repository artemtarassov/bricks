#if UNITY_IOS || UNITY_ANDROID || UNITY_EDITOR || UNITY_STANDALONE
#define FILEPREFS_UNITY
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using UnityEngine;



public static class FilePrefs
{
    private const string SaveFileName = "fileprefs.json";
    private const string BackupFileName = "fileprefs.backup.json";
    private static readonly object SyncRoot = new object();

    private static readonly Dictionary<string, Entry> Entries = new Dictionary<string, Entry>();

    private static bool _isLoaded;
    private static bool _isDirty;
    private static bool _quittingHandlerRegistered;

    public static string SavePath
    {
        get { return Path.Combine(Application.persistentDataPath, SaveFileName); }
    }

    public static string BackupPath
    {
        get { return Path.Combine(Application.persistentDataPath, BackupFileName); }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        EnsureLoaded();

        if (_quittingHandlerRegistered)
        {
            return;
        }

#if FILEPREFS_UNITY
        Application.quitting += Save;
#else
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
#endif
        _quittingHandlerRegistered = true;
    }




    public static void SetString(string key, string value)
    {
        EnsureLoaded();
        ValidateKey(key);
        SetEntry(key, EntryType.String, stringValue: value ?? string.Empty);
    }

    public static string GetString(string key)
    {
        return GetString(key, string.Empty);
    }

    public static string GetString(string key, string defaultValue)
    {
        EnsureLoaded();
        ValidateKey(key);

        Entry entry;
        if (Entries.TryGetValue(key, out entry))
        {
            return entry.Type == EntryType.String ? entry.StringValue : defaultValue;
        }

        return defaultValue;
    }

    public static void SetInt(string key, int value)
    {
        EnsureLoaded();
        ValidateKey(key);
        SetEntry(key, EntryType.Int, intValue: value);
    }

    public static int GetInt(string key)
    {
        return GetInt(key, 0);
    }

    public static int GetInt(string key, int defaultValue)
    {
        EnsureLoaded();
        ValidateKey(key);

        Entry entry;
        if (Entries.TryGetValue(key, out entry))
        {
            return entry.Type == EntryType.Int ? entry.IntValue : defaultValue;
        }

        return defaultValue;
    }

    public static void SetFloat(string key, float value)
    {
        EnsureLoaded();
        ValidateKey(key);
        SetEntry(key, EntryType.Float, floatValue: value);
    }

    public static float GetFloat(string key)
    {
        return GetFloat(key, 0f);
    }

    public static float GetFloat(string key, float defaultValue)
    {
        EnsureLoaded();
        ValidateKey(key);

        Entry entry;
        if (Entries.TryGetValue(key, out entry))
        {
            return entry.Type == EntryType.Float ? entry.FloatValue : defaultValue;
        }

        return defaultValue;
    }

    public static void SetBool(string key, bool value)
    {
        EnsureLoaded();
        ValidateKey(key);
        SetEntry(key, EntryType.Bool, boolValue: value);
    }

    public static bool GetBool(string key)
    {
        return GetBool(key, false);
    }

    public static bool GetBool(string key, bool defaultValue)
    {
        EnsureLoaded();
        ValidateKey(key);

        Entry entry;
        if (Entries.TryGetValue(key, out entry))
        {
            return entry.Type == EntryType.Bool ? entry.BoolValue : defaultValue;
        }

        return defaultValue;
    }

    public static bool HasKey(string key)
    {
        EnsureLoaded();
        ValidateKey(key);
        return Entries.ContainsKey(key);
    }

    public static void DeleteKey(string key)
    {
        EnsureLoaded();
        ValidateKey(key);

        lock (SyncRoot)
        {
            if (Entries.Remove(key))
            {
                _isDirty = true;
            }
        }

    }

    public static void DeleteAll()
    {
        EnsureLoaded();

        lock (SyncRoot)
        {
            Entries.Clear();
            _isDirty = true;
        }

        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }

        if (File.Exists(BackupPath))
        {
            File.Delete(BackupPath);
        }

    }

    public static void Save()
    {
        EnsureLoaded();

        lock (SyncRoot)
        {
            if (!_isDirty)
            {
                return;
            }

            if (Entries.Count == 0)
            {
                if (File.Exists(SavePath))
                {
                    File.Delete(SavePath);
                }

                if (File.Exists(BackupPath))
                {
                    File.Delete(BackupPath);
                }

                _isDirty = false;
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(SavePath) ?? Application.persistentDataPath);

            SaveFile saveFile = new SaveFile();
            foreach (KeyValuePair<string, Entry> pair in Entries)
            {
                saveFile.Entries.Add(pair.Value);
            }

            string payloadJson = JsonUtility.ToJson(saveFile, false);
            string envelopeJson = BuildEnvelopeJson(payloadJson);

            WriteTextAtomically(SavePath, envelopeJson);
            WriteTextAtomically(BackupPath, envelopeJson);
            _isDirty = false;
        }
    }

    public static void MigrateStringKey(string key)
    {
        EnsureLoaded();
        ValidateKey(key);
    }

    public static void MigrateIntKey(string key)
    {
        EnsureLoaded();
        ValidateKey(key);
    }

    public static void MigrateFloatKey(string key)
    {
        EnsureLoaded();
        ValidateKey(key);
    }

    public static void MigrateBoolKey(string key)
    {
        EnsureLoaded();
        ValidateKey(key);
    }

    private static void EnsureLoaded()
    {
        lock (SyncRoot)
        {
            if (_isLoaded)
            {
                return;
            }

            LoadFromDisk();
            _isLoaded = true;
        }
    }

    private static void LoadFromDisk()
    {
        Entries.Clear();

        if (TryLoadFromPath(SavePath, restoreToMainFile: false))
        {
            return;
        }

        if (TryLoadFromPath(BackupPath, restoreToMainFile: true))
        {
            return;
        }

        if (File.Exists(SavePath) || File.Exists(BackupPath))
        {
            Debug.LogWarning("FilePrefs could not load either the main save or the backup save. A new cache will be used.");
        }
    }

    private static void SetEntry(
        string key,
        EntryType type,
        string stringValue = null,
        int intValue = 0,
        float floatValue = 0f,
        bool boolValue = false)
    {
        ValidateKey(key);

        lock (SyncRoot)
        {
            Entry entry = new Entry();
            entry.Key = key;
            entry.Type = type;
            entry.StringValue = stringValue ?? string.Empty;
            entry.IntValue = intValue;
            entry.FloatValue = floatValue;
            entry.BoolValue = boolValue;

            Entries[key] = entry;
            _isDirty = true;
        }
    }

    private static bool TryLoadFromPath(string path, bool restoreToMainFile)
    {
        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            string json = File.ReadAllText(path);
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            SaveFile saveFile;
            if (!TryDeserializeSaveFile(json, out saveFile))
            {
                return false;
            }

            PopulateEntries(saveFile);

            if (restoreToMainFile && path == BackupPath)
            {
                WriteTextAtomically(SavePath, json);
            }

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("FilePrefs could not load save file at path: " + path + "\n" + exception);
            return false;
        }
    }

    private static bool TryDeserializeSaveFile(string json, out SaveFile saveFile)
    {
        saveFile = null;

        SaveEnvelope envelope = null;
        try
        {
            envelope = JsonUtility.FromJson<SaveEnvelope>(json);
        }
        catch
        {
            envelope = null;
        }

        if (envelope != null && !string.IsNullOrEmpty(envelope.PayloadJson))
        {
            string expectedChecksum = ComputeChecksum(envelope.PayloadJson);
            if (!string.Equals(expectedChecksum, envelope.Checksum, StringComparison.Ordinal))
            {
                return false;
            }

            try
            {
                saveFile = JsonUtility.FromJson<SaveFile>(envelope.PayloadJson);
            }
            catch
            {
                saveFile = null;
            }

            return saveFile != null && saveFile.Entries != null;
        }

        try
        {
            saveFile = JsonUtility.FromJson<SaveFile>(json);
        }
        catch
        {
            saveFile = null;
        }

        return saveFile != null &&
               saveFile.Entries != null &&
               json.IndexOf("\"Entries\"", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static void PopulateEntries(SaveFile saveFile)
    {
        Entries.Clear();

        for (int i = 0; i < saveFile.Entries.Count; i++)
        {
            Entry entry = saveFile.Entries[i];
            if (entry == null || string.IsNullOrEmpty(entry.Key))
            {
                continue;
            }

            Entries[entry.Key] = entry;
        }
    }

    private static string BuildEnvelopeJson(string payloadJson)
    {
        SaveEnvelope envelope = new SaveEnvelope();
        envelope.Version = 1;
        envelope.PayloadJson = payloadJson;
        envelope.Checksum = ComputeChecksum(payloadJson);
        return JsonUtility.ToJson(envelope, false);
    }

    private static string ComputeChecksum(string text)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(text ?? string.Empty);
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hash = sha256.ComputeHash(bytes);
            StringBuilder builder = new StringBuilder(hash.Length * 2);
            for (int i = 0; i < hash.Length; i++)
            {
                builder.Append(hash[i].ToString("x2"));
            }

            return builder.ToString();
        }
    }

    private static void WriteTextAtomically(string path, string contents)
    {
        string directory = Path.GetDirectoryName(path) ?? Application.persistentDataPath;
        Directory.CreateDirectory(directory);

        string tempPath = path + ".tmp";
        string backupReplacePath = path + ".old";

        byte[] bytes = Encoding.UTF8.GetBytes(contents ?? string.Empty);
        using (FileStream stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush(true);
        }

        try
        {
            if (File.Exists(path))
            {
                if (File.Exists(backupReplacePath))
                {
                    File.Delete(backupReplacePath);
                }

                File.Replace(tempPath, path, backupReplacePath);

                if (File.Exists(backupReplacePath))
                {
                    File.Delete(backupReplacePath);
                }
            }
            else
            {
                File.Move(tempPath, path);
            }
        }
        catch
        {
            if (File.Exists(tempPath))
            {
                File.Copy(tempPath, path, true);
                File.Delete(tempPath);
            }
        }
    }

    private static void ValidateKey(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("FilePrefs key cannot be null or empty.", "key");
        }
    }

#if !FILEPREFS_UNITY
    private static void OnProcessExit(object sender, EventArgs args)
    {
        Save();
    }
#endif

    [Serializable]
    private class SaveFile
    {
        public List<Entry> Entries = new List<Entry>();
    }

    [Serializable]
    private class SaveEnvelope
    {
        public int Version;
        public string PayloadJson;
        public string Checksum;
    }

    [Serializable]
    private class Entry
    {
        public string Key;
        public EntryType Type;
        public string StringValue;
        public int IntValue;
        public float FloatValue;
        public bool BoolValue;
    }

    private enum EntryType
    {
        String,
        Int,
        Float,
        Bool
    }
}

#if !FILEPREFS_UNITY
namespace UnityEngine
{
    using System;
    using System.IO;
    using System.Text.Json;

    public enum RuntimeInitializeLoadType
    {
        BeforeSceneLoad
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RuntimeInitializeOnLoadMethodAttribute : Attribute
    {
        public RuntimeInitializeOnLoadMethodAttribute()
        {
        }

        public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType loadType)
        {
            LoadType = loadType;
        }

        public RuntimeInitializeLoadType LoadType { get; private set; }
    }

    public static class Application
    {
        public static event Action quitting;

        public static string persistentDataPath
        {
            get
            {
                string overridePath = Environment.GetEnvironmentVariable("FILEPREFS_PERSISTENT_DATA_PATH");
                if (!string.IsNullOrEmpty(overridePath))
                {
                    return overridePath;
                }

                return Path.Combine(AppContext.BaseDirectory, "PersistentData");
            }
        }

        public static void Quit()
        {
            Action handler = quitting;
            if (handler != null)
            {
                handler();
            }
        }
    }

    public static class Debug
    {
        public static void LogWarning(string message)
        {
            Console.Error.WriteLine(message);
        }
    }

    public static class JsonUtility
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            IncludeFields = true
        };

        public static string ToJson<T>(T value, bool prettyPrint)
        {
            return JsonSerializer.Serialize(value, Options);
        }

        public static T FromJson<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, Options);
        }
    }
}
#endif
