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
        public void Executer(string cheminArchivesSource, string dossierDestination, Dictionary<string, string> fileExtensions, Action<string> log, params string[] operations)
        {
            int count = 0;
            string dossierDestinationModifie = dossierDestination;
            // Obtenir toutes les archives dans le dossier source
            var archives = Directory.GetFiles(cheminArchivesSource, "*.zip")
                            .Concat(Directory.GetFiles(cheminArchivesSource, "*.rar"))
                             .Concat(Directory.GetFiles(cheminArchivesSource, "*.7z"));
            log($" {archives.Count()} archives trouvé");

            if (!archives.Any())
            {
                log("Aucune archive présente dans le dossier : " + cheminArchivesSource);
                return;
            }

            foreach (var archiveEnTraitement in archives)
            {
                string TypeDeFichier = ""; // Extension trouvée
                bool fichierTrouveBool = false; // Si un fichier correspondant a été trouvé
                string dossierCorrompu = Path.Combine(dossierDestination, "corrompues"); // Dossier pour les archives corrompues

                // Utilisation de SharpCompress pour ouvrir les archives .zip et .rar
                try
                {
                    using (var archive = ArchiveFactory.Open(archiveEnTraitement))
                    {
                        foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                        {
                            string extension = Path.GetExtension(entry.Key).ToLower();
                            //log($"Fichier : {entry.Key} avec extension : {extension}");

                            // Si l'extension du fichier est dans la liste des extensions recherchées
                            if (fileExtensions.TryGetValue(extension, out TypeDeFichier))
                            {
                                fichierTrouveBool = true;
                                dossierDestinationModifie = Path.Combine(dossierDestination, TypeDeFichier);
                                break; // Sortir du foreach car on a trouvé un fichier correspondant
                            }

                            else if (extension == ".zip" || extension == ".rar" || extension == ".7z")
                            {
                                log($"Ouverture de l'archive imbriquée : {entry.Key} dans l'archive {archiveEnTraitement}");
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
                                        if (fichierTrouveBool = RechercheArchiveimbriquee.Recherche(innerArchive, ref TypeDeFichier, fileExtensions, 1)) break; // Récursivité pour les archives imbriquées
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log($"Erreur lors de l'ouverture de l'archive : {archiveEnTraitement}. Message d'erreur : {ex.Message}");

                    // Créer le dossier pour les archives corrompues si nécessaire
                    Directory.CreateDirectory(cheminArchivesSource);

                    // Déplacer l'archive dans le dossier "cassée"
                    string corruptedArchivePath = Path.Combine(cheminArchivesSource, Path.GetFileName(archiveEnTraitement));
                    if (!File.Exists(corruptedArchivePath))
                    {
                        try
                        {
                            File.Move(archiveEnTraitement, corruptedArchivePath);
                            log($"L'archive {archiveEnTraitement} a été déplacée dans le dossier 'cassée'.");
                        }
                        catch (Exception moveEx) { log($"Erreur lors du déplacement de l'archive corrompue : {moveEx.Message}"); }
                    }
                    else
                    {
                        log($"L'archive {archiveEnTraitement} existe déjà dans le dossier 'cassée'.");
                    }

                    // Continuer à la prochaine archive sans interrompre le programme
                    continue;
                }

                // Si un fichier a été trouvé, on déplace l'archive dans le dossier correspondant
                if (fichierTrouveBool)
                {
                    deplaceElement.deplacement(archiveEnTraitement, dossierDestinationModifie);
                    count++;
                }
            }

            log($"Classement des archives terminé. {count} archives rangées.");
            count = 0;
        }
    }
}
