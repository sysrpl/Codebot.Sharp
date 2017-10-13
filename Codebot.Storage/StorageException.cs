using System;
using System.Net;
using System.Xml;
using System.IO;

namespace Codebot.Storage
{
    /// <summary>
    /// The exception that is thrown when the Storage server returns a specially formatted error object
    /// that we can parse.
    /// </summary>
    public sealed class StorageException : Exception
    {
        /// <summary>
        /// Gets the error code returned by Storage.
        /// </summary>
        public StorageErrorCode ErrorCode { get; private set; }

        /// <summary>
        /// Gets the bucket name this error pertains to, if applicable.
        /// </summary>
        public string BucketName { get; private set; }

        /// <summary>
        /// Gets the ID of the request associated with the error.
        /// </summary>
        public string RequestID { get; private set; }

        /// <summary>
        /// Gets the ID of the host that returned the error.
        /// </summary>
        public string HostID { get; private set; }

        public StorageException(StorageErrorCode errorCode, string bucketName, string message, WebException innerException)
            : base(message, innerException)
        {
            this.ErrorCode = errorCode;
            this.BucketName = bucketName;
        }

        internal static StorageException FromErrorResponse(XmlReader reader, WebException exception)
        {
            if (reader.IsEmptyElement)
                throw new Exception("Expected a non-empty <Error> element.");

            reader.ReadStartElement("Error");

            StorageErrorCode errorCode = StorageErrorCode.Unknown;
            string message = null, bucketName = null, requestID = null, hostID = null;
            
            while (reader.Name != "Error")
            {
                switch (reader.Name)
                {
                    case "Code":
                        errorCode = ParseCode(reader.ReadElementContentAsString());
                        break;
                    case "Message":
                        message = reader.ReadElementContentAsString();
                        break;
                    case "BucketName":
                        bucketName = reader.ReadElementContentAsString();
                        break;
                    case "RequestID":
                        requestID = reader.ReadElementContentAsString();
                        break;
                    case "HostID":
                        hostID = reader.ReadElementContentAsString();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return new StorageException(errorCode, bucketName, message, exception)
            {
                RequestID = requestID,
                HostID = hostID
            };
        }

        internal static StorageException FromWebException(WebException exception)
        {
            HttpWebResponse response = (HttpWebResponse)exception.Response;

            // we need to check the response stream first to make sure there's actually
            // XML in the response. Storage sometimes sends error responses with content-type XML
            // and content-encoding chunked, then no actual data!
            var streamReader = new StreamReader(response.GetResponseStream());

            if (streamReader.EndOfStream)
                return null;
            
            var xmlReader = new XmlTextReader(streamReader)
            {
                WhitespaceHandling = WhitespaceHandling.Significant,
                Namespaces = false
            };
            
            xmlReader.MoveToContent();
            return FromErrorResponse(xmlReader, exception);
        }

        static StorageErrorCode ParseCode(string code)
        {
            if (Enum.IsDefined(typeof(StorageErrorCode), code))
                return (StorageErrorCode)Enum.Parse(typeof(StorageErrorCode), code);
            else
                return StorageErrorCode.Unknown;
        }
    }
}
