using SimpleNotes.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace SimpleNotes
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<Note> Notes { get; set; } = new ObservableCollection<Note>();
        public string NotesFolder { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Notes"); // TODO: replace hardcoded value

        private Dictionary<Note, string> hashes = new Dictionary<Note, string>();

        public MainWindow()
        {
            InitializeComponent();
            LoadNotes();
        }

        private void LoadNotes()
        {
            foreach (string notePath in Directory.EnumerateFiles(NotesFolder, "*.txt", SearchOption.TopDirectoryOnly))
                AddNote(new Note(Path.GetFileNameWithoutExtension(notePath), File.ReadAllText(notePath, Encoding.Default)));
        }

        private void SaveNotesClick(object sender, RoutedEventArgs e) => SaveNotes();
        private void SaveNotes()
        {
            throw new NotImplementedException();
            // save note
            // compute new hashes
        }

        private void AddNoteClick(object sender, RoutedEventArgs e)
        {
            TextInputDialog dialog = new TextInputDialog();
            dialog.Owner = this;
            if (dialog.ShowDialog() == true)
                AddNote(new Note(dialog.Text));
        }

        private void AddNote(Note note)
        {
            Notes.Add(note);
            hashes.Add(note, ComputeHash(note));
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

        protected virtual string ComputeHash(Note note)
        {
            HashAlgorithm hasher = MD5.Create();
            byte[] hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(note.Text + note.Name));
            return BitConverter.ToString(hash);
        }

        protected virtual bool IsSaved()
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
    }
}
