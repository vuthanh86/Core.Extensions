using Autofac;
using Microsoft.Extensions.Logging;
using NetCore.Extensions.Core.Security;
using NetCore.Extensions.Core.Threading;

namespace NetCore.Extensions.Core
{
    public class CoreModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            //builder.RegisterType<WireSerializer>().As<ISerializer>().SingleInstance();
            builder.RegisterType<DefaultEncryptionService>().As<IEncryptionService>().SingleInstance();
            builder.RegisterType<Scheduler>().AsImplementedInterfaces().SingleInstance().AutoActivate();
            //builder.RegisterType<AutoInjectLoader>().SingleInstance().AutoActivate();
        }
    }

    public class LogModule : Module
    {
        private readonly ILoggerFactory loggerFactory;

        public LogModule(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
        }

        public LogModule() : this(new LoggerFactory())
        {

        }

        protected override void Load(ContainerBuilder builder)
        {
            if(loggerFactory!=null)
                builder.Register(s => loggerFactory).As<ILoggerFactory>().SingleInstance();

            builder.Register(s => s.Resolve<ILoggerFactory>().CreateLogger(typeof(LogModule))).As<ILogger>().SingleInstance();
        }
    }
}
