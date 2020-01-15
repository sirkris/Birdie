using System;
using System.Collections.Generic;
using System.Text;

namespace Birdie.EventArgs
{
    public class AlarmActiveEventArgs
    {
        /// <summary>
        /// Is Birdie scheduled?
        /// </summary>
        public bool IsScheduled { get; set; }
    }
}
