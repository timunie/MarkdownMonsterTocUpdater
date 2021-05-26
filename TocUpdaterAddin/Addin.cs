using FontAwesome.WPF;
using MarkdownMonster;
using MarkdownMonster.AddIns;
using MarkdownMonster.Windows.DocumentOutlineSidebar;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Westwind.Utilities;

namespace TocUpdaterAddin
{
    /// <summary>
    /// @@@ TocUpdaterAddin @@@
    ///
    /// More info on Addin Development:
    /// https://markdownmonster.west-wind.com/docs/_4ne0s0qoi.htm
    /// </summary>
    public class TocUpdaterAddin : MarkdownMonsterAddin
    {
        private const string STR_StartDocumentOutline = "<!-- Start Document Outline -->";
        private const string STR_EndDocumentOutline = "<!-- End Document Outline -->";

        private static DocumentOutlineModel OutlineModel;
        private TocUpdaterAddinConfiguration Configuration { get; } = TocUpdaterAddinConfiguration.Current;

        private static readonly SelectiveStringComparer SelectiveStringComparer = new SelectiveStringComparer();

        /// <summary>
        /// Fired when the Addin is initially loaded. This is very early in
        /// the lifecycle and should only be used to create the addin name
        /// and UI options.
        /// </summary>
        /// <remarks>
        /// You do not have access to the Model or UI from this overload.
        /// </remarks>
        public override void OnApplicationStart()
        {
            base.OnApplicationStart();

            // Id - should match output folder name. REMOVE 'Addin' from the Id
            Id = "TocUpdater";

            // a descriptive name - shows up on labels and tooltips for components
            // REMOVE 'Addin' from the Name
            Name = "TOC Updater";

            // by passing in the add in you automatically
            // hook up OnExecute/OnExecuteConfiguration/OnCanExecute
            var menuItem = new AddInMenuItem(this)
            {
                Caption = Name,

                // if an icon is specified it shows on the toolbar
                // if not the add-in only shows in the add-ins menu
                FontawesomeIcon = FontAwesomeIcon.Magic
            };

            try
            {
                // We want a different icon based on the user selected theme
                var theme = Application.Current.Resources["Theme.BaseColorScheme"] as string;
                var iconFile = theme == "Dark" ? "Icon_22_Dark" : "Icon_22_Light";

                var icon = new BitmapImage();
                icon.BeginInit();
                icon.UriSource = new Uri($"pack://application:,,,/TocUpdaterAddin;component/Assets/{iconFile}.png", UriKind.RelativeOrAbsolute);
                icon.CacheOption = BitmapCacheOption.OnLoad;
                icon.EndInit();
                icon.Freeze();

                menuItem.IconImageSource = icon;
            }
            catch
            {
                // We did not recieve the icon. We will get the fallback value automatically, so no need to react here.
            }

            // if you don't want to display config or main menu item clear handler
            //menuItem.ExecuteConfiguration = null;

            // Must add the menu to the collection to display menu and toolbar items
            MenuItems.Add(menuItem);
        }

        /// <summary>
        /// Fired after the model has been loaded. If you need model access during loading
        /// this is the place to hook up your code.
        /// </summary>
        /// <param name="model">The Markdown Monster Application model</param>
        public override void OnModelLoaded(AppModel model)
        { }

        /// <summary>
        /// Fired after the Markdown Monster UI becomes available
        /// for manipulation.
        ///
        /// If you add UI elements as part of your Addin, this is the
        /// place where you can hook them up.
        /// </summary>
        public override void OnWindowLoaded()
        {
            OutlineModel = new DocumentOutlineModel();
        }

        /// <summary>
        /// Fired when you click the addin button in the toolbar.
        /// </summary>
        /// <param name="sender"></param>
        public override void OnExecute(object sender)
        {
            var wasEditorFocused = Model.IsEditorFocused;

            RefreshDocumentOutline();

            if (wasEditorFocused)
            {
                ActiveEditor.SetEditorFocus();
            }
        }

        /// <summary>
        /// Fired when you click on the configuration button in the addin
        /// </summary>
        /// <param name="sender">The Execute toolbar button for this addin</param>
        public override void OnExecuteConfiguration(object sender)
        {
            var dialog = new AddinSettings();
            dialog.ShowDialog();
        }

        /// <summary>
        /// Determines on whether the addin can be executed
        /// </summary>
        /// <param name="sender">The Execute toolbar button for this addin</param>
        /// <returns></returns>
        public override bool OnCanExecute(object sender)
        {
            // Only allow to run us if there is a doucment outline somewhere
            return ActiveDocument.CurrentText.Contains(STR_StartDocumentOutline);
        }

        public override bool OnBeforeSaveDocument(MarkdownDocument doc)
        {
            if (Configuration.RefreshTocBeforeSave)
            {
                RefreshDocumentOutline();
            }
            return base.OnBeforeSaveDocument(doc);
        }

        private void RefreshDocumentOutline()
        {
            var oldSelection = ActiveEditor.GetSelectionRange();
            var oldScrollPosition = ActiveEditor.GetScrollPosition();

            var md = ActiveEditor.GetMarkdown();

            if (!md.Contains(STR_StartDocumentOutline))
            {
                return;
            }

            var oldToc = StringUtils.ExtractString(md, STR_StartDocumentOutline, STR_EndDocumentOutline,
                returnDelimiters: true);

            var mdLines = md.GetLines();

            int tocPosition = -1;

            for (int i = 0; i < mdLines.Length; i++)
            {
                if (mdLines[i].Contains(STR_StartDocumentOutline, StringComparison.Ordinal))
                {
                    tocPosition = i;
                    break;
                }
            }

            var newToc = STR_StartDocumentOutline
                        + Environment.NewLine
                        + Environment.NewLine
                        + OutlineModel.CreateMarkdownOutline(ActiveDocument, tocPosition).TrimEnd()
                        + Environment.NewLine
                        + Environment.NewLine
                        + STR_EndDocumentOutline;

            // We don't want to disturb the user more than needed.
            // So let's check if we really have to replace the TOC.

            if (!SelectiveStringComparer.Equals(oldToc, newToc))
            {
                ActiveEditor.FindAndReplaceText(oldToc, newToc);

                // Restore the old selection. To do so we need to get the row offset
                var offset = oldSelection.StartRow > tocPosition ? StringUtils.CountLines(newToc) - StringUtils.CountLines(oldToc) : 0;

                ActiveEditor.SetScrollPosition(oldScrollPosition + offset);
                ActiveEditor.SetSelectionRange(oldSelection.StartRow + offset, oldSelection.StartColumn, oldSelection.EndRow + offset, oldSelection.EndColumn);
                ActiveEditor.SetEditorFocus();
                RefreshPreview();

                ShowStatus("Your TOC was updated", 2000);
            }
        }
    }
}