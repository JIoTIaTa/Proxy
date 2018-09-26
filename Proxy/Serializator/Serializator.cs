using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Proxy
{
    static class Serializator
    {
        static public T Read<T>(string fileName)
        {
            // создаем объект BinaryFormatter
            BinaryFormatter formatter = new BinaryFormatter();
            // десериализация из файла people.dat
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open))
                {
                    T newValue = (T)formatter.Deserialize(fs);
                    return newValue;
                }
            }
            catch
            {
                return default(T);
            }
                     
        }

        static public void Write<T>(T value, string fileName)
        {
            // создаем объект BinaryFormatter
            BinaryFormatter formatter = new BinaryFormatter();
            // получаем поток, куда будем записывать сериализованный объект
            using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, value);
            }
        }
    }
}