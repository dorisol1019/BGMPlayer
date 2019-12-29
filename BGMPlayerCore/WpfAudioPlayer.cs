using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using NAudio.Wave;
using NAudio.CoreAudioApi;

using System.Runtime.InteropServices;

using BGMPlayerCore;

namespace WpfAudioPlayer
{
    /// <summary>
    /// オーディオ再生を行います。
    /// </summary>
    sealed class AudioPlayer : IDisposable
    {
        private LoopStream _audioStream;
        private WaveChannel32 _volumeStream;
        private IWavePlayer _waveOut;
        private Stream strm;
        private WasapiOut2 _wasApi;
        private FileStream fs = null;
        private MemoryStream ms = null;
        public AudioPlayer() { }

        public async Task Play(string file, FileExtensionType ext)
        {
            try
            {
                await this.InitializeStream(file, ext);

            }
            catch (Exception exp)
            {
                this.Dispose();
                throw exp;
            }
            this.Play();
        }

        /// <summary>
        /// ファイルへのストリームを生成します。
        /// </summary>
        /// <param name="fileName">ファイルへのパス。</param>
        /// <exception cref="InvalidOperationException">ストリームの生成に失敗した。</exception>
        private async Task InitializeStream(string fileName, FileExtensionType ext)
        {
            WaveChannel32 stream;
            if (FileExtensionType.wave == ext)
            {
                WaveStream reader = new WaveFileReader(fileName);
                if (reader.WaveFormat.Encoding != WaveFormatEncoding.Pcm)
                {
                    reader = WaveFormatConversionStream.CreatePcmStream(reader);
                    reader = new BlockAlignReductionStream(reader);
                }

                if (reader.WaveFormat.BitsPerSample != 16)
                {
                    var format = new WaveFormat(reader.WaveFormat.SampleRate, 16, reader.WaveFormat.Channels);
                    reader = new WaveFormatConversionStream(format, reader);
                }

                stream = new WaveChannel32(reader);
            }
            else if (FileExtensionType.mp3 == ext)
            {
                fs = new FileStream(fileName, FileMode.Open);
                var buf = new byte[fs.Length];

                await fs.ReadAsync(buf, 0, buf.Length);

                fs.Dispose();

                ms = new MemoryStream(buf);

                var reader = new Mp3FileReader(ms);
                var pcmStream = WaveFormatConversionStream.CreatePcmStream(reader);
                var blockAlignedStream = new BlockAlignReductionStream(pcmStream);

                stream = new WaveChannel32(blockAlignedStream);
            }

            else if (FileExtensionType.ogg == ext)
            {
                var vorbisStream = new NAudio.Vorbis.VorbisWaveReader(fileName);

                stream = new WaveChannel32(vorbisStream);
            }

            else
            {
                throw new InvalidOperationException("Unsupported extension");
            }

            this._volumeStream = stream;
            this._audioStream = new LoopStream(stream, stream.WaveFormat.SampleRate / 10);

            this._wasApi = new WasapiOut2();
            this._wasApi.Init(this._audioStream);
        }

        /// <summary>
        /// 再生を一時停止します。
        /// </summary>
        public void Pause()
        {
            _wasApi.Pause();
        }

        /// <summary>
        /// 再生を開始します。
        /// </summary>
        public void Play()
        {
            switch (this._wasApi.PlaybackState)
            {
                case PlaybackState.Playing:
                    break;

                case PlaybackState.Paused:
                case PlaybackState.Stopped:
                    _wasApi.Play();
                    break;
            }
        }

        /// <summary>
        /// 再生を停止します。
        /// </summary>
        public void Stop()
        {
            _wasApi.Stop();
            this._audioStream?.Dispose();
        }

        /// <summary>
        /// ボリュームを取得または設定します。
        /// </summary>
        public float Volume
        {
            get
            {
                return this._volumeStream.Volume;
            }
            set
            {
                this._volumeStream.Volume = value;
            }
        }

        public int LoopCount => _audioStream?.loopcount ?? 0;

        /// <summary>
        /// リソースの解放を行います。
        /// </summary>
        public void Dispose()
        {
            if (this._waveOut != null)
            {
                this._waveOut.Stop();
            }
            if (this._wasApi != null)
            {
                this._wasApi.Stop();
            }
            if (this._audioStream != null)
            {
                this._volumeStream.Close();
                this._volumeStream = null;

                this._audioStream.Close();
                this._audioStream = null;
            }

            if (this._waveOut != null)
            {
                this._waveOut.Dispose();
                this._waveOut = null;
            }
            if (this._wasApi != null)
            {
                this._wasApi.Dispose();
                this._wasApi = null;
            }

            if (this.strm != null)
            {
                this.strm.Close();
                this.strm = null;
            }

            if (this.fs != null)
            {
                fs.Dispose();
                fs = null;
            }

            if (ms != null)
            {
                ms.Dispose();
                ms = null;
            }
        }
    }

    public class LoopStream : WaveStream
    {
        WaveStream sourceStream;


        public LoopStream(WaveStream source, int samplesPerNotification)
        {
            this.sourceStream = source;
            SourceStream = sourceStream;
            if (sourceStream.WaveFormat.BitsPerSample != 32)
                throw new ArgumentException("Metering Stream expects 32 bit floating point audio", "sourceStream");
            maxSamples = new float[sourceStream.WaveFormat.Channels];
            this.SamplesPerNotification = samplesPerNotification;
        }


        public override WaveFormat WaveFormat
        {
            get { return sourceStream.WaveFormat; }
        }

        public override long Length
        {
            get { return long.MaxValue / 32; }
        }

        public override long Position
        {
            get
            {
                return sourceStream.Position;
            }
            set
            {
                sourceStream.Position = value;
            }
        }

        public override bool HasData(int count)
        {
            // infinite loop
            return true;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = 0;

            while (read < count)
            {
                int required = count - read;
                int readThisTime = sourceStream.Read(buffer, offset + read, required);
                bool loopCountupFlag = false;
                if (readThisTime < required)
                {
                    loopcount++;
                    loopCountupFlag = true;
                    sourceStream.Position = 0;
                }

                if (sourceStream.Position >= sourceStream.Length)
                {
                    sourceStream.Position = 0;
                    if (!loopCountupFlag)
                        loopcount++;
                }
                read += readThisTime;
            }

            return read;
        }

        public int loopcount = 0;

        protected override void Dispose(bool disposing)
        {
            sourceStream.Dispose();
            loopcount = 0;
            base.Dispose(disposing);
        }
        public WaveStream SourceStream { get; private set; }
        public int SamplesPerNotification { get; set; }

        float[] maxSamples;
        int sampleCount;

        public event EventHandler<StreamVolumeEventArgs> StreamVolume;


        private void ProcessData(byte[] buffer, int offset, int count)
        {
            int index = 0;
            while (index < count)
            {
                for (int channel = 0; channel < maxSamples.Length; channel++)
                {
                    float sampleValue = Math.Abs(BitConverter.ToSingle(buffer, offset + index));
                    maxSamples[channel] = Math.Max(maxSamples[channel], sampleValue);
                    index += 4;
                }
                sampleCount++;
                if (sampleCount >= SamplesPerNotification)
                {
                    RaiseStreamVolumeNotification();
                    sampleCount = 0;
                    Array.Clear(maxSamples, 0, maxSamples.Length);

                }

            }
        }

        private void RaiseStreamVolumeNotification()
        {
            if (StreamVolume != null)
            {
                StreamVolume(this, new StreamVolumeEventArgs() { MaxSampleValues = (float[])maxSamples.Clone() });
            }
        }
    }

    public class StreamVolumeEventArgs : EventArgs
    {
        public float[] MaxSampleValues { get; set; }
    }
    public class WasapiOut2 : IWavePlayer, IWavePosition
    {
        private AudioClient audioClient;
        private MMDevice mmDevice;
        private readonly AudioClientShareMode shareMode;
        private AudioRenderClient renderClient;
        private IWaveProvider sourceProvider;
        private int latencyMilliseconds;
        private int bufferFrameCount;
        private int bytesPerFrame;
        private readonly bool isUsingEventSync;
        private EventWaitHandle frameEventWaitHandle;
        private byte[] readBuffer;
        private volatile PlaybackState playbackState;
        private Thread playThread;
        private WaveFormat outputFormat;
        private bool dmoResamplerNeeded;
        private readonly SynchronizationContext syncContext;

        /// <summary>
        /// Playback Stopped
        /// </summary>
        public event EventHandler<StoppedEventArgs> PlaybackStopped;

        /// <summary>
        /// WASAPI Out using default audio endpoint
        /// </summary>
        /// <param name="shareMode">ShareMode - shared or exclusive</param>
        /// <param name="latency">Desired latency in milliseconds</param>
        public WasapiOut2(AudioClientShareMode shareMode, int latency) :
            this(GetDefaultAudioEndpoint(), shareMode, true, latency)
        {

        }

        /// <summary>
        /// WASAPI Out using default audio endpoint
        /// </summary>
        /// <param name="shareMode">ShareMode - shared or exclusive</param>
        /// <param name="useEventSync">true if sync is done with event. false use sleep.</param>
        /// <param name="latency">Desired latency in milliseconds</param>
        public WasapiOut2(AudioClientShareMode shareMode, bool useEventSync, int latency) :
            this(GetDefaultAudioEndpoint(), shareMode, useEventSync, latency)
        {

        }
        public WasapiOut2() :
            this(GetDefaultAudioEndpoint(), AudioClientShareMode.Shared, false, 200)
        {

        }
        /// <summary>
        /// Creates a new WASAPI Output
        /// </summary>
        /// <param name="device">Device to use</param>
        /// <param name="shareMode"></param>
        /// <param name="useEventSync">true if sync is done with event. false use sleep.</param>
        /// <param name="latency"></param>
        public WasapiOut2(MMDevice device, AudioClientShareMode shareMode, bool useEventSync, int latency)
        {
            this.audioClient = device.AudioClient;
            this.mmDevice = device;
            this.shareMode = shareMode;
            this.isUsingEventSync = useEventSync;
            this.latencyMilliseconds = latency;
            this.syncContext = SynchronizationContext.Current;
        }

        static MMDevice GetDefaultAudioEndpoint()
        {
            if (Environment.OSVersion.Version.Major < 6)
            {
                throw new NotSupportedException("WASAPI supported only on Windows Vista and above");
            }
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            return enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
        }

        private void PlayThread()
        {
            ResamplerDmoStream resamplerDmoStream = null;
            IWaveProvider playbackProvider = this.sourceProvider;
            Exception exception = null;
            try
            {
                if (this.dmoResamplerNeeded)
                {
                    resamplerDmoStream = new ResamplerDmoStream(sourceProvider, outputFormat);
                    playbackProvider = resamplerDmoStream;
                }

                // fill a whole buffer
                bufferFrameCount = audioClient.BufferSize;
                bytesPerFrame = outputFormat.Channels * outputFormat.BitsPerSample / 8;
                readBuffer = new byte[bufferFrameCount * bytesPerFrame];
                FillBuffer(playbackProvider, bufferFrameCount);

                // Create WaitHandle for sync
                WaitHandle[] waitHandles = new WaitHandle[] { frameEventWaitHandle };

                audioClient.Start();

                while (playbackState != PlaybackState.Stopped)
                {
                    // If using Event Sync, Wait for notification from AudioClient or Sleep half latency
                    int indexHandle = 0;
                    if (isUsingEventSync)
                    {
                        indexHandle = WaitHandle.WaitAny(waitHandles, 3 * latencyMilliseconds, false);
                    }
                    else
                    {
                        Thread.Sleep(latencyMilliseconds / 2);
                    }

                    // If still playing and notification is ok
                    if (playbackState == PlaybackState.Playing && indexHandle != WaitHandle.WaitTimeout)
                    {
                        // See how much buffer space is available.
                        int numFramesPadding = 0;
                        if (isUsingEventSync)
                        {
                            // In exclusive mode, always ask the max = bufferFrameCount = audioClient.BufferSize
                            numFramesPadding = (shareMode == AudioClientShareMode.Shared) ? audioClient.CurrentPadding : 0;
                        }
                        else
                        {
                            numFramesPadding = audioClient.CurrentPadding;
                        }
                        int numFramesAvailable = bufferFrameCount - numFramesPadding;
                        if (numFramesAvailable > 0)
                        {
                            FillBuffer(playbackProvider, numFramesAvailable);
                        }
                    }
                }
                Thread.Sleep(latencyMilliseconds / 2);
                audioClient.Stop();
                if (playbackState == PlaybackState.Stopped)
                {
                    audioClient.Reset();
                }
            }
            catch (Exception e)
            {
                exception = e;
            }
            finally
            {
                if (resamplerDmoStream != null)
                {
                    resamplerDmoStream.Dispose();
                }
                RaisePlaybackStopped(exception);
            }
        }

        private void RaisePlaybackStopped(Exception e)
        {
            var handler = PlaybackStopped;
            if (handler != null)
            {
                if (this.syncContext == null)
                {
                    handler(this, new StoppedEventArgs(e));
                }
                else
                {
                    syncContext.Post(state => handler(this, new StoppedEventArgs(e)), null);
                }
            }
        }

        private void FillBuffer(IWaveProvider playbackProvider, int frameCount)
        {
            IntPtr buffer = renderClient.GetBuffer(frameCount);
            int readLength = frameCount * bytesPerFrame;
            int read = playbackProvider.Read(readBuffer, 0, readLength);
            if (read == 0)
            {
                playbackState = PlaybackState.Stopped;
            }
            Marshal.Copy(readBuffer, 0, buffer, read);
            int actualFrameCount = read / bytesPerFrame;
            /*if (actualFrameCount != frameCount)
            {
                Debug.WriteLine(String.Format("WASAPI wanted {0} frames, supplied {1}", frameCount, actualFrameCount ));
            }*/
            renderClient.ReleaseBuffer(actualFrameCount, AudioClientBufferFlags.None);
        }

        /// <summary>
        /// Gets the current position in bytes from the wave output device.
        /// (n.b. this is not the same thing as the position within your reader
        /// stream)
        /// </summary>
        /// <returns>Position in bytes</returns>
        public long GetPosition()
        {
            if (playbackState == NAudio.Wave.PlaybackState.Stopped)
            {
                return 0;
            }
            return (long)audioClient.AudioClockClient.AdjustedPosition;
        }

        /// <summary>
        /// Gets a <see cref="Wave.WaveFormat"/> instance indicating the format the hardware is using.
        /// </summary>
        public WaveFormat OutputWaveFormat
        {
            get { return outputFormat; }
        }

        #region IWavePlayer Members

        /// <summary>
        /// Begin Playback
        /// </summary>
        public void Play()
        {
            if (playbackState != PlaybackState.Playing)
            {
                if (playbackState == PlaybackState.Stopped)
                {
                    playThread = new Thread(new ThreadStart(PlayThread));
                    playbackState = PlaybackState.Playing;
                    playThread.Start();
                }
                else
                {
                    playbackState = PlaybackState.Playing;
                }


            }
        }

        /// <summary>
        /// Stop playback and flush buffers
        /// </summary>
        public void Stop()
        {
            if (playbackState != PlaybackState.Stopped)
            {
                playbackState = PlaybackState.Stopped;
                playThread.Join();
                playThread = null;
            }
        }

        /// <summary>
        /// Stop playback without flushing buffers
        /// </summary>
        public void Pause()
        {
            if (playbackState == PlaybackState.Playing)
            {
                playbackState = PlaybackState.Paused;
            }

        }

        /// <summary>
        /// Initialize for playing the specified wave stream
        /// </summary>
        /// <param name="waveProvider">IWaveProvider to play</param>
        public void Init(IWaveProvider waveProvider)
        {
            long latencyRefTimes = latencyMilliseconds * 10000;
            outputFormat = waveProvider.WaveFormat;
            // first attempt uses the WaveFormat from the WaveStream
            WaveFormatExtensible closestSampleRateFormat;
            if (!audioClient.IsFormatSupported(shareMode, outputFormat, out closestSampleRateFormat))
            {
                // Use closesSampleRateFormat (in sharedMode, it equals usualy to the audioClient.MixFormat)
                // See documentation : http://msdn.microsoft.com/en-us/library/ms678737(VS.85).aspx 
                // They say : "In shared mode, the audio engine always supports the mix format"
                // The MixFormat is more likely to be a WaveFormatExtensible.
                if (closestSampleRateFormat == null)
                {
                    WaveFormat correctSampleRateFormat = audioClient.MixFormat;
                    /*WaveFormat.CreateIeeeFloatWaveFormat(
                    audioClient.MixFormat.SampleRate,
                    audioClient.MixFormat.Channels);*/

                    if (!audioClient.IsFormatSupported(shareMode, correctSampleRateFormat))
                    {
                        // Iterate from Worst to Best Format
                        WaveFormatExtensible[] bestToWorstFormats = {
                                  new WaveFormatExtensible(
                                      outputFormat.SampleRate, 32,
                                      outputFormat.Channels),
                                  new WaveFormatExtensible(
                                      outputFormat.SampleRate, 24,
                                      outputFormat.Channels),
                                  new WaveFormatExtensible(
                                      outputFormat.SampleRate, 16,
                                      outputFormat.Channels),
                              };

                        // Check from best Format to worst format ( Float32, Int24, Int16 )
                        for (int i = 0; i < bestToWorstFormats.Length; i++)
                        {
                            correctSampleRateFormat = bestToWorstFormats[i];
                            if (audioClient.IsFormatSupported(shareMode, correctSampleRateFormat))
                            {
                                break;
                            }
                            correctSampleRateFormat = null;
                        }

                        // If still null, then test on the PCM16, 2 channels
                        if (correctSampleRateFormat == null)
                        {
                            // Last Last Last Chance (Thanks WASAPI)
                            correctSampleRateFormat = new WaveFormatExtensible(outputFormat.SampleRate, 16, 2);
                            if (!audioClient.IsFormatSupported(shareMode, correctSampleRateFormat))
                            {
                                throw new NotSupportedException("Can't find a supported format to use");
                            }
                        }
                    }
                    outputFormat = correctSampleRateFormat;
                }
                else
                {
                    outputFormat = closestSampleRateFormat;
                }

                // just check that we can make it.
                using (new ResamplerDmoStream(waveProvider, outputFormat))
                {
                }
                this.dmoResamplerNeeded = true;
            }
            else
            {
                dmoResamplerNeeded = false;
            }
            this.sourceProvider = waveProvider;

            // If using EventSync, setup is specific with shareMode
            if (isUsingEventSync)
            {
                // Init Shared or Exclusive
                if (shareMode == AudioClientShareMode.Shared)
                {
                    // With EventCallBack and Shared, both latencies must be set to 0
                    audioClient.Initialize(shareMode, AudioClientStreamFlags.EventCallback, 0, 0,
                        outputFormat, Guid.Empty);

                    // Get back the effective latency from AudioClient
                    latencyMilliseconds = (int)(audioClient.StreamLatency / 10000);
                }
                else
                {
                    // With EventCallBack and Exclusive, both latencies must equals
                    audioClient.Initialize(shareMode, AudioClientStreamFlags.EventCallback, latencyRefTimes, latencyRefTimes,
                                        outputFormat, Guid.Empty);
                }

                // Create the Wait Event Handle
                frameEventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
                audioClient.SetEventHandle(frameEventWaitHandle.SafeWaitHandle.DangerousGetHandle());
            }
            else
            {
                // Normal setup for both sharedMode
                audioClient.Initialize(shareMode, AudioClientStreamFlags.None, latencyRefTimes, 0,
                                    outputFormat, Guid.Empty);
            }

            // Get the RenderClient
            renderClient = audioClient.AudioRenderClient;
        }

        /// <summary>
        /// Playback State
        /// </summary>
        public PlaybackState PlaybackState
        {
            get { return playbackState; }
        }

        /// <summary>
        /// Volume
        /// </summary>
        public float Volume
        {
            get
            {
                return mmDevice.AudioEndpointVolume.MasterVolumeLevelScalar;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException("value", "Volume must be between 0.0 and 1.0");
                if (value > 1) throw new ArgumentOutOfRangeException("value", "Volume must be between 0.0 and 1.0");
                mmDevice.AudioEndpointVolume.MasterVolumeLevelScalar = value;
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {

            if (audioClient != null)
            {
                Stop();
                audioClient.Dispose();
                audioClient = null;
                renderClient = null;
            }

        }

        #endregion
    }
}
