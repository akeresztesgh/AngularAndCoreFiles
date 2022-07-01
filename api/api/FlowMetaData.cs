namespace api
{
    public class FlowMetaData
    {
        public long FlowChunkNumber { get; set; }
        public long FlowChunkSize { get; set; }
        public long FlowCurrentChunkSize { get; set; }
        public long FlowTotalSize { get; set; }
        public string FlowIdentifier { get; set; }
        public string FlowFilename { get; set; }
        public string FlowRelativePath { get; set; }
        public int FlowTotalChunks { get; set; }

        public long FileOffset
        {
            get { return FlowChunkSize * (FlowChunkNumber - 1); }
        }

    }
}
