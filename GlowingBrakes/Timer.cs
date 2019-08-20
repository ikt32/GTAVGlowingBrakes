using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlowingBrakes
{
    public class Timer
    {
        private Int64 mPeriod;
        private Int64 mPreviousTime;

        private Int64 now()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        public Timer(Int64 timeout)
        {
            mPeriod = timeout;
            mPreviousTime = now();
        }

        public void Reset()
        {
            mPreviousTime = now();
        }

        public void Reset(Int64 newTimeout)
        {
            mPeriod = newTimeout;
            mPreviousTime = now();
        }
        public bool Expired()
        {
            return now() > mPreviousTime + mPeriod;
        }
        public Int64 Elapsed()
        {
            return now() - mPreviousTime;
        }
        public Int64 Period()
        {
            return mPeriod;
        }
    }
}
