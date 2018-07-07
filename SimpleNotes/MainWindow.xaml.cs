using Ookii.Dialogs.Wpf;
using SimpleNotes.Helpers;
using SimpleNotes.Properties;
using SimpleNotes.ViewModels;
using System;
using System.Globalization;
using System.IO;
using System.Security;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using static SimpleNotes.Helpers.StringUtils;
using static SimpleNotes.TextInputDialog;

namespace SimpleNotes
{
    public partial class MainWindow : Window
    {
        public NotesManager NotesManager { get; set; }
        private Note SelectedNote => notesTabControl.SelectedItem as Note;

        public MainWindow()
        {
            NotesManager = new NotesManager(Settings.Default.NotesFolderPath);
            TrySettingCulture();
            InitializeComponent();
            NotesManager.LoadNotes();
        }

        private void TrySettingCulture()
        {
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
                LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
                    new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
            }
            catch (CultureNotFoundException) { } // Stick with default culture if not present on system
        }

        private void AddNote()
        {
            TextInputDialog dialog = new TextInputDialog();
            dialog.Submitted += (s, e) => ValidateTitle(e);
            if (dialog.ShowDialog(this) == true)
            {
                Note note = new Note(dialog.Text);
                NotesManager.AddNote(note);
                notesTabControl.SelectedItem = note;
            }
        }

        private void SaveCurrentNote()
        {
            if (SelectedNote != null)
                NotesManager.SaveNote(SelectedNote);
        }

        private void DeleteCurrentNote()
        {
            if (SelectedNote != null)
                NotesManager.DeleteNote(SelectedNote);
        }

        private void RenameCurrentNote()
        {
            if (SelectedNote == null) return;
            TextInputDialog dialog = new TextInputDialog(SelectedNote.Name);
            dialog.Submitted += (s, e) => ValidateTitle(e);
            if (dialog.ShowDialog(this) == true)
                SelectedNote.Name = dialog.Text;
        }

        private void ChooseFolder()
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            if (Directory.Exists(NotesManager.NotesFolder))
                dialog.SelectedPath = FileUtils.AddDirectorySeparator(NotesManager.NotesFolder); // open current folder, not just select its name
            if (dialog.ShowDialog(this).GetValueOrDefault(false))
                NotesManager.NotesFolder = dialog.SelectedPath;
        }

        private void ValidateTitle(SubmitEventArgs<string> eventArgs)
        {
            eventArgs.Cancel = true;
            if (eventArgs.Value == "")
                MessageBox.Show("Note name can't be empty", "Invalid name", MessageBoxButton.OK, MessageBoxImage.Information);
            else if (NotesManager.HasNote(eventArgs.Value))
                MessageBox.Show("A note with this name already exists.", "Invalid name", MessageBoxButton.OK, MessageBoxImage.Information);
            else if (FileUtils.IsInvalidFileName(eventArgs.Value))
                MessageBox.Show("Note name must be a valid filename.", "Invalid name", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                eventArgs.Cancel = false;
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!NotesManager.AllSaved())
            {
                MessageBoxResult shouldSave = MessageBox.Show("Do you want to save your changes?", "Save changes?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (shouldSave == MessageBoxResult.Yes)
                    NotesManager.SaveNotes();
                else if (shouldSave == MessageBoxResult.Cancel)
                    e.Cancel = true;
            }
            Settings.Default.NotesFolderPath = NotesManager.NotesFolder;
            Settings.Default.Save();
        }

        private void HandleExceptions(Action action)
        {
            try { action(); }
            catch (UnauthorizedAccessException)
            { MessageBox.Show($"The application is not authorized to acces a note file.", "Not authorized", MessageBoxButton.OK, MessageBoxImage.Error); }
            catch (PathTooLongException)
            { MessageBox.Show($"The path to a note file is too long.", "Path too long", MessageBoxButton.OK, MessageBoxImage.Error); }
            catch (DirectoryNotFoundException)
            { MessageBox.Show($"The specified directory was not found.", "Directory not found", MessageBoxButton.OK, MessageBoxImage.Error); }
            catch (IOException)
            { MessageBox.Show($"There was a problem writing or reading from a file.{Nl}Maybe the file doesn't exist or is used by another program.", "Could't access file", MessageBoxButton.OK, MessageBoxImage.Error); }
            catch (NotSupportedException)
            { MessageBox.Show($"The required operation is not supported vy your system.", "Operation not supported", MessageBoxButton.OK, MessageBoxImage.Error); }
            catch (SecurityException)
            { MessageBox.Show($"The required operation couldn't be performed because of insufficient permissions.", "Security error", MessageBoxButton.OK, MessageBoxImage.Error); }
            catch (ObjectDisposedException)
            { MessageBox.Show($"The program tried using a file, which was already closed{Nl}Please try again.", "File already closed", MessageBoxButton.OK, MessageBoxImage.Error); }
            catch (Exception ex)
            { MessageBox.Show($"A problem occured while running the requested operation.{Nl}We are sorry for any inconveniences.{Nl}{Nl}Details: {ex.Message}", "An exception occured", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void SaveAllCommandExecuted(object sender, ExecutedRoutedEventArgs e) => HandleExceptions(NotesManager.SaveNotes);
        private void SaveCommandExecuted(object sender, ExecutedRoutedEventArgs e) => HandleExceptions(SaveCurrentNote);
        private void NewCommandExecuted(object sender, ExecutedRoutedEventArgs e) => HandleExceptions(AddNote);
        private void DeleteCommandExecuted(object sender, ExecutedRoutedEventArgs e) => HandleExceptions(DeleteCurrentNote);
        private void RenameCommandExecuted(object sender, ExecutedRoutedEventArgs e) => HandleExceptions(RenameCurrentNote);
        private void OpenFolderCommandExecutes(object sender, ExecutedRoutedEventArgs e) => HandleExceptions(ChooseFolder);
    }
}
