using FileSystem = Microsoft.VisualBasic.FileIO.FileSystem;
using UIOption = Microsoft.VisualBasic.FileIO.UIOption;
using RecycleOption = Microsoft.VisualBasic.FileIO.RecycleOption;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace SimpleNotes.ViewModels
{
    public class NotesManager : INotifyPropertyChanged
    {
        public ObservableCollection<Note> Notes { get; protected set; } = new ObservableCollection<Note>();
        private string directory;
        public string Directory { get => directory; set { directory = value; ReloadNotes(); OnPropertyChanged(nameof(Directory)); } }

        public NotesManager(string directory)
        {
            Directory = directory;
        }

        public void LoadNotes()
        {
            foreach (string notePath in System.IO.Directory.EnumerateFiles(Directory, "*.txt", SearchOption.TopDirectoryOnly))
                Notes.Add(Note.FromFile(notePath));
        }

        public void AddNote(Note note) => Notes.Add(note);

        public void SaveNotes()
        {
            foreach (Note note in Notes)
                note.Save(Directory);
        }

        public void SaveNote(Note note) => note.Save(Directory);

        public void DeleteNote(Note note)
        {
            if (!Notes.Contains(note))
                throw new ArgumentException("Cannot delete note that doesn't exist.");
            string path = Path.Combine(Directory, $"{note.Name}.txt");
            if (File.Exists(path))
                FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            Notes.Remove(note);
        }

        private void ReloadNotes()
        {
            Notes.Clear();
            LoadNotes();
        }

        public bool HasNote(string title) => Notes.Any(n => n.Name == title);

        public bool AllSaved() => Notes.All(n => n.IsSaved);

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
