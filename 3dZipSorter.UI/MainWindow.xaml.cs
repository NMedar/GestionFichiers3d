using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Net.Sockets;
using SharpCompress;
using _3dZipSorter.fonctions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace _3dZipSorter.UI
{
    public partial class MainWindow : Window
    {
        public Dictionary<string, string> Modes { get; set; }
        private Dictionary<string, string> fileExtensions;
        private string dossierSource, dossierDestination = string.Empty;

        private readonly Dictionary<string, Type> modeFonctions = new Dictionary<string, Type>
        {
            { "organisationDossiers", typeof(OrganisationDossiers) },
            { "Tri_Archives", typeof(Trier_Archives) },
            { "extraireArchive", typeof(ExtraireArchive) },
        };

        public MainWindow()
        {
            InitializeComponent();

            string projectDirectory = "S:\\projets\\dev\\3d zip sorter\\3dZipSorter";//AppDomain.CurrentDomain.BaseDirectory;
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
                // Charger le dictionnaire depuis le fichier JSON
                fileExtensions = FileExtensionLoader.LoadFileExtensions(filePath);
                Console.WriteLine("Extensions chargées avec succès !");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur lors du chargement des extensions : {ex.Message}");
                fileExtensions = new Dictionary<string, string>(); // Initialise avec un dictionnaire vide en cas d'erreur
            }
        }

        private void ModeSelector_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ComboBox modeSelector = (System.Windows.Controls.ComboBox)FindName("ModeSelector");
            if (ModeSelector.SelectedItem is KeyValuePair<string, string> selectedMode)
            {
                ActionButton.Visibility = Visibility.Visible;
                result.Text += $"Mode sélectionné : {selectedMode.Key}\nDescription : {selectedMode.Value}";
                if (selectedMode.Key == "organisationDossiers")
                {
                    DestinationTextBox.Visibility = Visibility.Collapsed;
                }
                else
                {
                    DestinationTextBox.Visibility = Visibility.Visible;
                }
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
                            result.Text += $"Début de l'opération {selectedMode.Key} \n";
                            fonctionInstance.Executer(dossierSource, dossierDestination, fileExtensions, (message) =>
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    result.AppendText(message + Environment.NewLine);
                                    result.ScrollToEnd();
                                });
                            });
                            result.Text += $"Mode {selectedMode.Key} exécuté avec succès.";
                        }
                        else
                        {
                            result.Text += $"La classe pour {selectedMode.Key} n'implémente pas IFonction. \n";
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Erreur lors de l'exécution : {ex.Message}");
                    }
                }
                else
                {
                    result.Text += "Mode non pris en charge \n";
                }
            }
            else
            {
                result.Text += "Veuillez sélectionner un mode de fonctionnement. \n";
            }
        }
    }
}