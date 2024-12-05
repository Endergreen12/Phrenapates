using Microsoft.EntityFrameworkCore;
using Phrenapates.Services;
using Plana.FlatData;
using Plana.Database;
using Plana.MX.GameLogic.DBModel;
using Plana.MX.GameLogic.Parcel;
using Plana.MX.NetworkProtocol;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class WeekDungeon : ProtocolHandlerBase
    {
        private readonly ISessionKeyService sessionKeyService;
        private readonly SCHALEContext context;
        private readonly ExcelTableService excelTableService;

        public WeekDungeon(IProtocolHandlerFactory protocolHandlerFactory, ISessionKeyService _sessionKeyService, SCHALEContext _context, ExcelTableService _excelTableService) : base(protocolHandlerFactory)
        {
            sessionKeyService = _sessionKeyService;
            context = _context;
            excelTableService = _excelTableService;
        }

        [ProtocolHandler(Protocol.WeekDungeon_List)]
        public ResponsePacket ListHandler(WeekDungeonListRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            return new WeekDungeonListResponse()
            {
                AdditionalStageIdList = new(),
                WeekDungeonStageHistoryDBList = account.WeekDungeonStageHistories.ToList(),
            };
        }

        [ProtocolHandler(Protocol.WeekDungeon_EnterBattle)]
        public ResponsePacket EnterBattleHandler(WeekDungeonEnterBattleRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            // Consume currencies
            var weekDungeonExcel = excelTableService.GetTable<WeekDungeonExcelTable>().UnPack().DataList.Where(x => x.StageId == req.StageUniqueId).ToList().First();
            var currencyDict = account.Currencies.First();

            List<long> costIdList = weekDungeonExcel.StageEnterCostId;
            List<int> costAmountList = weekDungeonExcel.StageEnterCostAmount;
            for (int i = 0; i < costIdList.Count; i++)
            {
                var targetCurrencyType = (CurrencyTypes)costIdList[i];
                currencyDict.CurrencyDict[targetCurrencyType] -= costAmountList[i];
                currencyDict.UpdateTimeDict[targetCurrencyType] = DateTime.Now;
            }

            context.Entry(account.Currencies.First()).State = EntityState.Modified;
            context.SaveChanges();

            return new WeekDungeonEnterBattleResponse()
            {
                ParcelResultDB = new()
                {
                    AccountCurrencyDB = account.Currencies.First(),
                }
            };
        }

        [ProtocolHandler(Protocol.WeekDungeon_BattleResult)]
        public ResponsePacket BattleResultHandler(WeekDungeonBattleResultRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var currencies = account.Currencies.First();
            var weekDungeonExcel = excelTableService.GetTable<WeekDungeonExcelTable>().UnPack().DataList.Where(x => x.StageId == req.StageUniqueId).ToList().First();
            WeekDungeonStageHistoryDB historyDb = new();
            ParcelResultDB parcelResultDb = new()
            {
                AccountCurrencyDB = currencies,
                DisplaySequence = new()
            };

            if (!req.Summary.IsAbort && req.Summary.EndType == Plana.MX.Logic.Battles.BattleEndType.Clear)
            {
                historyDb = WeekDungeonService.CreateWeekDungeonStageHistoryDB(req.AccountId, weekDungeonExcel);
                WeekDungeonService.CalcStarGoals(weekDungeonExcel, historyDb, req.Summary, req.Summary.EndType == Plana.MX.Logic.Battles.BattleEndType.Clear);

                if (account.WeekDungeonStageHistories.Any(x => x.StageUniqueId == req.StageUniqueId))
                {
                    var existStarGoalRecord = account.WeekDungeonStageHistories.Where(x => x.StageUniqueId == req.StageUniqueId).First().StarGoalRecord;
                    foreach (var goalPair in historyDb.StarGoalRecord)
                    {
                        if (existStarGoalRecord.ContainsKey(goalPair.Key))
                        {
                            if(goalPair.Value > existStarGoalRecord[goalPair.Key])
                            {
                                existStarGoalRecord[goalPair.Key] = goalPair.Value;
                            }
                        }
                        else
                        {
                            existStarGoalRecord.Add(goalPair.Key, goalPair.Value);
                        }
                    }

                    context.Entry(account.WeekDungeonStageHistories.First()).State = EntityState.Modified;

                    historyDb.StarGoalRecord = existStarGoalRecord;
                }
                else
                {
                    account.WeekDungeonStageHistories.Add(historyDb);
                }
            }
            else
            {
                // Return currencies
                List<long> costIdList = weekDungeonExcel.StageEnterCostId;
                List<int> costAmountList = weekDungeonExcel.StageEnterCostAmount;

                for (int i = 0; i < costIdList.Count; i++)
                {
                    var targetCurrencyType = (CurrencyTypes)costIdList[i];
                    currencies.CurrencyDict[targetCurrencyType] += costAmountList[i];
                    currencies.UpdateTimeDict[targetCurrencyType] = DateTime.Now;

                    parcelResultDb.DisplaySequence.Add(new()
                    {
                        Amount = costAmountList[i],
                        Key = new()
                        {
                            Type = ParcelType.Currency,
                            Id = costIdList[i]
                        }
                    });
                }

                context.Entry(currencies).State = EntityState.Modified;
            }

            context.SaveChanges();

            return new WeekDungeonBattleResultResponse()
            {
                WeekDungeonStageHistoryDB = historyDb,
                LevelUpCharacterDBs = new(),
                ParcelResultDB = parcelResultDb,
            };
        }
    }
}
