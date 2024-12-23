using Phrenapates.Services;
using Plana.Database;
using Plana.MX.GameLogic.DBModel;
using Plana.MX.NetworkProtocol;
using Plana.MX.Logic.Battles;
using Phrenapates.Managers;
using Plana.FlatData;
using System.Text.Json;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class MultiFloorRaid : ProtocolHandlerBase
    {
        private readonly ISessionKeyService sessionKeyService;
        private readonly SCHALEContext context;
        private readonly ExcelTableService excelTableService;

        public MultiFloorRaid(IProtocolHandlerFactory protocolHandlerFactory, ISessionKeyService _sessionKeyService, SCHALEContext _context, ExcelTableService _excelTableService) : base(protocolHandlerFactory)
        {
            sessionKeyService = _sessionKeyService;
            context = _context;
            excelTableService = _excelTableService;
        }

        [ProtocolHandler(Protocol.MultiFloorRaid_Sync)]
        public ResponsePacket SyncHandler(MultiFloorRaidSyncRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var raidData = excelTableService.GetExcelList<MultiFloorRaidSeasonManageExcel>("MultiFloorRaidSeasonManageDBSchema")
            .FirstOrDefault(x => x.SeasonId == account.ContentInfo.MultiFloorRaidDataInfo.SeasonId);
            var serverTime = DateTime.Parse(raidData.SeasonStartDate).AddDays(1);
            var db = new MultiFloorRaidDB()
                {
                    ServerId = 1,
                    SeasonId = account.ContentInfo.MultiFloorRaidDataInfo.SeasonId,
                    ClearedDifficulty = 124,
                    RewardDifficulty = 124,
                    LastClearDate = serverTime,
                    LastRewardDate = serverTime,
                    TotalReceivedRewards = new(),
                    TotalReceivableRewards = new()
                };
            return new MultiFloorRaidSyncResponse()
            {
                MultiFloorRaidDBs = [db]
            };
        }

        [ProtocolHandler(Protocol.MultiFloorRaid_EnterBattle)]
        public ResponsePacket EnterBattleHandler(MultiFloorRaidEnterBattleRequest req)
        {
            return new MultiFloorRaidEnterBattleResponse()
            {
                AssistCharacterDBs = new()
            };
        }

        [ProtocolHandler(Protocol.MultiFloorRaid_EndBattle)]
        public ResponsePacket EndBattleHandler(MultiFloorRaidEndBattleRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var multiFloorRaidData = account.ContentInfo.MultiFloorRaidDataInfo;
            MultiFloorRaidDB db = new() { SeasonId = multiFloorRaidData.SeasonId };

            if (!req.Summary.IsAbort && req.Summary.EndType == BattleEndType.Clear)
            {
                if (account.MultiFloorRaids.Any(x => x.AccountServerId == req.AccountId))
                {
                    db = account.MultiFloorRaids.Where(x => x.AccountServerId == req.AccountId).First();
                }
                else
                {
                    account.MultiFloorRaids.Add(db);
                }

                db.SeasonId = db.SeasonId != multiFloorRaidData.SeasonId ? multiFloorRaidData.SeasonId : db.SeasonId;
                db.ClearedDifficulty = 124;
                db.LastClearDate = DateTime.Now;
                db.RewardDifficulty = 124;
                db.LastRewardDate = DateTime.Now;
                db.ClearBattleFrame = req.Summary.EndFrame;
                db.AllCleared = false;
                db.HasReceivableRewards = false;
                db.TotalReceivedRewards = new();
                db.TotalReceivableRewards = new();

                context.SaveChanges();
            }

            return new MultiFloorRaidEndBattleResponse()
            {
                MultiFloorRaidDB = db,
                ParcelResultDB = new()
            };
        }

        [ProtocolHandler(Protocol.MultiFloorRaid_ReceiveReward)]
        public ResponsePacket RecieveRewardHandler(MultiFloorRaidEndBattleRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            return new MultiFloorRaidEndBattleResponse()
            {
                MultiFloorRaidDB = account.MultiFloorRaids.LastOrDefault() ?? new(),
                ParcelResultDB = new()
            };
        }
    }
}
