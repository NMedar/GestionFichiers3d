using Xunit;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;


namespace _3dZipSorter.Tests;

public class OrganisationDossiersTests
{
    [Fact]
    public void Test_CreationDossiersBlender()
    {
        // Arrange
        string dossierTest = Path.Combine(Path.GetTempPath(), "DossierTest");
        Directory.CreateDirectory(dossierTest);

        string fichierBlender = Path.Combine(dossierTest, "test.blend");
        File.Create(fichierBlender).Dispose();

        var organisationDossiers = new _3dZipSorter.fonctions.OrganisationDossiers();
        var fileExtensions = new Dictionary<string, string>();

        // Act
        organisationDossiers.Executer(dossierTest, "/chemin/fauxTextureSansModel", fileExtensions);

        // Assert
        string dossierBlender = Path.Combine(dossierTest, "test");
        Assert.True(Directory.Exists(dossierBlender));
        Assert.True(File.Exists(Path.Combine(dossierBlender, "test.blend")));

        // Clean up
        Directory.Delete(dossierTest, true);
    }

    [Fact]
    public void Test_IdentificationSurcoucheDossier()
    {

    }

    public void Test_IdentificationDossierTextureOrphelin()
    {
        
    }

    public void Test_IdentificationProjetSansMigniature()
    {
        
    }
}