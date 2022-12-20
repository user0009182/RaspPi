using Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hub
{
    class IncomingMessageProcessor
    {
        BlockingCollection<BaseMessage> incomingMessageQueue;
        EventTracer trace;
        CancellationToken cancellationToken;
        CancellationTokenSource cancellationSource = new CancellationTokenSource();
        Hub server;

        public IncomingMessageProcessor(Hub server, BlockingCollection<BaseMessage> incomingMessageQueue, EventTracer tracer)
        {
            this.server = server;
            this.incomingMessageQueue = incomingMessageQueue;
            this.trace = tracer;
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
            
            var device = server.GetConnectedDevice(targetId);
            if (device == null)
            {
                //device is not connected
                //send back response to that effect
                var newResponse = new ResponseMessage(request.RequestId, Encoding.ASCII.GetBytes("notfound"));
                device = server.GetConnectedDevice(request.SourceDeviceId);
                if (device == null)
                {
                    //TODO
                    trace.Failure(TraceEventId.SendResponseFailed, request.SourceDeviceId.ToString());
                    return;
                }
                device.Send(newResponse);
                return;
            }

            uint requestId = server.GenerateRequestId();
            server.RoutedRequestTable.AddEntry(requestId, targetId, request.SourceDeviceId, request.RequestId);
            var newRequest = new RequestMessage(requestId, request.TargetDeviceName, targetId, request.RequestData);
            device.Send(newRequest);
        }

        void ProcessResponse(ResponseMessage response)
        {
            //lookup corresponding entry for the request sent out
            var routedRequest = server.RoutedRequestTable.TakeRoutedRequest(response.RequestId, response.SourceDeviceId);
            if (routedRequest == null)
            {
                //  $"No routed request entry matching received response message reqid:{response.RequestId} src device:{response.SourceDeviceId}"
                trace.Error(TraceEventId.MissingRouteRequestEntry, Convert.ToString(response.RequestId), response.SourceDeviceId.ToString());
                return;
            }
            //prepare to forward response onto the original source of the request
            var device = server.GetConnectedDevice(routedRequest.SrcDeviceId);
            if (device == null)
            {
                //source device is not connected
                //TODO
                //$"Could not forward response from {response.SourceDeviceId} to {routedRequest.SrcDeviceId}"
                trace.Failure(TraceEventId.ResponseForwardingFailed, response.SourceDeviceId.ToString(), routedRequest.SrcDeviceId.ToString());
                return;
            }
            var newResponse = new ResponseMessage(routedRequest.SrcRequestId, response.RequestData);
            device.Send(newResponse);
        }
    }
}
