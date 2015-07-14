using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MarkedNet
{
    public class Token
    {
        public string text { get; set; }
        public string type { get; set; }


        public int depth { get; set; }
        public bool escaped { get; set; }
        public string lang { get; set; }
        public bool ordered { get; set; }

        public bool pre { get; set; }

        public IList<string> header { get; set; }
        public IList<string> align { get; set; }
        public IList<string> cells { get; set; }
    }
}
