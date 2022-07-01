using Microsoft.Extensions.Caching.Memory;

namespace api
{
    public interface IFlowUploadProcessorCore
    {
        DateTime CompletedDateTime { get; }
        bool IsComplete { get; }
        bool HasRecievedChunk(FlowMetaData chunkMeta);
        Task<bool> ProcessUploadChunkRequest(HttpRequest request, FlowMetaData filePart, string uploadPath);
    }

    public class FlowUploadProcessorCore : IFlowUploadProcessorCore
    {
        private static readonly object lockObject = new object();

        public FlowUploadProcessorCore(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
        }

        //================================================================================
        // Class Methods
        //================================================================================
        #region Methods
        /// <summary>
        /// Track our in progress uploads, by using a cache, we make sure we don't accumulate memory
        /// </summary>
        private readonly IMemoryCache memoryCache;

        private FileMetaData? GetFileMetaData(string flowIdentifier)
        {
            if (memoryCache.TryGetValue(flowIdentifier, out var fileMetaData))
            {
                return fileMetaData as FileMetaData;
            }
            return null;
        }

        /// <summary>
        /// Keep an upload in cache for two hours after it is last used
        /// </summary>
        private static MemoryCacheEntryOptions DefaultCacheItemPolicy()
        {
            return new MemoryCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };
        }

        /// <summary>
        /// (Thread Safe) Marks a chunk as recieved.
        /// </summary>
        private bool RegisterSuccessfulChunk(FlowMetaData chunkMeta)
        {
            var fileMeta = GetFileMetaData(chunkMeta.FlowIdentifier);
            if (fileMeta == null)
            {
                lock (lockObject)
                {
                    fileMeta = GetFileMetaData(chunkMeta.FlowIdentifier);
                    if (fileMeta == null)
                    {
                        fileMeta = new FileMetaData(chunkMeta);
                        memoryCache.Set(chunkMeta.FlowIdentifier, fileMeta, DefaultCacheItemPolicy());
                    }
                }
            }

            fileMeta.RegisterChunkAsReceived(chunkMeta);
            lock (lockObject)
            {
                memoryCache.Set(chunkMeta.FlowIdentifier, fileMeta, DefaultCacheItemPolicy());
            }

            if (fileMeta.IsComplete)
            {
                // Since we are using a cache and memory is automatically disposed,
                // we don't need to do this, so we won't so we can keep a record of
                // our completed uploads.
                memoryCache.Remove(chunkMeta.FlowIdentifier);
            }
            return fileMeta.IsComplete;
        }

        public bool HasRecievedChunk(FlowMetaData chunkMeta)
        {
            var fileMeta = GetFileMetaData(chunkMeta.FlowIdentifier);
            bool wasRecieved = fileMeta != null && fileMeta.HasChunk(chunkMeta);
            return wasRecieved;
        }

        public bool IsComplete { get; private set; }

        public DateTime CompletedDateTime { get; private set; }

        public async Task<bool> ProcessUploadChunkRequest(HttpRequest request, FlowMetaData filePart, string uploadPath)
        {
            var fileData = request.Form.Files[0];
            var filePath = $"{uploadPath}/{filePart.FlowFilename}";
            using (var flowFileStream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write))
            {
                flowFileStream.SetLength(filePart.FlowTotalSize);
                flowFileStream.Seek(filePart.FileOffset, 0);
                {
                    await fileData.CopyToAsync(flowFileStream);
                }
            }

            IsComplete = RegisterSuccessfulChunk(filePart);
            if (IsComplete)
            {
                CompletedDateTime = DateTime.Now;
            }
            return IsComplete;
        }
        #endregion
    }
}
