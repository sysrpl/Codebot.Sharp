using System;
using System.Net;
using System.Text;

namespace Codebot.Storage
{
    /// <summary>
    /// The base class for all Storage requests.
    /// </summary>
    public abstract class StorageRequest
    {
        /// <summary>
        /// Returns true if Abort has been invoked
        /// </summary>
		public bool Aborted { get; private set; };

		/// <summary>
		/// Cancels an asynchronous request to Storage.
		/// </summary>
	 	public void Abort()
		{
			if (!Aborted)
				WebRequest.Abort();
			Aborted = true;
		}

		string bucketName; // remember this for signing the request later

        /// <summary>
        /// Gets the service this request will operate against.
        /// </summary>
        public StorageService Service { get; private set; }

        protected HttpWebRequest WebRequest { get; private set; }

        internal StorageRequest(StorageService service, string method, string bucketName, string objectKey,
            string queryString)
        {
            this.Service = service;
            this.bucketName = bucketName;
            this.WebRequest = CreateWebRequest(method, objectKey, queryString);
        }

        HttpWebRequest CreateWebRequest(string method, string objectKey, string queryString)
        {
            var uriString = new StringBuilder(Service.UseSsl ? "https://" : "http://");

            if (bucketName != null && Service.UseSubdomains)
                uriString.Append(bucketName).Append('.');

            uriString.Append(Service.Host);

            if (Service.CustomPort != 0)
                uriString.Append(':').Append(Service.CustomPort);

            uriString.Append('/');

            if (bucketName != null && !Service.UseSubdomains)
                uriString.Append(bucketName).Append('/');

            // EscapeDataString allows you to use basically any key for an object, including
            // keys with tricky URI characters like "+".
            if (objectKey != null)
                uriString.Append(Uri.EscapeDataString(objectKey));

            if (queryString != null)
                uriString.Append(queryString);

            var uri = new Uri(uriString.ToString());

            HttpWebRequest request = (HttpWebRequest)System.Net.WebRequest.Create(uri);
            request.Method = method;
            request.AllowWriteStreamBuffering = true; // AddObject will make this false
            request.AllowAutoRedirect = true;

            // Storage will never "timeout" a request. However, network delays may still cause a
            // timeout according to WebRequest's ReadWriteTimeout property, which you can modify.
            request.Timeout = int.MaxValue;
            
            return request;
        }

        #region Expose Storage-relevant mirrored properties of HttpWebRequest

        /// <summary>
        /// Gets a value that indicates whether a response has been received from Storage.
        /// </summary>
        public bool HaveResponse
        {
            get { return WebRequest.HaveResponse; }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether to make a persistent connection to Storage.
        /// </summary>
        public bool KeepAlive
        {
            get { return WebRequest.KeepAlive; }
            set { WebRequest.KeepAlive = value; }
        }

        /// <summary>
        /// Gets or sets proxy information for this request.
        /// </summary>
        public IWebProxy Proxy
        {
            get { return WebRequest.Proxy; }
            set { WebRequest.Proxy = value; }
        }

        /// <summary>
        /// Gets or sets a time-out in milliseconds when writing to or reading from a stream.
        /// The default value is 5 minutes.
        /// </summary>
        public int ReadWriteTimeout
        {
            get { return WebRequest.ReadWriteTimeout; }
            set { WebRequest.ReadWriteTimeout = value; }
        }

        /// <summary>
        /// Gets the service point to use for this request. See remarks on this property if you
        /// plan on using Expect100Continue.
        /// </summary>
        /// <remarks>
        /// In specific circumstances, the Storage request will hang indefinitely if Expect100Continue is 
        /// set to true and Storage immediately responds with a HTTP 5xx server error after the request is 
        /// issued and before any data is written to the stream. The downside to leaving this property 
        /// at false is that you'll waste bandwidth and time if Storage knows the PUT is going to fail at 
        /// the very start of the request.
        /// </remarks>
        public ServicePoint ServicePoint
        {
            get { return WebRequest.ServicePoint; }
        }
        #endregion

        protected void AuthorizeIfNecessary()
        {
            if (!StorageAuthorizer.IsAuthorized(WebRequest)) Authorize();
        }

        protected virtual void Authorize()
        {
            if (StorageAuthorizer.IsAuthorized(WebRequest))
                throw new InvalidOperationException("This request has already been authorized.");

            Service.AuthorizeRequest(this, WebRequest, bucketName);
        }

        protected void TryThrowStorageException(WebException exception)
        {
            // if this is a protocol error and the response type is XML, we can expect that
            // Storage sent us an <Error> message.
            if (exception.Status == WebExceptionStatus.ProtocolError &&
                exception.Response.ContentType == "application/xml" &&
                (exception.Response.ContentLength > 0 || 
                 exception.Response.Headers[HttpResponseHeader.TransferEncoding] == "chunked"))
            {
                var wrapped = StorageException.FromWebException(exception);
                if (wrapped != null)
                    throw wrapped; // do this on a separate statement so the debugger can re-execute
            }
        }
    }

    /// <summary>
    /// Describes an event involving an StorageRequest.
    /// </summary>
    public class StorageRequestArgs : EventArgs
    {
        public StorageRequest Request { get; private set; }

        public StorageRequestArgs(StorageRequest request)
        {
            this.Request = request;
        }
    }

    /// <summary>
    /// Common base class for all concrete StorageRequests, pairs each one tightly with its StorageResponse
    /// counterpart.
    /// </summary>
    public abstract class StorageRequest<TResponse> : StorageRequest
        where TResponse : StorageResponse, new()
    {
        internal StorageRequest(StorageService service, string method, string bucketName, string objectKey,
            string queryString)
            : base(service, method, bucketName, objectKey, queryString)
        {
        }

        /// <summary>
        /// Gets the Storage REST response synchronously.
        /// </summary>
        public virtual TResponse GetResponse()
        {
            AuthorizeIfNecessary();

            try
            {
                return new TResponse { WebResponse = (HttpWebResponse)WebRequest.GetResponse() };
            }
            catch (WebException exception)
            {
                TryThrowStorageException(exception);
                throw;
            }
        }

        /// <summary>
        /// Begins an asynchronous request to Storage.
        /// </summary>
        public virtual IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
        {
            AuthorizeIfNecessary();
            return WebRequest.BeginGetResponse(callback, state);
        }

        /// <summary>
        /// Ends an asynchronous call to BeginGetResponse().
        /// </summary>
        public virtual TResponse EndGetResponse(IAsyncResult asyncResult)
        {
            try
            {
                return new TResponse { WebResponse = (HttpWebResponse)WebRequest.EndGetResponse(asyncResult) };
            }
            catch (WebException exception)
            {
                TryThrowStorageException(exception);
                throw;
            }
        }
    }
}
