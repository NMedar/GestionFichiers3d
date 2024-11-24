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
            // Définir les répertoires de test
            Directory.CreateDirectory(dossierCible);

            foreach (var fileExtension in fileExtensions)
            {
                // Chemins des fichiers
                string FilePath = Path.Combine(dossierCible, $"test{fileExtension.Key}");

                // Création des fichiers de test
                File.WriteAllText(FilePath, $"Contenu de test pour le fichier {fileExtension.Key}");

                // Création des archives
                CreerArchive(dossierCible, FilePath, $"archive_{fileExtension.Key.Replace(".","")}.zip" );

                // Suppression des fichiers individuels après l'archivage pour garder uniquement les archives
                File.Delete(FilePath);
            }
        }

        private static void CreerArchive(string dossierSource, string fichier, string nomArchive)
        {
            string cheminArchive = Path.Combine(dossierSource, nomArchive);

            using (ZipArchive archive = ZipFile.Open(cheminArchive, ZipArchiveMode.Create))
            {
                // Ajouter le fichier dans l'archive
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
                { ".obj", "Models/obj" },
                { ".fbx", "Models/fbx" },
                { ".blend", "Models/blend" }
            };

        PreparationDesArchivesTest.CreerFichiersEtArchives(dossierSource, fileExtensions);

        // Act
        var archivesTrieur = new _3dZipSorter.fonctions.GestionExtractionArchives();
        archivesTrieur.Executer(dossierSource, dossierDestination, fileExtensions);

        // Assert
        foreach (var ext in fileExtensions)
        {
            string expectedDirectory = Path.Combine(dossierDestination, ext.Value);
            Assert.True(Directory.Exists(expectedDirectory), $"Dossier {expectedDirectory} non trouvé.");

            string expectedFile = Path.Combine(expectedDirectory, $"test{ext.Key}");
            Assert.True(File.Exists(expectedFile), $"Fichier {expectedFile} non trouvé.");
        }

        // Clean-up
        Directory.Delete(dossierSource, true);
        Directory.Delete(dossierDestination, true);
    }
}