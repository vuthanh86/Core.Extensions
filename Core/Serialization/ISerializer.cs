using System;
using System.Collections.Generic;
using System.IO;

namespace Core.Serialization
{
    public interface ISerializer
    {
        void Serialize(object obj, Stream stream);
        T Deserialize<T>(Stream stream);
        object Deserialize(Stream stream);
    }

    public interface ISerializerFactory
    {
        ISerializer NewSerializer();
        void AddTypes(Dictionary<Type, uint> typeMaps);
        void AddTypes(Type type, uint typeId);
        Dictionary<Type, uint> GetTypeMap();
        Dictionary<Type, uint> AddTypes(IEnumerable<Type> types);
    }
}
