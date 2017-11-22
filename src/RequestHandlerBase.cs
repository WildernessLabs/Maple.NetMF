using System;
using Microsoft.SPOT;
using System.Net;
using System.Collections;
using System.Text;

namespace Maple
{
    public abstract class RequestHandlerBase : IRequestHandler
    {
        protected HttpListenerContext Context { get; set; }
        protected Hashtable QueryString { get; set; }
        protected Hashtable Form { get; set; }
        protected RequestHandlerBase(HttpListenerContext context)
        {
            this.Context = context;

            if (context.Request.RawUrl.Split('?').Length > 1)
            {
                var queryString = context.Request.RawUrl.Split('?')[1];
                var pairs = queryString.Split('&');
                this.QueryString = new Hashtable(pairs.Length);
                foreach (var pair in pairs)
                {
                    var keyValue = pair.Split('=');
                    this.QueryString.Add(keyValue[0], keyValue[1]);
                }

                // TODO: Read body and parse form for application/x-www-form-urlencoded

                // TODO: Read body and parse JSON for application/json
            }
        }

        protected void WriteToOutputStream(string data)
        {
            var payload = Encoding.UTF8.GetBytes(data);
            using (this.Context.Response.OutputStream)
            {
                // TODO: buffer this
                this.Context.Response.OutputStream.Write(payload, 0, payload.Length);
                this.Context.Response.OutputStream.Flush();
            }
        }
    }

    public interface IRequestHandler
    {
    }
}
