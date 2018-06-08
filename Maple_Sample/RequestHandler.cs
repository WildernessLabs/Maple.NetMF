using System;
using Microsoft.SPOT;
using Maple;

namespace Maple_Sample
{
    public class RequestHandler : RequestHandlerBase
    {
        public RequestHandler() { }

        public void getHello()
        {
            this.Context.Response.ContentType = ContentTypes.Application_Text;
            this.Context.Response.StatusCode = 200;
            this.Send("hello world");
        }
    }
}
