using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LitJson;
using NetworkTypes;

namespace Assets.Scripts.Networking
{
    public class RemoteInvokeMethod
    {
        public string ServiceClassName { get; set; }
        public string MethodName { get; set; }
        public List<SerializableType> Parameters { get; set; }
        public static readonly string AssemblyName = "NetworkTypes";
        public static bool UseJsonSerialization = false;

        public RemoteInvokeMethod(List<SerializableType> parameters)
        {
            ServiceClassName = "Handler";
            Parameters = parameters;
        }

        public RemoteInvokeMethod(string serviceClassName, Command methodName, List<SerializableType> parameters)
        {
            ServiceClassName = serviceClassName;
            MethodName = methodName.ToString();
            Parameters = parameters;
        }

        public RemoteInvokeMethod(string serviceClassName, string methodName, List<SerializableType> parameters)
        {
            ServiceClassName = serviceClassName;
            MethodName = methodName;
            Parameters = parameters;
        }

        public RemoteInvokeMethod(Command methodName, List<SerializableType> parameters)
        {
            ServiceClassName = "Handler";
            MethodName = methodName.ToString();
            Parameters = parameters;
        }

        public RemoteInvokeMethod(string serviceClassName, Command methodName)
        {
            ServiceClassName = serviceClassName;
            MethodName = methodName.ToString();
            Parameters = new List<SerializableType>();
        }

        public static byte[] WriteToStream(RemoteInvokeMethod toWrite)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write(toWrite.ServiceClassName);
            writer.Write(toWrite.MethodName);

            writer.Write(toWrite.Parameters.Count.ToString());
            foreach (var parameter in toWrite.Parameters)
            {
                var type = parameter.GetType();
                writer.Write(type.ToString());
                if (UseJsonSerialization)
                {
                    writer.Write(JsonMapper.ToJson(parameter));
                    continue;
                }
                parameter.Serialize(writer, type, parameter);
            }
            return stream.GetBuffer();
        }

        public static RemoteInvokeMethod ReadFromStream(MemoryStream from)
        {
            var reader = new BinaryReader(from);
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            var className = reader.ReadString();
            var methodName = reader.ReadString();
            var count = reader.ReadString();
            var paramsCount = Convert.ToInt32(count);
            var parameters = new List<SerializableType>();
            for (var i = 0; i < paramsCount; i++)
            {
                var type = reader.ReadString();
                var objectType = Type.GetType(type);
                var item = Activator.CreateInstance(objectType) as SerializableType;
                if (UseJsonSerialization)
                {
                    var json = reader.ReadString();
                    item = JsonToObject(json, objectType);
                }
                else
                {
                    item.Deserialize(reader, objectType, item);
                }
                parameters.Add(item);
            }
            var instance = new RemoteInvokeMethod(className, methodName, parameters);
            return instance;
        }

        public static SerializableType JsonToObject(string json, Type type)
        {
            var methods = typeof(JsonMapper).GetMethods();
            foreach (var method in methods)
            {
                if (method.IsGenericMethod && method.Name == "ToObject" && method.GetParameters().Any(x=> x.ParameterType == typeof(string)))
                {
                    var generic = method.MakeGenericMethod(type);
                    return generic.Invoke(null, new object[] { json }) as SerializableType;
                }
            }
            return new SerializableType();
        }
    }
}
