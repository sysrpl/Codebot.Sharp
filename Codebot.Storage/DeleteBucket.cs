using System;
using System.Net;

namespace Codebot.Storage
{
    /// <summary>
    /// Deletes a bucket from Storage. The bucket must be empty.
    /// </summary>
    public class DeleteBucketRequest : StorageRequest<DeleteBucketResponse>
    {
        public DeleteBucketRequest(StorageService service, string bucketName)
            : base(service, "DELETE", bucketName, null, null)
        {
        }
    }

    /// <summary>
    /// Represents an Storage response for a deleted bucket.
    /// </summary>
    public sealed class DeleteBucketResponse : StorageResponse
    {
        protected override void ProcessResponse()
        {
            if (WebResponse.StatusCode != HttpStatusCode.NoContent)
                throw new Exception("Unexpected status code: " + WebResponse.StatusCode);
        }
    }
}
