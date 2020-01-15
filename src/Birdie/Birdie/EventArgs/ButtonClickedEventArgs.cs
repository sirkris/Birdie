using System;
using System.Collections.Generic;
using System.Text;

namespace Birdie.EventArgs
{
    public class ButtonClickedEventArgs
    {
        /// <summary>
        /// When the click event was registered.
        /// </summary>
        public DateTime ClickedAt { get; set; }
    }
}
