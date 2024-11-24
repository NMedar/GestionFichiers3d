using SharpCompress.Archives;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dZipSorter.fonctions
{
    public class GestionExtractionArchives : IFonction
    {
        public void Executer(string sourceDirectory, string targetDirectory, Dictionary<string, string> placeholder)
        {
            ExtractArchivesRecursively(sourceDirectory, targetDirectory);
        }

        private void ExtractArchivesRecursively(string sourceDirectory, string targetDirectory)
        {
            var archives = Directory.GetFiles(sourceDirectory, "*.*", SearchOption.AllDirectories)
                            .Where(file => file.EndsWith(".zip") || file.EndsWith(".rar") || file.EndsWith(".7z"))
                            .ToList();

            foreach (var archivePath in archives)
            {
                try
                {
                    using (var archive = ArchiveFactory.Open(archivePath))
                    {
                        foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                        {
                            string extractionPath = Path.Combine(targetDirectory, entry.Key);

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
                                    ExtractArchivesRecursively(Path.GetDirectoryName(tempFilePath), targetDirectory); // Récursion
                                }
                            }
                            else
                            {
                                // Extraire le fichier
                                entry.WriteToDirectory(targetDirectory, new ExtractionOptions { ExtractFullPath = true, Overwrite = true });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors du traitement de l'archive {archivePath}: {ex.Message}");
                    // Optionnel : déplacer l'archive corrompue dans un dossier spécifique
                }
            }
        }
    }


}
