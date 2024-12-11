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


namespace _3dZipSorter.UI
{
    public partial class MainWindow : Window
    {
        public Dictionary<string, string> Modes { get; set; }
        private string dossierSource, dossierDestination = string.Empty;

        public MainWindow()
        {
            InitializeComponent();

            // Initialisation des modes de fonctionnement
            Modes = new Dictionary<string, string>
            {
                { "Tri_Archives", "Permet de trier les archives d'un dossier en fonction de leurs contenus" },
                { "extraireArchive", "Extrais les archives et archives imbriqués présent dans un dossier" },
                { "organisationDossiers", "réorganise les fichiers et dossiers" }
            };

            ModeSelector.ItemsSource = Modes;
            DestinationTextBox.Visibility = Visibility.Hidden;
        }

        private void ModeSelector_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            System.Windows.Forms.ComboBox modeSelector = (System.Windows.Forms.ComboBox)FindName("ModeSelector");
            if (ModeSelector.SelectedItem is KeyValuePair<string, string> selectedMode)
            {
                System.Windows.Forms.MessageBox.Show($"Mode sélectionné : {selectedMode.Key}\nDescription : {selectedMode.Value}");
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
            dossierDestination = DestinationTextBox.Text;
            dossierSource = SourceTextBox.Text;
            if (string.IsNullOrEmpty(dossierSource))
            {   
                System.Windows.Forms.MessageBox.Show($"Erreur, le dossier source requiére un dossier valide pour fonctionner");
            }
            else { }
        }
    }
}