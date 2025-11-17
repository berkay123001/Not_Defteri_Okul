using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModernNotepad.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private string _text = "";
        private string? _filePath;

        public string Text
        {
            get => _text;
            set => this.RaiseAndSetIfChanged(ref _text, value);
        }

        public ICommand NewFileCommand { get; }
        public ICommand OpenFileCommand { get; }
        public ICommand SaveFileCommand { get; }
        public ICommand SaveFileAsCommand { get; }

        public MainWindowViewModel()
        {
            NewFileCommand = ReactiveCommand.Create(NewFile);
            OpenFileCommand = ReactiveCommand.CreateFromTask(OpenFile);
            SaveFileCommand = ReactiveCommand.CreateFromTask(SaveFile);
            SaveFileAsCommand = ReactiveCommand.CreateFromTask(SaveFileAs);
        }

        private void NewFile()
        {
            Text = "";
            _filePath = null;
        }

        private async Task OpenFile()
        {
            var dialog = new OpenFileDialog();
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                var result = await dialog.ShowAsync(mainWindow);
                if (result != null && result.Length > 0)
                {
                    _filePath = result[0];
                    Text = await File.ReadAllTextAsync(_filePath);
                }
            }
        }

        private async Task SaveFile()
        {
            if (_filePath == null)
            {
                await SaveFileAs();
            }
            else
            {
                await File.WriteAllTextAsync(_filePath, Text);
            }
        }

        private async Task SaveFileAs()
        {
            var dialog = new SaveFileDialog();
            var mainWindow = GetMainWindow();
            if (mainWindow != null)
            {
                var result = await dialog.ShowAsync(mainWindow);
                if (result != null)
                {
                    _filePath = result;
                    await File.WriteAllTextAsync(_filePath, Text);
                }
            }
        }

        private Window? GetMainWindow()
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                return desktop.MainWindow;
            }
            return null;
        }
    }
}
