using Plana.Database;
using Plana.FlatData;
using Plana.NetworkProtocol;
using Phrenapates.Managers;
using Phrenapates.Services;

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
            var targetSeason = raidSeasonExcel.FirstOrDefault(x => x.SeasonId == account.RaidInfo.EliminateRaidDataInfo.SeasonId);

            return new EliminateRaidLobbyResponse()
            {
                SeasonType = RaidSeasonType.Open,
                RaidLobbyInfoDB = EliminateRaidManager.Instance.GetLobby(account.RaidInfo, targetSeason),
            };
        }

        [ProtocolHandler(Protocol.EliminateRaid_CreateBattle)]
        public ResponsePacket CreateBattleHandler(EliminateRaidCreateBattleRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            var raidSeasonExcel = excelTableService.GetTable<EliminateRaidStageExcelTable>().UnPack().DataList;
            var targetSeason = raidSeasonExcel.FirstOrDefault(x => x.Id == req.RaidUniqueId);

            account.RaidInfo.EliminateRaidDataInfo.CurrentRaidUniqueId = req.RaidUniqueId;
            account.RaidInfo.EliminateRaidDataInfo.CurrentDifficulty = targetSeason.Difficulty;

            context.Entry(account).Property(x => x.RaidInfo).IsModified = true; // force update
            context.SaveChanges();

            var raid = EliminateRaidManager.Instance.CreateRaid(account.RaidInfo, account.ServerId, account.Nickname, req.IsPractice, req.RaidUniqueId);
            var battle = EliminateRaidManager.Instance.CreateBattle(account.ServerId, account.Nickname, req.RaidUniqueId);
            AssistCharacterDB assistCharacter = new AssistCharacterDB();
            if (req.AssistUseInfo != null)
            {
                assistCharacter = new AssistCharacterDB()
                {
                    AccountId = req.AssistUseInfo.CharacterAccountId,
                    AssistCharacterServerId = req.AssistUseInfo.CharacterDBId,
                    SlotNumber = req.AssistUseInfo.SlotNumber,
                    EchelonType = req.AssistUseInfo.EchelonType,
                    AssistRelation = req.AssistUseInfo.AssistRelation,
                    IsMulligan = req.AssistUseInfo.IsMulligan,
                    IsTSAInteraction = req.AssistUseInfo.IsTSAInteraction,
                };
            }

            return new EliminateRaidCreateBattleResponse()
            {
                RaidDB = raid,
                RaidBattleDB = battle,
                AssistCharacterDB = assistCharacter
            };
        }

        [ProtocolHandler(Protocol.EliminateRaid_EnterBattle)] // clicked restart
        public ResponsePacket EliminateRaidEnterBattleHandler(EliminateRaidEnterBattleRequest req)
        {
            var raid = EliminateRaidManager.Instance.RaidDB;
            var battle = EliminateRaidManager.Instance.RaidBattleDB;
            AssistCharacterDB assistCharacter = new() {};
            if (req.AssistUseInfo != null)
            {
                assistCharacter = new AssistCharacterDB()
                {
                    AccountId = req.AssistUseInfo.CharacterAccountId,
                    AssistCharacterServerId = req.AssistUseInfo.CharacterDBId,
                    SlotNumber = req.AssistUseInfo.SlotNumber,
                    EchelonType = req.AssistUseInfo.EchelonType,
                    AssistRelation = req.AssistUseInfo.AssistRelation,
                    IsMulligan = req.AssistUseInfo.IsMulligan,
                    IsTSAInteraction = req.AssistUseInfo.IsTSAInteraction
                };
            }

            return new EliminateRaidEnterBattleResponse()
            {
                RaidDB = raid,
                RaidBattleDB = battle,
                AssistCharacterDB = assistCharacter
            };
        }

        [ProtocolHandler(Protocol.EliminateRaid_EndBattle)]
        public ResponsePacket EliminateRaidEndBattleHandler(EliminateRaidEndBattleRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            var raidStageTable = excelTableService.GetTable<EliminateRaidStageExcelTable>().UnPack().DataList;
            var currentRaidData = raidStageTable.FirstOrDefault(x => x.Id == account.RaidInfo.EliminateRaidDataInfo.CurrentRaidUniqueId);
            
            var totalTime = req.Summary.EndFrame/30f;
            var timeScore = RaidUtils.CalculateTimeScore(totalTime, account.RaidInfo.EliminateRaidDataInfo.CurrentDifficulty);
            var hpPercentScorePoint = currentRaidData.HPPercentScore;
            var defaultClearPoint = currentRaidData.DefaultClearScore;

            var rankingPoint = timeScore + hpPercentScorePoint + defaultClearPoint;

            account.RaidInfo.EliminateRaidDataInfo.BestRankingPoint = rankingPoint > account.RaidInfo.EliminateRaidDataInfo.BestRankingPoint ? rankingPoint : account.RaidInfo.EliminateRaidDataInfo.BestRankingPoint;
            account.RaidInfo.EliminateRaidDataInfo.TotalRankingPoint += rankingPoint;
            context.Entry(account).Property(x => x.RaidInfo).IsModified = true;
            context.SaveChanges();

            var battle = EliminateRaidManager.Instance.RaidBattleDB;
            var bossResult = req.Summary.RaidSummary.RaidBossResults.FirstOrDefault();

            battle.CurrentBossHP = bossResult.EndHpRateRawValue;
            battle.CurrentBossGroggy = bossResult.GroggyRateRawValue;
            battle.CurrentBossAIPhase = bossResult.AIPhase;
            battle.SubPartsHPs = bossResult.SubPartsHPs;

            return new EliminateRaidEndBattleResponse()
            {
                RankingPoint = rankingPoint,
                BestRankingPoint = account.RaidInfo.EliminateRaidDataInfo.BestRankingPoint,
                ClearTimePoint = timeScore,
                HPPercentScorePoint = hpPercentScorePoint,
                DefaultClearPoint = defaultClearPoint
            };
        }

        [ProtocolHandler(Protocol.EliminateRaid_OpponentList)]
        public ResponsePacket EliminateRaidOpponentListHandler(EliminateRaidOpponentListRequest req)
        {
            return new EliminateRaidOpponentListResponse()
            {
                OpponentUserDBs = []
            };
        }
    }
}
