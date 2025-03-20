using System.Globalization;

namespace NotesAPP
{
    internal class Program
    {
        private static List<Note> notes = []; // List to store all notes
        private static JsonBase64FileData fileHandler = new(); // Handler for file operations
        private static CancellationTokenSource cancellationTokenSource = new(); // Managing background tasks

        private static bool notificationIsEnabled = false; // Toggle for reminders
        private static string fileName = "notes.json";

        private static void Main(string[] args)
        {
            // Load notes from file if it exists
            if (File.Exists(fileName))
            {
                notes = fileHandler.LoadFromFile<List<Note>>(fileName);
            }

            // Start the background task for checking notifications
            var notificationTask = CheckNotificationsAsync(cancellationTokenSource.Token);

            // Main menu loop
            while (true)
            {
                Console.Clear();

                DisplayMenu();

                // Get user's choice
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

                // Sort notes and save
                notes.Sort();
                fileHandler.SaveToFile(notes, fileName);

                // Wait for user input before continuing
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

        // Toggles the reminders on or off.
        private static void ToggleReminders()
        {
            notificationIsEnabled = !notificationIsEnabled;
            Console.WriteLine($"Reminders {(notificationIsEnabled ? "enabled" : "disabled")}.");
        }

        // Exits the program gracefully.
        private static void ExitProgram()
        {
            cancellationTokenSource.Cancel(); // Stop the background task
            Console.WriteLine("Exiting the program...");
            Environment.Exit(0);
        }

        static void CreateNewNote()
        {
            Note note = new(); // Create a new note object

            // Prompt the user to enter the note's title
            Console.Write("Enter title: ");
            note.title = Console.ReadLine();

            // Prompt the user to enter the note's category
            Console.Write("Enter category: ");
            note.category = Console.ReadLine();

            // Prompt the user to enter the reminder time in a specific format
            Console.Write("Enter reminder time (HH:mm dd-MM-yyyy): ");
            DateTime date = DateTime.MinValue;

            // Try to parse the input into a DateTime object
            bool success = DateTime.TryParseExact(Console.ReadLine(), "HH:mm dd-MM-yyyy", CultureInfo.CurrentCulture,
                DateTimeStyles.AllowLeadingWhite, out date);

            // Set the reminder time and display success/failure message
            note.reminderTime = date;
            Console.WriteLine($"{(success ? "Success" : "Failure")}: date set to {date}");

            // Prompt the user to enter the note's content
            Console.Write("Enter content: ");
            note.content = Console.ReadLine();

            // Add the note to the list
            notes.Add(note);

            // Confirm that the note has been added
            Console.WriteLine("Note added.");
        }

        static void ShowAllNotes()
        {
            // Check if there are any notes
            if (notes.Count == 0)
            {
                Console.WriteLine("No notes found.");
                return;
            }

            // Display all notes
            Console.WriteLine(new string('-', 20));
            for (int i = 0; i < notes.Count; i++)
            {
                Note note = notes[i];
                Console.WriteLine($"Note #{i + 1} ");
                Console.WriteLine($"Title: {note.title}");
                Console.WriteLine($"Category: {note.category}");
                Console.WriteLine($"Reminder time: {note.reminderTime}");
                Console.WriteLine($"Content: {note.content}");
                Console.WriteLine(new string('-', 20)); // Separator line
            }
        }

        static void ShowCategory()
        {
            // Get a list of unique categories
            List<string> categories = GetCategories();
            int idOfCategory = 0;

            if (categories.Count == 0)
            {
                Console.WriteLine("No categories found.");
                return;
            }

            // Prompt the user to select a category by number
            Console.WriteLine("Enter the category number:");

            // Display all categories
            for (int i = 0; i < categories.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {categories[i]}");
            }

            // Try to parse the user's input into an integer
            bool success = int.TryParse(Console.ReadLine(), out idOfCategory);

            // Validate the input
            if (idOfCategory <= 0 || idOfCategory > categories.Count || !success)
            {
                Console.WriteLine("Category not found.");
                return;
            }

            // Display all notes in the selected category
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

            // Prompt the user to enter the note's serial number
            Console.Write("Enter the note's serial number: ");
            bool success = int.TryParse(Console.ReadLine(), out id);

            // Validate the input
            if (id <= 0 || id > notes.Count || !success)
            {
                Console.WriteLine("Note not found.");
                return;
            }

            // Get the note to edit
            Note note = notes[id - 1];

            // Edit the title
            Console.WriteLine($"Title: {note.title} - replace with (leave blank to keep current):");
            string title = Console.ReadLine();
            note.title = string.IsNullOrEmpty(title) ? note.title : title;

            // Edit the category
            Console.WriteLine($"Category: {note.category} - replace with (leave blank to keep current):");
            string category = Console.ReadLine();
            note.category = string.IsNullOrEmpty(category) ? note.category : category;

            // Edit the reminder time
            Console.WriteLine($"Reminder time (HH:mm dd-MM-yyyy) - replace with (leave blank to keep current):");
            DateTime date = DateTime.MinValue;

            // Parse the input date
            DateTime.TryParseExact(Console.ReadLine(), "HH:mm dd-MM-yyyy", CultureInfo.CurrentCulture,
                DateTimeStyles.AllowLeadingWhite, out date);

            // Update the reminder time if a valid date was entered
            note.reminderTime = (date == DateTime.MinValue) ? note.reminderTime : date;

            // Edit the content 
            Console.WriteLine($"Content: {note.content} - replace with (leave blank to keep current):");
            string content = Console.ReadLine();
            note.content = string.IsNullOrEmpty(content) ? note.content : content;

            Console.WriteLine("Note updated successfully.");
        }
        static void DeleteNote()
        {
            // Prompt the user to enter the note's serial number
            Console.Write("Enter the note's serial number: ");

            int id = 0;

            // Try to parse the user's input into an integer
            bool success = int.TryParse(Console.ReadLine(), out id);

            // Validate the input
            if (id <= 0 || id > notes.Count || !success)
            {
                Console.WriteLine("Note not found.");
                return;
            }

            // Remove the note at the specified index
            notes.RemoveAt(id - 1);

            // Notify the user that the note has been deleted
            Console.WriteLine("Note deleted successfully.");
        }

        static async Task CheckNotificationsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested) // Run until cancellation is requested
            {
                foreach (var note in notes)
                {
                    // If reminder time is reached, notification is enabled, and not yet notified
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
            string title = Console.ReadLine().ToLower(); // Convert input to lowercase for case-insensitive search

            Console.Clear();

            // Iterate through all notes
            for (int i = 0; i < notes.Count; i++)
            {
                Note note = notes[i];

                // Check if the note's title contains the search term
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
            List<string> listOfCategories = new(); // Create a list to store unique categories

            // Iterate through all notes
            for (int i = 0; i < notes.Count; i++)
            {
                Note note = notes[i];

                // Add the category to the list if it's not already present
                if (!listOfCategories.Contains(note.category))
                {
                    listOfCategories.Add(note.category);
                }
            }

            return listOfCategories;
        }
    }
}