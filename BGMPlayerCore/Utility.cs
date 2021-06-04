using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGMPlayerCore
{
    class Utility
    {
        internal static MMDevice GetDefaultAudioEndpoint()
        {
            if (Environment.OSVersion.Version.Major < 6)
            {
                throw new NotSupportedException("WASAPI supported only on Windows Vista and above");
            }

            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            return enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
        }
    }
}
