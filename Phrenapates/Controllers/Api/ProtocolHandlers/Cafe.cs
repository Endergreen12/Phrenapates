using Plana.Database;
using Plana.FlatData;
using Plana.NetworkProtocol;
using Phrenapates.Services;
using Phrenapates.Utils;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class Cafe : ProtocolHandlerBase
    {
        private ISessionKeyService sessionKeyService;
        private SCHALEContext context;

        public Cafe(IProtocolHandlerFactory protocolHandlerFactory, ISessionKeyService _sessionKeyService, SCHALEContext _context) : base(protocolHandlerFactory)
        {
            sessionKeyService = _sessionKeyService;
            context = _context;
        }

        [ProtocolHandler(Protocol.Cafe_Get)]
        public ResponsePacket GetHandler(CafeGetInfoRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var cafeDb = account.Cafes.FirstOrDefault();

            // Cafe Handler stuff
            cafeDb.LastUpdate = DateTime.Now;
            cafeDb.LastSummonDate = DateTime.MinValue;

            // Data and stuff
            cafeDb.FurnitureDBs = account.Furnitures.Select(x => 
                {
                    return new FurnitureDB()
                    {
                        CafeDBId = x.CafeDBId,
                        UniqueId = x.UniqueId,
                        Location = x.Location,
                        PositionX = x.PositionX,
                        PositionY = x.PositionY,
                        Rotation = x.Rotation,
                        ItemDeploySequence = x.ItemDeploySequence,
                        StackCount = x.StackCount
                    };
                }
            ).ToList();

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

            return new CafeGetInfoResponse()
            {
                CafeDB = cafeDb,
                CafeDBs = [.. account.Cafes],
                FurnitureDBs = cafeDb.FurnitureDBs
            };
        }

        [ProtocolHandler(Protocol.Cafe_Ack)]
        public ResponsePacket AckHandler(CafeAckRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var cafeDb = account.Cafes.FirstOrDefault();
            
            // Cafe Handler stuff
            cafeDb.LastUpdate = DateTime.Now;
            cafeDb.LastSummonDate = DateTime.MinValue;

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

        public static CafeDB CreateCafe(long accountId, List<CharacterDB> defaultCharacters)
        {
            return new()
            {
                CafeDBId = 0,
                CafeId = 1,
                AccountId = accountId,
                CafeRank = 10,
                LastUpdate = DateTime.Now,
                LastSummonDate = DateTime.MinValue,
                CafeVisitCharacterDBs = [],
                FurnitureDBs = [],
                ProductionAppliedTime = DateTime.Now,
                ProductionDB = new()
                {
                    CafeDBId = 1,
                    AppliedDate = DateTime.Now,
                    ComfortValue = 5500,
                    ProductionParcelInfos =
                    [
                        new CafeProductionParcelInfo()
                        {
                            Key = {
                                Type = ParcelType.Currency,
                                Id = 1,
                            },
                            Amount = 9999999
                        },
                        new CafeProductionParcelInfo()
                        {
                            Key = {
                                Type = ParcelType.Currency,
                                Id = 5
                            },
                            Amount = 9999999
                        },
                    ]
                },
            };
        }
    }
}
