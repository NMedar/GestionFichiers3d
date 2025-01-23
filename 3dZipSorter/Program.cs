using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace _3dZipSorter.fonctions
{
    class Program
    {
        static void Main(string[] args)
        {
            string? dossier = null;
            while (dossier != string.Empty)
            {
                string dossierSource;
                string? input = null;

                if (string.IsNullOrEmpty(dossier))
                {
                    Console.WriteLine("Indiquez le dossier à traiter.");
                    input = Console.ReadLine();
                    dossierSource = !string.IsNullOrEmpty(input) ? input : string.Empty;
                }
                else
                {
                    dossierSource = dossier;
                }

                Console.WriteLine("Indiquez le dossier de destination");
                string? output = Console.ReadLine();
                string dossierDestination = string.IsNullOrEmpty(output) ? string.IsNullOrEmpty(input) ? string.Empty : input : output;

                if (string.IsNullOrEmpty(dossierSource) || string.IsNullOrEmpty(dossierDestination))
                {
                    Console.WriteLine("Les chemins d'accès ou de sortie ne sont pas définis.");
                    return;
                }

                // Liste des extensions recherchées
                string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string filePath = Path.Combine(projectDirectory, "fileExtensions.json");
                Dictionary<string, string> fileExtensions = new Dictionary<string, string>();
                try
                {
                    fileExtensions = FileExtensionLoader.LoadFileExtensions(filePath);
                    Console.WriteLine("Extensions chargées avec succès !");
                    foreach (var entry in fileExtensions)
                    {
                        Console.WriteLine($"{entry.Key}: {entry.Value}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur : {ex.Message}");
                }

                Trier_Archives trier_Archives = new Trier_Archives();
                trier_Archives.Executer(dossierSource, dossierDestination, fileExtensions, message => Console.WriteLine(message));
                Console.WriteLine("Souhaitez-vous organiser un nouveau dossier ? Si oui, donnez la nouvelle adresse, sinon appuyez sur Entrée.");
                dossier = Console.ReadLine();
            }
        }

    }
}
