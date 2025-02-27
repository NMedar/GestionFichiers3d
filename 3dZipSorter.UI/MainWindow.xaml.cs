using System.Windows;
using System.Windows.Controls;
using _3dZipSorter.fonctions;
using System.IO;
using MahApps.Metro.Controls;
using System.Text.Json;
using _3dZipSorter.Database;
using static System.Net.WebRequestMethods;
using File = System.IO.File;
using static _3dZipSorter.fonctions.FileExtensionLoader;


namespace _3dZipSorter.UI
{
    public partial class MainWindow : MetroWindow
    {
        public Dictionary<string, string> Modes { get; set;}
        private Dictionary<string, string> archiveRegles;
        public string FileJsonPath { get; set; }
        private DatabaseManager _databaseManager;

        private readonly Dictionary<string, Type> modeFonctions = new Dictionary<string, Type>
        {
            { "organisationDossiers", typeof(OrganisationDossiers) },
            { "Tri_Archives", typeof(Trier_Archives) },
            { "extraireArchive", typeof(ExtraireArchive) },
        };

        private Dictionary<string, string> modesOrganisation = new Dictionary<string, string>
    {
        { "rangement des fichiers blenders", "DéplacementFichiersBlend" },
        { "rangement des textures", "DéplacementDossiersTextures" },
        { "rangement des dossiers", "RéorganisationDossiers" },
        { "identification des projet sans mignature", "IdentificationProjetsSansMiniature" },
        { "rangement de l'ensemble des dossiers", ""}
    };

        public MainWindow()
        {
            InitializeComponent();

            SelecteurFonctionOrganisation.ItemsSource = modesOrganisation;
            SelecteurFonctionOrganisation.SelectedValuePath = "Value";

            FileJsonPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fileExtensions.json");
            DataContext = this; // Liez les données pour le binding


            string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = System.IO.Path.Combine(projectDirectory, "fileExtensions.json");

            // Initialisation des modes de fonctionnement
            Modes = new Dictionary<string, string>
            {
                { "Tri_Archives", "Permet de trier les archives d'un dossier en fonction de leurs contenus" },
                { "extraireArchive", "Extrais les archives et archives imbriqués présent dans un dossier" },
                { "organisationDossiers", "réorganise les fichiers et dossiers" }
            };

            ModeSelector.ItemsSource = Modes;
            DestinationTextBox.Visibility = Visibility.Hidden;

            ActionButton.Visibility = Visibility.Collapsed;

            try
            {
                // Initialisez la base de données
                string databasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "3dZipSorter.db");
                _databaseManager = new DatabaseManager(databasePath);

                // Exemple : Charger les règles de tri des archives
                archiveRegles = _databaseManager.GetAllArchiveSortingRules();
                if(archiveRegles.Count<1)
                {
                    System.Windows.MessageBox.Show("Aucune règle de tri trouvée dans la base de données.");
                    LogListView.Items.Add("Aucune règle de tri trouvée dans la base de données.");
                }
                else
                {
                    System.Windows.MessageBox.Show("Règles de tri chargées avec succès.");
                }               
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors de la création de la base de données : {ex.Message}");
                LogListView.Items.Add($"Erreur lors de la création de la base de données : {ex.Message}");
                try 
                {
                    //Charger le dictionnaire depuis le fichier JSON
                    archiveRegles = FileExtensionLoader.LoadFileExtensions(filePath);
                }
                catch (Exception ex2)
                {                    
                    System.Windows.MessageBox.Show($"Erreur lors du chargement des extensions : {ex2.Message}");
                    LogListView.Items.Add($"Erreur lors du chargement des extensions : {ex2.Message}");
                    archiveRegles = new Dictionary<string, string>(); // Initialise avec un dictionnaire vide en cas d'erreur
                }
            }
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Enregistrez les paramètres (par exemple, le chemin des extensions)
                var settings = new Dictionary<string, string>
                {
                    { "FileExtensionsPath", FileJsonPath }
                };
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText("settings.json", json);
                System.Windows.MessageBox.Show("Paramètres enregistrés avec succès !");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors de l'enregistrement des paramètres : {ex.Message}");
            }
        }
        private void LoadSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Chargez les paramètres (par exemple, le chemin des extensions)
                string settings = File.ReadAllText("settings.json");
                var jsonSettings = JsonSerializer.Deserialize<Dictionary<string, string>>(settings);
                if (jsonSettings != null && jsonSettings.ContainsKey("FileExtensionsPath"))
                {
                    FileJsonPath = jsonSettings["FileExtensionsPath"];
                }
                System.Windows.MessageBox.Show("Paramètres chargés avec succès !");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors du chargement des paramètres : {ex.Message}");
            }
        }
        private void ModeSelector_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ComboBox modeSelector = (System.Windows.Controls.ComboBox)FindName("ModeSelector");
            if (ModeSelector.SelectedItem is KeyValuePair<string, string> selectedMode)
            {
                ActionButton.Visibility = Visibility.Visible;
                LogListView.Items.Add($"Mode sélectionné : {selectedMode.Key}\nDescription : {selectedMode.Value}");
                if (selectedMode.Key == "organisationDossiers")
                {
                    DestinationTextBox.Visibility = Visibility.Collapsed;
                    SelecteurFonctionOrganisation.Visibility = Visibility.Visible;
                }
                else
                {
                    SelecteurFonctionOrganisation.Visibility = Visibility.Collapsed;
                    DestinationTextBox.Visibility = Visibility.Visible;
                }
            }
        }

        private void SelecteurFonctionOrganisation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (SelecteurFonctionOrganisation.SelectedItem is KeyValuePair<string, string> selectedMode)
            {
                string modeSelectionne = selectedMode.Value;
            }
        }

        private void BrowseSource_Click(object sender, RoutedEventArgs e)
        {
            // Ouvre un dialogue pour sélectionner un dossier
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    SourceTextBox.Text = dialog.SelectedPath;
                }
            }
        }

        private void BrowseDestination_Click(object sender, RoutedEventArgs e)
        {
            // Ouvre un dialogue pour sélectionner un dossier
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    DestinationTextBox.Text = dialog.SelectedPath;
                }
            }
        }

        private void Action_Click(object sender, RoutedEventArgs e)
        {
            string dossierDestination = DestinationTextBox.Text;
            string dossierSource = SourceTextBox.Text;

            // Validation des entrées
            if (string.IsNullOrEmpty(dossierSource) || !Directory.Exists(dossierSource))
            {
                System.Windows.MessageBox.Show("Erreur, le dossier source requiert un dossier valide pour fonctionner");
                return;
            }
            if (string.IsNullOrEmpty(dossierDestination))
            {
                dossierDestination = dossierSource;
                System.Windows.MessageBox.Show("Le dossier de destination étant vide le dossier source seras utilisé.");
                return;
            } else if (!Directory.Exists(dossierDestination))
            {
                System.Windows.MessageBox.Show("Le dossier de destination est invalide ou n'existe pas.");
                return;
            }

            // Vérifiez si un mode est sélectionné
            if (ModeSelector.SelectedItem is KeyValuePair<string, string> selectedMode)
            {
                if (modeFonctions.TryGetValue(selectedMode.Key, out var fonctionType))
                {
                    try
                    {
                        // Instanciez dynamiquement la classe et appelez Executer
                        if (Activator.CreateInstance(fonctionType) is IFonction fonctionInstance)
                        {
                            string[] modeOrganisation= new string[0];
                            if (selectedMode.Value == "Organisation" && SelecteurFonctionOrganisation.SelectedItem is KeyValuePair<string, string> selectedOrgMode)
                            {
                                modeOrganisation.Append(selectedOrgMode.Value);
                            }
                            LogListView.Items.Add($"Début de l'opération {selectedMode.Key}");
                            fonctionInstance.Executer(dossierSource, dossierDestination, archiveRegles, (message) =>
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    LogListView.Items.Add(message + Environment.NewLine);
                                });
                            }, modeOrganisation);
                            LogListView.Items.Add($"Mode {selectedMode.Key} exécuté avec succès.");
                        }
                        else
                        {
                            LogListView.Items.Add($"La classe pour {selectedMode.Key} n'implémente pas IFonction.");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Erreur lors de l'exécution : {ex.Message}");
                    }
                }
                else
                {
                    LogListView.Items.Add("Mode non pris en charge");
                }
            }
            else
            {
                LogListView.Items.Add("Veuillez sélectionner un mode de fonctionnement.");
            }
        }

        private void AddOrUpdateArchiveSortingRule_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddOrUpdateFileSortingRule_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LoadArchiveSortingRules_Click(object sender, RoutedEventArgs e)
        {
            //todo : ajouter une vérification pour savoir si le fichier correspond à la structure de la base de données
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Fichiers JSON (*.json)|*.json|Tous les fichiers (*.*)|*.*",
                Title = "Sélectionnez un fichier JSON"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Lire le contenu du fichier JSON
                    string jsonContent = File.ReadAllText(openFileDialog.FileName);

                    // Désérialiser le JSON en un dictionnaire
                    var rules = new Dictionary<string, string>();
                    if (archiveRegles == null)
                        rules = LoadFileExtensions(openFileDialog.FileName);
                    else
                        rules = LoadFileExtensionsAndUpdate(openFileDialog.FileName, archiveRegles);
                    if (rules != null)
                    {
                        // Ajouter les règles dans la base de données
                        foreach (var rule in rules)
                        {
                            _databaseManager.InsertOrUpdateArchiveSortingRules(rule.Key, rule.Value);
                        }
                        // Recharger les règles dans l'interface utilisateur
                        
                        System.Windows.MessageBox.Show("Règles chargées avec succès !");
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Erreur lors du chargement du fichier : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadFileSortingRules_Click(object sender, RoutedEventArgs e)
        {
            //todo : définir la structure de la base de données pour les règles d'organisation des dossiers'
            //todo : ajouter une vérification pour savoir si le fichier correspond à la structure de la base de données
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Fichiers JSON (*.json)|*.json|Tous les fichiers (*.*)|*.*",
                Title = "Sélectionnez un fichier JSON"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    System.Windows.MessageBox.Show("Règles pour gérer l'organision pas encore définis !");
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Erreur lors du chargement du fichier : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}