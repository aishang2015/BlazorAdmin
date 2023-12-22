using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAdmin.Im.Events
{
    public class UpdateNoCountEvent
    {
        public UpdateNoCountEventType Type { get; set; }


        public int Count { get; set; }
    }

    public enum UpdateNoCountEventType
    {
        Add,
        Sub
    }
}
