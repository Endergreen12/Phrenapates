using Phrenapates.Services;
using Phrenapates.Managers;
using Plana.Database;
using Plana.FlatData;
using Plana.MX.GameLogic.DBModel;
using Plana.MX.NetworkProtocol;
using System.Text.Json;

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
            var bossData = characterStatExcel.FirstOrDefault(x => x.CharacterId == currentRaidData.BossCharacterId.FirstOrDefault());

            account.ContentInfo.EliminateRaidDataInfo.CurrentRaidUniqueId = req.RaidUniqueId;
            account.ContentInfo.EliminateRaidDataInfo.CurrentDifficulty = currentRaidData.Difficulty;

            context.Entry(account).Property(x => x.ContentInfo).IsModified = true; // force update
            context.SaveChanges();

            var raidLobbyInfoDB = EliminateRaidManager.Instance.EliminateRaidLobbyInfoDB;

            var bossHp = (raidLobbyInfoDB?.PlayingRaidDB?.RaidBossDBs != null && raidLobbyInfoDB.PlayingRaidDB.RaidBossDBs.Count > 0) ? raidLobbyInfoDB.PlayingRaidDB.RaidBossDBs.First().BossCurrentHP : bossData.MaxHP100;
            var raid = EliminateRaidManager.Instance.CreateRaid(account.ContentInfo, account.ServerId, account.Nickname, account.Level, account.RepresentCharacterServerId, req.IsPractice, req.RaidUniqueId, bossHp);
            var battle = EliminateRaidManager.Instance.CreateBattle(account.ServerId, account.Nickname, account.RepresentCharacterServerId, req.RaidUniqueId, bossHp);
            AssistCharacterDB assistCharacter = new();
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
                AssistCharacterDB = assistCharacter,
                ServerTimeTicks = EliminateRaidManager.Instance.GetServerTime().Ticks
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
                AssistCharacterDB = assistCharacter,
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
            //Console.WriteLine(JsonSerializer.Serialize(currentRaidData));
            var bossData = raidExcelTable.FirstOrDefault(x => x.CharacterId == currentRaidData.BossCharacterId.FirstOrDefault());

            var raidLobbyInfoDB = EliminateRaidManager.Instance.EliminateRaidLobbyInfoDB;
            var bossResult = req.Summary.RaidSummary.RaidBossResults.FirstOrDefault();
            var previousBattleHP =  (raidLobbyInfoDB?.PlayingRaidDB?.RaidBossDBs != null && raidLobbyInfoDB.PlayingRaidDB.RaidBossDBs.Count > 0) ? raidLobbyInfoDB.PlayingRaidDB.RaidBossDBs.First().BossCurrentHP : bossData.MaxHP100;
            var totalHpLeft = previousBattleHP - bossResult.RaidDamage.GivenDamage;

            if (totalHpLeft > 0)
            {
                List<long> characterId = req.Summary.Group01Summary.Heroes.Select(x => x.ServerId).ToList()
                    .Concat(req.Summary.Group01Summary.Supporters.Select(x => x.ServerId)).ToList();
                EliminateRaidManager.Instance.SaveBattle(account.ServerId, characterId, totalHpLeft, bossResult.RaidDamage.GivenGroggyPoint, bossResult.RaidDamage.Index);
                account.ContentInfo.EliminateRaidDataInfo.TimeBonus += req.Summary.EndFrame;
                return new EliminateRaidEndBattleResponse();
            }

            var totalTime = req.Summary.EndFrame/30f;
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
                RankingPoint = account.ContentInfo.RaidDataInfo.TotalRankingPoint,
                BestRankingPoint = account.ContentInfo.RaidDataInfo.BestRankingPoint
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
