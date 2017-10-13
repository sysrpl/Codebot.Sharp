using System;
using System.ComponentModel;

namespace Codebot.Storage
{
    /// <summary>
    /// Provides progress data for Storage object transfer operations.
    /// </summary>
    [Serializable]
    public class StorageProgressEventArgs : ProgressChangedEventArgs
    {
        /// <summary>
        /// Gets the request associated with object being transferred.
        /// </summary>
		public StorageRequest Request { get; private set; }

        /// <summary>
        /// Gets the bucket of the object being transferred.
        /// </summary>
        public string BucketName { get; private set; }

        /// <summary>
        /// Gets the key of the object being transferred.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Gets the number of bytes transferred. 
        /// </summary>
        public long BytesTransferred { get; private set; }

        /// <summary>
        /// Gets the total number of bytes in the transfer operation.
        /// </summary>
        public long BytesTotal { get; private set; }

        public StorageProgressEventArgs(StorageRequest request, string bucketName, string key,
            long bytesTransferred, long bytesTotal)
            : base((int)Math.Round(bytesTransferred * 100.0 / bytesTotal), null)
        {
			this.Request = request;
            this.BucketName = bucketName;
            this.Key = key;
            this.BytesTransferred = bytesTransferred;
            this.BytesTotal = bytesTotal;
        }
    }
}
