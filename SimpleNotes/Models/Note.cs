using System.ComponentModel;

namespace SimpleNotes.Models
{
    public class Note : INotifyPropertyChanged
    {
        private string name;
        public string Name { get => name; set { name = value; OnPropertyChanged(nameof(Name)); } }
        public string Text { get; set; }

        public Note(string name, string text = "")
        {
            Name = name;
            Text = text;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
