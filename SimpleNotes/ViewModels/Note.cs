using System;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace SimpleNotes.ViewModels
{
    public class Note : INotifyPropertyChanged
    {
        private string oldName;
        private string name;
        public string Name { get => name; set { name = value; OnPropertyChanged(nameof(Name)); } }

        private string oldText;
        private string text;
        public string Text { get => text; set { text = value; OnPropertyChanged(nameof(Text)); } }

        public bool IsSaved => oldName == name && oldText == text;
        private void NoteIsSaved()
        {
            oldName = name;
            oldText = text;
        }

        public Note(string name = "", string text = "")
        {
            Name = name;
            Text = text;
        }

        public void Save(string folder)
        {
            string oldPath = Path.Combine(folder, $"{oldName}.txt");
            string newPath = Path.Combine(folder, $"{Name}.txt");
            if (oldName != Name)
            {
                if (File.Exists(oldPath))
                    File.Move(oldPath, newPath);
                else
                    File.CreateText(newPath).Close();
            }
            if (oldText != Text)
                using (StreamWriter writer = new StreamWriter(newPath, false, Encoding.UTF8))
                    writer.Write(Text);
            NoteIsSaved();
        }

        public Note Load(string path)
        {
            Name = Path.GetFileNameWithoutExtension(path);
            Text = File.ReadAllText(path, Encoding.UTF8);
            NoteIsSaved();
            return this;
        }

        public static Note FromFile(string path) => new Note().Load(path);

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
