using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3dZipSorter.fonctions
{
    internal class deplaceElement 
    {
        public static void deplacement(string fichierCible, string dossierSource, string dossierDestination)
        {
            Console.WriteLine($"rangement du fichier {fichierCible} dans le dossier {dossierDestination}");
            string targetFolder = Path.Combine(dossierSource, dossierDestination);
            Directory.CreateDirectory(targetFolder); // Créer le dossier si nécessaire

            string destinationPath = Path.Combine(targetFolder, Path.GetFileName(fichierCible));
            
            // Vérifier si le fichier existe déjà
            if (!File.Exists(destinationPath))
            {
                File.Move(fichierCible, destinationPath);        
            }
            else
            {
                File.Move(fichierCible, destinationPath+"1");
                Console.WriteLine($"Le fichier {fichierCible} existe déjà dans {targetFolder}.");
            }
        }
    }
}
