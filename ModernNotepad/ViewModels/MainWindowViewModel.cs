using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ModernNotepad.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _text = "";
        private string? _filePath;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand NewFileCommand { get; }
        public ICommand OpenFileCommand { get; }
        public ICommand SaveFileCommand { get; }
        public ICommand SaveFileAsCommand { get; }

        public MainWindowViewModel()
        {
            NewFileCommand = new AsyncRelayCommand(NewFile);
            OpenFileCommand = new AsyncRelayCommand(OpenFile);
            SaveFileCommand = new AsyncRelayCommand(SaveFile);
            SaveFileAsCommand = new AsyncRelayCommand(SaveFileAs);
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task NewFile()
        {
            // Eğer metin varsa kaydetmek isteyip istemediğini sor
            if (!string.IsNullOrEmpty(Text))
            {
                var mainWindow = GetMainWindow();
                if (mainWindow != null)
                {
                    var result = await ShowMessageBox(mainWindow, 
                        "Kaydetmek istiyor musunuz?", 
                        "Mevcut dosya kaydedilmemiş değişiklikler içeriyor. Kaydetmek istiyor musunuz?");
                    
                    if (result == "Yes")
                    {
                        await SaveFile();
                    }
                    else if (result == "Cancel")
                    {
                        return; // İptal - hiçbir şey yapma
                    }
                }
            }
            
            Text = "";
            _filePath = null;
        }

        private async Task<string> ShowMessageBox(Window owner, string title, string message)
        {
            var dialog = new Window
            {
                Title = title,
                Width = 400,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            var panel = new StackPanel { Margin = new Avalonia.Thickness(20) };
            
            panel.Children.Add(new TextBlock 
            { 
                Text = message,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Margin = new Avalonia.Thickness(0, 0, 0, 20)
            });

            var buttonPanel = new StackPanel 
            { 
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
            };

            string? result = null;

            var yesButton = new Button 
            { 
                Content = "Evet",
                Width = 80,
                Margin = new Avalonia.Thickness(0, 0, 10, 0)
            };
            yesButton.Click += (s, e) => { result = "Yes"; dialog.Close(); };

            var noButton = new Button 
            { 
                Content = "Hayır",
                Width = 80,
                Margin = new Avalonia.Thickness(0, 0, 10, 0)
            };
            noButton.Click += (s, e) => { result = "No"; dialog.Close(); };

            var cancelButton = new Button 
            { 
                Content = "İptal",
                Width = 80
            };
            cancelButton.Click += (s, e) => { result = "Cancel"; dialog.Close(); };

            buttonPanel.Children.Add(yesButton);
            buttonPanel.Children.Add(noButton);
            buttonPanel.Children.Add(cancelButton);

            panel.Children.Add(buttonPanel);
            dialog.Content = panel;

            await dialog.ShowDialog(owner);
            return result ?? "Cancel";
        }

        private async Task OpenFile()
        {
            var mainWindow = GetMainWindow();
            if (mainWindow?.StorageProvider != null)
            {
                var result = await mainWindow.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
                {
                    Title = "Dosya Aç",
                    AllowMultiple = false
                });
                
                if (result.Count > 0)
                {
                    var file = result[0];
                    using var stream = await file.OpenReadAsync();
                    using var reader = new StreamReader(stream);
                    Text = await reader.ReadToEndAsync();
                    _filePath = file.Path.LocalPath;
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
            var mainWindow = GetMainWindow();
            if (mainWindow?.StorageProvider != null)
            {
                var result = await mainWindow.StorageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
                {
                    Title = "Farklı Kaydet"
                });
                
                if (result != null)
                {
                    _filePath = result.Path.LocalPath;
                    using var stream = await result.OpenWriteAsync();
                    using var writer = new StreamWriter(stream);
                    await writer.WriteAsync(Text);
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

    // Basit Command implementasyonu
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public event EventHandler? CanExecuteChanged;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => _execute();

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    // Async Command implementasyonu
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;
        private bool _isExecuting;

        public event EventHandler? CanExecuteChanged;

        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => !_isExecuting && (_canExecute?.Invoke() ?? true);

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            _isExecuting = true;
            RaiseCanExecuteChanged();

            try
            {
                await _execute();
            }
            finally
            {
                _isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
