using System;
using System.Collections.Generic;
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
    }
}
