using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ExtensaoZero.Core
{
    [Serializable]
    public sealed class SaveData
    {
        public int version = 1;
        public int chapter = 0;
        public int missionStep = 0;
        public float playTime = 0f;
        public bool missionCompleted = false;
        public List<string> evidenceIds = new List<string>();
        public List<string> unlocked = new List<string>();
        public List<string> seenCalls = new List<string>();
    }

    /// <summary>
    /// Salvamento estritamente local. Sem nuvem, sem conta, sem histórico de rede.
    /// Escrita em arquivo temporário para reduzir risco de corrupção.
    /// </summary>
    public static class SaveSystem
    {
        private const string FileName = "extensao-zero-save.json";

        public static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

        public static SaveData Load()
        {
            try
            {
                if (!File.Exists(SavePath)) return new SaveData();
                var json = File.ReadAllText(SavePath);
                var data = JsonUtility.FromJson<SaveData>(json);
                if (data == null) return new SaveData();
                data.evidenceIds ??= new List<string>();
                data.unlocked ??= new List<string>();
                data.seenCalls ??= new List<string>();
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[EZ] Falha ao carregar save: {e.Message}");
                return new SaveData();
            }
        }

        public static void Save(SaveData data)
        {
            if (data == null) return;
            try
            {
                var directory = Path.GetDirectoryName(SavePath);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

                var json = JsonUtility.ToJson(data, true);
                var temp = SavePath + ".tmp";
                File.WriteAllText(temp, json);
                if (File.Exists(SavePath)) File.Delete(SavePath);
                File.Move(temp, SavePath);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[EZ] Falha ao salvar: {e.Message}");
            }
        }

        public static void Reset()
        {
            try
            {
                if (File.Exists(SavePath)) File.Delete(SavePath);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[EZ] Falha ao apagar save: {e.Message}");
            }
        }
    }
}
