using MarkdownMonster.AddIns;

namespace TocUpdaterAddin
{
    public class TocUpdaterAddinConfiguration : BaseAddinConfiguration<TocUpdaterAddinConfiguration>
    {
        public TocUpdaterAddinConfiguration()
        {
            // uses this file for storing settings in `%appdata%\Markdown Monster`
            // to persist settings call `TocUpdaterAddinConfiguration.Current.Write()`
            // at any time or when the addin is shut down
            ConfigurationFilename = "TocUpdaterAddin.json";
        }

        // Add properties for any configuration setting you want to persist and reload
        // you can access this object as
        //     TocUpdaterAddinConfiguration.Current.PropertyName

        private bool _RefreshTocBeforeSave;

        public bool RefreshTocBeforeSave
        {
            get { return _RefreshTocBeforeSave; }
            set { _RefreshTocBeforeSave = value; OnPropertyChanged(nameof(RefreshTocBeforeSave)); }
        }
    }
}