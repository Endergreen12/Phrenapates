using Phrenapates.Services;
using Plana.Database;
using Plana.FlatData;
using Plana.MX.GameLogic.DBModel;
using Plana.MX.NetworkProtocol;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class EventContent : ProtocolHandlerBase
    {
        private readonly ISessionKeyService sessionKeyService;
        private readonly SCHALEContext context;
        private readonly ExcelTableService excelTableService;

        public EventContent(IProtocolHandlerFactory protocolHandlerFactory, ISessionKeyService _sessionKeyService, SCHALEContext _context, ExcelTableService _excelTableService) : base(protocolHandlerFactory)
        {
            sessionKeyService = _sessionKeyService;
            context = _context;
            excelTableService = _excelTableService;
        }

        [ProtocolHandler(Protocol.EventContent_CollectionList)]
        public ResponsePacket CollectionListHandler(EventContentCollectionListRequest req)
        {
            //???
            var eventContentCollectionDB = excelTableService.GetTable<EventContentCollectionExcelTable>().UnPack().DataList;
            var eventData = eventContentCollectionDB
                .Where(x => x.EventContentId == req.EventContentId).ToList()
                .Select(x => {
                    return new EventContentCollectionDB()
                    {
                        UniqueId = 1,
                        EventContentId = x.EventContentId,
                        ReceiveDate = DateTime.Now,
                        GroupId = x.GroupId
                    };
                }).ToList();
            return new EventContentCollectionListResponse()
            {
                EventContentUnlockCGDBs = eventData
            };
        }

        [ProtocolHandler(Protocol.EventContent_AdventureList)]
        public ResponsePacket AdventureListHandler(EventContentAdventureListRequest req)
        {
            //Not sure how it used and works for.
            /*var campaignStage = sessionKeyService.GetAccount(req.SessionKey).CampaignStageHistories.ToList();
            var strategyObjectExcel = excelTableService.GetTable<CampaignStrategyObjectExcelTable>().UnPack().DataList;

            var strategyObject = strategyObjectExcel
                .Select(x => {
                    return new StrategyObjectHistoryDB()
                    {
                        StrategyObjectId = x.Id
                    };
                }).ToList();*/

            return new EventContentAdventureListResponse()
            {
                //StageHistoryDBs = campaignStage,
                //StrategyObjecthistoryDBs = strategyObject,
            };
        }

        [ProtocolHandler(Protocol.EventContent_BoxGachaShopList)]
        public ResponsePacket BoxGachaShopListHandler(EventContentBoxGachaShopListRequest req)
        {
            return new EventContentBoxGachaShopListResponse();
        }

        [ProtocolHandler(Protocol.EventContent_ScenarioGroupHistoryUpdate)]
        public ResponsePacket ScenarioGroupHistoryUpdateHandler(EventContentScenarioGroupHistoryUpdateRequest req)
        {
            return new EventContentScenarioGroupHistoryUpdateResponse();
        }

        [ProtocolHandler(Protocol.EventContent_PermanentList)]
        public ResponsePacket PermanentListHandler(EventContentPermanentListRequest req)
        {
            return new EventContentPermanentListResponse();
        }

    }
}
