namespace NuvasiveWeatherApp
{
    #region using statements

    using Services;
    using Views;

    using Microsoft.Practices.Unity;
    using Prism.Unity;

    #endregion

    public partial class App : PrismApplication
    {
        public App(IPlatformInitializer initializer = null) : base(initializer) { }

        protected override void OnInitialized()
        {
            InitializeComponent();

            NavigationService.NavigateAsync("MainPage");
        }

        protected override void RegisterTypes()
        {
            Container.RegisterType<IOpenWeatherMgr, OpenWeatherMgr>(new ContainerControlledLifetimeManager());

            Container.RegisterTypeForNavigation<MainPage>();
        }
    }
}
