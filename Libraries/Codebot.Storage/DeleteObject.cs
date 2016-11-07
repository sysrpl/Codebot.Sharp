using System;
using System.Net;

namespace Codebot.Storage
{
    /// <summary>
    /// Deletes an object in an Storage bucket.
    /// </summary>
    public class DeleteObjectRequest : StorageRequest<DeleteObjectResponse>
    {
        public DeleteObjectRequest(StorageService service, string bucketName, string key)
            : base(service, "DELETE", bucketName, key, null)
        {
        }
    }

    /// <summary>
    /// Represents the response for deleting an Storage object.
    /// </summary>
    public sealed class DeleteObjectResponse : StorageResponse
    {
        protected override void ProcessResponse()
        {
            if (WebResponse.StatusCode != HttpStatusCode.NoContent)
                throw new Exception("Unexpected status code: " + WebResponse.StatusCode);
        }
    }
}
