using Microsoft.EntityFrameworkCore;
using Phrenapates.Services;
using Plana.FlatData;
using Plana.Database;
using Plana.MX.GameLogic.DBModel;
using Plana.MX.GameLogic.Parcel;
using Plana.MX.NetworkProtocol;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class SchoolDungeon : ProtocolHandlerBase
    {
        private readonly ISessionKeyService sessionKeyService;
        private readonly SCHALEContext context;
        private readonly ExcelTableService excelTableService;

        public SchoolDungeon(IProtocolHandlerFactory protocolHandlerFactory, ISessionKeyService _sessionKeyService, SCHALEContext _context, ExcelTableService _excelTableService) : base(protocolHandlerFactory)
        {
            sessionKeyService = _sessionKeyService;
            context = _context;
            excelTableService = _excelTableService;
        }

        [ProtocolHandler(Protocol.SchoolDungeon_List)]
        public ResponsePacket ListHandler(SchoolDungeonListRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            return new SchoolDungeonListResponse()
            {
                SchoolDungeonStageHistoryDBList = account.SchoolDungeonStageHistories.ToList(),
            };
        }

        [ProtocolHandler(Protocol.SchoolDungeon_EnterBattle)]
        public ResponsePacket EnterBattleHandler(SchoolDungeonEnterBattleRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            // Consume currencies
            var schoolDungeonExcel = excelTableService.GetTable<SchoolDungeonStageExcelTable>().UnPack().DataList.Where(x => x.StageId == req.StageUniqueId).ToList().First();
            var currencyDict = account.Currencies.First();

            List<long> costIdList = schoolDungeonExcel.StageEnterCostId;
            List<long> costAmountList = schoolDungeonExcel.StageEnterCostAmount;
            for (int i = 0; i < costIdList.Count; i++)
            {
                var targetCurrencyType = (CurrencyTypes)costIdList[i];
                currencyDict.CurrencyDict[targetCurrencyType] -= costAmountList[i];
                currencyDict.UpdateTimeDict[targetCurrencyType] = DateTime.Now;
            }

            context.Entry(currencyDict).State = EntityState.Modified;
            context.SaveChanges();

            return new SchoolDungeonEnterBattleResponse()
            {
                ParcelResultDB = new()
                {
                    AccountCurrencyDB = currencyDict,
                }
            };
        }

        [ProtocolHandler(Protocol.SchoolDungeon_BattleResult)]
        public ResponsePacket BattleResultHandler(SchoolDungeonBattleResultRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var currencies = account.Currencies.First();
            var schoolDungeonExcel = excelTableService.GetTable<SchoolDungeonStageExcelTable>().UnPack().DataList.Where(x => x.StageId == req.StageUniqueId).ToList().First();
            SchoolDungeonStageHistoryDB historyDb = new();
            ParcelResultDB parcelResultDb = new()
            {
                AccountCurrencyDB = currencies,
                DisplaySequence = new()
            };

            if (!req.Summary.IsAbort && req.Summary.EndType == Plana.MX.Logic.Battles.BattleEndType.Clear)
            {
                historyDb = SchoolDungeonService.CreateSchoolDungeonStageHistoryDB(req.AccountId, schoolDungeonExcel);
                SchoolDungeonService.CalcStarGoals(schoolDungeonExcel, historyDb, req.Summary);

                if (account.SchoolDungeonStageHistories.Any(x => x.StageUniqueId == req.StageUniqueId))
                {
                    var existStageHistory = account.SchoolDungeonStageHistories.Where(x => x.StageUniqueId == req.StageUniqueId).First();
                    for(var i = 0; i < existStageHistory.StarFlags.Length; i++)
                    {
                        existStageHistory.StarFlags[i] = existStageHistory.StarFlags[i] ? true : historyDb.StarFlags[i];
                    }

                    context.Entry(account.WeekDungeonStageHistories.First()).State = EntityState.Modified;
                }
                else
                {
                    account.SchoolDungeonStageHistories.Add(historyDb);
                }
            }
            else
            {
                // Return currencies
                List<long> costIdList = schoolDungeonExcel.StageEnterCostId;
                List<long> costAmountList = schoolDungeonExcel.StageEnterCostAmount;

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

            return new SchoolDungeonBattleResultResponse()
            {
                SchoolDungeonStageHistoryDB = historyDb,
                LevelUpCharacterDBs = new(),
                ParcelResultDB = parcelResultDb
            };
        }
    }
}
