using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dZipSorter.fonctions
{
    public class OrganisationDossiers : IFonction
    {
        public void Executer(string? dossier, string textureSansModelDestination, Dictionary<string, string> fileExtensions)
        {
            if (string.IsNullOrEmpty(dossier) || !Directory.Exists(dossier))
            {
                Console.WriteLine("Le dossier spécifié n'existe pas.");
                return;
            }

            List<string> projetsSansMiniature = new List<string>(); // Liste des projets sans image

            // Vérifier la racine pour des fichiers Blender
            var fichiersBlender = Directory.GetFiles(dossier, "*.blend");
            foreach (var fichier in fichiersBlender)
            {
                // Placer chaque fichier dans un dossier du même nom
                string dossierCible = Path.Combine(dossier, Path.GetFileNameWithoutExtension(fichier));
                Directory.CreateDirectory(dossierCible);
                File.Move(fichier, Path.Combine(dossierCible, Path.GetFileName(fichier)));
            }

            // Placer les dossiers contenant "texture" dans "dossier_sans_famille"
            string dossierSansFamille = Path.Combine(textureSansModelDestination, "'\'dossier_sans_famille");
            Directory.CreateDirectory(dossierSansFamille);

            var dossiersTexture = Directory.GetDirectories(dossier, "*texture*");
            foreach (var dossierTexture in dossiersTexture)
            {
                string nomDossier = Path.GetFileName(dossierTexture);
                string nouveauNomDossier = nomDossier;

                // Si un dossier avec le même nom existe, ajouter "1" à la fin du nom
                string destinationPath = Path.Combine(dossierSansFamille, nouveauNomDossier);
                int i = 1;
                while (Directory.Exists(destinationPath))
                {
                    nouveauNomDossier = $"{nomDossier}{i}";
                    destinationPath = Path.Combine(dossierSansFamille, nouveauNomDossier);
                    i++;
                }

                // Déplacer le dossier dans dossier_sans_famille
                Directory.Move(dossierTexture, destinationPath);
            }

            // Parcourir chaque dossier pour réorganiser les sous-dossiers
            var sousDossiers = Directory.GetDirectories(dossier);
            foreach (var sousDossier in sousDossiers)
            {
                var contenu = Directory.GetFileSystemEntries(sousDossier);

                // Si un dossier ne contient qu'un seul sous-dossier, remonter le contenu
                if (contenu.Length == 1 && Directory.Exists(contenu[0]))
                {
                    string sousSousDossier = contenu[0];
                    foreach (var item in Directory.GetFileSystemEntries(sousSousDossier))
                    {
                        string nouveauChemin = Path.Combine(sousDossier, Path.GetFileName(item));
                        if (Directory.Exists(item))
                        {
                            Directory.Move(item, nouveauChemin);
                        }
                        else
                        {
                            File.Move(item, nouveauChemin);
                        }
                    }
                    Directory.Delete(sousSousDossier); // Supprimer le dossier vide
                }

                // Si le dossier ne contient pas d'image, ajouter son nom à la liste des projets sans miniature
                if (!Directory.GetFiles(sousDossier, "*.png").Any() && !Directory.GetFiles(sousDossier, "*.jpg").Any())
                {
                    projetsSansMiniature.Add(Path.GetFileName(sousDossier));
                }
            }

            // Enregistrer la liste des projets sans miniature dans un fichier texte à la racine
            string cheminListe = Path.Combine(dossier, "'\'projets_sans_miniature.txt");
            File.WriteAllLines(cheminListe, projetsSansMiniature);
        }
    }
}
