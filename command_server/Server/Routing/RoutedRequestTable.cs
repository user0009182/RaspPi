using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hub
{
    /// <summary>
    /// Contains records of requests that have been sent out to which a response is still be waited upon
    /// </summary>
    public class RoutedRequestTable
    {
        Dictionary<uint, RoutedRequest> routedRequests = new Dictionary<uint, RoutedRequest>();
        object lock_obj = new object();

        Hub server;
        EventTracer tracer;
        public RoutedRequestTable(Hub server, EventTracer tracer)
        {
            this.server = server;
            this.tracer = tracer;
        }

        /// <summary>
        /// Remove and return an entry for the given request ID
        /// Returns null if an entry for the given request ID doesn't exist
        /// Returns null if an entry for the given request ID exists but its source device ID doesn't match the given source device ID
        /// </summary>
        public RoutedRequest TakeRoutedRequest(uint requestId, Guid sourceDeviceId)
        {
            lock(lock_obj)
            {
                RoutedRequest routedRequest;
                if (!routedRequests.TryGetValue(requestId, out routedRequest))
                    return null;
                if (routedRequest.DestDeviceId != sourceDeviceId)
                    return null;
                routedRequests.Remove(requestId);
                return routedRequest;
            }
        }

        /// <summary>
        /// Returns the entry with the next (lowest) expiry time
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public RoutedRequest GetNextToExpire()
        {
            lock (lock_obj)
            {
                return routedRequests.Values.ToArray().OrderBy(r => r.ExpireTime).FirstOrDefault();
            }
        }

        internal void AddEntry(uint requestId, Guid destDeviceId, Guid srcDeviceId, uint srcRequestId)
        {
            lock (lock_obj)
            {
                var expireTime = DateTime.Now.AddSeconds(30);
                var routedRequest = new RoutedRequest(requestId, destDeviceId, srcDeviceId, srcRequestId, expireTime);
                routedRequests.Add(requestId, routedRequest);
            }
        }
    }
}
