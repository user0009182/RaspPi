using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    /// <summary>
    /// Contains records of requests that have been sent out to which a response is still be waited upon
    /// </summary>
    public class RoutedRequestTable
    {
        Dictionary<uint, RoutedRequest> routedRequests = new Dictionary<uint, RoutedRequest>();
        object lock_obj = new object();

        Server server;
        ILogger logger;
        public RoutedRequestTable(Server server, ILogger logger)
        {
            this.server = server;
            this.logger = logger;
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

    class ResponseTimeoutThread
    {
        private readonly Server server;
        RoutedRequestTable routedRequestTable;
        ILogger logger;
        public ResponseTimeoutThread(Server server, RoutedRequestTable routedRequestTable, ILogger logger)
        {
            this.server = server;
            this.routedRequestTable = routedRequestTable;
            this.logger = logger;
        }
        public void Start()
        {
            Task.Run(() => ThreadProc());
        }

        void ThreadProc()
        {
            while (true) //TODO cancellation
            {
                var entry = routedRequestTable.GetNextToExpire();
                if (entry == null)
                {
                    //wake up every 30 seconds to see if there is a request entry
                    Task.Delay(30000).Wait();
                    continue;
                }
                var secondsUntilExpire = entry.ExpireTime.Subtract(DateTime.Now).TotalSeconds;
                if (secondsUntilExpire < 0)
                {
                    TimeoutRequest(entry);
                    continue;
                }
                Task.Delay((int)(secondsUntilExpire * 1000)).Wait();
            }
        }

        void TimeoutRequest(RoutedRequest r)
        {
            //reobtain to ensure the entry hasn't been processed since
            var request = routedRequestTable.TakeRoutedRequest(r.RequestId, r.SrcDeviceId);
            if (request == null)
            {
                return;
            }

            PostTimeoutResponse(r.SrcDeviceId, r.SrcRequestId);
        }

        void PostTimeoutResponse(Guid targetDeviceId, uint requestId)
        {
            //prepare to forward response onto the original source of the request
            var handler = server.GetConnectedDevice(targetDeviceId);
            if (handler == null)
            {
                //TODO
                //logger.Log($"Could not forward response from {response.SourceDeviceId} to {routedRequest.SrcDeviceId}");
                return;
            }
            var newResponse = new ResponseMessage(requestId, Encoding.ASCII.GetBytes("timeout"));
            handler.SendQueue.Add(newResponse);
        }
    }


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
