using BGMList.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using BGMList.Models;

namespace BGMList
{
    public class BGMListModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("BGMList", typeof(Views.BGMList));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterInstance<IAllBGMs>(new AllBGMs());
            containerRegistry.RegisterInstance<ISelectedBGM>(new SelectedBGM());
        }
    }
}