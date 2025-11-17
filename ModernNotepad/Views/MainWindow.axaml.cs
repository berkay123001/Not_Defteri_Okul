using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Threading.Tasks;

namespace ModernNotepad.Views
{
    public partial class MainWindow : Window
    {
        private TextBox? editorTextBox;

        public MainWindow()
        {
            InitializeComponent();
            
            var exitMenuItem = this.FindControl<MenuItem>("ExitMenuItem");
            if (exitMenuItem != null)
                exitMenuItem.Click += ExitMenuItem_Click;

            editorTextBox = this.FindControl<TextBox>("EditorTextBox");
            
            // TextBox'a focus ver
            if (editorTextBox != null)
            {
                editorTextBox.AttachedToVisualTree += (s, e) => editorTextBox.Focus();
            }

            var undoMenuItem = this.FindControl<MenuItem>("UndoMenuItem");
            if (undoMenuItem != null) undoMenuItem.Click += UndoMenuItem_Click;
            
            var redoMenuItem = this.FindControl<MenuItem>("RedoMenuItem");
            if (redoMenuItem != null) redoMenuItem.Click += RedoMenuItem_Click;

            var aboutMenuItem = this.FindControl<MenuItem>("AboutMenuItem");
            if (aboutMenuItem != null) aboutMenuItem.Click += AboutMenuItem_Click;
            
            // Klavye kısayollarını ekle
            SetupKeyBindings();
        }

        private void SetupKeyBindings()
        {
            this.KeyDown += async (s, e) =>
            {
                if (e.KeyModifiers == KeyModifiers.Control)
                {
                    switch (e.Key)
                    {
                        case Key.Z:
                            UndoMenuItem_Click(null, null!);
                            e.Handled = true;
                            break;
                        case Key.Y:
                            RedoMenuItem_Click(null, null!);
                            e.Handled = true;
                            break;
                        case Key.X:
                            await CutMenuItemAsync();
                            e.Handled = true;
                            break;
                        case Key.C:
                            await CopyMenuItemAsync();
                            e.Handled = true;
                            break;
                        case Key.V:
                            await PasteMenuItemAsync();
                            e.Handled = true;
                            break;
                    }
                }
            };
        }

        private void UndoMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Undo clicked");
            if (editorTextBox != null)
            {
                System.Diagnostics.Debug.WriteLine($"TextBox found, CanUndo: {editorTextBox.CanUndo}");
                editorTextBox.Undo();
            }
        }

        private void RedoMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Redo clicked");
            if (editorTextBox != null)
            {
                System.Diagnostics.Debug.WriteLine($"TextBox found, CanRedo: {editorTextBox.CanRedo}");
                editorTextBox.Redo();
            }
        }

        private async void CutMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            await CutMenuItemAsync();
        }

        private async Task CutMenuItemAsync()
        {
            System.Diagnostics.Debug.WriteLine("Cut clicked");
            if (editorTextBox != null)
            {
                var selectedText = editorTextBox.SelectedText;
                System.Diagnostics.Debug.WriteLine($"SelectedText: '{selectedText}'");
                
                if (!string.IsNullOrEmpty(selectedText))
                {
                    try
                    {
                        var clipboard = this.Clipboard;
                        if (clipboard != null)
                        {
                            await clipboard.SetTextAsync(selectedText);
                            System.Diagnostics.Debug.WriteLine("Text copied to clipboard");
                            
                            var selectionStart = editorTextBox.SelectionStart;
                            var selectionLength = editorTextBox.SelectionEnd - editorTextBox.SelectionStart;
                            var text = editorTextBox.Text ?? "";
                            editorTextBox.Text = text.Remove(selectionStart, selectionLength);
                            editorTextBox.CaretIndex = selectionStart;
                            System.Diagnostics.Debug.WriteLine("Cut completed");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Cut error: {ex.Message}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No text selected");
                }
            }
        }

        private async void CopyMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            await CopyMenuItemAsync();
        }

        private async Task CopyMenuItemAsync()
        {
            System.Diagnostics.Debug.WriteLine("Copy clicked");
            if (editorTextBox != null)
            {
                var selectedText = editorTextBox.SelectedText;
                System.Diagnostics.Debug.WriteLine($"SelectedText: '{selectedText}'");
                
                if (!string.IsNullOrEmpty(selectedText))
                {
                    try
                    {
                        var clipboard = this.Clipboard;
                        if (clipboard != null)
                        {
                            await clipboard.SetTextAsync(selectedText);
                            System.Diagnostics.Debug.WriteLine("Copy completed");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Copy error: {ex.Message}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No text selected");
                }
            }
        }

        private async void PasteMenuItem_Click(object? sender, RoutedEventArgs e)
        {
            await PasteMenuItemAsync();
        }

        private async Task PasteMenuItemAsync()
        {
            System.Diagnostics.Debug.WriteLine("Paste clicked");
            if (editorTextBox != null)
            {
                try
                {
                    var clipboard = this.Clipboard;
                    if (clipboard != null)
                    {
                        var text = await clipboard.GetTextAsync();
                        System.Diagnostics.Debug.WriteLine($"Clipboard text: '{text}'");
                        
                        if (!string.IsNullOrEmpty(text))
                        {
                            var selectionStart = editorTextBox.SelectionStart;
                            var selectionLength = editorTextBox.SelectionEnd - editorTextBox.SelectionStart;
                            var currentText = editorTextBox.Text ?? "";
                            
                            if (selectionLength > 0)
                            {
                                currentText = currentText.Remove(selectionStart, selectionLength);
                            }
                            
                            editorTextBox.Text = currentText.Insert(selectionStart, text);
                            editorTextBox.CaretIndex = selectionStart + text.Length;
                            System.Diagnostics.Debug.WriteLine("Paste completed");
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Paste error: {ex.Message}");
                }
            }
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
