using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml.Serialization;

namespace NotesReplicatorX
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<Configuration> Configurations { get; set; }
        private const string SettingsFileName = "configurations.xml";
        private string SettingsPath = AssemblyDirectory + "\\" + SettingsFileName;

        public MainWindow()
        {
            InitializeComponent();
            LoadConfigurations();
            listBoxConfigurations.ItemsSource = Configurations;
            listBoxConfigurations.DisplayMemberPath = "Name";
            listBoxConfigurations.SelectedIndex = 0;
            _ = listBoxConfigurations.Focus();
        }

        private static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            SaveConfigurations();
        }

        private void SaveConfigurations()
        {
            using (StreamWriter sw = new StreamWriter(SettingsPath))
            {
                XmlSerializer xmls = new XmlSerializer(typeof(ObservableCollection<Configuration>));
                xmls.Serialize(sw, Configurations);
            }
        }

        private void LoadConfigurations()
        {
            if (File.Exists(SettingsPath))
            {
                using (StreamReader sr = new StreamReader(SettingsPath))
                {
                    XmlSerializer xmls = new XmlSerializer(typeof(ObservableCollection<Configuration>));
                    Configurations = xmls.Deserialize(sr) as ObservableCollection<Configuration>;
                }
            }
            else
            {
                Configurations = new ObservableCollection<Configuration>();
            }
        }

        private void BtnSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            foreach (Configuration c in Configurations)
            {
                if (c.Name.Equals(tbSaveAsConfigName.Text))
                {
                    c.SourceDirectory = tbSourceDirectory.Text;
                    c.SourceServer = tbSourceServer.Text;
                    c.TargetServer = tbTargetServer.Text;
                    return;
                }
            }
            Configuration newConfig = new Configuration { Name = tbSaveAsConfigName.Text, SourceDirectory = tbSourceDirectory.Text, SourceServer = tbSourceServer.Text, TargetServer = tbTargetServer.Text };
            Configurations.Add(newConfig);
        }

        private void BtnRemoveConfig_Click(object sender, RoutedEventArgs e)
        {
            Configuration sel = (Configuration)listBoxConfigurations.SelectedItem;
            Configurations.Remove(sel);
        }

        private void BtnAddConfig_Click(object sender, RoutedEventArgs e)
        {
            Configurations.Add(new Configuration());
            listBoxConfigurations.SelectedIndex = Configurations.Count - 1;
            _ = tbSourceServer.Focus();
        }

        private void BtnReplicate_Click(object sender, RoutedEventArgs e)
        {
            Configuration config = (Configuration)listBoxConfigurations.SelectedItem;
            Replicator replicator = new Replicator(config.SourceServer, config.TargetServer, config.SourceDirectory);
            replicator.Replicate();
        }
    }
}
