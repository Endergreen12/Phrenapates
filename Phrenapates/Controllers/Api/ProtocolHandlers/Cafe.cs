using Plana.Database;
using Plana.FlatData;
using Plana.NetworkProtocol;
using Phrenapates.Services;
using Phrenapates.Utils;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class Cafe : ProtocolHandlerBase
    {
        private readonly ISessionKeyService sessionKeyService;
        private readonly SCHALEContext context;
        private readonly ExcelTableService excelTableService;

        public Cafe(IProtocolHandlerFactory protocolHandlerFactory, ISessionKeyService _sessionKeyService, SCHALEContext _context, ExcelTableService _excelTableService) : base(protocolHandlerFactory)
        {
            sessionKeyService = _sessionKeyService;
            context = _context;
            excelTableService = _excelTableService;
        }

        [ProtocolHandler(Protocol.Cafe_Get)]
        public ResponsePacket GetHandler(CafeGetInfoRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var cafeDbAll = account.Cafes.ToList();
            var cafeDbOne = cafeDbAll.FirstOrDefault(x => x.CafeId == 1);
            var defaultFurnitureExcel = excelTableService.GetTable<DefaultFurnitureExcelTable>().UnPack().DataList;

            // Cafe Handler stuff
            cafeDbOne.LastUpdate = DateTime.Now;

            // Data and stuff
            var furnitures = account.Furnitures
            .Select(x => {
                return new FurnitureDB()
                {
                    CafeDBId = x.CafeDBId,
                    UniqueId = x.UniqueId,
                    Location = x.Location,
                    PositionX = x.PositionX,
                    PositionY = x.PositionY,
                    Rotation = x.Rotation,
                    ItemDeploySequence = x.ItemDeploySequence,
                    StackCount = x.StackCount,
                    ServerId = x.ServerId
                };
            }).ToList();

            cafeDbOne.FurnitureDBs = furnitures
            .Where(x => x.CafeDBId == cafeDbOne.CafeDBId && defaultFurnitureExcel.Select(y => y.Id).ToList().Contains(x.UniqueId))
            .Select(x => {
                return new FurnitureDB()
                {
                    CafeDBId = x.CafeDBId,
                    UniqueId = x.UniqueId,
                    Location = x.Location,
                    PositionX = x.PositionX,
                    PositionY = x.PositionY,
                    Rotation = x.Rotation,
                    ItemDeploySequence = x.ItemDeploySequence,
                    StackCount = x.StackCount,
                    ServerId = x.ServerId
                };
            }).ToList();

            cafeDbOne.CafeVisitCharacterDBs.Clear();
            var count = 0;
            foreach (var character in RandomList.GetRandomList(account.Characters.ToList(), account.Characters.Count < 5 ? account.Characters.Count : new Random().Next(3, 6)))
            {
                cafeDbOne.CafeVisitCharacterDBs.Add(count, 
                    new CafeCharacterDB()
                    {
                        IsSummon = false,
                        LastInteractTime = DateTime.Now,
                        UniqueId = character.UniqueId,
                        ServerId = character.ServerId,
                    }
                );
                count++;
            };
            context.SaveChanges();

            return new CafeGetInfoResponse()
            {
                CafeDB = cafeDbOne,
                CafeDBs = cafeDbAll,
                FurnitureDBs = furnitures
            };
        }

        [ProtocolHandler(Protocol.Cafe_Ack)]
        public ResponsePacket AckHandler(CafeAckRequest req)
        {
            // Unable to make the client send this protocol.
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var cafeDb = account.Cafes.FirstOrDefault();
            
            // Cafe Handler stuff
            cafeDb.LastUpdate = DateTime.Now;

            // TODO: Implement unowned characters
            cafeDb.CafeVisitCharacterDBs.Clear();
            var count = 0;
            foreach (var character in RandomList.GetRandomList(account.Characters.ToList(), account.Characters.Count < 5 ? account.Characters.Count : new Random().Next(3, 6)))
            {
                cafeDb.CafeVisitCharacterDBs.Add(count, 
                    new CafeCharacterDB()
                    {
                        IsSummon = false,
                        LastInteractTime = DateTime.Now,
                        UniqueId = character.UniqueId,
                        ServerId = character.ServerId,
                    }
                );
                count++;
            };
            context.SaveChanges();

            return new CafeAckResponse()
            {
                CafeDB = cafeDb
            };
        }

        [ProtocolHandler(Protocol.Cafe_Open)]
        public ResponsePacket OpenHandler(CafeOpenRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var cafeDb = account.Cafes.FirstOrDefault(x => x.AccountId == req.AccountId);

            context.SaveChanges();

            return new CafeOpenResponse()
            {
                OpenedCafeDB = cafeDb,
                FurnitureDBs = account.Furnitures.ToList()
            };
        }

        [ProtocolHandler(Protocol.Cafe_RankUp)]
        public ResponsePacket RankUpHandler(CafeRankUpRequest req)
        {
            return new CafeRankUpResponse();
        }

        [ProtocolHandler(Protocol.Cafe_Deploy)]
        public ResponsePacket DeployHandler(CafeDeployFurnitureRequest req)
        {
            return new CafeDeployFurnitureResponse()
            {

            };
        }

        [ProtocolHandler(Protocol.Cafe_Relocate)]
        public ResponsePacket RelocateHandler(CafeRelocateFurnitureRequest req)
        {
            return new CafeRelocateFurnitureResponse();
        }

        [ProtocolHandler(Protocol.Cafe_Remove)]
        public ResponsePacket RemoveHanlder(CafeRemoveFurnitureRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var cafeDb = account.Cafes.FirstOrDefault(x => x.AccountId == req.AccountId);

            var removedFurniture = new List<FurnitureDB>();

            foreach (var furniture in req.FurnitureServerIds)
            {
                if(account.Furnitures.FirstOrDefault(x => x.CafeDBId == furniture) == null) continue;
                removedFurniture.Add(account.Furnitures.FirstOrDefault(x => x.CafeDBId == furniture));
            }
            context.SaveChanges();

            return new CafeRemoveFurnitureResponse()
            {
                CafeDB = cafeDb,
                FurnitureDBs = removedFurniture
            };
        }

        [ProtocolHandler(Protocol.Cafe_RemoveAll)]
        public ResponsePacket CafeRemoveAllHandler(CafeRemoveAllFurnitureRequest req)
        {
            return new CafeRemoveAllFurnitureResponse();
        }

        [ProtocolHandler(Protocol.Cafe_SummonCharacter)]
        public ResponsePacket SummonCharacterHandler(CafeSummonCharacterRequest req)
        {
            return new CafeSummonCharacterResponse();
        }

        [ProtocolHandler(Protocol.Cafe_Interact)]
        public ResponsePacket InteractHandler(CafeInteractWithCharacterRequest req)
        {
            return new CafeInteractWithCharacterResponse();
        }

        [ProtocolHandler(Protocol.Cafe_GiveGift)]
        public ResponsePacket CafeGiveGiftHandler(CafeGiveGiftRequest req)
        {
            return new CafeGiveGiftResponse();
        }

        [ProtocolHandler(Protocol.Cafe_ReceiveCurrency)]
        public ResponsePacket ReceiveCurrencyHandler(CafeReceiveCurrencyRequest req)
        {
            return new CafeReceiveCurrencyResponse();
        }

        [ProtocolHandler(Protocol.Cafe_ListPreset)]
        public ResponsePacket ListPresetHandler(CafeListPresetRequest req)
        {
            return new CafeListPresetResponse();
        }

        [ProtocolHandler(Protocol.Cafe_ApplyPreset)]
        public ResponsePacket ApplyPresetHandler(CafeApplyPresetRequest req)
        {
            return new CafeApplyPresetResponse();
        }
        
        [ProtocolHandler(Protocol.Cafe_ApplyTemplate)]
        public ResponsePacket ApplyTemplateHandler(CafeApplyTemplateRequest req)
        {
            return new CafeApplyTemplateResponse();
        }

        [ProtocolHandler(Protocol.Cafe_RenamePreset)]
        public ResponsePacket RenamePresetHandler(CafeRenamePresetRequest req)
        {
            return new CafeRenamePresetResponse();
        }

        [ProtocolHandler(Protocol.Cafe_ClearPreset)]
        public ResponsePacket ClearPresetHandler(CafeClearPresetRequest req)
        {
            return new CafeClearPresetResponse();
        }

        [ProtocolHandler(Protocol.Cafe_TrophyHistory)]
        public ResponsePacket TrophyHistoryHandler(CafeTrophyHistoryRequest req)
        {
            return new CafeTrophyHistoryResponse();
        }
        
        [ProtocolHandler(Protocol.Cafe_UpdatePresetFurniture)]
        public ResponsePacket UpdatePresetFurnitureHandler(CafeUpdatePresetFurnitureRequest req)
        {
            return new CafeUpdatePresetFurnitureResponse();
        }
        
        public static CafeDB CreateCafe(long accountId)
        {
            return new()
            {
                CafeDBId = 0,
                CafeId = 1,
                AccountId = accountId,
                CafeRank = 10,
                LastUpdate = DateTime.Now,
                LastSummonDate = DateTimeOffset.Parse("2023-01-01T00:00:00Z").UtcDateTime,
                CafeVisitCharacterDBs = [],
                FurnitureDBs = [],
                ProductionAppliedTime = DateTime.Now,
                ProductionDB = new()
                {
                    CafeDBId = 0,
                    AppliedDate = DateTime.Now,
                    ComfortValue = 5500,
                    ProductionParcelInfos =
                    [
                        new CafeProductionParcelInfo()
                        {
                            Key = {
                                Type = ParcelType.Currency,
                                Id = (long)CurrencyTypes.Gold,
                            },
                            Amount = 9999999
                        },
                        new CafeProductionParcelInfo()
                        {
                            Key = {
                                Type = ParcelType.Currency,
                                Id = (long)CurrencyTypes.ActionPoint
                            },
                            Amount = 500
                        },
                    ]
                },
            };
        }

        public static CafeDB CreateSecondCafe(long accountId)
        {
            return new()
            {
                CafeDBId = 0,
                CafeId = 2,
                AccountId = accountId,
                CafeRank = 10,
                LastUpdate = DateTime.Now,
                LastSummonDate = DateTimeOffset.Parse("2023-01-01T00:00:00Z").UtcDateTime,
                CafeVisitCharacterDBs = [],
                FurnitureDBs = [],
                ProductionAppliedTime = DateTime.Now,
                ProductionDB = new()
                {
                    CafeDBId = 0,
                    AppliedDate = DateTime.Now,
                    ComfortValue = 5500,
                    ProductionParcelInfos =
                    [
                        new CafeProductionParcelInfo()
                        {
                            Key = {
                                Type = ParcelType.Currency,
                                Id = (long)CurrencyTypes.Gold,
                            },
                            Amount = 9999999
                        }
                    ]
                },
            };
        }
    }
}
