using System;
using System.Collections.Generic;

namespace Proxy.BookWorker
{
    public interface IBookWorker
    {
        event Action<object> BookLoaded;
        event Action<object> BookClosed;
        /// <summary>
        /// Загрузити (відкрити) книгу
        /// </summary>
        /// <param name="fileName">Місце, де лежить книга</param>
        void LoadBook(string fileName);
        /// <summary>
        /// Закрити поточну книгу
        /// </summary>
        void CloseBook();
        /// <summary>
        /// Читати поточну книгу
        /// </summary>
        /// <param name="rowsCount">кількість рядків 1 стовпця</param>
        /// <returns></returns>
        List<string> Read(int rowsCount);
        /// <summary>
        /// Читати всі комірки поточної книги
        /// </summary>
        /// <returns></returns>
        List<string> Read();
        /// <summary>
        /// Читати всі комірки поточної книги
        /// </summary>
        /// <returns>Значення - комірка </returns>
        Dictionary<string, string> ReadWithCellsReference();
        /// <summary>
        /// Писати в книгу
        /// </summary>
        void Write();
        /// <summary>
        /// Писати в книгу
        /// </summary>
        /// <param name="rows">Список рядків для запису</param>
        void Write(List<string> rows);
        void Write(Dictionary<string,string> valueCellDictionary);
    }
}
