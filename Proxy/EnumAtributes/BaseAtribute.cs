using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proxy.EnumAtributes
{

    public abstract class BaseAttribute : Attribute
    {
        private readonly object _value;
        public BaseAttribute(object value) { this._value = value; }

        public object GetValue() { return this._value; }

        public class Age : BaseAttribute { public Age(int value) : base(value) { } }
        public class Weight : BaseAttribute { public Weight(double value) : base(value) { } }
        public class RussianName : BaseAttribute { public RussianName(string value) : base(value) { } }
        public class GoogleDriveType : BaseAttribute { public GoogleDriveType(string value) : base(value) { } }

    }
    public static class EnumAttributesBaseLogic
    {
        /// <param name="enumItem">Элемент перечисления</param>
        /// <param name="attributeType">Тип атрибута, значение которого хотим получить</param>
        /// <param name="defaultValue">
        /// Значение по-умолчанию, которое будет возвращено, если переданный
        /// элемент перечисления не помечен переданным атрибутом
        /// </param>
        /// <returns>Возвращает значение переданного атрибута у переданного элемента перечисления</returns>
        public static VAL GetAttributeValue<ENUM, VAL>(this ENUM enumItem, Type attributeType, VAL defaultValue)
        {
            var attribute = enumItem.GetType().GetField(enumItem.ToString()).GetCustomAttributes(attributeType, true)
                .Where(a => a is BaseAttribute)
                .Select(a => (BaseAttribute)a)
                .FirstOrDefault();

            return attribute == null ? defaultValue : (VAL)attribute.GetValue();
        }
    }

    public static class EnumExtensionMethods
    {
        public static int GetAge(this Enum enumItem)
        {
            return enumItem.GetAttributeValue(typeof(BaseAttribute.Age), 0);
        }

        public static double GetWeight(this Enum enumItem)
        {
            return enumItem.GetAttributeValue(typeof(BaseAttribute.Weight), 0d);
        }

        public static string GetRussianName(this Enum enumItem)
        {
            return enumItem.GetAttributeValue(typeof(BaseAttribute.RussianName), string.Empty);
        }

        public static string GetGoogleDriveType(this Enum enumItem)
        {
            return enumItem.GetAttributeValue(typeof(BaseAttribute.GoogleDriveType), string.Empty);
        }
    }

}
