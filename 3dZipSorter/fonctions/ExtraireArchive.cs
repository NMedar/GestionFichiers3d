using SharpCompress.Archives;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dZipSorter.fonctions
{
    public class ExtraireArchive
    { 
        public string ExtractArchivesRecursively(string archivePath, string targetDirectory)
        {
                try
                {                    
                    using (var archive = ArchiveFactory.Open(archivePath))
                    {
                        string dossierCible = Path.Combine(targetDirectory,Path.GetFileNameWithoutExtension(archivePath));
                        int compteur = 1;
                        string dossierUnique = dossierCible;
                        while (Directory.Exists(dossierUnique))
                        {
                            dossierUnique = $"{dossierCible} ({compteur})";
                            compteur++;
                        }
                        Directory.CreateDirectory(dossierUnique);
                        foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                        {
                            string extractionPath = Path.Combine(dossierUnique, entry.Key);

                            // Si c'est encore une archive, traiter récursivement
                            if (Path.GetExtension(entry.Key).ToLower() == ".zip" || Path.GetExtension(entry.Key).ToLower() == ".rar")
                            {
                                using (var entryStream = entry.OpenEntryStream())
                                {
                                    string tempFilePath = Path.Combine(Path.GetTempPath(), entry.Key);
                                    using (var tempFile = File.Create(tempFilePath))
                                    {
                                        entryStream.CopyTo(tempFile);
                                    }
                                    ExtractArchivesRecursively(Path.GetDirectoryName(tempFilePath), dossierUnique); // Récursion
                                }
                            }
                            else
                            {
                                // Extraire le fichier
                                entry.WriteToDirectory(dossierUnique, new ExtractionOptions { ExtractFullPath = true, Overwrite = true });
                            }
                        }
                        return "archive_extraite";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors du traitement de l'archive {archivePath}: {ex.Message}");
                    if (Path.GetExtension(archivePath) == ".7z") return "7z";
                    return "erreur";
                }            
        }
    }
}