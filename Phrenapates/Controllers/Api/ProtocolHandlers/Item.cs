using Plana.Database;
using Plana.NetworkProtocol;
using Phrenapates.Services;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class Item : ProtocolHandlerBase
    {
        private ISessionKeyService sessionKeyService;
        private SCHALEContext context;

        public Item(IProtocolHandlerFactory protocolHandlerFactory, ISessionKeyService _sessionKeyService, SCHALEContext _context) : base(protocolHandlerFactory)
        {
            sessionKeyService = _sessionKeyService;
            context = _context;
        }

        [ProtocolHandler(Protocol.Item_List)]
        public ResponsePacket ListHandler(ItemListRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            return new ItemListResponse()
            {
                ItemDBs = [.. account.Items],
                ExpiryItemDBs = []
            };
        }

        [ProtocolHandler(Protocol.Item_AutoSynth)]
        public ResponsePacket AutoSynthHandler(ItemAutoSynthRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            return new ItemAutoSynthResponse()
            {
                ParcelResultDB = new()
            };
        }
    }
}
