using SharpCompress.Archives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dZipSorter.fonctions
{
    public class Trier_Archives : IFonction
    {
        public void Executer(string dossierSource, string dossierDestination, Dictionary<string, string> fileExtensions)
        {
            int count = 0;

            // Obtenir toutes les archives dans le dossier source
            var archives = Directory.GetFiles(dossierSource, "*.zip")
                            .Concat(Directory.GetFiles(dossierSource, "*.rar"))
                             .Concat(Directory.GetFiles(dossierSource, "*.7z"));
            Console.WriteLine($" {archives.Count()} archives trouvé");

            if (!archives.Any())
            {
                Console.WriteLine("Aucune archive présente dans le dossier : " + dossierSource);
                return;
            }

            foreach (var archivePath in archives)
            {
                string TypeDeFichier = ""; // Extension trouvée
                bool fichierTrouve = false; // Si un fichier correspondant a été trouvé
                string dossierCorompu = Path.Combine(dossierDestination, "cassee"); // Dossier pour les archives corrompues

                // Utilisation de SharpCompress pour ouvrir les archives .zip et .rar
                try
                {
                    using (var archive = ArchiveFactory.Open(archivePath))
                    {
                        foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                        {
                            string extension = Path.GetExtension(entry.Key).ToLower();
                            //Console.WriteLine($"Fichier : {entry.Key} avec extension : {extension}");

                            // Si l'extension du fichier est dans la liste des extensions recherchées
                            if (fileExtensions.TryGetValue(extension, out TypeDeFichier))
                            {                                
                                fichierTrouve = true;
                                break; // Sortir du foreach car on a trouvé un fichier correspondant
                            }

                            else if (extension == ".zip" || extension == ".rar" || extension == ".7z")
                            {
                                Console.WriteLine($"Ouverture de l'archive imbriquée : {entry.Key} dans l'archive {archivePath}");
                                // Ouvre l'archive imbriquée directement depuis le flux
                                using (var entryStream = entry.OpenEntryStream())
                                using (var memoryStream = new MemoryStream())
                                {
                                    // Copier le contenu dans un MemoryStream
                                    entryStream.CopyTo(memoryStream);
                                    memoryStream.Seek(0, SeekOrigin.Begin); // Revenir au début du MemoryStream

                                    // Ouvrir l'archive imbriquée
                                    using (var innerArchive = ArchiveFactory.Open(memoryStream))
                                    {
                                        if (fichierTrouve = RechercheArchiveimbriquee.Recherche(innerArchive, ref TypeDeFichier, fileExtensions, 1)) break; // Récursivité pour les archives imbriquées
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de l'ouverture de l'archive : {archivePath}. Message d'erreur : {ex.Message}");

                    // Créer le dossier pour les archives corrompues si nécessaire
                    Directory.CreateDirectory(dossierCorompu);

                    // Déplacer l'archive dans le dossier "cassée"
                    string corruptedArchivePath = Path.Combine(dossierCorompu, Path.GetFileName(archivePath));
                    if (!File.Exists(corruptedArchivePath))
                    {
                        try
                        {
                            File.Move(archivePath, corruptedArchivePath);
                            Console.WriteLine($"L'archive {archivePath} a été déplacée dans le dossier 'cassée'.");
                        }
                        catch (Exception moveEx) { Console.WriteLine($"Erreur lors du déplacement de l'archive corrompue : {moveEx.Message}"); }
                    }
                    else
                    {
                        Console.WriteLine($"L'archive {archivePath} existe déjà dans le dossier 'cassée'.");
                    }

                    // Continuer à la prochaine archive sans interrompre le programme
                    continue;
                }

                // Si un fichier a été trouvé, on déplace l'archive dans le dossier correspondant
                if (fichierTrouve)
                {
                    deplaceElement.deplacement(archivePath, dossierDestination, TypeDeFichier);
                    count++;
                }
            }

            Console.WriteLine($"Classement des archives terminé. {count} archives rangées.");
            count = 0;
        }
    }
}
