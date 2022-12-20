using Protocol;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Hub
{
    class ResponseTimeoutThread
    {
        private readonly Hub server;
        RoutedRequestTable routedRequestTable;
        EventTracer tracer;
        public ResponseTimeoutThread(Hub server, RoutedRequestTable routedRequestTable, EventTracer tracer)
        {
            this.server = server;
            this.routedRequestTable = routedRequestTable;
            this.tracer = tracer;
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
            var device = server.GetConnectedDevice(targetDeviceId);
            if (device == null)
            {
                //TODO
                //logger.Log($"Could not forward response from {response.SourceDeviceId} to {routedRequest.SrcDeviceId}");
                return;
            }
            var newResponse = new ResponseMessage(requestId, Encoding.ASCII.GetBytes("timeout"));
            device.Send(newResponse);
        }
    }
}
