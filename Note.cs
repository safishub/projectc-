namespace NotesAPP
{
    public class Note : IComparable<Note>
    {
        public string title { get; set; }
        public string category { get; set; }
        public DateTime reminderTime { get; set; }
        public string content { get; set; }
        public bool isNotificated { get; set; } = false;

        // Compare notes by category and title
        public int CompareTo(Note? note)
        {
            if (note is null) throw new ArgumentException("Incorrect compare");
            if (category == note.category) return title.CompareTo(note.title);
            return category.CompareTo(note.category);
        }
    }
    public class ListOfNotes
    {
        public List<Note> notes { get; set; }
    }
}
