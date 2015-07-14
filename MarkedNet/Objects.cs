using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkedNet
{
    public class TableCellFlags
    {
        public bool header { get; set; }
        public string align { get; set; }
    }

    public class LinkObj
    {
        public string href { get; set; }
        public string title { get; set; }
    }
}
