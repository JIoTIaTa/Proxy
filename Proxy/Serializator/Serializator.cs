﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Proxy
{
    static class Serializator
    {
        static public T Read<T>(T value, string fileName)
        {
            // создаем объект BinaryFormatter
            BinaryFormatter formatter = new BinaryFormatter();
            // десериализация из файла people.dat
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
                {
                    T newValue = (T)formatter.Deserialize(fs);
                    return newValue;
                }
            }
            catch (Exception)
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

                Console.WriteLine("Объект сериализован");
            }
        }
    }
}