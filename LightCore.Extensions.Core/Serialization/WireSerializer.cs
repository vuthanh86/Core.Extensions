//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using CMS.LightCore.Extensions.Core.Extensions;
//using Wire;
//using Wire.Extensions;
//using Wire.SerializerFactories;
//using Wire.ValueSerializers;

//namespace CMS.LightCore.Extensions.Core.Serialization
//{
//    public class WireSerializer : ISerializer
//    {
//        private readonly Serializer s;

//        public static IEnumerable<Type> BasicTypes => RootTypes(new[]
//        {
//            typeof(Int32),
//            typeof(Int64),
//            typeof(Int16),
//            typeof(UInt32),
//            typeof(UInt64),
//            typeof(UInt16),
//            typeof(Byte),
//            typeof(SByte),
//            typeof(bool),
//            typeof(DateTime),
//            typeof(String),
//            typeof(Guid),
//            typeof(float),
//            typeof(Double),
//            typeof(Decimal),
//            typeof(Char),
//            typeof(object)
//        });

//        private static IEnumerable<Type> RootTypes(IEnumerable<Type> types)
//        {
//            var x = new HashSet<Type>();

//            foreach (var t in types)
//            {
//                var arr = new[]
//                {
//                    typeof(List<>).MakeGenericType(t),
//                    t.MakeArrayType()
//                };

//                foreach (var subType in arr.SelectMany(v=>v.GetAllSubTypes()))
//                {
//                    if (x.Contains(subType)) continue;
//                    x.Add(subType);
//                }
//            }

//            return x;
//        }

//        public WireSerializer():this(BasicTypes)
//        {
            
//        }

//        public WireSerializer(IEnumerable<Type> knownTypes)
//        {
//            var opt = new SerializerOptions(preserveObjectReferences: true,knownTypes:knownTypes);

//            var field = typeof(SerializerOptions).GetTypeInfo().GetDeclaredField("ValueSerializerFactories");
//            var factories = (ValueSerializerFactory[])field.GetValue(opt);
//            var newFactories = new List<ValueSerializerFactory>(factories);

//            var oldEnumerableSerializerFacType = typeof(EnumerableSerializerFactory);
//            var index = newFactories.IndexOf(f => f.GetType() == oldEnumerableSerializerFacType);

//            if (index >= 0)
//            {
//                newFactories.Insert(index, new NewEnumerableSerializerFactory());
//            }
//            else
//            {
//                newFactories.Add(new NewEnumerableSerializerFactory());
//            }

//            field.SetValue(opt, newFactories.ToArray());

//            s = new Serializer(opt);
//        }

//        public void Serialize(object obj, Stream stream)
//        {
//            s.Serialize(obj, stream);
//        }

//        public T Deserialize<T>(Stream stream)
//        {
//            return s.Deserialize<T>(stream);
//        }

//        public object Deserialize(Stream stream)
//        {
//            return s.Deserialize(stream);
//        }
//    }

//    public class NewEnumerableSerializerFactory : ValueSerializerFactory
//    {
//        private readonly FieldInfo preserveObjectReferencesMethod;

//        public NewEnumerableSerializerFactory()
//        {
//            var type = typeof(SerializerOptions);

//            preserveObjectReferencesMethod = type.GetTypeInfo().GetDeclaredField("PreserveObjectReferences");
//        }

//        private bool PreserveObjectReferences(SerializerOptions opt)
//        {
//            return (bool)preserveObjectReferencesMethod.GetValue(opt);
//        }

//        public override bool CanSerialize(Serializer serializer, Type type)
//        {
//            return IsEnumerable(type);
//        }

//        public override bool CanDeserialize(Serializer serializer, Type type)
//        {
//            return CanSerialize(serializer, type);
//        }

//        private static bool IsEnumerable(Type type)
//        {
//            return type
//                .GetTypeInfo()
//                .GetInterfaces()
//                .Any(
//                    intType =>
//                        intType.GetTypeInfo().IsGenericType &&
//                        intType.GetTypeInfo().GetGenericTypeDefinition() == typeof(IEnumerable<>));
//        }

//        public static IEnumerable<FieldInfo> GetFieldInfos(Type type)
//        {
//            var typeInfo = type.GetTypeInfo();

//            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
//                .Where(fi => (fi.Attributes & FieldAttributes.NotSerialized) == 0)
//                .OrderBy(f => f.Name, StringComparer.Ordinal);

//            if (typeInfo.BaseType == null)
//            {
//                return fields;
//            }
//            else
//            {
//                var baseFields = GetFieldInfos(typeInfo.BaseType);
//                return baseFields.Concat(fields);
//            }
//        }

//        public override ValueSerializer BuildSerializer(Serializer serializer, Type type,
//            ConcurrentDictionary<Type, ValueSerializer> typeMapping)
//        {
//            var x = new ObjectSerializer(type);
//            typeMapping.TryAdd(type, x);
//            var preserveObjectReferences = PreserveObjectReferences(serializer.Options);

//            var fields = GetFieldInfos(type).OrderBy(t=>t.Name).Select(f => new KeyValuePair<FieldInfo, ValueSerializer>(f, serializer.GetSerializerByType(f.FieldType))).ToArray();

//            ObjectWriter writer = (stream, o, session) =>
//            {
//                if (preserveObjectReferences)
//                {
//                    session.TrackSerializedObject(o);
//                }

//                foreach (var f in fields)
//                {
//                    stream.WriteObject(f.Key.GetValue(o), f.Key.FieldType, f.Value, preserveObjectReferences, session);
//                }
//            };

//            ObjectReader reader = (stream, session) =>
//            {
//                var instance = Activator.CreateInstance(type);

//                if (preserveObjectReferences)
//                {
//                    session.TrackDeserializedObject(instance);
//                }

//                foreach (var f in fields)
//                {
//                    var v = stream.ReadObject(session);
//                    f.Key.SetValue(instance, v);
//                }


//                return instance;
//            };

//            x.Initialize(reader, writer);
//            return x;
//        }
//    }

//}
