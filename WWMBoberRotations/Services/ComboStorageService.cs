using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using WWMBoberRotations.Models;

namespace WWMBoberRotations.Services
{
    public class ComboStorageService
    {
        private readonly string _dataFilePath;
        private readonly string _autoSaveFilePath;
        private readonly string _dataDir;

        public ComboStorageService()
        {
            _dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            _dataFilePath = Path.Combine(_dataDir, "combos.json");
            _autoSaveFilePath = Path.Combine(_dataDir, ".autosave.json");
            
            if (!Directory.Exists(_dataDir))
                Directory.CreateDirectory(_dataDir);
        }

        public List<Combo> LoadCombos()
        {
            try
            {
                if (!File.Exists(_dataFilePath))
                    return new List<Combo>();

                var json = File.ReadAllText(_dataFilePath);
                var combos = JsonConvert.DeserializeObject<List<Combo>>(json);
                return combos ?? new List<Combo>();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to load combos", ex);
                return new List<Combo>();
            }
        }

        public void SaveCombos(IEnumerable<Combo> combos)
        {
            try
            {
                var json = JsonConvert.SerializeObject(combos, Formatting.Indented);
                
                if (!Directory.Exists(_dataDir))
                    Directory.CreateDirectory(_dataDir);
                    
                File.WriteAllText(_dataFilePath, json);
                DeleteAutoSave();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to save combos", ex);
                throw;
            }
        }

        public void AutoSaveCombos(IEnumerable<Combo> combos)
        {
            try
            {
                var json = JsonConvert.SerializeObject(combos, Formatting.Indented);
                File.WriteAllText(_autoSaveFilePath, json);
            }
            catch (Exception ex)
            {
                Logger.Warning($"Auto-save failed: {ex.Message}");
            }
        }

        public bool HasAutoSave()
        {
            return File.Exists(_autoSaveFilePath);
        }

        public DateTime? GetAutoSaveTime()
        {
            if (!HasAutoSave())
                return null;
                
            return File.GetLastWriteTime(_autoSaveFilePath);
        }

        public List<Combo> LoadAutoSave()
        {
            try
            {
                if (!File.Exists(_autoSaveFilePath))
                    return new List<Combo>();

                var json = File.ReadAllText(_autoSaveFilePath);
                var combos = JsonConvert.DeserializeObject<List<Combo>>(json);
                return combos ?? new List<Combo>();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to load autosave", ex);
                return new List<Combo>();
            }
        }

        public void DeleteAutoSave()
        {
            try
            {
                if (File.Exists(_autoSaveFilePath))
                    File.Delete(_autoSaveFilePath);
            }
            catch (Exception ex)
            {
                Logger.Warning($"Failed to delete autosave: {ex.Message}");
            }
        }

        public void ExportCombos(IEnumerable<Combo> combos, string filePath)
        {
            var json = JsonConvert.SerializeObject(combos, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public List<Combo> ImportCombos(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var combos = JsonConvert.DeserializeObject<List<Combo>>(json);
            return combos ?? new List<Combo>();
        }
    }
}
