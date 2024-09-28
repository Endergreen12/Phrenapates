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
            cafeDb.FurnitureDBs = account.Furnitures.ToList();

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
            
            // Cafe Handler stuff
            cafeDb.LastUpdate = DateTime.Now;
            cafeDb.LastSummonDate = DateTime.MinValue;

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
            
            // Cafe Handler stuff
            cafeDb.LastUpdate = DateTime.Now;
            cafeDb.LastSummonDate = DateTime.MinValue;

            var removedFurniture = new List<FurnitureDB>();
;
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

        public static CafeDB CreateCafe(long accountId)
        {
            return new()
            {
                CafeDBId = 0,
                CafeId = 0,
                AccountId = accountId,
                CafeRank = 0,
                LastUpdate = DateTime.Now,
                LastSummonDate = DateTime.MinValue,
                CafeVisitCharacterDBs = new(),
                FurnitureDBs = new(),
                ProductionAppliedTime = DateTime.Now,
                ProductionDB = new(),
            };
        }
    }
}
