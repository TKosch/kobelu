using System;
using System.Collections.Generic;
using System.Text;

namespace KoBeLU.Schema
{
    public class BoxLayout
    {
        public string Name { get; set; }

        public IList<Box> Boxes { get; set; }
    }
}
