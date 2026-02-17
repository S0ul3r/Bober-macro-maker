using System;
using System.IO;
using Newtonsoft.Json;
using WWMBoberRotations.Models;
using WWMBoberRotations.Services;
using Xunit;

namespace WWMBoberRotations.Tests.Services
{
    public class ComboStorageServiceTests : IDisposable
    {
        private readonly string _tempDir;

        public ComboStorageServiceTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "WWMBoberRotationsTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempDir))
            {
                try
                {
                    Directory.Delete(_tempDir, true);
                }
                catch { /* best effort */ }
            }
        }

        [Fact]
        public void LoadCombos_WhenFileDoesNotExist_ReturnsEmptyList()
        {
            var service = new ComboStorageService(_tempDir);
            var combos = service.LoadCombos();
            Assert.NotNull(combos);
            Assert.Empty(combos);
        }

        [Fact]
        public void SaveCombos_AndLoadCombos_RoundTripsCorrectly()
        {
            var service = new ComboStorageService(_tempDir);
            var combo = new Combo
            {
                Name = "TestCombo",
                Hotkey = "f1",
                IsEnabled = true
            };
            combo.Actions.Add(new ComboAction { Type = ActionType.KeyPress, Key = "q", DelayAfter = 100 });

            service.SaveCombos(new[] { combo });
            var loaded = service.LoadCombos();

            Assert.Single(loaded);
            Assert.Equal("TestCombo", loaded[0].Name);
            Assert.Equal("f1", loaded[0].Hotkey);
            Assert.True(loaded[0].IsEnabled);
            Assert.Single(loaded[0].Actions);
            Assert.Equal(ActionType.KeyPress, loaded[0].Actions[0].Type);
            Assert.Equal("q", loaded[0].Actions[0].Key);
            Assert.Equal(100, loaded[0].Actions[0].DelayAfter);
        }

        [Fact]
        public void HasAutoSave_WhenNoAutosave_ReturnsFalse()
        {
            var service = new ComboStorageService(_tempDir);
            Assert.False(service.HasAutoSave());
        }

        [Fact]
        public void AutoSaveCombos_ThenHasAutoSave_ReturnsTrue()
        {
            var service = new ComboStorageService(_tempDir);
            service.AutoSaveCombos(new[] { new Combo { Name = "A" } });
            Assert.True(service.HasAutoSave());
        }

        [Fact]
        public void LoadAutoSave_AfterAutoSave_ReturnsCombos()
        {
            var service = new ComboStorageService(_tempDir);
            var combo = new Combo { Name = "AutosaveCombo" };
            service.AutoSaveCombos(new[] { combo });

            var loaded = service.LoadAutoSave();
            Assert.Single(loaded);
            Assert.Equal("AutosaveCombo", loaded[0].Name);
        }

        [Fact]
        public void DeleteAutoSave_RemovesFile()
        {
            var service = new ComboStorageService(_tempDir);
            service.AutoSaveCombos(new[] { new Combo { Name = "A" } });
            Assert.True(service.HasAutoSave());

            service.DeleteAutoSave();
            Assert.False(service.HasAutoSave());
        }

        [Fact]
        public void ExportCombos_AndImportCombos_RoundTrips()
        {
            var service = new ComboStorageService(_tempDir);
            var exportPath = Path.Combine(_tempDir, "export.json");
            var combo = new Combo { Name = "Exported", Hotkey = "x" };

            service.ExportCombos(new[] { combo }, exportPath);
            Assert.True(File.Exists(exportPath));

            var imported = service.ImportCombos(exportPath);
            Assert.Single(imported);
            Assert.Equal("Exported", imported[0].Name);
            Assert.Equal("x", imported[0].Hotkey);
        }

        [Fact]
        public void GetAutoSaveTime_WhenNoAutosave_ReturnsNull()
        {
            var service = new ComboStorageService(_tempDir);
            Assert.Null(service.GetAutoSaveTime());
        }

        [Fact]
        public void GetAutoSaveTime_AfterAutoSave_ReturnsValue()
        {
            var service = new ComboStorageService(_tempDir);
            service.AutoSaveCombos(new[] { new Combo { Name = "A" } });
            var time = service.GetAutoSaveTime();
            Assert.NotNull(time);
        }
    }
}
