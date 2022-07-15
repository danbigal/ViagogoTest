using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viagogo
{
    public class CustomerEvent
    {
        public Customer Customer{ get; set; }
        public Event Event { get; set; }
        public int Distance { get; set; }
    }
}
