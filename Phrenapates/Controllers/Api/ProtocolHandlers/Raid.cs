using Plana.Database;
using Plana.FlatData;
using Plana.NetworkProtocol;
using Phrenapates.Managers;
using Phrenapates.Services;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class Raid : ProtocolHandlerBase
    {
        private readonly ISessionKeyService sessionKeyService;
        private readonly SCHALEContext context;
        private readonly ExcelTableService excelTableService;

        public Raid(IProtocolHandlerFactory protocolHandlerFactory, ISessionKeyService _sessionKeyService, SCHALEContext _context, ExcelTableService _excelTableService) : base(protocolHandlerFactory)
        {
            sessionKeyService = _sessionKeyService;
            context = _context;
            excelTableService = _excelTableService;
        }

        [ProtocolHandler(Protocol.Raid_Lobby)]
        public ResponsePacket LobbyHandler(RaidLobbyRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            var raidSeasonExcel = excelTableService.GetTable<RaidSeasonManageExcelTable>().UnPack().DataList;
            var targetSeason = raidSeasonExcel.FirstOrDefault(x => x.SeasonId == account.RaidInfo.SeasonId);

            return new RaidLobbyResponse()
            {
                SeasonType = RaidSeasonType.Open,
                RaidLobbyInfoDB = RaidManager.Instance.GetLobby(account.RaidInfo, targetSeason),
            };
        }

        [ProtocolHandler(Protocol.Raid_CreateBattle)]
        public ResponsePacket CreateBattleHandler(RaidCreateBattleRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            account.RaidInfo.CurrentRaidUniqueId = req.RaidUniqueId;
            account.RaidInfo.CurrentDifficulty = req.Difficulty;

            context.Entry(account).Property(x => x.RaidInfo).IsModified = true; // force update
            context.SaveChanges();

            var raid = RaidManager.Instance.CreateRaid(account.RaidInfo, account.ServerId, account.Nickname, req.IsPractice, req.RaidUniqueId);
            var battle = RaidManager.Instance.CreateBattle(account.ServerId, account.Nickname, req.RaidUniqueId);

            return new RaidCreateBattleResponse()
            {
                RaidDB = raid,
                RaidBattleDB = battle,
                AssistCharacterDB = new () { }
            };
        }

        [ProtocolHandler(Protocol.Raid_EnterBattle)] // clicked restart
        public ResponsePacket EnterBattleHandler(RaidEnterBattleRequest req)
        {
            var raid = RaidManager.Instance.RaidDB;
            var battle = RaidManager.Instance.RaidBattleDB;

            return new RaidEnterBattleResponse()
            {
                RaidDB = raid,
                RaidBattleDB = battle,
                AssistCharacterDB = new() { }
            };
        }

        [ProtocolHandler(Protocol.Raid_EndBattle)]
        public ResponsePacket EndBattleHandler(RaidEndBattleRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            var raidStageTable = excelTableService.GetTable<RaidStageExcelTable>().UnPack().DataList;
            var currentRaidData = raidStageTable.FirstOrDefault(x => x.Id == account.RaidInfo.CurrentRaidUniqueId);
            
            var totalTime = req.Summary.EndFrame/30f;
            var timeScore = RaidManager.CalculateTimeScore(totalTime, account.RaidInfo.CurrentDifficulty);
            var hpPercentScorePoint = currentRaidData.HPPercentScore;
            var defaultClearPoint = currentRaidData.DefaultClearScore;

            var rankingPoint = timeScore + hpPercentScorePoint + defaultClearPoint;

            account.RaidInfo.BestRankingPoint = rankingPoint > account.RaidInfo.BestRankingPoint ? rankingPoint : account.RaidInfo.BestRankingPoint;
            account.RaidInfo.TotalRankingPoint += rankingPoint;
            context.Entry(account).Property(x => x.RaidInfo).IsModified = true; // force update
            context.SaveChanges();

            // saving battle result to continue on next attempt doesn't work
            var battle = RaidManager.Instance.RaidBattleDB;
            var bossResult = req.Summary.RaidSummary.RaidBossResults.FirstOrDefault();

            battle.CurrentBossHP = bossResult.EndHpRateRawValue;
            battle.CurrentBossGroggy = bossResult.GroggyRateRawValue;
            battle.CurrentBossAIPhase = bossResult.AIPhase;
            battle.SubPartsHPs = bossResult.SubPartsHPs;

            return new RaidEndBattleResponse()
            {
                RankingPoint = rankingPoint,
                BestRankingPoint = account.RaidInfo.BestRankingPoint,
                ClearTimePoint = timeScore,
                HPPercentScorePoint = hpPercentScorePoint,
                DefaultClearPoint = defaultClearPoint
            };
        }

        [ProtocolHandler(Protocol.Raid_OpponentList)]
        public ResponsePacket OpponentListHandler(RaidOpponentListRequest req)
        {
            return new RaidOpponentListResponse()
            {
                OpponentUserDBs = []
            };
        }
    }
}
