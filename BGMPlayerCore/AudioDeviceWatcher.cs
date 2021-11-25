using NAudio.CoreAudioApi;
using Reactive.Bindings;

namespace BGMPlayerCore;

public class AudioDeviceWatcher
{
    public ReactiveProperty<MMDevice> CurrentDefaultDevice { get; private set; } =
        new ReactiveProperty<MMDevice>(initialValue: Utility.GetDefaultAudioEndpoint());

    public async Task Watch(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            MMDevice? device = Utility.GetDefaultAudioEndpoint();
            if (CurrentDefaultDevice.Value.ID != device.ID)
            {
                CurrentDefaultDevice.Value = device;
            }
            await Task.Delay(100, cancellationToken);
        }
    }
}
