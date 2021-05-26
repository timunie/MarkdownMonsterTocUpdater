using MahApps.Metro.Controls;

namespace TocUpdaterAddin
{
    /// <summary>
    /// Interaction logic for AddinSettings.xaml
    /// </summary>
    public partial class AddinSettings : MetroWindow
    {
        public AddinSettings()
        {
            InitializeComponent();
        }

        private void CloseSettings_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }
    }
}