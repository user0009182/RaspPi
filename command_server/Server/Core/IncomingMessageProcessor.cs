using Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class IncomingMessageProcessor
    {
        BlockingCollection<BaseMessage> incomingMessageQueue;
        private ILogger logger;
        CancellationToken cancellationToken;
        CancellationTokenSource cancellationSource = new CancellationTokenSource();
        Server server;

        public IncomingMessageProcessor(Server server, BlockingCollection<BaseMessage> incomingMessageQueue, ILogger logger)
        {
            this.server = server;
            this.incomingMessageQueue = incomingMessageQueue;
            this.logger = logger;
            cancellationToken = cancellationSource.Token;
        }

        public void Start()
        {
            Task.Run(() => ThreadProc());
        }

        public void Stop()
        {
            cancellationSource.Cancel();
        }

        void ThreadProc()
        {
            while (!cancellationSource.IsCancellationRequested)
            {
                try
                {
                    var message = incomingMessageQueue.Take(cancellationToken);
                    if (cancellationSource.IsCancellationRequested)
                        break;
                    ProcessMessage(message);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        void ProcessMessage(BaseMessage message)
        {
            switch (message.Type)
            {
                case DeviceProtocolMessageType.Request:
                    ProcessRequest(message as RequestMessage);
                    break;
                case DeviceProtocolMessageType.Response:
                    ProcessResponse(message as ResponseMessage);
                    break;
            }
        }

        void ProcessRequest(RequestMessage request)
        {
            var targetId = request.TargetDeviceId;
            //handle internal command
            if (targetId == server.DeviceId)
            {
                server.CommandQueue.Add(request);
                return;
            }

            //command needs to be forwarded elsewhere
            //determine target device_id of request
            if (request.TargetDeviceName != null)
            {
                targetId = server.Router.ResolveDeviceId(request.TargetDeviceName);
            }
            
            var handler = server.GetConnectedDevice(targetId);
            if (handler == null)
            {
                //device is not connected
                //send back response to that effect
                var newResponse = new ResponseMessage(request.RequestId, Encoding.ASCII.GetBytes("notfound"));
                var sourceHandler = server.GetConnectedDevice(request.SourceDeviceId);
                if (sourceHandler == null)
                {
                    //TODO
                    logger.Log($"could not send response to {request.SourceDeviceId}");
                    return;
                }
                sourceHandler.SendQueue.Add(newResponse);
                return;
            }

            uint requestId = server.GenerateRequestId();
            server.RoutedRequestTable.AddEntry(requestId, targetId, request.SourceDeviceId, request.RequestId);
            var newRequest = new RequestMessage(requestId, request.TargetDeviceName, targetId, request.RequestData);
            handler.SendQueue.Add(newRequest);
        }

        void ProcessResponse(ResponseMessage response)
        {
            //lookup corresponding entry for the request sent out
            var routedRequest = server.RoutedRequestTable.TakeRoutedRequest(response.RequestId, response.SourceDeviceId);
            if (routedRequest == null)
            {
                logger.Log($"No routed request entry matching received response message reqid:{response.RequestId} src device:{response.SourceDeviceId}");
                return;
            }
            //prepare to forward response onto the original source of the request
            var handler = server.GetConnectedDevice(routedRequest.SrcDeviceId);
            if (handler == null)
            {
                //source device is not connected
                //TODO
                logger.Log($"Could not forward response from {response.SourceDeviceId} to {routedRequest.SrcDeviceId}");
                return;
            }
            var newResponse = new ResponseMessage(routedRequest.SrcRequestId, response.RequestData);
            handler.SendQueue.Add(newResponse);
        }
    }
}
