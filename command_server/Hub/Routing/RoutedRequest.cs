using System;

namespace Hub
{
    public class RoutedRequest
    {
        public uint RequestId { get; set; }
        public Guid DestDeviceId { get; set; }

        public Guid SrcDeviceId { get; set; }
        public uint SrcRequestId { get; set; }

        public DateTime ExpireTime { get; set; }

        public RoutedRequest(uint requestId, Guid destDeviceId, Guid srcDeviceId, uint srcRequestId, DateTime expireTime)
        {
            RequestId = requestId;
            DestDeviceId = destDeviceId;
            SrcDeviceId = srcDeviceId;
            SrcRequestId = srcRequestId;
            ExpireTime = expireTime;
        }
    }
}
