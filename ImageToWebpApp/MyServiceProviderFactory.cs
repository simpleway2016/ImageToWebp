namespace ImageToWebpApp
{
    class MyServiceProviderFactory : IServiceProviderFactory<IServiceProvider>
    {
        public static IServiceProvider ServiceProvider;
        public IServiceProvider CreateBuilder(IServiceCollection services)
        {
            return ServiceProvider = services.BuildServiceProvider();
        }

        public IServiceProvider CreateServiceProvider(IServiceProvider containerBuilder)
        {
            return containerBuilder;
        }
    }
}
