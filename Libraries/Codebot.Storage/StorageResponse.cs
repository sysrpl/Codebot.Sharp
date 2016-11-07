using System;
using System.Net;
using System.Xml;

namespace Codebot.Storage
{
    /// <summary>
    /// Provides a base class which encapsulates an Amazon REST WebResponse and sets up an
    /// appropriate XmlReader for the kinds of XML data Amazon sends back.
    /// </summary>
    public abstract class StorageResponse : IDisposable
    {
        HttpWebResponse response;
        XmlReader reader;

        /// <summary>
        /// Gets an XmlReader for parsing Amazon XML responses, creating one if necessary and
        /// calling MoveToContent().
        /// </summary>
        protected XmlReader Reader
        {
            get { return reader ?? (reader = CreateXmlReader()); }
        }

        protected internal HttpWebResponse WebResponse
        {
            get { return response; }
            internal set
            {
                CheckResponse(value);
                this.response = value;
                ProcessResponse();
            }
        }

        public StorageResponse() { }

        protected virtual void ProcessResponse() { }

        static void CheckResponse(HttpWebResponse response)
        {
            // see if the server told us to screw off
            if (response.StatusCode == HttpStatusCode.TemporaryRedirect &&
                response.Headers[HttpResponseHeader.Location] == "http://aws.amazon.com/Storage")
                throw new Exception("A GetAllBuckets request was rejected by the Storage server. Did you forget to authorize the request?");
        }

        XmlReader CreateXmlReader()
        {
            var reader = new XmlTextReader(WebResponse.GetResponseStream())
            {
                WhitespaceHandling = WhitespaceHandling.Significant,
                Namespaces = false
            };
            reader.MoveToContent();
            return reader;
        }

        /// <summary>
        /// Closes our response stream. You must call this method when you are finished with
        /// this response.
        /// </summary>
        public void Close()
        {
            WebResponse.Close();
        }
        
        // explicit implementation so you can use the "using" keyword
        void IDisposable.Dispose()
        {
            Close();
        }
    }
}
