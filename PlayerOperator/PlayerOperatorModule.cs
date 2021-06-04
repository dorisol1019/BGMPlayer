using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace PlayerOperator
{
    public class PlayerOperatorModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            IRegionManager? regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("PlayerOperator", typeof(Views.PlayerOperator));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}
