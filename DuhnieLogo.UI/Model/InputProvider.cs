using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DuhnieLogo.UI.Model
{
    class InputProvider
    {
        private readonly Queue<QueuedKeyPress> buffer = new Queue<QueuedKeyPress>();
        private readonly System.Timers.Timer cleanupTimer = new System.Timers.Timer(20);
        private readonly object waitLock = new object();

        public InputProvider()
        {
            cleanupTimer.Elapsed += CleanupTimer_Elapsed;
            cleanupTimer.Start();
        }

        private void CleanupTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            EatExpiredKeyPresses();
        }

        public void KeyDown(System.Windows.Forms.Keys key)
        {
            buffer.Enqueue(new QueuedKeyPress { Key = key, Time = DateTime.Now.Ticks });

            lock (waitLock)
            {
                Monitor.Pulse(waitLock);
            }
        }

        public bool KeyPressAvailable => buffer.Any();

        public System.Windows.Forms.Keys WaitForKeyPress()
        {
            if(buffer.Any())
                return GetLastKeyPress();

            lock (waitLock)
            {
                Monitor.Wait(waitLock);
                return GetLastKeyPress();
            }
        }

        public System.Windows.Forms.Keys GetLastKeyPress()
        {
            return buffer.Dequeue().Key;
        }

        private void EatExpiredKeyPresses()
        {
            while(buffer.Count > 0 && buffer.Peek().Expired)
            {
                buffer.Dequeue();
            }
        }

        private class QueuedKeyPress
        {
            public System.Windows.Forms.Keys Key { get; set; }
            public long Time { get; set; }

            public bool Expired => DateTime.Now.Ticks - Time > 10000 * 20;
        }
    }
}
