using System;
using System.Threading;
using System.Timers;

namespace WindowsFormsApp4
{
    class Clock
    {
        private DateTime _yesterday = DateTime.Today;
        public int upTime;

        public event EventHandler NewDay;

        public Clock()
        {
            var t = new Thread(CustomTimer);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        public void CustomTimer()
        {
            while (true)
            {
                if (_yesterday != DateTime.Today)
                {
                    if (NewDay != null) NewDay(this, EventArgs.Empty);
                    _yesterday = DateTime.Today;
                }

                upTime += 1;

                Thread.Sleep(1000);
            }
        }

        void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
          
        }
    }
}
