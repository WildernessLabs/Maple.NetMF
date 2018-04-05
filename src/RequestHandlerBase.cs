using System;
using Microsoft.SPOT;
using System.Net;
using System.Collections;
using System.Text;

namespace Maple
{
    public abstract class RequestHandlerBase : IRequestHandler
    {
        private const int bufferSize = 4096;
        protected Hashtable QueryString { get; set; }
        protected Hashtable Form { get; set; }
        protected Hashtable Body { get; set; }

        protected RequestHandlerBase()
        {
        }

        public HttpListenerContext Context
        {
            get { return _context; }
            set
            {
                _context = value;

                if (_context.Request.RawUrl.Split('?').Length > 1)
                {
                    var q = _context.Request.RawUrl.Split('?')[1];
                    this.QueryString = ParseUrlPairs(q);
                }

                switch (_context.Request.ContentType)
                {
                    case ContentTypes.Application_Form_UrlEncoded:
                        this.Form = ParseUrlPairs(ReadInputStream());
                        break;
                    case ContentTypes.Application_Json:
                        this.Body = Json.NETMF.JsonSerializer.DeserializeString(ReadInputStream()) as Hashtable;
                        break;
                }
            }
        }
        private HttpListenerContext _context;

        protected void Send()
        {
            Send(null);
        }

        protected void Send(object output)
        {
            if(this.Context.Response.ContentType == ContentTypes.Application_Json)
            {
                var json = Json.NETMF.JsonSerializer.SerializeObject(output);
                WriteOutputStream(Encoding.UTF8.GetBytes(json));
            }
            else
            {
                // default is to process output as a string
                WriteOutputStream(Encoding.UTF8.GetBytes(output != null ? output.ToString() : string.Empty));
            }
        }

        private string ReadInputStream()
        {
            int i = 0;
            int len = (int)this.Context.Request.ContentLength64;
            byte[] buffer = new byte[bufferSize];
            string result = string.Empty;

            while (i * bufferSize <= len)
            {
                int min = Min(bufferSize, (len - (i * bufferSize)));
                this.Context.Request.InputStream.Read(buffer, 0, min);
                result += new String(Encoding.UTF8.GetChars(buffer, 0, min));
                i++;
            }

            return result;
        }

        private void WriteOutputStream(byte[] data)
        {
            this.Context.Response.ContentLength64 = data.Length;

            using (this.Context.Response.OutputStream)
            {
                int i = 0;
                byte[] buffer = new byte[bufferSize];

                while (i * bufferSize <= data.Length)
                {
                    int min = Min(bufferSize, data.Length - (i * bufferSize));
                    Array.Copy(data, i * bufferSize, buffer, 0, min);
                    this.Context.Response.OutputStream.Write(buffer, 0, min);
                    i++;
                }
                this.Context.Response.OutputStream.Flush();
            }
        }

        private Hashtable ParseUrlPairs(string s)
        {
            var pairs = s.Split('&');
            Hashtable result = new Hashtable(pairs.Length);
            foreach (var pair in pairs)
            {
                var keyValue = pair.Split('=');
                result.Add(keyValue[0], keyValue[1]);
            }
            return result;
        }

        private int Min(int a, int b)
        {
            if (a <= b)
                return a;
            else
                return b;
        }
    }
}
