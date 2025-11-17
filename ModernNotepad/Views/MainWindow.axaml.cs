using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;

namespace ModernNotepad.Views
{
    public partial class MainWindow : Window
    {
        private TextBox editorTextBox;

        public MainWindow()
        {
            InitializeComponent();
            this.FindControl<MenuItem>("ExitMenuItem").Click += ExitMenuItem_Click;

            editorTextBox = this.FindControl<TextBox>("EditorTextBox");

            this.FindControl<MenuItem>("UndoMenuItem").Click += (s, e) => editorTextBox?.Undo();
            this.FindControl<MenuItem>("RedoMenuItem").Click += (s, e) => editorTextBox?.Redo();
            this.FindControl<MenuItem>("CutMenuItem").Click += (s, e) => editorTextBox?.Cut();
            this.FindControl<MenuItem>("CopyMenuItem").Click += (s, e) => editorTextBox?.Copy();
            this.FindControl<MenuItem>("PasteMenuItem").Click += (s, e) => editorTextBox?.Paste();

            this.FindControl<MenuItem>("AboutMenuItem").Click += AboutMenuItem_Click;
        }

        private void ExitMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }

        private async void AboutMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            await aboutWindow.ShowDialog(this);
        }
    }
}
