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

        private string notesFolder;
        public string NotesFolder { get => notesFolder; set { notesFolder = value; LoadNotes(); OnPropertyChanged(nameof(NotesFolder)); } }

        public NotesManager(string notesFolder)
        {
            NotesFolder = notesFolder;
        }

        public void LoadNotes()
        {
            if (NotesFolder != "" && Directory.Exists(NotesFolder))
            {
                Notes.Clear();
                foreach (string notePath in Directory.EnumerateFiles(NotesFolder, "*.txt", SearchOption.TopDirectoryOnly))
                    Notes.Add(Note.FromFile(notePath));
            }
        }

        public void AddNote(Note note) => Notes.Add(note);

        public void SaveNotes()
        {
            foreach (Note note in Notes)
                note.Save(NotesFolder);
        }

        public void SaveNote(Note note) => note.Save(NotesFolder);

        public void DeleteNote(Note note)
        {
            if (!Notes.Contains(note))
                throw new ArgumentException("Cannot delete note that doesn't exist.");
            string path = Path.Combine(NotesFolder, $"{note.Name}.txt");
            if (File.Exists(path))
                FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            Notes.Remove(note);
        }

        public bool HasNote(string title) => Notes.Any(n => n.Name == title);

        public bool AllSaved() => Notes.All(n => n.IsSaved);

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
