using SimpleNotes.Helpers;
using SimpleNotes.Models;
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
using System.Windows.Input;
using System.Windows.Markup;
using static SimpleNotes.TextInputDialog;

namespace SimpleNotes
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<Note> Notes { get; set; } = new ObservableCollection<Note>();
        public string NotesFolder { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Notes"); // TODO: replace hardcoded value

        private Dictionary<Note, string> hashes = new Dictionary<Note, string>();

        public MainWindow()
        {
            TrySettingCulture();
            InitializeComponent();
            LoadNotes();
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

        private void LoadNotes()
        {
            foreach (string notePath in Directory.EnumerateFiles(NotesFolder, "*.txt", SearchOption.TopDirectoryOnly))
            {
                Note note = new Note(Path.GetFileNameWithoutExtension(notePath), File.ReadAllText(notePath, Encoding.UTF8));
                Notes.Add(note);
                hashes.Add(note, ComputeHash(note));
            }
        }

        private void SaveAllCommandExecuted(object sender, ExecutedRoutedEventArgs e) => SaveNotes();
        private void SaveNotes()
        {
            hashes.Clear();
            foreach (Note note in Notes)
                SaveNote(note);
        }

        private void SaveCommandExecuted(object sender, ExecutedRoutedEventArgs e) => SaveNote();
        private void SaveNote(Note note)
        {
            string path = Path.Combine(NotesFolder, note.Name + ".txt");
            using (StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8))
                writer.Write(note.Text);
            // saving old note
            if (hashes.ContainsKey(note))
                hashes[note] = ComputeHash(note);
            // saving new note
            else
                hashes.Add(note, ComputeHash(note));
        }

        private void SaveNote()
        {
            if (notesTabControl.SelectedItem == null) return;
            Note note = notesTabControl.SelectedItem as Note;
            SaveNote(note);
        }

        private void AddNote()
        {
            TextInputDialog dialog = new TextInputDialog();
            dialog.Submitted += (s, e) => ValidateTitle(e);
            if (dialog.ShowDialog(this) == true)
                Notes.Add(new Note(dialog.Text));
        }

        private void ValidateTitle(SubmitEventArgs<string> eventArgs)
        {
            eventArgs.Cancel = true;
            if (eventArgs.Value == "")
                MessageBox.Show("Note name can't be empty", "Invalid name", MessageBoxButton.OK, MessageBoxImage.Information);
            else if (Notes.Any(n => n.Name == eventArgs.Value))
                MessageBox.Show("A note with this name already exists.", "Invalid name", MessageBoxButton.OK, MessageBoxImage.Information);
            else if (FileUtils.IsInvalidFileName(eventArgs.Value))
                MessageBox.Show("Note name must be a valid filename.", "Invalid name", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                eventArgs.Cancel = false;
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsSaved())
            {
                MessageBoxResult shouldSave = MessageBox.Show("Do you want to save your changes?", "Save changes?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (shouldSave == MessageBoxResult.Yes)
                    SaveNotes();
                else if (shouldSave == MessageBoxResult.Cancel)
                    e.Cancel = true;
            }
        }

        private string ComputeHash(Note note)
        {
            HashAlgorithm hasher = MD5.Create();
            byte[] hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(note.Text + note.Name));
            return BitConverter.ToString(hash);
        }

        private bool IsSaved()
        {
            if (Notes.Count != hashes.Count)
                return false;
            foreach (Note n in Notes)
            {
                if (!hashes.ContainsKey(n))
                    return false;
                string oldHash = hashes[n];
                string newHash = ComputeHash(n);
                if (oldHash != newHash)
                    return false;
            }
            return true;
        }

        private void NewCommandExecuted(object sender, ExecutedRoutedEventArgs e) => AddNote();
    }
}
