using SimpleNotes.Converters;
using SimpleNotes.Helpers;
using SimpleNotes.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using static SimpleNotes.TextInputDialog;

namespace SimpleNotes
{
    public partial class MainWindow : Window
    {
        public NotesManager NotesManager { get; set; } = new NotesManager(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Notes")); // TODO: load from saved settings
        private Note SelectedNote => notesTabControl.SelectedItem as Note;

        public MainWindow()
        {
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
            if (SelectedNote == null) return;
            NotesManager.SaveNote(SelectedNote);
        }

        private void DeleteCurrentNote()
        {
            if (SelectedNote == null) return;
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
        }

        private void SaveAllCommandExecuted(object sender, ExecutedRoutedEventArgs e) => NotesManager.SaveNotes();
        private void SaveCommandExecuted(object sender, ExecutedRoutedEventArgs e) => SaveCurrentNote();
        private void NewCommandExecuted(object sender, ExecutedRoutedEventArgs e) => AddNote();
        private void DeleteCommandExecuted(object sender, ExecutedRoutedEventArgs e) => DeleteCurrentNote();
        private void RenameCommandExecuted(object sender, ExecutedRoutedEventArgs e) => RenameCurrentNote();
    }
}
