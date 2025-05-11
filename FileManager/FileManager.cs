using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;


namespace Recorder.FileManager
{


    public class FileManager<T>
    {
        private readonly string filePath;

        public FileManager(string path)
        {
            filePath = path;
        }

        public void SaveToFile(List<T> objects)
        {
            string json = JsonConvert.SerializeObject(objects, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public List<T> LoadFromFile()
        {
            if (!File.Exists(filePath))
                return new List<T>();

            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<T>>(json);
        }
    }

}
