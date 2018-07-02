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
            {
                Note note = new Note(Path.GetFileNameWithoutExtension(notePath), File.ReadAllText(notePath, Encoding.UTF8));
                Notes.Add(note);
                hashes.Add(note, ComputeHash(note));
            }
        }

        private void SaveNotesClick(object sender, RoutedEventArgs e) => SaveNotes();
        private void SaveNotes()
        {
            hashes.Clear();
            foreach (Note note in Notes)
            {
                string path = Path.Combine(NotesFolder, note.Name + ".txt");
                using (StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8))
                    writer.Write(note.Text);
                hashes.Add(note, ComputeHash(note));
            }
        }

        private void AddNoteClick(object sender, RoutedEventArgs e)
        {
            TextInputDialog dialog = new TextInputDialog();
            dialog.Owner = this;
            if (dialog.ShowDialog() == true)
                Notes.Add(new Note(dialog.Text));
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
    }
}
