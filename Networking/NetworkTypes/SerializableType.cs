using System;
using System.Collections;
using System.IO;
using System.Reflection;

namespace NetworkTypes
{
    public class SerializableType
    {
        public void Serialize(BinaryWriter writer, Type type, object parent)
        {
            foreach (var propertyInfo in type.GetProperties())
            {
                if (propertyInfo.PropertyType.IsGenericType)
                {
                    var genericArguments = propertyInfo.PropertyType.GetGenericArguments();
                    var values = propertyInfo.GetValue(parent, null) as IList;
                    writer.Write(values.Count);
                    foreach (var item in values)
                    {
                        var scopeParent = item as SerializableType ?? parent;
                        Serialize(writer, genericArguments[0], scopeParent);
                    }
                    return;
                }
                if (propertyInfo.PropertyType.IsEnum)
                {
                    var enumValue = Convert.ToInt32(propertyInfo.GetValue(parent, null));
                    writer.Write(enumValue);
                    continue;
                }
                if (typeof(SerializableType).IsAssignableFrom(propertyInfo.PropertyType))
                {
                    Serialize(writer, propertyInfo.PropertyType, propertyInfo.GetValue(parent, null));
                    return;
                }
                ToBinary(writer, propertyInfo, parent);
            }
        }

        public void ToBinary(BinaryWriter writer, PropertyInfo propertyInfo, object item)
        {
            var value = propertyInfo.GetValue(item, null);
            if (value != null)
            {
                typeof(BinaryWriter)
                .GetMethod("Write", new[] { propertyInfo.PropertyType })
                .Invoke(writer, new[] { value });
            }
        }

        public void Deserialize(BinaryReader reader, Type type, object parent)
        {
            foreach (var propertyInfo in type.GetProperties())
            {
                if (propertyInfo.PropertyType.IsGenericType)
                {
                    FillCollection(propertyInfo, parent, reader);
                    return;
                }
                if (typeof(SerializableType).IsAssignableFrom(propertyInfo.PropertyType))
                {
                    var item = Activator.CreateInstance(propertyInfo.PropertyType);
                    propertyInfo.SetValue(parent, item, null);
                    Deserialize(reader, propertyInfo.PropertyType, item);
                    return;
                }

                SetProperty(propertyInfo, parent, reader);
            }
        }

        private void FillCollection(PropertyInfo propertyInfo, object parent, BinaryReader reader)
        {
            var count = reader.ReadInt32();
            var argumentType = propertyInfo.PropertyType.GetGenericArguments()[0];
            var list = propertyInfo.GetValue(parent, null) as IList;
            for (var i = 0; i < count; i++)
            {
                var item = Activator.CreateInstance(argumentType);
                if (list == null) continue;
                list.Add(item);
                Deserialize(reader, argumentType, item);
            }
        }

        private static void SetProperty(PropertyInfo propertyInfo, object parent, BinaryReader reader)
        {
            if (propertyInfo.PropertyType.IsEnum)
            {
                var enumValue = reader.ReadInt32();
                propertyInfo.SetValue(parent, Enum.ToObject(propertyInfo.PropertyType, enumValue), null);
                return;
            }
            var methodName = "Read" + propertyInfo.PropertyType.ToString().Split('.')[1];
            var value = typeof(BinaryReader).GetMethod(methodName).Invoke(reader, null);
            if (value != null)
            {
                propertyInfo.SetValue(parent, value, null);
            }
        }
    }
}
