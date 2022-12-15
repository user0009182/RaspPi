using System;

namespace Protocol
{
    public enum TraceEventId
    {
        TlsAuthenticatingAsServer,
        TlsAuthenticationSuccess,
        ServerStarting,
        ListenerStarted,
        DeviceDeregistered,
        ClientConnecting,
        HandshakeAsServerBegin,
        HandshakeAsServerFailed,
        HandshakeAsServerSuccess,
        ClientConnected,
        ClientAcceptError,
        ReceivedKeepaliveResponse,
        IdleTimeoutTriggered,
        ClientMessageReceiveError,
        ClientMessageSendError,
        DeviceHandshakeAsServerFailure,
        HandshakeAsClientReceiveRemoteName,
        HandshakeAsClientBegin,
        HandshakeAsClientSuccess,
        HandshakeAsClientFailed,
        ServerCertificateValidationFailed,
        ServerCertificateValidationSuccess,
        ConnectingAsClient,
        ConnectedAsClient,
        ClientCertificatePath,
        TlsAuthenticatingAsClient,
        DeviceHandshakeAsClientFailure,
        SendResponseFailed,
        MissingRouteRequestEntry,
        ResponseForwardingFailed,
        ResolveFailure,
        HandlerFault
    }
}
