using Plana.Database;
using Plana.Database.ModelExtensions;
using Plana.FlatData;
using Plana.NetworkProtocol;
using Phrenapates.Services;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class CharacterGear : ProtocolHandlerBase
    {
        private readonly ISessionKeyService sessionKeyService;
        private readonly SCHALEContext context;
        private readonly ExcelTableService excelTableService;

        public CharacterGear(IProtocolHandlerFactory protocolHandlerFactory, ISessionKeyService _sessionKeyService, SCHALEContext _context, ExcelTableService _excelTableService) : base(protocolHandlerFactory)
        {
            sessionKeyService = _sessionKeyService;
            context = _context;
            excelTableService = _excelTableService;
        }

        [ProtocolHandler(Protocol.CharacterGear_Unlock)]
        public ResponsePacket UnlockHandler(CharacterGearUnlockRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);

            var gearExcelTable = excelTableService.GetTable<CharacterGearExcelTable>().UnPack().DataList;
            var targetCharacter = account.Characters.FirstOrDefault(x => x.ServerId == req.CharacterServerId);

            var gearId = gearExcelTable.FirstOrDefault(x => x.CharacterId == targetCharacter.UniqueId).Id;

            var newGear = new GearDB()
            {
                UniqueId = gearId,
                Level = 1,
                SlotIndex = req.SlotIndex,
                BoundCharacterServerId = req.CharacterServerId,
                Tier = 1,
                Exp = 0,
            };

            account.AddGears(context, [newGear]);
            context.SaveChanges();

            return new CharacterGearUnlockResponse()
            {
                GearDB = newGear,
                CharacterDB = targetCharacter,
            };
        }

        [ProtocolHandler(Protocol.CharacterGear_TierUp)]
        public ResponsePacket TierUpHandler(CharacterGearTierUpRequest req)
        {   // doesnt work
            var targetGear = context.Gears.FirstOrDefault(x => x.ServerId == req.GearServerId);

            targetGear.Tier++;
            context.SaveChanges();

            return new CharacterGearTierUpResponse()
            {
                GearDB = targetGear,
            };
        }
    }
}
