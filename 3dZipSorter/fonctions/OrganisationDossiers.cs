using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static _3dZipSorter.Database.DatabaseManager;



namespace _3dZipSorter.fonctions
{
    public class OrganisationDossiers : IFonction
    {
        public void Executer(string? dossier, string textureSansModelDestination, List<ArchiveSortingRule> fileExtensions, Action<string> log, params string[] operations)
        {
            // Validation des entrées
            if (string.IsNullOrEmpty(dossier) || !Directory.Exists(dossier))
            {
                log("Le dossier spécifié n'existe pas.");
                return;
            }

            if (string.IsNullOrEmpty(textureSansModelDestination) || !Directory.Exists(textureSansModelDestination))
            {
                log("Le dossier de destination est invalide.");
                return;
            }

            if (operations.Length == 0 || operations.Contains("DéplacementFichiersBlend"))
                DeplacementFichiersBlend(dossier, log);

            if (operations.Length == 0 || operations.Contains("DéplacementDossiersTextures"))
                DeplacementDossiersTextures(dossier, textureSansModelDestination, log);

            if (operations.Length == 0 || operations.Contains("RéorganisationDossiers"))
                RéorganisationDossiers(dossier, log);

            if (operations.Length == 0 || operations.Contains("IdentificationProjetsSansMiniature"))
                IdentificationProjetsSansMiniature(dossier, log);
        }

        private void DeplacementFichiersBlend(string dossier, Action<string> log)
        {
            String dossierDestination = string.Empty;
            foreach (var fichier in Directory.GetFiles(dossier, "*.blend", SearchOption.TopDirectoryOnly))
            {
                dossierDestination = Path.Combine(dossier, Path.GetFileNameWithoutExtension(fichier));
                deplaceElement.deplacement(fichier, dossierDestination);
            }
        }

        private void DeplacementDossiersTextures(string dossier, string textureSansModelDestination, Action<string> log)
        {
            string dossierTexturesOrphelines = Path.Combine(textureSansModelDestination, "textures orphelines");
            bool tousImages = false;
            Directory.CreateDirectory(dossierTexturesOrphelines);

            foreach (var dossierTexture in Directory.GetDirectories(dossier, "*texture*", SearchOption.TopDirectoryOnly))
            {
                var fichiers = Directory.GetFiles(dossierTexture);
                if (Directory.GetDirectories(dossierTexture).Length > 0)
                {
                    log?.Invoke($"Le dossier {dossierTexture} contient des sous-dossiers et est ignoré.");
                    continue;
                }
                tousImages = fichiers.All(f =>
                {
                    string extension = Path.GetExtension(f).ToLower();
                    return extension == ".png" || extension == ".jpg" || extension == ".jpeg" ||
                           extension == ".gif" || extension == ".bmp" || extension == ".tiff";
                });
                if (tousImages) deplaceElement.deplacement(dossierTexture, dossierTexturesOrphelines);
            }
            foreach (var dossierTexture in Directory.GetDirectories(dossier, "*", SearchOption.TopDirectoryOnly))
            {
                var fichiers = Directory.GetFiles(dossierTexture);
                if (fichiers.Length == 0 || Directory.GetDirectories(dossierTexture).Length > 0)
                {
                    log?.Invoke($"Dossier ignoré (vide) : {dossierTexture}");
                    continue;
                }
                tousImages = fichiers.All(f =>
                {
                    string extension = Path.GetExtension(f).ToLower();
                    return extension == ".png" || extension == ".jpg" || extension == ".jpeg" ||
                           extension == ".gif" || extension == ".bmp" || extension == ".tiff";
                });
                if(tousImages)deplaceElement.deplacement(dossierTexture, dossierTexturesOrphelines);
            }
        }

        private void RéorganisationDossiers(string dossier, Action<string> log)
        {
            foreach (var sousDossier in Directory.GetDirectories(dossier))
            {
                var contenu = Directory.GetFileSystemEntries(sousDossier);
                if (contenu.Length == 1 && Directory.Exists(contenu[0]))
                {
                    string sousSousDossier = contenu[0];
                    foreach (var item in Directory.GetFileSystemEntries(sousSousDossier))
                    {
                        deplaceElement.deplacement(item, sousDossier);
                    }
                    Directory.Delete(sousSousDossier);
                }
            }
        }

        private void IdentificationProjetsSansMiniature(string dossier, Action<string> log)
        {
            var projetsSansMiniature = new List<string>();
            var extensionsImages = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".tiff", ".webp" };
            bool contientImage = false;

            foreach (var sousDossier in Directory.GetDirectories(dossier))
            {
                contientImage = extensionsImages.Any(ext => 
                    Directory.GetFiles(sousDossier, $"*{ext}", SearchOption.TopDirectoryOnly).Any());

                if (!contientImage)
                    projetsSansMiniature.Add(Path.GetFileName(sousDossier));
            }

            try
            {
                string cheminListe = Path.Combine(dossier, "projets_sans_miniature.txt");
                File.WriteAllLines(cheminListe, projetsSansMiniature);
            }
            catch (Exception ex)
            {
                log($"Erreur lors de l'écriture du fichier : {ex.Message}");
            }
        }
    }
}