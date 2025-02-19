using System.IO.Compression;
using System.IO;
using Xunit;

namespace _3dZipSorter.Tests;

public class ArchivesTrieurTest
{
    public class PreparationDesArchivesTest
    {
        public static void CreerFichiersEtArchives(string dossierCible, Dictionary<string, string> fileExtensions)
        {
            // D�finir les r�pertoires de test
            Directory.CreateDirectory(dossierCible);

            foreach (var fileExtension in fileExtensions)
            {
                // Chemins des fichiers
                string FilePath = Path.Combine(dossierCible, $"test{fileExtension.Key}");

                // Cr�ation des fichiers de test
                File.WriteAllText(FilePath, $"Contenu de test pour le fichier {fileExtension.Key}");

                // Cr�ation des archives
                CreerArchive(dossierCible, FilePath, $"archive_{fileExtension.Key.Replace(".","")}.zip" );

                // Suppression des fichiers individuels apr�s l'archivage pour garder uniquement les archives
                File.Delete(FilePath);
            }
        }

        private static void CreerArchive(string dossierSource, string fichier, string nomArchive)
        {
            string cheminArchive = Path.Combine(dossierSource, nomArchive);
            using (ZipArchive archive = ZipFile.Open(cheminArchive, ZipArchiveMode.Create))
            {
                archive.CreateEntryFromFile(fichier, Path.GetFileName(fichier));
            }
        }
    }

    [Fact]
    public void TrierArchives_SortsArchivesByFilesExtension()
    {        
        // Arrange
        var dossierSource = Path.Combine(Path.GetTempPath(), "testSource");
        var dossierDestination = Path.Combine(Path.GetTempPath(), "testDestination");

        var fileExtensions = new Dictionary<string, string>
            {
                { ".obj", "Models\\obj" },
                { ".fbx", "Models\\fbx" },
                { ".blend", "Models\\blend" }
            };

        PreparationDesArchivesTest.CreerFichiersEtArchives(dossierSource, fileExtensions);

        // Act
        var archivesTrieur = new _3dZipSorter.fonctions.Trier_Archives();
        archivesTrieur.Executer(dossierSource, dossierDestination, fileExtensions, message => Console.WriteLine(message));

        // Assert
        foreach (var ext in fileExtensions)
        {
            string expectedDirectory = Path.Combine(dossierDestination, ext.Value);
            Assert.True(Directory.Exists(expectedDirectory), $"Dossier {expectedDirectory} non trouv�.");

            string expectedArchivePath = Path.Combine(expectedDirectory, $"archive_{ext.Key.Replace(".", "")}.zip");
            Assert.True(File.Exists(expectedArchivePath), $"Archive {expectedArchivePath} non trouv�e.");
        }

        // Clean-up
        Directory.Delete(dossierSource, true);
        Directory.Delete(dossierDestination, true);
    }
}