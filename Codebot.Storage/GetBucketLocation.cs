
namespace Codebot.Storage
{
    /// <summary>
    /// Queries Storage for the hosted location of a bucket.
    /// </summary>
    public class GetBucketLocationRequest : StorageRequest<GetBucketLocationResponse>
    {
        public GetBucketLocationRequest(StorageService service, string bucketName)
            : base(service, "GET", bucketName, null, "?location")
        {
        }
    }

    /// <summary>
    /// The Storage response for the hosted location of a bucket.
    /// </summary>
    public sealed class GetBucketLocationResponse : StorageResponse
    {
        /// <summary>
        /// Gets true if the bucket was created in the Europe location.
        /// </summary>
        public bool IsEurope { get; private set; }

        protected override void ProcessResponse()
        {
            string location = Reader.ReadElementContentAsString("LocationConstraint", "");

            if (location == "EU")
                IsEurope = true;
        }
    }
}
