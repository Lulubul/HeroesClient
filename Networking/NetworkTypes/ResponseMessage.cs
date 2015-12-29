using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkTypes
{
    public class ResponseMessage : SerializableType
    {
        public string Response { get; set; }
        public string Message { get; set; }
    }
}
