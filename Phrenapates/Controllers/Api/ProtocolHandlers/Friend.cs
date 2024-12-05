using Plana.MX.NetworkProtocol;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class Friend : ProtocolHandlerBase
    {
        public Friend(IProtocolHandlerFactory protocolHandlerFactory) : base(protocolHandlerFactory) { }

        [ProtocolHandler(Protocol.Friend_Check)]
        public ResponsePacket CheckHandler(FriendCheckRequest req)
        {

            return new FriendCheckResponse()
            {
                
            };
        }
    }
}
