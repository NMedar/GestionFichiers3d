using Xunit;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using _3dZipSorter.fonctions;

namespace _3dZipSorter.Tests
{ 
    public class ArchiveExtracteurTest
    {
        [Fact]
        public void ExtraireArchives_ExtractsCorrectly()
        {
            // Arrange : Création des dossiers temporaires
            var dossierSource = Path.Combine(Path.GetTempPath(), "testSource");
            var dossierDestination = Path.Combine(Path.GetTempPath(), "testDestination");
            if (Directory.Exists(dossierSource))
                Directory.Delete(dossierSource, true);
            if (Directory.Exists(dossierDestination))
                Directory.Delete(dossierDestination, true);

            Directory.CreateDirectory(dossierSource);
            Directory.CreateDirectory(dossierDestination);

            // Création d'une archive .zip pour le test
            string archiveZipPath = Path.Combine(dossierSource, "testArchive.zip");
            using (var archive = ZipFile.Open(archiveZipPath, ZipArchiveMode.Create))
            {
                var entry = archive.CreateEntry("fichierTest.txt");
                using (var writer = new StreamWriter(entry.Open()))
                {
                    writer.Write("Contenu de test");
                }
            }
            var archiveExtracteur = new fonctions.GestionExtractionArchives();

            Console.WriteLine("DEBUG: Avant d'appeler Executer()");
            // Act : Exécuter l'extraction
            archiveExtracteur.Executer(dossierSource, dossierDestination, new Dictionary<string, string> {
                { ".zip", "ZipFiles" },
                { ".rar", "RarFiles" }
            }, message => Console.WriteLine(message));
            Console.WriteLine("DEBUG: Avant d'appeler Executer()");

            // Assert : Vérifier que le dossier "ZipFiles" a été créé et contient les fichiers extraits
            string zipOutputFolder = Path.Combine(dossierDestination, "testArchive");
            Assert.True(Directory.Exists(zipOutputFolder), "Le dossier ZipFiles doit être créé");
            Assert.True(Directory.GetFiles(zipOutputFolder).Length > 0, "L'archive ZIP doit être extraite avec au moins un fichier");

            // Clean-up : Suppression des dossiers de test
            Directory.Delete(dossierSource, true);
            Directory.Delete(dossierDestination, true);
        }

    }
}
