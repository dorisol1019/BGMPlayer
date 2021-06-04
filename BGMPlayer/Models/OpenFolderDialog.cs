namespace BGMPlayer.Models
{
    internal class OpenFolderDialog
    {
        public string FolderName { get; private set; } = "";
        public void Show()
        {
            var dig = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog()
            {
                Title = "フォルダを開く",
                InitialDirectory = System.Environment.CurrentDirectory,
                IsFolderPicker = true,
                AddToMostRecentlyUsedList = false,
                Multiselect = false,
                ShowPlacesList = true,
                AllowNonFileSystemItems = false,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureValidNames = true,
                EnsureReadOnly = false,
                DefaultDirectory = System.Environment.CurrentDirectory
            };
            if (dig.ShowDialog() == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
            {
                FolderName = dig.FileName;
            }
        }
    }
}
