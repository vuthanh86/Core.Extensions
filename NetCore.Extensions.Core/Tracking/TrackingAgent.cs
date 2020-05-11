using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetCore.Extensions.Core.Extensions;
using StatsdClient;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace NetCore.Extensions.Core.Tracking
{
    public class TrackingConfig
    {
        public string ConnectionString { get; set; }
        public string[] DefaultTags { get; set; }
    }

    public static class TrackingAgent
    {
        public static ILogger Logger;
        public static string[] DefaultTags { get; private set; } = Array.Empty<string>();

//        static TrackingAgent()
//        {
//            var dogstatsdConfig = new StatsdConfig
//            {
//                StatsdServerName = "127.0.0.1"
//            };
//
//            DogStatsd.Configure(dogstatsdConfig);
//        }

        public static void Config(TrackingConfig config)
        {
            var arr = config.ConnectionString.Split(':');
            var dogstatsdConfig  = new StatsdConfig
            {
                StatsdServerName = arr[0].Trim()
            };

            if (arr.Length == 2)
            {
                
            }

            DefaultTags = config.DefaultTags ?? Array.Empty<string>();
            DogStatsd.Configure(dogstatsdConfig);
        }

        static string[] BuildTags(string[] tags)
        {
            var t = new string[tags.Length + DefaultTags.Length];

            Array.Copy(tags,t, tags.Length);
            Array.Copy(DefaultTags,0, t,tags.Length,DefaultTags.Length);

            return t;
        }

        class XTimer : IDisposable
        {
            private readonly string actionName;
            private readonly string[] tags;
            private readonly Stopwatch sw;

            public XTimer(string actionName, params  string[] tags)
            {
                this.actionName = actionName;
                this.tags = tags;
                sw = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                sw.Stop();
#if DEBUG
                Logger?.LogInformation($"{actionName} {string.Join(",", tags)} {sw.ElapsedMilliseconds:n0}ms");
#endif
            }
        }

        static IDisposable StartTimer(string actionName, params string[] tags)
        {
#if DEBUG
            return new XTimer(actionName,tags);
#else
            return DogStatsd.StartTimer(actionName, tags:BuildTags(tags));
#endif
        }

        public static void Count(string @eventName, params string[] tags)
        {
            DogStatsd.Increment(eventName, tags: BuildTags(tags));
        }

        public static void Event(string eventName, string desc, params string[] tags)
        {
            Logger?.Info($"{eventName} [{string.Join(",",tags)}] {desc}");

            DogStatsd.Event(eventName, desc.Substring(0, 8000), tags: BuildTags(tags));
        }

        public static void Event(string eventName, string desc, Exception exception, params string[] tags)
        {
            if (exception == null)
            {
                Event(eventName, desc, tags);
            }
            else
            {
                var description = GetExceptionDescription(desc, exception);

                Error(eventName, description, tags);
            }
        }

        public static void Error(string eventName, Exception e, params string[] tags)
        {
            var desc = GetExceptionDescription(string.Empty, e);

            Error(eventName, desc, tags);
        }

        public static void Error(string eventName, string message, Exception e, params string[] tags)
        {
            var desc = GetExceptionDescription(message, e);

            Error(eventName, desc, tags);
        }

        private static string GetExceptionDescription(string message, Exception e)
        {
            var maxLength = 8000;

            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(message))
                sb.AppendLine(message);

            if (e != null)
            {
                var c = e;
                while (c != null)
                {
                    sb.AppendLine(c.Message);
                    c = c.InnerException;
                }
                sb.Append(e.StackTrace);
            }
            else
            {
                sb.Append("Unknow Error");
            }

            var l = Math.Min(sb.Length, maxLength);

            return sb.ToString().Substring(0, l);
        }

        public static void Error(string eventName, string desc, params string[] tags)
        {
#if DEBUG
            Logger?.Error($"{eventName}.Error: {desc}");
#endif
            eventName = eventName + ".Error";

            Count(eventName, tags);
            
            Logger?.Error($"{eventName} [{string.Join(",",tags)}] {desc}");

            DogStatsd.Event(eventName, desc.Substring(0, 8000), "error", tags: BuildTags(tags));
        }

        public static void Warning(string eventName, string desc, params string[] tags)
        {
#if DEBUG
            Logger?.LogWarning($"{eventName}.Warning: {desc}");
#endif
            eventName = eventName + ".Warning";
            Count(eventName);
            
            Logger?.LogWarning($"{eventName} [{string.Join(",",tags)}] {desc}");

            DogStatsd.Event(eventName, desc.Substring(0,8000), "warning", tags: BuildTags(tags));
        }

        public static void Measure(Action action, string actionName, params string[] tags)
        {
            using (StartTimer(actionName, tags))
            {
                action.Invoke();
            }
        }

        public static T Measure<T>(Func<T> action, string actionName, params string[] tags)
        {
            using (StartTimer(actionName, tags))
            {
                return action.Invoke();
            }
        }

        public static async Task Measure(Task task, string actionName, params string[] tags)
        {
            using (StartTimer(actionName, tags))
            {
                await task;
            }
        }

        public static async Task<T> Measure<T>(Task<T> task, string actionName, params string[] tags)
        {
            using (StartTimer(actionName, tags))
            {
                return await task;
            }
        }

        public static async Task<T> Measure<T>(Func<Task<T>> func, string actionName, params string[] tags)
        {
            using (StartTimer(actionName, tags))
            {
                try
                {
                    return await func();
                }
                catch (Exception ex)
                {
                    Error(actionName, ex, tags);
                    throw;
                }
            }
        }
    }

    public static class TrackingExtensions
    {
        public static string TagSiteId(this Guid siteId)
        {
            return $"siteId:{siteId}";
        }

        public static string TagProviderName(this string name)
        {
            return $"providerName:{name}";
        }
    }
}
