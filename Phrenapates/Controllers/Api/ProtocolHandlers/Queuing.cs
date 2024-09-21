using Plana.NetworkProtocol;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class Queuing : ProtocolHandlerBase
    {
        public Queuing(IProtocolHandlerFactory protocolHandlerFactory) : base(protocolHandlerFactory) { }

        [ProtocolHandler(Protocol.Queuing_GetTicket)]
        public ResponsePacket GetTicketHandler(QueuingGetTicketRequest req)
        {
            return new QueuingGetTicketResponse()
            {
                EnterTicket = $"{req.YostarUID}:{req.YostarToken}"
            };
        }

        [ProtocolHandler(Protocol.Queuing_GetTicketGL)]
        public ResponsePacket GetTicketGLHandler(QueuingGetTicketGLRequest req)
        {
            return new QueuingGetTicketGLResponse()
            {
                EnterTicket = $"{req.NpSN}:{req.NpToken}"
            };
        }
    }
}
