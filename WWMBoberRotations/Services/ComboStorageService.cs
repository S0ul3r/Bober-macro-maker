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

        public ComboStorageService()
        {
            var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            _dataFilePath = Path.Combine(dataDir, "combos.json");
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
                File.WriteAllText(_dataFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving combos: {ex.Message}");
                throw;
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
