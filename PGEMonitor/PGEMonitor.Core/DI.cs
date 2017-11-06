using Autofac;

namespace PGEMonitor.Core
{
    public static class DI
    {
        public static ContainerBuilder Builder { get; set; }
        public static IContainer Container { get; set; }

        static DI()
        {
            Builder = new ContainerBuilder();
        }
    }
}
