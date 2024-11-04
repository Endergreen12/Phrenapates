using Plana.Database;
using Plana.FlatData;
using Plana.NetworkProtocol;
using Plana.Parcel;
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

            var furnitures = account.Furnitures.Select(x => {
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

            cafeDbOne.LastUpdate = DateTime.Now;
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
            
            cafeDb.LastUpdate = DateTime.Now;
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
            var cafeDbAll = account.Cafes.ToList();
            var cafeDb = cafeDbAll.FirstOrDefault(x => x.CafeId == req.CafeId);
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
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var cafeDb = account.Cafes.FirstOrDefault(x => x.CafeDBId == req.CafeDBId);
            var defaultFurnitureExcel = excelTableService.GetTable<DefaultFurnitureExcelTable>().UnPack().DataList.GetRange(0, 3);
            var furnitureExcel = excelTableService.GetTable<FurnitureExcelTable>().UnPack().DataList;
            var furnitureTempExcel = excelTableService.GetTable<FurnitureTemplateElementExcelTable>().UnPack().DataList;

            var inventoryFurniture = account.Furnitures.FirstOrDefault(x =>
                x.Location == FurnitureLocation.Inventory &&
                req.FurnitureDB.UniqueId == x.UniqueId &&
                x.ItemDeploySequence == 0
            );

            var placedFurniture = new FurnitureDB()
            {
                CafeDBId = req.CafeDBId,
                UniqueId = req.FurnitureDB.UniqueId,
                Location = req.FurnitureDB.Location,
                PositionX = req.FurnitureDB.PositionX,
                PositionY = req.FurnitureDB.PositionY,
                Rotation = req.FurnitureDB.Rotation,
                StackCount = 1,
            };
            account.Furnitures.Add(placedFurniture);
            context.SaveChanges();
            
            placedFurniture = account.Furnitures.FirstOrDefault(x =>
                x.PositionX == req.FurnitureDB.PositionX &&
                x.PositionY == req.FurnitureDB.PositionY &&
                x.CafeDBId == req.CafeDBId &&
                x.UniqueId == req.FurnitureDB.UniqueId &&
                x.ItemDeploySequence == 0 &&
                x.StackCount == 1);
            placedFurniture.ItemDeploySequence = placedFurniture.ServerId;

            if(furnitureTempExcel.Select(x => x.FurnitureId).Contains(placedFurniture.UniqueId))
            {
                var furnitureTemp = account.Furnitures.FirstOrDefault(x => 
                    x.Location == req.FurnitureDB.Location &&
                    x.CafeDBId == req.CafeDBId    
                );
                if (defaultFurnitureExcel.Select(x => x.Id).Contains(placedFurniture.UniqueId)) 
                {
                    furnitureTemp.Location = FurnitureLocation.Inventory;
                    furnitureTemp.ItemDeploySequence = 0;
                }
                else account.Furnitures.Remove(furnitureTemp); 
            }
            else if(defaultFurnitureExcel.Select(x => x.Id).Contains(placedFurniture.UniqueId))
            {
                var furniturePerm = account.Furnitures.FirstOrDefault(x => 
                    x.UniqueId == placedFurniture.UniqueId &&
                    x.CafeDBId == req.CafeDBId
                );
                furniturePerm.Location = defaultFurnitureExcel.FirstOrDefault(x => x.Id == placedFurniture.UniqueId).Location;
                furniturePerm.ItemDeploySequence = placedFurniture.ServerId;
            }
            context.SaveChanges();

            placedFurniture = new FurnitureDB
            {
                CafeDBId = placedFurniture.CafeDBId,
                UniqueId = placedFurniture.UniqueId,
                Location = placedFurniture.Location,
                PositionX = placedFurniture.PositionX,
                PositionY = placedFurniture.PositionY,
                Rotation = placedFurniture.Rotation,
                ItemDeploySequence = placedFurniture.ItemDeploySequence,
                StackCount = placedFurniture.StackCount,
                ServerId = placedFurniture.ServerId
            };

            cafeDb.LastUpdate = DateTime.Now;
            cafeDb.FurnitureDBs = account.Furnitures
            .Where(x => 
                x.CafeDBId == req.CafeDBId &&
                x.ItemDeploySequence != 0)
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

            var comfortValue = 0;
            foreach (var furniture in cafeDb.FurnitureDBs)
            {
                comfortValue += (int)furnitureExcel.FirstOrDefault(x => x.Id == furniture.UniqueId).ComfortBonus;
            }
            cafeDb.ProductionDB.ComfortValue = comfortValue;

            context.SaveChanges();

            return new CafeDeployFurnitureResponse()
            {
                CafeDB = cafeDb,
                NewFurnitureServerId = placedFurniture.ServerId,
                ChangedFurnitureDBs = [inventoryFurniture, placedFurniture]
            };
        }

        [ProtocolHandler(Protocol.Cafe_Relocate)]
        public ResponsePacket RelocateHandler(CafeRelocateFurnitureRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var cafeDb = account.Cafes.FirstOrDefault(x => x.CafeDBId == req.CafeDBId);
            var furniture = account.Furnitures.FirstOrDefault(x => x.ItemDeploySequence == req.FurnitureDB.ServerId);
            cafeDb.FurnitureDBs.Remove(furniture);
            furniture.PositionX = req.FurnitureDB.PositionX;
            furniture.PositionY = req.FurnitureDB.PositionY;
            furniture.Rotation = req.FurnitureDB.Rotation;
            context.SaveChanges();
            var relocatedFurniture = new FurnitureDB()
            {
                CafeDBId = furniture.CafeDBId,
                UniqueId = furniture.UniqueId,
                Location = furniture.Location,
                PositionX = furniture.PositionX,
                PositionY = furniture.PositionY,
                Rotation = furniture.Rotation,
                StackCount = furniture.StackCount,
                ItemDeploySequence = furniture.ItemDeploySequence
            };
            
            cafeDb.FurnitureDBs = account.Furnitures
            .Where(x => 
                x.CafeDBId == req.CafeDBId &&
                x.ItemDeploySequence != 0)
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
            context.SaveChanges();

            return new CafeRelocateFurnitureResponse()
            {
                CafeDB = cafeDb,
                RelocatedFurnitureDB = relocatedFurniture
            };
        }

        [ProtocolHandler(Protocol.Cafe_Remove)]
        public ResponsePacket RemoveHanlder(CafeRemoveFurnitureRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var cafeDb = account.Cafes.FirstOrDefault(x => x.CafeDBId == req.CafeDBId);
            var furnitureExcel = excelTableService.GetTable<FurnitureExcelTable>().UnPack().DataList;

            var removedFurniture = account.Furnitures.FirstOrDefault(x =>
                x.CafeDBId == req.CafeDBId &&
                x.ServerId == req.FurnitureServerIds[0]
            );

            account.Furnitures.Remove(removedFurniture);
            context.SaveChanges();

            cafeDb.LastUpdate = DateTime.Now;
            cafeDb.FurnitureDBs = account.Furnitures
            .Where(x => 
                x.CafeDBId == req.CafeDBId &&
                x.ItemDeploySequence != 0)
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

            var comfortValue = 0;
            foreach (var furniture in cafeDb.FurnitureDBs)
            {
                comfortValue += (int)furnitureExcel.FirstOrDefault(x => x.Id == furniture.UniqueId).ComfortBonus;
            }
            cafeDb.ProductionDB.ComfortValue = comfortValue;

            context.SaveChanges();

            return new CafeRemoveFurnitureResponse()
            {
                CafeDB = cafeDb,
                FurnitureDBs = [removedFurniture]
            };
        }

        [ProtocolHandler(Protocol.Cafe_RemoveAll)]
        public ResponsePacket CafeRemoveAllHandler(CafeRemoveAllFurnitureRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var cafeDb = account.Cafes.FirstOrDefault(x => x.CafeDBId == req.CafeDBId);
            var defaultFurnitureExcel = excelTableService.GetTable<DefaultFurnitureExcelTable>().UnPack().DataList.GetRange(0, 3);
            var furnitureExcel = excelTableService.GetTable<FurnitureExcelTable>().UnPack().DataList;

            var removedFurniture = account.Furnitures.Where(x =>
                x.CafeDBId == req.CafeDBId &&
                x.Location != FurnitureLocation.Inventory &&
                x.ItemDeploySequence != 0 &&
                !defaultFurnitureExcel.Any(y => y.Id == x.UniqueId)
            ).ToList();

            context.Furnitures.RemoveRange(removedFurniture);
            context.SaveChanges();

            cafeDb.LastUpdate = DateTime.Now;
            cafeDb.FurnitureDBs = account.Furnitures
            .Where(x => 
                x.CafeDBId == req.CafeDBId &&
                x.ItemDeploySequence != 0)
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

            var comfortValue = 0;
            foreach (var furniture in cafeDb.FurnitureDBs)
            {
                comfortValue += (int)furnitureExcel.FirstOrDefault(x => x.Id == furniture.UniqueId).ComfortBonus;
            }
            cafeDb.ProductionDB.ComfortValue = comfortValue;
            
            context.SaveChanges();

            return new CafeRemoveAllFurnitureResponse()
            {
                CafeDB = cafeDb,
                FurnitureDBs = removedFurniture
            };
        }

        [ProtocolHandler(Protocol.Cafe_SummonCharacter)]
        public ResponsePacket SummonCharacterHandler(CafeSummonCharacterRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var cafeDbAll = account.Cafes.ToList();
            var cafeDb = cafeDbAll.FirstOrDefault(x => x.CafeDBId == req.CafeDBId);
            var characterData = account.Characters.FirstOrDefault(x => x.ServerId == req.CharacterServerId);
            
            var count = cafeDb.CafeVisitCharacterDBs.Keys.Last();
            count++;
            cafeDb.LastUpdate = DateTime.Now;
            cafeDb.LastSummonDate = DateTime.Now;
            cafeDb.CafeVisitCharacterDBs.Add(count, 
                new CafeCharacterDB()
                {
                    IsSummon = true,
                    UniqueId = characterData.UniqueId,
                    ServerId = characterData.ServerId,
                }
            );
            context.SaveChanges();

            return new CafeSummonCharacterResponse()
            {
                CafeDB = cafeDb,
                CafeDBs = cafeDbAll
            };
        }

        [ProtocolHandler(Protocol.Cafe_Interact)]
        public ResponsePacket InteractHandler(CafeInteractWithCharacterRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var cafeDb = account.Cafes.FirstOrDefault(x => x.CafeDBId == req.CafeDBId);

            //No relationship rank handler yet

            cafeDb.LastUpdate = DateTime.Now;
            cafeDb.CafeVisitCharacterDBs.Values.FirstOrDefault(x => x.UniqueId == req.CharacterId).LastInteractTime = DateTime.Now;
            context.SaveChanges();

            return new CafeInteractWithCharacterResponse()
            {
                CafeDB = cafeDb,
                CharacterDB = account.Characters.FirstOrDefault(x => x.UniqueId == req.CharacterId),
                ParcelResultDB = new ParcelResultDB(),
            };
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
        
        public static CafeDB CreateCafe(long accountId, List<FurnitureDB> furnitures, Dictionary<long, CafeCharacterDB> characters)
        {
            return new()
            {
                CafeDBId = 0,
                CafeId = 1,
                AccountId = accountId,
                CafeRank = 10,
                LastUpdate = DateTime.Now,
                LastSummonDate = DateTimeOffset.Parse("2023-01-01T00:00:00Z").UtcDateTime,
                CafeVisitCharacterDBs = characters,
                FurnitureDBs = furnitures.Select(x => {
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
                }).ToList(),
                ProductionAppliedTime = DateTimeOffset.Parse("2023-01-01T00:00:00Z").UtcDateTime,
                ProductionDB = new()
                {
                    CafeDBId = 0,
                    AppliedDate = DateTimeOffset.Parse("2023-01-01T00:00:00Z").UtcDateTime,
                    ComfortValue = 60,
                    ProductionParcelInfos =
                    [
                        new CafeProductionParcelInfo()
                        {
                            Key = {
                                Type = ParcelType.Currency,
                                Id = (long)CurrencyTypes.Gold,
                            },
                            Amount = 0
                        },
                        new CafeProductionParcelInfo()
                        {
                            Key = {
                                Type = ParcelType.Currency,
                                Id = (long)CurrencyTypes.ActionPoint
                            },
                            Amount = 0
                        },
                    ]
                },
            };
        }

        public static CafeDB CreateSecondCafe(long accountId, List<FurnitureDB> furnitures, Dictionary<long, CafeCharacterDB> characters)
        {
            return new()
            {
                CafeDBId = 1,
                CafeId = 2,
                AccountId = accountId,
                CafeRank = 10,
                LastUpdate = DateTime.Now,
                LastSummonDate = DateTimeOffset.Parse("2023-01-01T00:00:00Z").UtcDateTime,
                CafeVisitCharacterDBs = characters,
                FurnitureDBs = furnitures.Select(x => {
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
                }).ToList(),
                ProductionAppliedTime = DateTimeOffset.Parse("2023-01-01T00:00:00Z").UtcDateTime,
                ProductionDB = new()
                {
                    CafeDBId = 1,
                    AppliedDate = DateTimeOffset.Parse("2023-01-01T00:00:00Z").UtcDateTime,
                    ComfortValue = 60,
                    ProductionParcelInfos =
                    [
                        new CafeProductionParcelInfo()
                        {
                            Key = {
                                Type = ParcelType.Currency,
                                Id = (long)CurrencyTypes.Gold,
                            },
                            Amount = 0
                        }
                    ]
                },
            };
        }
    }
}
