using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;

namespace NetCore.Extensions.Core.IoC
{
    public class AutoInjectLoader
    {
        public AutoInjectLoader(IComponentContext container)
        {
            var assemblies = AppDomain
                .CurrentDomain
                .GetAssemblies();

            var tuples = (from assembly in assemblies
                from type in assembly.GetTypes()
                let shouldInjecteds = GetInjectFields(type).ToList()
                where shouldInjecteds.Any()
                select new Tuple<Type, IEnumerable<FieldInfo>>(type, shouldInjecteds)).ToList();

            foreach (var tuple in tuples)
            {
                var fields = tuple.Item2;
                foreach (var f in fields)
                {
                    if (container.IsRegistered(f.FieldType))
                    {
                        var fieldInstance = container.Resolve(f.FieldType);
                        f.SetValue(null, fieldInstance);
                    }
                }
            }
        }

        private IEnumerable<FieldInfo> GetInjectFields(Type type)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            foreach (var fieldInfo in fields)
            {
                if (fieldInfo.CustomAttributes.Any(a => a.AttributeType == typeof(AutoInjectAttribute)))
                {
                    yield return fieldInfo;
                }
            }
        }
    }
}
