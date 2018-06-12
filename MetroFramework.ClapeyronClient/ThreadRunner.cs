using System.Threading;

namespace MetroFramework.ClapeyronClient
{
    class ThreadRunner
    {
        private Thread _thread;
        public volatile bool is_terminated = false;
        public ManualResetEvent ev_suspend = new ManualResetEvent(true);

        public ThreadRunner(ThreadStart action)
        {
            _thread = new Thread(action);
            _thread.Start();
        }

        public void Dispose()
        {
            is_terminated = true;
            Resume();
            _thread.Join();
            ev_suspend.Close();
        }

        public void Suspend()
        {
            ev_suspend.Reset();
        }

        public void Resume()
        {
            ev_suspend.Set();
        }

        public void Background(bool state)
        {
            _thread.IsBackground = state;
        }
    }
}
