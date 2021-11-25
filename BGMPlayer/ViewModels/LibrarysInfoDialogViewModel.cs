using BGMPlayer.Models;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace BGMPlayer.ViewModels;

public class LibrarysInfoDialogViewModel : BindableBase, IDialogAware
{
    public string Title => "ライブラリの情報";

    public string Text { get; set; } = "";

    private readonly LibrarysInfo librarysInfo = new LibrarysInfo();

    public event Action<IDialogResult> RequestClose = (_) => { };

    public LibrarysInfoDialogViewModel()
    {
        Text = librarysInfo.Text;
    }

    public bool CanCloseDialog()
    {
        return true;
    }

    public void OnDialogClosed()
    {
    }

    public void OnDialogOpened(IDialogParameters parameters)
    {
    }
}
