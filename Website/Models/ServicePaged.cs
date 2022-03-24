using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website.Models
{
    public class ServicePaged<T>
    {
        public T Data { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
