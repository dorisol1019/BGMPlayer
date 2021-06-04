using NAudio.CoreAudioApi;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BGMPlayerCore
{
    public class AudioDeviceWatcher
    {
        public ReactiveProperty<MMDevice> CurrentDefaultDevice { get; private set; } =
            new ReactiveProperty<MMDevice>(initialValue: Utility.GetDefaultAudioEndpoint());

        public async Task Watch(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var device = Utility.GetDefaultAudioEndpoint();
                if (CurrentDefaultDevice.Value.ID != device.ID)
                {
                    CurrentDefaultDevice.Value = device;
                }
                await Task.Delay(100, cancellationToken);
            }
        }
    }
}
