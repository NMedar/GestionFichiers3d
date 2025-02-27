using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace _3dZipSorter.fonctions
{
    public class FileExtensionLoader
    {

        public static Dictionary<string, string> LoadFileExtensions(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Fichier introuvable : {filePath}");
            }

            string jsonContent = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent)
                   ?? new Dictionary<string, string>();
        }        
        public static Dictionary<string, string> LoadFileExtensionsAndUpdate(string filePath, Dictionary<string, string> result)
        {
            var tempResult = LoadFileExtensions(filePath);
            if (result == null)
                        {
                            throw new FileNotFoundException($"Dictionaire introuvable");
                        }
            return UpdateDictionary(tempResult,result);
        }
        public static Dictionary<string, string> UpdateDictionary(Dictionary<string, string> dictionaryF, Dictionary<string, string> dictionaryS)
            {
                foreach (var rule in dictionaryS)
                {
                    if (dictionaryF.ContainsKey(rule.Key))
                    {
                    // Mettre à jour la valeur existante
                    dictionaryF[rule.Key] = rule.Value;
                    }
                    else
                    {
                    // Ajouter une nouvelle entrée
                    dictionaryF.Add(rule.Key, rule.Value);
                    }
                }
            return dictionaryF;
            }
        
        public static void SaveFileExtensions(string filePath, Dictionary<string, string> fileExtensions)
        {
            string jsonContent = JsonSerializer.Serialize(fileExtensions, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, jsonContent);
        }

    }
}
