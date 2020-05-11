using Autofac;
using Microsoft.EntityFrameworkCore;

namespace Core.Repository
{
    public interface IDbContextFactory
    {
        DbContext CreateDbContext();
    }

    public class DbContextFactory : IDbContextFactory
    {
        private readonly IComponentContext componentContext;

        public DbContextFactory(IComponentContext componentContext)
        {
            this.componentContext = componentContext;

            //using (var ctx = CreateDbContext())
            //{
            //    //ctx.Database.Migrate();
            //    ctx.Database.EnsureCreated();
            //}
        }

        public DbContext CreateDbContext()
        {
            return componentContext.Resolve<DbContext>();
        }
    }

    public class EntityModule<TDbContext> : Module where TDbContext : DbContext
    {
        private readonly string connectionString;

        public EntityModule(string connectionString)
        {
            this.connectionString = connectionString;
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.RegisterType<DbContextFactory>().As<IDbContextFactory>().SingleInstance().AutoActivate();
            builder.RegisterType<TDbContext>().As<DbContext>().As<TDbContext>().InstancePerDependency();
            builder.RegisterType<EntityQueue>().As<IEntityQueue>().SingleInstance();
            builder.RegisterGeneric(typeof(Repository<,>)).As(typeof(IRepository<,>)).SingleInstance();

            var dbOptionBuilder = new DbContextOptionsBuilder();

            dbOptionBuilder.UseNpgsql(connectionString);
            builder.Register(s => dbOptionBuilder.Options).As<DbContextOptions>().SingleInstance();
        }
    }
}
