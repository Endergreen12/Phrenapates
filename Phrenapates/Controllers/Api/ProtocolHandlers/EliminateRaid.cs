using Phrenapates.Services;
using Phrenapates.Managers;
using Plana.Database;
using Plana.FlatData;
using Plana.MX.GameLogic.DBModel;
using Plana.MX.NetworkProtocol;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class EliminateRaid : ProtocolHandlerBase
    {
        private readonly ISessionKeyService sessionKeyService;
        private readonly SCHALEContext context;
        private readonly ExcelTableService excelTableService;
        public EliminateRaid(IProtocolHandlerFactory protocolHandlerFactory, ISessionKeyService _sessionKeyService, SCHALEContext _context, ExcelTableService _excelTableService) : base(protocolHandlerFactory)
        {
            sessionKeyService = _sessionKeyService;
            context = _context;
            excelTableService = _excelTableService;
        }

        [ProtocolHandler(Protocol.EliminateRaid_Login)]
        public ResponsePacket EliminateRaidLoginHandler(EliminateRaidLoginRequest req)
        {
            return new EliminateRaidLoginResponse();
        }

        [ProtocolHandler(Protocol.EliminateRaid_Lobby)]
        public ResponsePacket EliminateRaidLobbyHandler(EliminateRaidLobbyRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            var raidSeasonExcel = excelTableService.GetTable<EliminateRaidSeasonManageExcelTable>().UnPack().DataList;
            var targetSeason = raidSeasonExcel.FirstOrDefault(x => x.SeasonId == account.ContentInfo.EliminateRaidDataInfo.SeasonId);
            var serverTimeTicks = EliminateRaidManager.Instance.CreateServerTime(targetSeason, account.ContentInfo).Ticks;

            return new EliminateRaidLobbyResponse()
            {
                SeasonType = RaidSeasonType.Open,
                RaidLobbyInfoDB = EliminateRaidManager.Instance.GetLobby(account.ContentInfo, targetSeason),
                ServerTimeTicks = serverTimeTicks
            };
        }

        [ProtocolHandler(Protocol.EliminateRaid_CreateBattle)]
        public ResponsePacket CreateBattleHandler(EliminateRaidCreateBattleRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            var raidStageExcel = excelTableService.GetTable<EliminateRaidStageExcelTable>().UnPack().DataList;
            var characterStatExcel = excelTableService.GetTable<CharacterStatExcelTable>().UnPack().DataList;
            var currentRaidData = raidStageExcel.FirstOrDefault(x => x.Id == req.RaidUniqueId);

            account.ContentInfo.EliminateRaidDataInfo.CurrentRaidUniqueId = req.RaidUniqueId;
            account.ContentInfo.EliminateRaidDataInfo.CurrentDifficulty = currentRaidData.Difficulty;

            context.Entry(account).Property(x => x.ContentInfo).IsModified = true; // force update
            context.SaveChanges();

            var raidLobbyInfoDB = EliminateRaidManager.Instance.EliminateRaidLobbyInfoDB;
            
            var raid = EliminateRaidManager.Instance.CreateRaid(
                account.ContentInfo,
                account.ServerId, account.Nickname, account.Level, account.RepresentCharacterServerId,
                req.IsPractice, req.RaidUniqueId,
                currentRaidData, characterStatExcel
            );
            var battle = EliminateRaidManager.Instance.CreateBattle(
                account.ServerId, account.Nickname, account.RepresentCharacterServerId,
                req.RaidUniqueId, characterStatExcel.FirstOrDefault(x => x.CharacterId == raidStageExcel.FirstOrDefault(y => y.Id == req.RaidUniqueId).BossCharacterId.FirstOrDefault()).MaxHP100
            );
            return new EliminateRaidCreateBattleResponse()
            {
                RaidDB = raid,
                RaidBattleDB = battle,
                ServerTimeTicks = EliminateRaidManager.Instance.GetServerTime().Ticks
            };
        }

        [ProtocolHandler(Protocol.EliminateRaid_EnterBattle)] // clicked restart
        public ResponsePacket EliminateRaidEnterBattleHandler(EliminateRaidEnterBattleRequest req)
        {
            var raid = EliminateRaidManager.Instance.RaidDB;
            var battle = EliminateRaidManager.Instance.RaidBattleDB;

            return new EliminateRaidEnterBattleResponse()
            {
                RaidDB = raid,
                RaidBattleDB = battle,
                ServerTimeTicks = EliminateRaidManager.Instance.GetServerTime().Ticks
            };
        }

        [ProtocolHandler(Protocol.EliminateRaid_EndBattle)]
        public ResponsePacket EliminateRaidEndBattleHandler(EliminateRaidEndBattleRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            var raidStageTable = excelTableService.GetTable<EliminateRaidStageExcelTable>().UnPack().DataList;
            var raidExcelTable = excelTableService.GetTable<CharacterStatExcelTable>().UnPack().DataList;
            var currentRaidData = raidStageTable.FirstOrDefault(x => x.Id == account.ContentInfo.EliminateRaidDataInfo.CurrentRaidUniqueId);

            bool isCleared = EliminateRaidManager.Instance.SaveBattle(account.ServerId, req.Summary);

            if (!isCleared)
            {
                account.ContentInfo.EliminateRaidDataInfo.TimeBonus += req.Summary.EndFrame;
                return new EliminateRaidEndBattleResponse()
                {
                    ServerTimeTicks = EliminateRaidManager.Instance.GetServerTime().Ticks
                };
            }

            var bossResult = req.Summary.RaidSummary.RaidBossResults.FirstOrDefault();

            var totalTime = (req.Summary.EndFrame + account.ContentInfo.EliminateRaidDataInfo.TimeBonus)/30f;
            var timeScore = RaidService.CalculateTimeScore(totalTime, account.ContentInfo.EliminateRaidDataInfo.CurrentDifficulty);
            var hpPercentScorePoint = currentRaidData.HPPercentScore;
            var defaultClearPoint = currentRaidData.DefaultClearScore;

            var rankingPoint = timeScore + hpPercentScorePoint + defaultClearPoint;

            account.ContentInfo.EliminateRaidDataInfo.BestRankingPoint = rankingPoint > account.ContentInfo.EliminateRaidDataInfo.BestRankingPoint ? rankingPoint : account.ContentInfo.EliminateRaidDataInfo.BestRankingPoint;
            account.ContentInfo.EliminateRaidDataInfo.TotalRankingPoint += rankingPoint;
            account.ContentInfo.EliminateRaidDataInfo.TimeBonus = 0;
            context.Entry(account).Property(x => x.ContentInfo).IsModified = true; // force update
            context.SaveChanges();

            var battle = EliminateRaidManager.Instance.RaidBattleDB;

            battle.CurrentBossHP = bossResult.EndHpRateRawValue;
            battle.CurrentBossGroggy = bossResult.GroggyRateRawValue;
            battle.CurrentBossAIPhase = bossResult.AIPhase;
            battle.SubPartsHPs = bossResult.SubPartsHPs;

            EliminateRaidManager.Instance.ClearPlayingBossDB();

            return new EliminateRaidEndBattleResponse()
            {
                RankingPoint = rankingPoint,
                BestRankingPoint = account.ContentInfo.EliminateRaidDataInfo.BestRankingPoint,
                ClearTimePoint = timeScore,
                HPPercentScorePoint = hpPercentScorePoint,
                DefaultClearPoint = defaultClearPoint,
                ServerTimeTicks = EliminateRaidManager.Instance.GetServerTime().Ticks
            };
        }

        [ProtocolHandler(Protocol.EliminateRaid_OpponentList)]
        public ResponsePacket EliminateRaidOpponentListHandler(EliminateRaidOpponentListRequest req)
        {
            return new EliminateRaidOpponentListResponse()
            {
                OpponentUserDBs = [],
                ServerTimeTicks = EliminateRaidManager.Instance.GetServerTime().Ticks

            };
        }

        [ProtocolHandler(Protocol.EliminateRaid_GiveUp)]
        public ResponsePacket GiveUpHandler(EliminateRaidGiveUpRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            var giveUpRaid = new RaidGiveUpDB()
            {
                Ranking = 1,
                RankingPoint = account.ContentInfo.EliminateRaidDataInfo.TotalRankingPoint,
                BestRankingPoint = account.ContentInfo.EliminateRaidDataInfo.BestRankingPoint
            };

            EliminateRaidManager.Instance.ClearPlayingBossDB();

            return new EliminateRaidGiveUpResponse()
            {
                RaidGiveUpDB = giveUpRaid,
                ServerTimeTicks = EliminateRaidManager.Instance.GetServerTime().Ticks
            };
        }
    }
}
