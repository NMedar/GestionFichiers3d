using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;



namespace _3dZipSorter.fonctions
{
    public class OrganisationDossiers : IFonction
    {
        public void Executer(string? dossier, string textureSansModelDestination, Dictionary<string, string> fileExtensions, Action<string> log)
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

            List<string> projetsSansMiniature = new List<string>();

            // Déplacer chaque fichier .blend dans son propre dossier
            foreach (var fichier in Directory.GetFiles(dossier, "*.blend"))
            {
                string nomDossier = Path.GetFileNameWithoutExtension(fichier);
                deplaceElement.deplacement(fichier, dossier);
            }

            // Déplacer les dossiers nommés "_texture_" dans le dossier texture orpheline
            string dossierTexturesOrphelines = Path.Combine(textureSansModelDestination, "textures orphelines");
            Directory.CreateDirectory(dossierTexturesOrphelines);
            foreach (var dossierTexture in Directory.GetDirectories(dossier, "*texture*", SearchOption.TopDirectoryOnly))
            {
                string nomDossier = Path.GetFileName(dossierTexture);
                string destinationPath = Path.Combine(dossierTexturesOrphelines, nomDossier);
                deplaceElement.deplacement(dossierTexture, destinationPath);
            }

            // Parcourir chaque sous-dossier pour réorganiser
            foreach (var sousDossier in Directory.GetDirectories(dossier))
            {
                var contenu = Directory.GetFileSystemEntries(sousDossier);
                // Si un dossier contient un seul sous-dossier, remontez son contenu
                if (contenu.Length == 1 && Directory.Exists(contenu[0]))
                {
                    string sousSousDossier = contenu[0];
                    foreach (var item in Directory.GetFileSystemEntries(sousSousDossier))
                    {
                        string nouveauChemin = Path.Combine(sousDossier, Path.GetFileName(item));
                        deplaceElement.deplacement(item, nouveauChemin);
                    }
                    Directory.Delete(sousSousDossier); // Supprimez le dossier vide
                }

                // Vérifiez la présence d'images dans le dossier
                var extensionsImages = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".tiff",".webp" };
                bool contientImage = extensionsImages.Any(ext => Directory.GetFiles(sousDossier, $"*{ext}").Any());
                if (!contientImage)
                {
                    projetsSansMiniature.Add(Path.GetFileName(sousDossier));
                }
                // Ajoutez un dossier "_Textures" si manquant
                string dossierTextures = Path.Combine(sousDossier, $"{Path.GetFileName(sousDossier)}_Textures");
                if (!Directory.Exists(dossierTextures))
                {
                    Directory.CreateDirectory(dossierTextures);
                }
            }

            // Sauvegarder la liste des projets sans miniature
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