using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dZipSorter.fonctions
{
    internal class deplaceElement 
    {
        public static void deplacement(string elementCible, string dossierDestination)
        {
            int suffix = 2;

            if (!Directory.Exists(dossierDestination))
                Directory.CreateDirectory(dossierDestination);            

            try
            {//on vérifie si la cible à déplacer est un dossier ou un fichier pour utiliser la bonne methode.
                if (Directory.Exists(elementCible))
                {                    
                    string? nomDossierCible = Path.GetFileName(elementCible);
                    string nomDossierDestination = Path.Combine(dossierDestination, nomDossierCible);
                    while (Directory.Exists(nomDossierDestination))
                    {
                        nomDossierDestination = $"{Path.Combine(dossierDestination, nomDossierCible)}_3dZipSorter_{suffix}";
                        suffix++;
                    }
                    Directory.Move(elementCible, nomDossierDestination);
                }
                else
                {
                    string extension = Path.GetExtension(elementCible);
                    string nomFichier = Path.GetFileNameWithoutExtension(elementCible);
                    string nomFichierDestination = Path.Combine(dossierDestination, Path.GetFileName(elementCible));                    
                    while (File.Exists(nomFichierDestination))
                    {
                        nomFichierDestination = $"{Path.Combine(dossierDestination, nomFichier)}_3dZipSorter_{suffix}{extension}";
                        suffix++;
                    }
                    File.Move(elementCible, nomFichierDestination);
                }
            }
            catch { }
        }
    }
}
