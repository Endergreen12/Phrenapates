using Microsoft.EntityFrameworkCore;
using Plana.Database;
using Plana.FlatData;
using Plana.Migrations.SqlServerMigrations;
using Plana.NetworkProtocol;
using Plana.Parcel;
using Phrenapates.Services;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class Campaign : ProtocolHandlerBase
    {
        private ISessionKeyService sessionKeyService;
        private SCHALEContext context;
        private ExcelTableService excelTableService;

        public Campaign(IProtocolHandlerFactory protocolHandlerFactory, ISessionKeyService _sessionKeyService, SCHALEContext _context, ExcelTableService _excelTableService) : base(protocolHandlerFactory)
        {
            sessionKeyService = _sessionKeyService;
            context = _context;
            excelTableService = _excelTableService;
        }

        [ProtocolHandler(Protocol.Campaign_List)]
        public ResponsePacket ListHandler(CampaignListRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            return new CampaignListResponse()
            {
                CampaignChapterClearRewardHistoryDBs = new(),
                StageHistoryDBs = account.CampaignStageHistories.ToList(),
                StrategyObjecthistoryDBs = new(),
                DailyResetCountDB = new()
            };
        }

        [ProtocolHandler(Protocol.Campaign_EnterMainStage)]
        public ResponsePacket EnterMainStageHandler(CampaignEnterMainStageRequest req)
        {
            // TODO: Implement
            return new CampaignEnterMainStageResponse()
            {
                SaveDataDB = new CampaignMainStageSaveDB()
            };
        }

        [ProtocolHandler(Protocol.Campaign_EnterSubStage)]
        public ResponsePacket EnterSubStageHandler(CampaignEnterSubStageRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var parcelResultDb = ConsumeOrReturnCurrencies(account, req.StageUniqueId);

            return new CampaignEnterSubStageResponse()
            {
                ParcelResultDB = parcelResultDb,
                SaveDataDB = new()
                {
                    ContentType = ContentType.CampaignSubStage,
                }
            };
        }

        [ProtocolHandler(Protocol.Campaign_SubStageResult)]
        public ResponsePacket SubStageResultHandler(CampaignSubStageResultRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var currencies = account.Currencies.First();
            CampaignStageHistoryDB historyDb = new();
            ParcelResultDB parcelResultDb = new()
            {
                AccountCurrencyDB = currencies,
                DisplaySequence = new()
            };

            if (CheckIfCleared(req.Summary))
            {
                historyDb = CampaignService.CreateStageHistoryDB(req.AccountId, new() { UniqueId = req.Summary.StageId, ChapterUniqueId = GetChapterIdFromStageId(req.Summary.StageId) });
                CampaignService.CalcStrategySkipStarGoals(historyDb, req.Summary);

                if (account.CampaignStageHistories.Any(x => x.StageUniqueId == req.Summary.StageId))
                {
                    var existHistory = account.CampaignStageHistories.Where(x => x.StageUniqueId == req.Summary.StageId).First();
                    MergeExistHistoryWithNew(ref existHistory, historyDb);

                    historyDb = existHistory;
                }
                else
                {
                    account.CampaignStageHistories.Add(historyDb);
                }
            }
            else
            {
                // Return currencies
                parcelResultDb = ConsumeOrReturnCurrencies(account, req.Summary.StageId, true);
            }

            context.SaveChanges();

            return new CampaignSubStageResultResponse()
            {
                TacticRank = 0,
                CampaignStageHistoryDB = historyDb,
                LevelUpCharacterDBs = new(),
                ParcelResultDB = parcelResultDb,
                FirstClearReward = new(),
                ThreeStarReward = new()
            };
        }

        [ProtocolHandler(Protocol.Campaign_EnterTutorialStage)]
        public ResponsePacket EnterTutorialStageHandler(CampaignEnterTutorialStageRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var parcelResultDb = ConsumeOrReturnCurrencies(account, req.StageUniqueId);

            return new CampaignEnterTutorialStageResponse()
            {
                ParcelResultDB = parcelResultDb,
                SaveDataDB = new()
                {
                    ContentType = ContentType.CampaignTutorialStage
                }
            };
        }

        [ProtocolHandler(Protocol.Campaign_TutorialStageResult)]
        public ResponsePacket TutorialStageResultHandler(CampaignTutorialStageResultRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            CampaignStageHistoryDB historyDb = new();

            if(CheckIfCleared(req.Summary))
            {
                historyDb = CampaignService.CreateStageHistoryDB(req.AccountId, new() { UniqueId = req.Summary.StageId, ChapterUniqueId = GetChapterIdFromStageId(req.Summary.StageId) });
                historyDb.ClearTurnRecord = 1;

                if(!account.CampaignStageHistories.Any(x => x.StageUniqueId == req.Summary.StageId))
                {
                    account.CampaignStageHistories.Add(historyDb);
                }
            }

            context.SaveChanges();

            return new CampaignTutorialStageResultResponse()
            {
                CampaignStageHistoryDB = historyDb,
                ParcelResultDB = new(),
                ClearReward = new(),
                FirstClearReward = new(),
            };
        }

        [ProtocolHandler(Protocol.Campaign_EnterMainStageStrategySkip)]
        public ResponsePacket EnterMainStageStrategySkipHandler(CampaignEnterMainStageStrategySkipRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var parcelResultDb = ConsumeOrReturnCurrencies(account, req.StageUniqueId);

            return new CampaignEnterMainStageStrategySkipResponse()
            {
                ParcelResultDB = parcelResultDb
            };
        }

        [ProtocolHandler(Protocol.Campaign_MainStageStrategySkipResult)]
        public ResponsePacket MainStageStrategySkipResultHandler(CampaignMainStageStrategySkipResultRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var currencies = account.Currencies.First();
            
            CampaignStageHistoryDB historyDb = new();
            ParcelResultDB parcelResultDb = new()
            {
                AccountCurrencyDB = currencies,
                DisplaySequence = new()
            };

            if (CheckIfCleared(req.Summary))
            {
                historyDb = CampaignService.CreateStageHistoryDB(req.AccountId, new() { UniqueId = req.Summary.StageId, ChapterUniqueId = GetChapterIdFromStageId(req.Summary.StageId) });
                CampaignService.CalcStrategySkipStarGoals(historyDb, req.Summary);

                if (account.CampaignStageHistories.Any(x => x.StageUniqueId == req.Summary.StageId))
                {
                    var existHistory = account.CampaignStageHistories.Where(x => x.StageUniqueId == req.Summary.StageId).First();
                    MergeExistHistoryWithNew(ref existHistory, historyDb);

                    historyDb = existHistory;
                }
                else
                {
                    account.CampaignStageHistories.Add(historyDb);
                }
            } else
            {
                // Return currencies
                parcelResultDb = ConsumeOrReturnCurrencies(account, req.Summary.StageId, true);
            }

            context.SaveChanges();

            return new CampaignMainStageStrategySkipResultResponse()
            {
                TacticRank = 0,
                CampaignStageHistoryDB = historyDb,
                LevelUpCharacterDBs = new(),
                ParcelResultDB = parcelResultDb,
                FirstClearReward = new(),
                ThreeStarReward = new()
            };
        }

        public ParcelResultDB ConsumeOrReturnCurrencies(AccountDB account, long stageId, bool doReturn = false)
        {
            var currencies = account.Currencies.First();

            var campaignExcel = excelTableService.GetTable<CampaignStageExcelTable>().UnPack().DataList.Where(x => x.Id == stageId).First();

            var costId = campaignExcel.StageEnterCostId;
            var costAmount = campaignExcel.StageEnterCostAmount;
            currencies.CurrencyDict[(CurrencyTypes)costId] -= costAmount * (doReturn ? -1 : 1);
            currencies.UpdateTimeDict[(CurrencyTypes)costId] = DateTime.Now;

            context.Entry(currencies).State = EntityState.Modified;
            context.SaveChanges();

            return new() {
                AccountCurrencyDB = currencies,
                DisplaySequence = new()
                {
                    new()
                    {
                        Amount = costAmount,
                        Key = new()
                        {
                            Type = ParcelType.Currency,
                            Id = costId
                        }
                    }
                }
            };
        }

        public void MergeExistHistoryWithNew(ref CampaignStageHistoryDB existHistoryDb, CampaignStageHistoryDB newHistoryDb)
        {
            existHistoryDb.Star1Flag = existHistoryDb.Star1Flag ? true : newHistoryDb.Star1Flag;
            existHistoryDb.Star2Flag = existHistoryDb.Star2Flag ? true : newHistoryDb.Star2Flag;
            existHistoryDb.Star3Flag = existHistoryDb.Star3Flag ? true : newHistoryDb.Star3Flag;

            existHistoryDb.TodayPlayCount += 1;
            existHistoryDb.LastPlay = DateTime.Now;

            context.Entry(existHistoryDb).State = EntityState.Modified;
            context.SaveChanges();
        }

        public bool CheckIfCleared(BattleSummary summary)
        {
            return !summary.IsAbort && summary.EndType == Plana.MX.Logic.Battles.BattleEndType.Clear;
        }

        public long GetChapterIdFromStageId(long stageId)
        {
            var campaignChapterExcel = excelTableService.GetTable<CampaignChapterExcelTable>().UnPack().DataList
                .Where(x => x.NormalCampaignStageId.Contains(stageId) || x.HardCampaignStageId.Contains(stageId) || x.NormalExtraStageId.Contains(stageId) ||
                x.VeryHardCampaignStageId.Contains(stageId)).ToList().First();

            return campaignChapterExcel.Id;
        }
    }
}
