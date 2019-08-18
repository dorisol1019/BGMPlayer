using BGMPlayer;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayerOperator.ViewModels
{
    public class PlayerOperatorViewModel : BindableBase
    {
        private IBGMPlayerService bgmPlayerService;
        public PlayerOperatorViewModel(IBGMPlayerService bgmPlayerService)
        {
            this.bgmPlayerService = bgmPlayerService;
        }
    }
}
