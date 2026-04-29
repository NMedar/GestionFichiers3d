using SharpCompress.Archives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static _3dZipSorter.Database.DatabaseManager;

namespace _3dZipSorter.fonctions
{
    internal class RechercheArchiveimbriquee
    {
        public static bool Recherche(IArchive archive, ref string TypeDeFichier, List<ArchiveSortingRule> fileExtensions, int couche)
        {
            bool fichierTrouve = false;
            foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
            {
                string fileExtension = Path.GetExtension(entry.Key).ToLower();
                //Console.WriteLine($"Fichier trouvé dans l'archive imbriquée : {entry.Key} (Extension : {fileExtension})");

                // Si c'est encore une archive compressée, appeler la méthode récursive
                if (couche < 2 && (fileExtension == ".zip" || fileExtension == ".rar" || fileExtension == ".7z"))
                {
                    //Console.WriteLine($"Ouverture de l'archive imbriquée : {entry.Key}");

                    // Utilisation d'un MemoryStream pour rendre le flux seekable
                    using (var entryStream = entry.OpenEntryStream())
                    using (var memoryStream = new MemoryStream())
                    {
                        // Copier le contenu dans un MemoryStream
                        entryStream.CopyTo(memoryStream);
                        memoryStream.Seek(0, SeekOrigin.Begin); // Revenir au début du MemoryStream

                        // Ouvrir l'archive imbriquée
                        using (var innerArchive = ArchiveFactory.Open(memoryStream))
                        {
                            couche = couche + 1;
                            fichierTrouve = RechercheArchiveimbriquee.Recherche(innerArchive, ref TypeDeFichier, fileExtensions, couche); // Récursivité pour les archives imbriquées
                        }
                    }
                }

                else
                {
                    var rule = fileExtensions.FirstOrDefault(r => r.Extension == fileExtension);
                    if (rule != null)
                    {
                        TypeDeFichier = rule.DestinationFile;
                        fichierTrouve = true;
                        break; // Sortir si un fichier valide a été trouvé
                    }
                }
            }
            return fichierTrouve;
        }
    }
}
