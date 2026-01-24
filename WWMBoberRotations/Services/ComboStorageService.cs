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
            
            // Ensure data directory exists
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
                Console.WriteLine($"Error loading combos: {ex.Message}");
                return new List<Combo>();
            }
        }

        public void SaveCombos(IEnumerable<Combo> combos)
        {
            try
            {
                var json = JsonConvert.SerializeObject(combos, Formatting.Indented);
                
                // Ensure directory exists
                if (!Directory.Exists(_dataDir))
                    Directory.CreateDirectory(_dataDir);
                    
                File.WriteAllText(_dataFilePath, json);
                
                // Delete autosave after successful save
                DeleteAutoSave();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving combos: {ex.Message}");
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
                Console.WriteLine($"Error auto-saving combos: {ex.Message}");
                // Don't throw - autosave failures shouldn't crash the app
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
                Console.WriteLine($"Error loading autosave: {ex.Message}");
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
                Console.WriteLine($"Error deleting autosave: {ex.Message}");
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
