using NAudio.CoreAudioApi;

namespace BGMPlayerCore;

internal class Utility
{
    internal static MMDevice GetDefaultAudioEndpoint()
    {
        if (Environment.OSVersion.Version.Major < 6)
        {
            throw new NotSupportedException("WASAPI supported only on Windows Vista and above");
        }

        var enumerator = new MMDeviceEnumerator();
        return enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
    }
}
