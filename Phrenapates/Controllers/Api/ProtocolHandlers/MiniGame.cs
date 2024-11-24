using Plana.Database;
using Plana.NetworkProtocol;
using Phrenapates.Services;
using Plana.FlatData;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class MiniGame : ProtocolHandlerBase
    {
        private readonly ISessionKeyService sessionKeyService;
        private readonly SCHALEContext context;
        private readonly ExcelTableService excelTableService;

        public MiniGame(IProtocolHandlerFactory protocolHandlerFactory, ISessionKeyService _sessionKeyService, SCHALEContext _context, ExcelTableService _excelTableService) : base(protocolHandlerFactory)
        {
            sessionKeyService = _sessionKeyService;
            context = _context;
            excelTableService = _excelTableService;
        }

        [ProtocolHandler(Protocol.MiniGame_StageList)]
        public ResponsePacket StageListHandler(MiniGameStageListRequest req)
        {
            var mgMissionExcel = excelTableService.GetTable<EventContentStageExcelTable>().UnPack().DataList;
            var mgMissionData = mgMissionExcel
                .Where(x => x.EventContentId == req.EventContentId)
                .Select(x => {
                    return new MiniGameHistoryDB()
                    {
                        EventContentId = x.EventContentId,
                        UniqueId = x.Id,
                        AccumulatedScore = 0,
                        IsFullCombo = false,
                        ClearDate = DateTime.Now,
                        HighScore = 0
                    };
                }).ToList();

            return new MiniGameStageListResponse()
            {
                MiniGameHistoryDBs = mgMissionData
            };
        }

        [ProtocolHandler(Protocol.MiniGame_MissionList)]
        public ResponsePacket MissionListHandler(MiniGameMissionListRequest req)
        {
            //Not work
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var mgMissionExcel = excelTableService.GetTable<EventContentMissionExcelTable>().UnPack().DataList;
            var mgMissionExcel2 = excelTableService.GetTable<EventContentMissionExcelTable>().UnPack().DataList;
            var mgMissionData = mgMissionExcel
                .Where(x => x.EventContentId == req.EventContentId)
                .Select(x => x.Id).ToList();

            var count = 0;
            var missionProgress = mgMissionExcel2
                .Where(x => x.EventContentId == req.EventContentId)
                .Select(x => {
                    count++;
                    return new MissionProgressDB()
                    {
                        Account = account,
                        AccountServerId = req.AccountId,
                        MissionUniqueId = x.Id,
                        StartTime = DateTime.MinValue,
                        Complete = true,
                        ServerId = count
                    };
                }).ToList();

            return new MiniGameMissionListResponse()
            {
                MissionHistoryUniqueIds = mgMissionData,
                MissionProgressDBs = missionProgress
            };
        }

        [ProtocolHandler(Protocol.MiniGame_EnterStage)]
        public ResponsePacket EnterStageHandler(MiniGameEnterStageRequest req)
        {
            return new MiniGameEnterStageResponse();
        }
    }
}
