using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static _3dZipSorter.Database.DatabaseManager;

namespace _3dZipSorter.fonctions
{
    public class FileExtensionLoader
    {

        public static List<ArchiveSortingRule> LoadFileExtensions(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Fichier introuvable : {filePath}");
            }

            string jsonContent = File.ReadAllText(filePath);
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent)
               ?? new Dictionary<string, string>();

            // Convertir le dictionnaire en List<ArchiveSortingRule>
            return dict.Select(kv => new ArchiveSortingRule
            {
                Extension = kv.Key,
                DestinationFile = kv.Value
            }).ToList();
        }
        public static List<ArchiveSortingRule> LoadFileExtensionsAndUpdate(string filePath, List<ArchiveSortingRule> result)
        {
            var tempResult = FileExtensionLoader.LoadFileExtensions(filePath);
            if (result == null)
            {
                throw new FileNotFoundException($"Liste introuvable");
            }
            return UpdateList(tempResult, result);
        }

        public static List<ArchiveSortingRule> UpdateList(List<ArchiveSortingRule> listF, List<ArchiveSortingRule> listS)
        {
            var dict = listF.ToDictionary(x => x.Extension, x => x);
            foreach (var rule in listS)
            {
                if (dict.ContainsKey(rule.Extension))
                {
                    dict[rule.Extension].DestinationFile = rule.DestinationFile;
                }
                else
                {
                    dict.Add(rule.Extension, rule);
                }
            }
            return dict.Values.ToList();
        }

        public static void SaveFileExtensions(string filePath, List<ArchiveSortingRule> fileExtensions)
        {
            var dict = fileExtensions.ToDictionary(x => x.Extension, x => x.DestinationFile);
            string jsonContent = JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, jsonContent);
        }

    }
}
