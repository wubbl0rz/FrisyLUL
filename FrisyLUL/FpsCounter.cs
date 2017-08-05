using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrisyLUL
{
    class FpsCounter
    {
        private int frames = 0;
        private double fps = 0;

        private DateTime start = DateTime.Now;

        public void Update() => this.frames++;

        public int GetFps() {
            var now = DateTime.Now;

            if (now > start.AddSeconds(1))
            {
                var delta = now - start;
                this.fps = frames / delta.TotalSeconds;
                start = now;
                this.frames = 0;
            }
            return (int)this.fps;
        }
    }
}
