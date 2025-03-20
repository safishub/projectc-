using System.Globalization;

namespace NotesAPP
{
    internal class Program
    {
        private static List<Note> notes = []; 
        private static JsonBase64FileData fileHandler = new(); 
        private static CancellationTokenSource cancellationTokenSource = new(); background tasks

        private static bool notificationIsEnabled = false; // reminders
        private static string fileName = "notes.json";

        private static void Main(string[] args)
        {
            // Load notes from file if it exists
            if (File.Exists(fileName))
            {
                notes = fileHandler.LoadFromFile<List<Note>>(fileName);
            }

        
            var notificationTask = CheckNotificationsAsync(cancellationTokenSource.Token);

            
            while (true)
            {
                Console.Clear();

                DisplayMenu();

                
                string choice = Console.ReadLine();

                Console.Clear();

                switch (choice)
                {
                    case "1":
                        CreateNewNote();
                        break;
                    case "2":
                        ShowCategory();
                        break;
                    case "3":
                        ShowAllNotes();
                        break;
                    case "4":
                        FindNote();
                        break;
                    case "5":
                        ShowAllNotes();
                        EditNote();
                        break;
                    case "6":
                        ShowAllNotes();
                        DeleteNote();
                        break;
                    case "7":
                        ToggleReminders();
                        break;
                    case "8":
                        ExitProgram();
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }

                
                notes.Sort();
                fileHandler.SaveToFile(notes, fileName);

                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }

        private static void DisplayMenu()
        {
            Console.WriteLine("Menu:");
            Console.WriteLine("1. Add a note");
            Console.WriteLine("2. View categories");
            Console.WriteLine("3. View all notes");
            Console.WriteLine("4. Find a note");
            Console.WriteLine("5. Edit a note");
            Console.WriteLine("6. Delete a note");
            Console.WriteLine($"7. {(notificationIsEnabled ? "Disable" : "Enable")} reminders");
            Console.WriteLine("8. Exit");
            Console.Write("Choose an option: ");
        }

        private static void ToggleReminders()
        {
            notificationIsEnabled = !notificationIsEnabled;
            Console.WriteLine($"Reminders {(notificationIsEnabled ? "enabled" : "disabled")}.");
        }

        private static void ExitProgram()
        {
            cancellationTokenSource.Cancel(); // Stop the background task
            Console.WriteLine("Exiting the program...");
            Environment.Exit(0);
        }

        static void CreateNewNote()
        {
            Note note = new(); // Create a new note object

            
            Console.Write("Enter title: ");
            note.title = Console.ReadLine();

            
            Console.Write("Enter category: ");
            note.category = Console.ReadLine();

            
            Console.Write("Enter reminder time (HH:mm dd-MM-yyyy): ");
            DateTime date = DateTime.MinValue;

            
            bool success = DateTime.TryParseExact(Console.ReadLine(), "HH:mm dd-MM-yyyy", CultureInfo.CurrentCulture,
                DateTimeStyles.AllowLeadingWhite, out date);

            
            note.reminderTime = date;
            Console.WriteLine($"{(success ? "Success" : "Failure")}: date set to {date}");

            
            Console.Write("Enter content: ");
            note.content = Console.ReadLine();

            
            notes.Add(note);

            
            Console.WriteLine("Note added.");
        }

        static void ShowAllNotes()
        {
            
            if (notes.Count == 0)
            {
                Console.WriteLine("No notes found.");
                return;
            }

            
            Console.WriteLine(new string('-', 20));
            for (int i = 0; i < notes.Count; i++)
            {
                Note note = notes[i];
                Console.WriteLine($"Note #{i + 1} ");
                Console.WriteLine($"Title: {note.title}");
                Console.WriteLine($"Category: {note.category}");
                Console.WriteLine($"Reminder time: {note.reminderTime}");
                Console.WriteLine($"Content: {note.content}");
                Console.WriteLine(new string('-', 20)); // Separation
            }
        }

        static void ShowCategory()
        {
            
            List<string> categories = GetCategories();
            int idOfCategory = 0;

            if (categories.Count == 0)
            {
                Console.WriteLine("No categories found.");
                return;
            }

            
            Console.WriteLine("Enter the category number:");

            
            for (int i = 0; i < categories.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {categories[i]}");
            }

            bool success = int.TryParse(Console.ReadLine(), out idOfCategory);

            if (idOfCategory <= 0 || idOfCategory > categories.Count || !success)
            {
                Console.WriteLine("Category not found.");
                return;
            }

            // all notes in the selected category
            foreach (Note note in notes)
            {
                if (note.category == categories[idOfCategory - 1])
                {
                    Console.WriteLine($"Title: {note.title}");
                    Console.WriteLine($"Category: {note.category}");
                    Console.WriteLine($"Reminder time: {note.reminderTime}");
                    Console.WriteLine($"Content: {note.content}");
                    Console.WriteLine(new string('-', 20)); // Separator line
                }
            }
        }

        static void EditNote()
        {
            int id = 0;

            
            Console.Write("Enter the note's serial number: ");
            bool success = int.TryParse(Console.ReadLine(), out id);

            if (id <= 0 || id > notes.Count || !success)
            {
                Console.WriteLine("Note not found.");
                return;
            }

            // Get the note to edit
            Note note = notes[id - 1];

            Console.WriteLine($"Title: {note.title} - replace with (leave blank to keep current):");
            string title = Console.ReadLine();
            note.title = string.IsNullOrEmpty(title) ? note.title : title;

            Console.WriteLine($"Category: {note.category} - replace with (leave blank to keep current):");
            string category = Console.ReadLine();
            note.category = string.IsNullOrEmpty(category) ? note.category : category;

            Console.WriteLine($"Reminder time (HH:mm dd-MM-yyyy) - replace with (leave blank to keep current):");
            DateTime date = DateTime.MinValue;

            DateTime.TryParseExact(Console.ReadLine(), "HH:mm dd-MM-yyyy", CultureInfo.CurrentCulture,
                DateTimeStyles.AllowLeadingWhite, out date);

            note.reminderTime = (date == DateTime.MinValue) ? note.reminderTime : date;
 
            Console.WriteLine($"Content: {note.content} - replace with (leave blank to keep current):");
            string content = Console.ReadLine();
            note.content = string.IsNullOrEmpty(content) ? note.content : content;

            Console.WriteLine("Note updated successfully.");
        }
        static void DeleteNote()
        {

            Console.Write("Enter the note's serial number: ");

            int id = 0;

            bool success = int.TryParse(Console.ReadLine(), out id);

            if (id <= 0 || id > notes.Count || !success)
            {
                Console.WriteLine("Note not found.");
                return;
            }

            notes.RemoveAt(id - 1);

            Console.WriteLine("Note deleted successfully.");
        }

        static async Task CheckNotificationsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested) 
            {
                foreach (var note in notes)
                {
                    
                    if (DateTime.Now >= note.reminderTime && !note.isNotificated && notificationIsEnabled)
                    {
                        Console.WriteLine($"\n\nNotification: {note.title} - {note.content}");
                        Console.Beep(); // Play a beep sound
                        note.isNotificated = true; // Mark as notified
                    }
                }
                await Task.Delay(1000, cancellationToken); // Wait for 1 second
            }
        }

        static void FindNote()
        {
            Console.Write("Search by title: ");
            string title = Console.ReadLine().ToLower(); // Convert input to lowercase

            Console.Clear();

            // Iterate through all notes
            for (int i = 0; i < notes.Count; i++)
            {
                Note note = notes[i];

                if (note.title.ToLower().Contains(title))
                {
                    Console.WriteLine($"Title: {note.title}");
                    Console.WriteLine($"Category: {note.category}");
                    Console.WriteLine($"Reminder time: {note.reminderTime}");
                    Console.WriteLine($"Content: {note.content}");
                    Console.WriteLine(new string('-', 20)); // Separator line
                }
            }
        }

        static List<string> GetCategories()
        {
            List<string> listOfCategories = new(); 

            // Iterate through all notes
            for (int i = 0; i < notes.Count; i++)
            {
                Note note = notes[i];

                
                if (!listOfCategories.Contains(note.category))
                {
                    listOfCategories.Add(note.category);
                }
            }

            return listOfCategories;
        }
    }
}
