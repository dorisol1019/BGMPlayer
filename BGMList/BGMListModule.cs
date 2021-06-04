using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace BGMList
{
    public class BGMListModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            IRegionManager? regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("BGMList", typeof(Views.BGMList));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
        }
    }
}
