using System.Text;
using System.Text.Json;

namespace NotesAPP
{
    public class JsonBase64FileData
    { 
        public void SaveToFile<T>(T data, string path)
        {
            string json = JsonSerializer.Serialize(data);

            string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

            File.WriteAllText(path, base64);
        }

        
        public T LoadFromFile<T>(string path)
        {
            string base64 = File.ReadAllText(path);

            string json = Encoding.UTF8.GetString(Convert.FromBase64String(base64));

            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
