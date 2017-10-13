using System;
using System.Collections.Generic;

namespace Codebot.Storage
{
    /// <summary>
    /// Gets all buckets owned by you.
    /// </summary>
    public class GetAllBucketsRequest : StorageRequest<GetAllBucketsResponse>
    {
        public GetAllBucketsRequest(StorageService service)
            : base(service, "GET", null, null, null)
        {
        }
    }

    /// <summary>
    /// Contains the Storage response with all your owned buckets.
    /// </summary>
    public sealed class GetAllBucketsResponse : StorageResponse
    {
        public Identity Owner { get; private set; }

        protected override void ProcessResponse()
        {
            // See http://docs.amazonwebservices.com/AmazonStorage/2006-03-01/RESTServiceGET.html

            // read everything up to the list of buckets

            Reader.ReadStartElement("ListAllMyBucketsResult");

            if (Reader.Name == "Owner")
                this.Owner = new Identity(Reader);
            else
                throw new Exception("Expected <Owner>.");

            // we're expecting the buckets list now
            if (Reader.Name != "Buckets")
                throw new Exception("Expected <Buckets>.");
        }

        /// <summary>
        /// Provides a forward-only reader for efficiently enumerating through the response
        /// list of Buckets.
        /// </summary>
        public IEnumerable<Bucket> Buckets
        {
            get
            {
                if (!Reader.IsEmptyElement && Reader.Read())
                    while (Reader.Name == "Bucket")
                        yield return new Bucket(Reader);
            }
        }
    }
}
