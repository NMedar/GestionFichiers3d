using Xunit;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using _3dZipSorter.fonctions;


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
        organisationDossiers.Executer(dossierTest, dossierTest, fileExtensions, message => Console.WriteLine(message), "DéplacementFichiersBlend");

        // Assert
        string dossierBlender = Path.Combine(dossierTest, Path.GetFileNameWithoutExtension(fichierBlender));
        Assert.True(Directory.Exists(dossierBlender));
        Assert.True(File.Exists(Path.Combine(dossierBlender, Path.GetFileName(fichierBlender))));

        // Clean up
        Directory.Delete(dossierTest, true);
    }

    [Fact]
    public void Test_IdentificationSurcoucheDossier()
    {
        // Arrange: Création d'un dossier temporaire
        string dossierTemp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dossierTemp);

        // Cas 1 : Un dossier avec un seul sous-dossier (Doit être traité)
        string dossierA = Path.Combine(dossierTemp, "DossierA");
        string sousDossierA = Path.Combine(dossierA, "SousDossierA");
        Directory.CreateDirectory(dossierA);
        Directory.CreateDirectory(sousDossierA);
        File.WriteAllText(Path.Combine(sousDossierA, "test.txt"), "fichier test");

        // Cas 2 : Un dossier avec un fichier (Ne doit pas être traité)
        string dossierB = Path.Combine(dossierTemp, "DossierB");
        Directory.CreateDirectory(dossierB);
        File.WriteAllText(Path.Combine(dossierB, "image.png"), "contenu image");

        // Cas 3 : Un dossier avec un fichier et un sous-dossier (Ne doit pas être traité)
        string dossierC = Path.Combine(dossierTemp, "DossierC");
        string sousDossierC = Path.Combine(dossierC, "SousDossierC");
        Directory.CreateDirectory(dossierC);
        Directory.CreateDirectory(sousDossierC);
        File.WriteAllText(Path.Combine(dossierC, "document.txt"), "texte");

        // Cas 4 : Un dossier vide (Ne doit pas être traité)
        string dossierD = Path.Combine(dossierTemp, "DossierD");
        Directory.CreateDirectory(dossierD);

        // Simuler le log
        void Log(string message) => Console.WriteLine(message);

        // Act : Exécuter la méthode
        OrganisationDossiers orga = new OrganisationDossiers();
        orga.Executer(dossierTemp, dossierTemp, new Dictionary<string, string>(), Log, "RéorganisationDossiers");

        // Assert : Vérifier les résultats
        Assert.False(Directory.Exists(sousDossierA), "Le sous-dossier de DossierA aurait dû être déplacé.");
        Assert.True(File.Exists(Path.Combine(dossierA, "test.txt")), "Le fichier test aurait dû être remonté.");

        Assert.True(Directory.Exists(dossierB), "DossierB ne doit pas être modifié.");
        Assert.True(File.Exists(Path.Combine(dossierB, "image.png")), "Le fichier image.png ne doit pas être déplacé.");

        Assert.True(Directory.Exists(dossierC), "DossierC ne doit pas être modifié.");
        Assert.True(Directory.Exists(sousDossierC), "SousDossierC ne doit pas être supprimé.");
        Assert.True(File.Exists(Path.Combine(dossierC, "document.txt")), "Le fichier document.txt ne doit pas être déplacé.");

        Assert.True(Directory.Exists(dossierD), "DossierD ne doit pas être supprimé.");

        // Nettoyage : Supprimer le dossier temporaire
        Directory.Delete(dossierTemp, true);
    }

    [Fact]
    public void Test_IdentificationDossierTextureOrpheline()
    {
        // Arrange: Création d'un dossier temporaire
        string dossierTemp = Path.Combine(Path.GetTempPath(), "3dZipSorter_test_TextureOrpheline");
        Directory.CreateDirectory(dossierTemp);

        // Cas 1 : Dossier nommé "texture" (Doit être déplacé)
        string dossierTexture = Path.Combine(dossierTemp, "texture");
        Directory.CreateDirectory(dossierTexture);

        // Cas 2 : Dossier contenant uniquement des images (Doit être déplacé)
        string dossierImages = Path.Combine(dossierTemp, "DossierImages");
        Directory.CreateDirectory(dossierImages);
        File.WriteAllText(Path.Combine(dossierImages, "image1.png"), "image");
        File.WriteAllText(Path.Combine(dossierImages, "image2.jpg"), "image");

        // Cas 3 : Dossier contenant une image et un autre fichier (Ne doit pas être déplacé)
        string dossierMixte = Path.Combine(dossierTemp, "DossierMixte");
        Directory.CreateDirectory(dossierMixte);
        File.WriteAllText(Path.Combine(dossierMixte, "image.png"), "image");
        File.WriteAllText(Path.Combine(dossierMixte, "document.txt"), "texte");

        // Cas 4 : Dossier contenant un sous-dossier et des images (Ne doit pas être déplacé)
        string dossierAvecSousDossier = Path.Combine(dossierTemp, "DossierAvecSousDossier");
        Directory.CreateDirectory(dossierAvecSousDossier);
        string sousDossier = Path.Combine(dossierAvecSousDossier, "SousDossier");
        Directory.CreateDirectory(sousDossier);
        File.WriteAllText(Path.Combine(dossierAvecSousDossier, "image.png"), "image");

        // Cas 5 : Deux dossiers nommés "textures" pour tester le renommage
        string dossierTexture1 = Path.Combine(dossierTemp, "textures");
        Directory.CreateDirectory(dossierTexture1);

        // Simuler le log
        void Log(string message) => Console.WriteLine(message);

        // Act : Exécuter la méthode
        OrganisationDossiers orga = new OrganisationDossiers();
        orga.Executer(dossierTemp, dossierTemp, new Dictionary<string, string>(), Log, "DéplacementDossiersTextures");

        // Cas 5 suite : Deux dossiers nommés "textures" pour tester le renommage
        Directory.CreateDirectory(dossierTexture1); // Devrait être renommé en "textures_2"
        orga.Executer(dossierTemp, dossierTemp, new Dictionary<string, string>(), Log, "DéplacementDossiersTextures");

        // Assert : Vérifier que les bons dossiers ont été déplacés
        Assert.False(Directory.Exists(dossierTexture), "Le dossier nommé 'texture' aurait dû être déplacé.");
        Assert.False(Directory.Exists(dossierImages), "Le dossier contenant uniquement des images aurait dû être déplacé.");
        Assert.True(Directory.Exists(dossierMixte), "Le dossier contenant une image et un autre fichier ne doit pas être déplacé.");
        Assert.True(Directory.Exists(dossierAvecSousDossier), "Le dossier contenant un sous-dossier ne doit pas être déplacé.");

        // Vérifier les dossiers déplacés
        string[] dossiersOrphelins = Directory.GetDirectories(Path.Combine(dossierTemp, "textures orphelines"));
        Assert.Contains(dossiersOrphelins, d => d.Contains("texture"));
        Assert.Contains(dossiersOrphelins, d => d.Contains("DossierImages"));

        // Vérifier le renommage
        string[] nomsDossiersOrphelins = dossiersOrphelins.Select(Path.GetFileName).ToArray();
        Assert.Contains("textures", nomsDossiersOrphelins);
        Assert.Contains("texture_3dZipSorter_2", nomsDossiersOrphelins); // Le second dossier doit être renommé

        // Nettoyage : Supprimer le dossier temporaire
        Directory.Delete(dossierTemp, true);
    }

    [Fact]
    public void Test_IdentificationProjetSansMigniature()
    {
        // Arrange: Création d'un dossier temporaire
        string dossierTemp = Path.Combine(Path.GetTempPath(), "3dZipSorter_test_ProjetSansMigniature");
        Directory.CreateDirectory(dossierTemp);

        // Cas 1 : Dossier sans image (Doit être ajouté)
        string dossierSansImage = Path.Combine(dossierTemp, "DossierSansImage");
        Directory.CreateDirectory(dossierSansImage);

        // Cas 2 : Dossier contenant une image à la racine (Ne doit pas être ajouté)
        string dossierAvecImage = Path.Combine(dossierTemp, "DossierAvecImage");
        Directory.CreateDirectory(dossierAvecImage);
        File.WriteAllText(Path.Combine(dossierAvecImage, "apercu.jpg"), "image");

        // Cas 3 : Dossier contenant une image dans un sous-dossier (Doit être ajouté)
        string dossierImageSousDossier = Path.Combine(dossierTemp, "DossierImageSousDossier");
        Directory.CreateDirectory(dossierImageSousDossier);
        string sousDossier = Path.Combine(dossierImageSousDossier, "SousDossier");
        Directory.CreateDirectory(sousDossier);
        File.WriteAllText(Path.Combine(sousDossier, "image.png"), "image");

        // Cas 4 : Dossier vide (Doit être ajouté)
        string dossierVide = Path.Combine(dossierTemp, "DossierVide");
        Directory.CreateDirectory(dossierVide);

        // Liste pour stocker les projets sans miniature
        List<string> projetsSansMiniature = new List<string>();

        // Simuler le log
        void Log(string message) => Console.WriteLine(message);

        // Act : Exécuter la méthode
        OrganisationDossiers orga = new OrganisationDossiers();
        orga.Executer(dossierTemp, dossierTemp, new Dictionary<string, string>(), Log, "IdentificationProjetsSansMiniature");

        // Vérifier le fichier généré
        string cheminListe = Path.Combine(dossierTemp, "projets_sans_miniature.txt");
        if (File.Exists(cheminListe))
        {
            projetsSansMiniature = File.ReadAllLines(cheminListe).ToList();
        }

        // Assert : Vérifier que les bons dossiers sont détectés
        Assert.Contains("DossierSansImage", projetsSansMiniature);
        Assert.Contains("DossierImageSousDossier", projetsSansMiniature);
        Assert.Contains("DossierVide", projetsSansMiniature);
        Assert.DoesNotContain("DossierAvecImage", projetsSansMiniature);

        // Nettoyage : Supprimer le dossier temporaire
        Directory.Delete(dossierTemp, true);
    }
}