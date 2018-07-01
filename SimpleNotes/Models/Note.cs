namespace SimpleNotes.Models
{
    public class Note
    {
        public string Name { get; set; }
        public string Text { get; set; }

        public Note(string name, string text = "")
        {
            Name = name;
            Text = text;
        }
    }
}
