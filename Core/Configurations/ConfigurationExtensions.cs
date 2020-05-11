using System;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Core.Configurations
{
    public static class ConfigurationExtensions
    {
        public static ContainerBuilder Configure<T>(this ContainerBuilder builder, IConfigurationRoot configuration, string section) where T : class, new()
        {
            builder.Register(s => new OptionsWrapper<T>(configuration.GetSection(section).Get<T>())).As<IOptions<T>>().SingleInstance();

            return builder;
        }

        public static ContainerBuilder Configure<T>(this ContainerBuilder builder, IConfigurationSection configurationSection) where T : class, new()
        {
            var xx = configurationSection.Get<T>();
            builder.Register(s => new OptionsWrapper<T>(xx)).As<IOptions<T>>().SingleInstance();

            return builder;
        }

        public static ContainerBuilder Configure<T>(this ContainerBuilder builder, T option) where T : class, new()
        {
            builder.Register(s => new OptionsWrapper<T>(option)).As<IOptions<T>>().SingleInstance();

            return builder;
        }

        public static ContainerBuilder Configure<T>(this ContainerBuilder builder, Func<T> @delegate) where T : class, new()
        {
            builder.Register(s => new OptionsWrapper<T>(@delegate())).As<IOptions<T>>().SingleInstance();

            return builder;
        }
    }
}
