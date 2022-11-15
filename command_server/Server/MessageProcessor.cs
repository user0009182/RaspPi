using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class MessageProcessor
    {
        BlockingCollection<BaseMessage> messageQueue;
        CancellationToken cancellationToken;
        CancellationTokenSource cancellationSource = new CancellationTokenSource();
        public MessageProcessor(BlockingCollection<BaseMessage> messageQueue)
        {
            this.messageQueue = messageQueue;
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
            while (true)
            {
                try
                {
                    var message = messageQueue.Take(cancellationToken);
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

            }
        }

        void ProcessRequest(RequestMessage message)
        {
            //forward request onto server


        }
    }
}
