using Plana.Database;
using Plana.NetworkProtocol;
using Phrenapates.Services;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class EventContent : ProtocolHandlerBase
    {
        private ISessionKeyService sessionKeyService;
        private SCHALEContext context;

        public EventContent(IProtocolHandlerFactory protocolHandlerFactory, ISessionKeyService _sessionKeyService, SCHALEContext _context) : base(protocolHandlerFactory)
        {
            sessionKeyService = _sessionKeyService;
            context = _context;
        }

        [ProtocolHandler(Protocol.EventContent_CollectionList)]
        public ResponsePacket CollectionListHandler(EventContentCollectionListRequest req)
        {

            return new EventContentCollectionListResponse()
            {

            };
        }
    }
}
