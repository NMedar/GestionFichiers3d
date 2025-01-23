using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dZipSorter.fonctions
{
    internal class deplaceElement 
    {
        public static void deplacement(string fichierCible, string dossierDestination)
        {
            int suffix = 2;
            string destinationPath = Path.Combine(dossierDestination, Path.GetFileName(fichierCible));

            while (Directory.Exists(destinationPath))
            {
                destinationPath = $"{destinationPath}_{suffix}";
                suffix++;
            }
            Directory.CreateDirectory(destinationPath);
            
            //on vérifie si la cible à déplacer est un fichier ou un dossier.
            if (Directory.Exists(fichierCible))
            Directory.Move(fichierCible, destinationPath);
            else
            {
                while (File.Exists(destinationPath))
                {
                    string extension = Path.GetExtension(fichierCible);
                    string baseName = Path.Combine(dossierDestination, Path.GetFileNameWithoutExtension(fichierCible));
                    destinationPath = $"{baseName}_{suffix}{extension}";
                    suffix++;
                }
                File.Move(fichierCible, destinationPath);
            }
        }
    }
}
