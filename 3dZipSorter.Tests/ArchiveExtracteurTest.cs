using Xunit;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace _3dZipSorter.Tests
{ 
    public class ArchiveExtracteurTest
    {
        [Fact]
        public void ExtraireArchives_ExtractsCorrectly()
        {
            // Arrange
            var dossierSource = Path.Combine(Path.GetTempPath(), "testSource");
            var dossierDestination = Path.Combine(Path.GetTempPath(), "testDestination");

            _3dZipSorter.fonctions.Trier_Archives ArchiveExtracteur;
            ArchiveExtracteur = new _3dZipSorter.fonctions.Trier_Archives();

            Directory.CreateDirectory(dossierSource);
            Directory.CreateDirectory(dossierDestination);

            // Ajouter une archive test .zip ou .rar dans dossierSource pour simuler l'extraction
            // Utiliser une méthode d'initialisation pour créer une archive factice ou mocker cette partie

            // Act
            ArchiveExtracteur.Executer(dossierSource, dossierDestination, new Dictionary<string, string> {
                { ".zip", "ZipFiles" },
                { ".rar", "RarFiles" }
            }, message => Console.WriteLine(message));

            // Assert
            // Vérifiez que les fichiers attendus sont présents dans le dossier de destination
            Assert.True(Directory.Exists(Path.Combine(dossierDestination, "ZipFiles")));
            Assert.True(Directory.GetFiles(Path.Combine(dossierDestination, "ZipFiles")).Length > 0);

            // Clean-up
            Directory.Delete(dossierSource, true);
            Directory.Delete(dossierDestination, true);
        }

    }
}
