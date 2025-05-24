using System.Diagnostics;

namespace Konamiman.Z80dotNet
{
    /// <summary>
    /// Default implementation of <see cref="IClockSynchronizer"/>.
    /// </summary>
    public class ClockSynchronizer : IClockSynchronizer
    {
        private const int MinMicrosecondsToWait = 10*1000;
        private long TicksPerMicrosecond = Stopwatch.Frequency / 1000000;
        private Stopwatch stopWatch = new();
        private long accumulatedMicroseconds;

        public decimal EffectiveClockFrequencyInMHz { get; set; }

        public void Start()
        {
            stopWatch.Reset();
            stopWatch.Start();
        }

        public void Stop()
        {
            stopWatch.Stop();
        }

        public void TryWait(int periodLengthInCycles)
        {
            accumulatedMicroseconds += (long)(periodLengthInCycles / EffectiveClockFrequencyInMHz);
            var elapsedMicroseconds = stopWatch.ElapsedTicks / TicksPerMicrosecond;
            var microsecondsPending = accumulatedMicroseconds - elapsedMicroseconds;
            if(microsecondsPending >= MinMicrosecondsToWait) 
            {
                Thread.Sleep((int)(microsecondsPending / 1000));
                accumulatedMicroseconds = 0;
                stopWatch.Reset();
            }
        }
    }
}
