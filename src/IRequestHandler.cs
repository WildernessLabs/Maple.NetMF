using System;
using Microsoft.SPOT;
using System.Net;

namespace Maple
{
    public interface IRequestHandler
    {
        HttpListenerContext Context { get; set; }
    }
}
