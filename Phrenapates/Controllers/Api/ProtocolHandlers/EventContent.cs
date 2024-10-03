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

        [ProtocolHandler(Protocol.EventContent_AdventureList)]
        public ResponsePacket AdventureListHandler(EventContentAdventureListRequest req)
        {

            return new EventContentAdventureListResponse()
            {

            };
        }

        [ProtocolHandler(Protocol.EventContent_BoxGachaShopList)]
        public ResponsePacket BoxGachaShopListHandler(EventContentBoxGachaShopListRequest req)
        {

            return new EventContentBoxGachaShopListResponse()
            {

            };
        }

        [ProtocolHandler(Protocol.EventContent_ScenarioGroupHistoryUpdate)]
        public ResponsePacket ScenarioGroupHistoryUpdateHandler(EventContentScenarioGroupHistoryUpdateRequest req)
        {

            return new EventContentScenarioGroupHistoryUpdateResponse()
            {

            };
        }
    }
}
