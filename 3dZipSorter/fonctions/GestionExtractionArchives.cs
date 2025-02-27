using SharpCompress.Archives;
using SharpCompress.Common;

namespace _3dZipSorter.fonctions
{
    public class GestionExtractionArchives : IFonction
    {
        public void Executer(String dossierSource, string dossierDestination, Dictionary<string, string> fileExtensions, Action<string> log, params string[] operations)
        {
            int count = 0;
            string result= string.Empty;

            // Obtenir toutes les archives dans le dossier source
            var archives = Directory.GetFiles(dossierSource, "*.zip")
                            .Concat(Directory.GetFiles(dossierSource, "*.rar"))
                             .Concat(Directory.GetFiles(dossierSource, "*.7z"));
            log?.Invoke($" {archives.Count()} archives trouvé");

            if (archives.Count() < 1)
            {
                log?.Invoke("Aucune archive présente dans le dossier : " + dossierSource);
                return;
            }
            var archiveExtracteur = new fonctions.ExtraireArchive();

            foreach (var archivePath in archives)
            {
                string dossierCorompu = Path.Combine(dossierDestination, "corrompues"); // Dossier pour les archives corrompues

                // Utilisation de SharpCompress pour ouvrir les archives .zip et .rar
                try
                {
                    result = archiveExtracteur.ExtractArchivesRecursively(archivePath, dossierDestination);
                    if (result == "7z") archiveExtracteur = new fonctions.ExtraireArchive();
                    else if (result == "erreur")
                    { // Déplacer l'archive dans le dossier "cassée"
                        Directory.CreateDirectory(dossierCorompu);
                        string corruptedArchivePath = Path.Combine(dossierCorompu, Path.GetFileName(archivePath));
                        if (!File.Exists(corruptedArchivePath))
                        {
                            try
                            {
                                File.Move(archivePath, corruptedArchivePath);
                                log?.Invoke($"L'archive {archivePath} a été déplacée dans le dossier 'cassée'.");
                            }
                            catch (Exception moveEx) { log?.Invoke($"Erreur lors du déplacement de l'archive corrompue : {moveEx.Message}"); }
                        }
                        else
                        {
                            log?.Invoke($"L'archive {archivePath} existe déjà dans le dossier 'cassée'.");
                        }
                    }
                    else 
                    { 
                        log?.Invoke($"archive : {archivePath} extraite");
                        
                    }
                }
                catch (Exception ex)
                {
                    log?.Invoke($"Erreur lors de l'ouverture de l'archive : {archivePath}. Message d'erreur : {ex.Message}");
                    // Continuer à la prochaine archive sans interrompre le programme
                    continue;
                }
            }
            log?.Invoke($"Classement des archives terminé. {count} archives rangées.");
            count = 0;
        }
    }
}
