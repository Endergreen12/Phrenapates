using Phrenapates.Managers;
using Phrenapates.Services;
using Plana.Database;
using Plana.FlatData;
using Plana.MX.GameLogic.DBModel;
using Plana.MX.NetworkProtocol;

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
            var targetSeason = raidSeasonExcel.FirstOrDefault(x => x.SeasonId == account.ContentInfo.RaidDataInfo.SeasonId);
            long serverTimeTicks = RaidManager.Instance.CreateServerTime(targetSeason, account.ContentInfo).Ticks;
            return new RaidLobbyResponse()
            {
                SeasonType = RaidSeasonType.Open,
                RaidLobbyInfoDB = RaidManager.Instance.GetLobby(account.ContentInfo, targetSeason),
                ServerTimeTicks = serverTimeTicks
            };
        }

        [ProtocolHandler(Protocol.Raid_CreateBattle)]
        public ResponsePacket CreateBattleHandler(RaidCreateBattleRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            var raidStageExcel = excelTableService.GetTable<RaidStageExcelTable>().UnPack().DataList;
            var characterStatExcel = excelTableService.GetTable<CharacterStatExcelTable>().UnPack().DataList;
            var currentRaidData = raidStageExcel.FirstOrDefault(x => x.Id == req.RaidUniqueId);

            account.ContentInfo.RaidDataInfo.CurrentRaidUniqueId = req.RaidUniqueId;
            account.ContentInfo.RaidDataInfo.CurrentDifficulty = req.Difficulty;

            context.Entry(account).Property(x => x.ContentInfo).IsModified = true; // force update
            context.SaveChanges();
            
            var raidLobbyInfoDB = RaidManager.Instance.RaidLobbyInfoDB;

            var raid = RaidManager.Instance.CreateRaid(
                account.ContentInfo,
                account.ServerId, account.Nickname, account.Level, account.RepresentCharacterServerId,
                req.IsPractice, req.RaidUniqueId,
                currentRaidData, characterStatExcel
            );
            var battle = RaidManager.Instance.CreateBattle(
                account.ServerId, account.Nickname, account.RepresentCharacterServerId,
                req.RaidUniqueId, characterStatExcel.FirstOrDefault(x => x.CharacterId == raidStageExcel.FirstOrDefault(y => y.Id == req.RaidUniqueId).BossCharacterId.FirstOrDefault()).MaxHP100
            );

            return new RaidCreateBattleResponse()
            {
                RaidDB = raid,
                RaidBattleDB = battle,
                ServerTimeTicks = RaidManager.Instance.GetServerTime().Ticks
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
                ServerTimeTicks = RaidManager.Instance.GetServerTime().Ticks
            };
        }

        [ProtocolHandler(Protocol.Raid_EndBattle)]
        public ResponsePacket EndBattleHandler(RaidEndBattleRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            var raidStageTable = excelTableService.GetTable<RaidStageExcelTable>().UnPack().DataList;
            var characterStatExcelTable = excelTableService.GetTable<CharacterStatExcelTable>().UnPack().DataList;
            var currentRaidData = raidStageTable.FirstOrDefault(x => x.Id == account.ContentInfo.RaidDataInfo.CurrentRaidUniqueId);

            bool isCleared = RaidManager.Instance.SaveBattle(account.ServerId, req.Summary);

            if (!isCleared)
            {
                account.ContentInfo.RaidDataInfo.TimeBonus += req.Summary.EndFrame;
                return new RaidEndBattleResponse()
                {
                    ServerTimeTicks = RaidManager.Instance.GetServerTime().Ticks
                };
            }

            var bossResult = req.Summary.RaidSummary.RaidBossResults.FirstOrDefault();

            var totalTime = (req.Summary.EndFrame + account.ContentInfo.RaidDataInfo.TimeBonus)/30f;
            var timeScore = RaidService.CalculateTimeScore(totalTime, account.ContentInfo.RaidDataInfo.CurrentDifficulty);
            var hpPercentScorePoint = currentRaidData.HPPercentScore;
            var defaultClearPoint = currentRaidData.DefaultClearScore;

            var rankingPoint = timeScore + hpPercentScorePoint + defaultClearPoint;

            account.ContentInfo.RaidDataInfo.BestRankingPoint = rankingPoint > account.ContentInfo.RaidDataInfo.BestRankingPoint ? rankingPoint : account.ContentInfo.RaidDataInfo.BestRankingPoint;
            account.ContentInfo.RaidDataInfo.TotalRankingPoint += rankingPoint;
            account.ContentInfo.RaidDataInfo.TimeBonus = 0;
            context.Entry(account).Property(x => x.ContentInfo).IsModified = true; // force update
            context.SaveChanges();

            RaidManager.Instance.ClearPlayingBossDB();

            return new RaidEndBattleResponse()
            {
                RankingPoint = rankingPoint,
                BestRankingPoint = account.ContentInfo.RaidDataInfo.BestRankingPoint,
                ClearTimePoint = timeScore,
                HPPercentScorePoint = hpPercentScorePoint,
                DefaultClearPoint = defaultClearPoint,
                ServerTimeTicks = RaidManager.Instance.GetServerTime().Ticks
            };
        }

        [ProtocolHandler(Protocol.Raid_GiveUp)]
        public ResponsePacket GiveUpHandler(RaidGiveUpRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            var giveUpRaid = new RaidGiveUpDB()
            {
                Ranking = 1,
                RankingPoint = account.ContentInfo.RaidDataInfo.TotalRankingPoint,
                BestRankingPoint = account.ContentInfo.RaidDataInfo.BestRankingPoint
            };

            RaidManager.Instance.ClearPlayingBossDB();

            return new RaidGiveUpResponse()
            {
                RaidGiveUpDB = giveUpRaid,
                ServerTimeTicks = RaidManager.Instance.GetServerTime().Ticks
            };
        }

        [ProtocolHandler(Protocol.Raid_OpponentList)]
        public ResponsePacket OpponentListHandler(RaidOpponentListRequest req)
        {
            return new RaidOpponentListResponse()
            {
                OpponentUserDBs = [],
                ServerTimeTicks = RaidManager.Instance.GetServerTime().Ticks
            };
        }
    }
}
