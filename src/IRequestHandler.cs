using System.Net;

namespace Maple
{
    public interface IRequestHandler
    {
        HttpListenerContext Context { get; set; }
    }
}
