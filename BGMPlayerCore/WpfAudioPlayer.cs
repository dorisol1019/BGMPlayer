using BGMPlayerCore;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Reactive.Bindings;
using System;
using System.IO;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace WpfAudioPlayer
{
    /// <summary>
    /// オーディオ再生を行います。
    /// </summary>
    internal sealed class AudioPlayer : IDisposable
    {
        private LoopStream _audioStream;
        private WaveChannel32 _volumeStream;
        private IWavePlayer _waveOut;
        private Stream strm;
        private WasapiOut2 _wasApi;
        private FileStream fs = null;
        private MemoryStream ms = null;
        private LoopMetadata? loopMetadata;
        public AudioPlayer() { }

        public async Task Play(string file, BgmType bgmType)
        {
            try
            {
                await InitializeStream(file, bgmType);

            }
            catch (Exception exp)
            {
                Dispose();
                throw exp;
            }
            Play();
        }

        /// <summary>
        /// ファイルへのストリームを生成します。
        /// </summary>
        /// <param name="fileName">ファイルへのパス。</param>
        /// <exception cref="InvalidOperationException">ストリームの生成に失敗した。</exception>
        private async Task InitializeStream(string fileName, BgmType bgmType)
        {
            loopMetadata = null;
            WaveChannel32 stream;
            if (BgmType.Wave == bgmType)
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
            else if (BgmType.MP3 == bgmType)
            {
                fs = new FileStream(fileName, FileMode.Open);
                byte[]? buf = new byte[fs.Length];

                await fs.ReadAsync(buf, 0, buf.Length);

                fs.Dispose();

                ms = new MemoryStream(buf);

                var reader = new Mp3FileReader(ms);
                WaveStream? pcmStream = WaveFormatConversionStream.CreatePcmStream(reader);
                var blockAlignedStream = new BlockAlignReductionStream(pcmStream);

                stream = new WaveChannel32(blockAlignedStream);
            }

            else if (BgmType.Ogg == bgmType)
            {
                var vorbisStream = new NAudio.Vorbis.VorbisWaveReader(fileName);

                (long?, long?) GetLoopMetadata()
                {
                    long? loopStart = null, loopLength = null;
                    foreach (string? metadata in vorbisStream.Comments)
                    {
                        if (!metadata.Contains('='))
                        {
                            continue;
                        }

                        string? key = metadata.Split('=')[0];
                        string? value = metadata.Split('=')[1];
                        if (key == "LOOPSTART")
                        {
                            loopStart = long.Parse(value);
                        }
                        else if (key == "LOOPLENGTH")
                        {
                            loopLength = long.Parse(value);
                        }
                        if (loopStart != null && loopLength != null)
                        {
                            break;
                        }
                    }
                    return (loopStart, loopLength);
                }

                (long? loopStart, long? loopLength) = GetLoopMetadata();
                if (loopStart != null && loopLength != null)
                {
                    loopMetadata = new LoopMetadata()
                    {
                        // ByteParFrame = 2(ステレオ) * 32(WaveChannel32のBitsPerSample) / 8 = 8
                        // ByteParFrameがWaveChannel32である限り8だからsample値に8倍
                        Start = loopStart.Value * 8,
                        Length = loopLength.Value * 8
                    };
                }

                stream = new WaveChannel32(vorbisStream);
            }

            else
            {
                throw new InvalidOperationException("Unsupported BgmType");
            }

            _volumeStream = stream;
            _audioStream = new LoopStream(stream, stream.WaveFormat.SampleRate / 10, loopMetadata);

            _wasApi = new WasapiOut2();
            _wasApi.Init(_audioStream);
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
            switch (_wasApi.PlaybackState)
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
            _audioStream?.Dispose();
        }

        /// <summary>
        /// ボリュームを取得または設定します。
        /// </summary>
        public float Volume
        {
            get => _volumeStream.Volume;
            set => _volumeStream.Volume = value;
        }

        public int LoopCount => _audioStream?.loopcount ?? 0;

        /// <summary>
        /// リソースの解放を行います。
        /// </summary>
        public void Dispose()
        {
            if (_waveOut != null)
            {
                _waveOut.Stop();
            }
            if (_wasApi != null)
            {
                _wasApi.Stop();
            }
            if (_audioStream != null)
            {
                _volumeStream.Close();
                _volumeStream = null;

                _audioStream.Close();
                _audioStream = null;
            }

            if (_waveOut != null)
            {
                _waveOut.Dispose();
                _waveOut = null;
            }
            if (_wasApi != null)
            {
                _wasApi.Dispose();
                _wasApi = null;
            }

            if (strm != null)
            {
                strm.Close();
                strm = null;
            }

            if (fs != null)
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
        private readonly WaveStream sourceStream;
        private readonly LoopMetadata? loopMetadata;



        public LoopStream(WaveStream source, int samplesPerNotification, LoopMetadata? loopMetadata)
        {
            sourceStream = source;
            SourceStream = sourceStream;
            if (sourceStream.WaveFormat.BitsPerSample != 32)
            {
                throw new ArgumentException("Metering Stream expects 32 bit floating point audio", "sourceStream");
            }

            maxSamples = new float[sourceStream.WaveFormat.Channels];
            SamplesPerNotification = samplesPerNotification;
            this.loopMetadata = loopMetadata;
        }


        public override WaveFormat WaveFormat => sourceStream.WaveFormat;

        public override long Length => long.MaxValue / 32;

        public override long Position
        {
            get => sourceStream.Position;
            set => sourceStream.Position = value;
        }

        public override bool HasData(int count)
        {
            // infinite loop
            return true;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = 0;

            // LoopStart,LoopLengthがあるときの挙動
            if (loopMetadata != null)
            {
                long loopEnd = loopMetadata.Start + loopMetadata.Length;
                while (read < count)
                {
                    int required = count - read;
                    int readThisTime = sourceStream.Read(buffer, offset + read, required);
                    bool loopCountupFlag = false;
                    if (readThisTime < required)
                    {
                        loopcount++;
                        loopCountupFlag = true;
                        sourceStream.Position = loopMetadata.Start;
                    }

                    if (sourceStream.Position >= loopEnd)
                    {
                        int yobun = (int)(sourceStream.Position - loopEnd);
                        readThisTime -= yobun;
                        sourceStream.Position = loopMetadata.Start;
                        if (!loopCountupFlag)
                        {
                            loopcount++;
                        }
                    }
                    read += readThisTime;
                }
                return read;
            }

            // 今までの挙動
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
                    {
                        loopcount++;
                    }
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

        private readonly float[] maxSamples;
        private int sampleCount;

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
        private WaveFormat outputFormat;
        private bool dmoResamplerNeeded;
        private readonly SynchronizationContext syncContext;

        private Task? playTask;

        private readonly CancellationTokenSource cancellationTokenSource = new();

        private readonly ReadOnlyReactiveProperty<MMDevice?> audioDevice;
        private readonly AudioDeviceWatcher audioDeviceWatcher;
        private Task? watchTask;

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
            this(Utility.GetDefaultAudioEndpoint(), shareMode, true, latency)
        {

        }

        /// <summary>
        /// WASAPI Out using default audio endpoint
        /// </summary>
        /// <param name="shareMode">ShareMode - shared or exclusive</param>
        /// <param name="useEventSync">true if sync is done with event. false use sleep.</param>
        /// <param name="latency">Desired latency in milliseconds</param>
        public WasapiOut2(AudioClientShareMode shareMode, bool useEventSync, int latency) :
            this(Utility.GetDefaultAudioEndpoint(), shareMode, useEventSync, latency)
        {

        }
        public WasapiOut2() :
            this(Utility.GetDefaultAudioEndpoint(), AudioClientShareMode.Shared, false, 200)
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
            audioClient = device.AudioClient;
            mmDevice = device;
            this.shareMode = shareMode;
            isUsingEventSync = useEventSync;
            latencyMilliseconds = latency;
            syncContext = SynchronizationContext.Current ?? throw new InvalidOperationException("SynchronizationContext.Current is null.");

            audioDeviceWatcher = new AudioDeviceWatcher();

            audioDevice = audioDeviceWatcher.CurrentDefaultDevice.ToReadOnlyReactiveProperty(mode: ReactivePropertyMode.None);
            audioDevice.Subscribe(async device =>
            {
                Pause();
                await Task.Delay(100);
                audioClient.Stop();
                mmDevice = device;
                audioClient = mmDevice.AudioClient;
                Init(sourceProvider!);
                await Task.Delay(100);
                audioClient.Start();
                Play();
            });
        }

        private async Task PlayTask(CancellationToken cancellationToken)
        {
            ResamplerDmoStream? resamplerDmoStream = null;
            IWaveProvider playbackProvider = sourceProvider;
            Exception? exception = null;
            try
            {
                if (dmoResamplerNeeded)
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
                var waitHandles = new WaitHandle[] { frameEventWaitHandle };

                audioClient.Start();

                while (playbackState != PlaybackState.Stopped)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    // If using Event Sync, Wait for notification from AudioClient or Sleep half latency
                    int indexHandle = 0;
                    if (isUsingEventSync)
                    {
                        indexHandle = WaitHandle.WaitAny(waitHandles, 3 * latencyMilliseconds, false);
                    }
                    else
                    {
                        await Task.Delay(latencyMilliseconds / 2, cancellationToken);
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
                await Task.Delay(latencyMilliseconds / 2, cancellationToken);
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
                if (exception is not null)
                {
                    RaisePlaybackStopped(exception);
                }
            }
        }

        private void RaisePlaybackStopped(Exception e)
        {
            EventHandler<StoppedEventArgs>? handler = PlaybackStopped;
            if (handler != null)
            {
                if (syncContext == null)
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
        public WaveFormat OutputWaveFormat => outputFormat;

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
                    CancellationToken token = cancellationTokenSource.Token;
                    playTask = Task.Run(() => PlayTask(token));
                    watchTask = Task.Factory.StartNew(() => audioDeviceWatcher.Watch(token), token, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
                    playbackState = PlaybackState.Playing;
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
                cancellationTokenSource.Cancel();
                if (playTask is null)
                {
                    throw new InvalidOperationException("playTask is null.");
                }

                if (watchTask is null)
                {
                    throw new InvalidOperationException("watchTask is null.");
                }

                playTask.Wait();
                watchTask.Wait();
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
            if (!audioClient.IsFormatSupported(shareMode, outputFormat, out WaveFormatExtensible closestSampleRateFormat))
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
                dmoResamplerNeeded = true;
            }
            else
            {
                dmoResamplerNeeded = false;
            }
            sourceProvider = waveProvider;

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
        public PlaybackState PlaybackState => playbackState;

        /// <summary>
        /// Volume
        /// </summary>
        public float Volume
        {
            get => mmDevice.AudioEndpointVolume.MasterVolumeLevelScalar;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Volume must be between 0.0 and 1.0");
                }

                if (value > 1)
                {
                    throw new ArgumentOutOfRangeException("value", "Volume must be between 0.0 and 1.0");
                }

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
            audioDevice?.Dispose();

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
