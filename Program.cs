using System.Globalization;

namespace NotesAPP
{
    internal class Program
    {
        private static List<Note> notes = []; 
        private static JsonBase64FileData fileHandler = new(); // Handler for file operations
        private static CancellationTokenSource cancellationTokenSource = new(); 

        private static bool notificationIsEnabled = false; // reminders 
        private static string fileName = "notes.json";

        private static void Main(string[] args)
        {
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

                // Handle user's choice
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

                //sorting and file sving 
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
            cancellationTokenSource.Cancel(); // shuts the notifs 
            Console.WriteLine("Exiting the program...");
            Environment.Exit(0);
        }

        static void CreateNewNote()
        {
            Note note = new(); 
            Console.Write("Enter title: ");
            note.title = Console.ReadLine();
            Console.Write("Enter category: ");
            note.category = Console.ReadLine();
            Console.Write("Enter reminder time (HH:mm dd-MM-yyyy): "); //only this format works, otherwise shows error 
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
                Console.WriteLine(new string('-', 20)); // separation 
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
            foreach (Note note in notes)
            {
                if (note.category == categories[idOfCategory - 1])
                {
                    Console.WriteLine($"Title: {note.title}");
                    Console.WriteLine($"Category: {note.category}");
                    Console.WriteLine($"Reminder time: {note.reminderTime}");
                    Console.WriteLine($"Content: {note.content}");
                    Console.WriteLine(new string('-', 20)); // Separation
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
            Note note = notes[id - 1];

    //Title
            Console.WriteLine($"Title: {note.title} - replace with (leave blank to keep current):");
            string title = Console.ReadLine();
            note.title = string.IsNullOrEmpty(title) ? note.title : title;

            // Category
            Console.WriteLine($"Category: {note.category} - replace with (leave blank to keep current):");
            string category = Console.ReadLine();
            note.category = string.IsNullOrEmpty(category) ? note.category : category;

            //reminder time
            Console.WriteLine($"Reminder time (HH:mm dd-MM-yyyy) - replace with (leave blank to keep current):");
            DateTime date = DateTime.MinValue;
            DateTime.TryParseExact(Console.ReadLine(), "HH:mm dd-MM-yyyy", CultureInfo.CurrentCulture,
                DateTimeStyles.AllowLeadingWhite, out date);
 // Update the reminder time
            note.reminderTime = (date == DateTime.MinValue) ? note.reminderTime : date;

// Content
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
                        Console.Beep(); // cool sound
                        note.isNotificated = true;
                    }
                }
                await Task.Delay(1000, cancellationToken);
            }
        }

        static void FindNote()
        {
            Console.Write("Search by title: ");
            string title = Console.ReadLine().ToLower();

            Console.Clear();
            for (int i = 0; i < notes.Count; i++)
            {
                Note note = notes[i];
                if (note.title.ToLower().Contains(title))
                {
                    Console.WriteLine($"Title: {note.title}");
                    Console.WriteLine($"Category: {note.category}");
                    Console.WriteLine($"Reminder time: {note.reminderTime}");
                    Console.WriteLine($"Content: {note.content}");
                    Console.WriteLine(new string('-', 20));
                }
            }
        }

        static List<string> GetCategories()
        {
            List<string> listOfCategories = new();
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

