using Plana.Database;
using Plana.Database.ModelExtensions;
using Plana.FlatData;
using Plana.NetworkProtocol;
using Plana.Parcel;
using Phrenapates.Services;
using Microsoft.EntityFrameworkCore;

namespace Phrenapates.Controllers.Api.ProtocolHandlers
{
    public class Character : ProtocolHandlerBase
    {
        private readonly ISessionKeyService sessionKeyService;
        private readonly SCHALEContext context;
        private readonly ExcelTableService excelTableService;

        public Character(IProtocolHandlerFactory protocolHandlerFactory, ISessionKeyService _sessionKeyService, SCHALEContext _context, ExcelTableService _excelTableService) : base(protocolHandlerFactory)
        {
            sessionKeyService = _sessionKeyService;
            context = _context;
            excelTableService = _excelTableService;
        }

        [ProtocolHandler(Protocol.Character_SetFavorites)]
        public ResponsePacket SetFavoritesHandler(CharacterSetFavoritesRequest req)
        {
            return new CharacterSetFavoritesResponse();
        }

        [ProtocolHandler(Protocol.Character_UpdateSkillLevel)]
        public ResponsePacket CharacterSkillLevelUpdateHandler(CharacterSkillLevelUpdateRequest req)
        {
            //TODO: implement decrease item response & better skillslot implementation
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var targetCharacter = account.Characters.FirstOrDefault(x => x.ServerId == req.TargetCharacterDBId);

            if(req.SkillSlot.ToString().StartsWith("ExSkill", StringComparison.OrdinalIgnoreCase))
            {
                targetCharacter.ExSkillLevel = req.Level;
            }
            else if (req.SkillSlot.ToString().StartsWith("PublicSkill", StringComparison.OrdinalIgnoreCase))
            {
                targetCharacter.PublicSkillLevel = req.Level;
            }
            else if (req.SkillSlot.ToString().StartsWith("Passive", StringComparison.OrdinalIgnoreCase))
            {
                targetCharacter.PassiveSkillLevel = req.Level;
            }
            else if (req.SkillSlot.ToString().StartsWith("ExtraPassive", StringComparison.OrdinalIgnoreCase))
            {
                targetCharacter.ExtraPassiveSkillLevel = req.Level;
            }
            context.SaveChanges();

            return new CharacterSkillLevelUpdateResponse()
            {
                CharacterDB = targetCharacter,
                // ParcelResultDB = new() { }
            };
        }

        [ProtocolHandler(Protocol.Character_BatchSkillLevelUpdate)]
        public ResponsePacket UpdateBatchSkillLevelHandler(CharacterBatchSkillLevelUpdateRequest req)
        {
            //TODO: implement decrease item response & better skillslot implementation
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var targetCharacter = account.Characters.FirstOrDefault(x => x.ServerId == req.TargetCharacterDBId);

            foreach (var skillReq in req.SkillLevelUpdateRequestDBs)
            {
                if(skillReq.SkillSlot.ToString().StartsWith("ExSkill", StringComparison.OrdinalIgnoreCase))
                {
                    targetCharacter.ExSkillLevel = skillReq.Level;
                }
                else if (skillReq.SkillSlot.ToString().StartsWith("PublicSkill", StringComparison.OrdinalIgnoreCase))
                {
                    targetCharacter.PublicSkillLevel = skillReq.Level;
                }
                else if (skillReq.SkillSlot.ToString().StartsWith("Passive", StringComparison.OrdinalIgnoreCase))
                {
                    targetCharacter.PassiveSkillLevel = skillReq.Level;
                }
                else if (skillReq.SkillSlot.ToString().StartsWith("ExtraPassive", StringComparison.OrdinalIgnoreCase))
                {
                    targetCharacter.ExtraPassiveSkillLevel = skillReq.Level;
                }
            }
            context.SaveChanges();

            return new CharacterBatchSkillLevelUpdateResponse()
            {
                CharacterDB = targetCharacter,
                // ParcelResultDB = new() { }
            };
        }

        [ProtocolHandler(Protocol.Character_UnlockWeapon)]
        public ResponsePacket UnlockWeaponHandler(CharacterUnlockWeaponRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var newWeapon = new WeaponDB()
            {
                UniqueId = account.Characters.FirstOrDefault(x => x.ServerId == req.TargetCharacterServerId).UniqueId,
                BoundCharacterServerId = req.TargetCharacterServerId,
                IsLocked = false,
                StarGrade = 1,
                Level = 1
            };

            account.AddWeapons(context, [newWeapon]);
            context.SaveChanges();

            return new CharacterUnlockWeaponResponse()
            {
                WeaponDB = newWeapon,
            };
        }

        [ProtocolHandler(Protocol.Character_PotentialGrowth)]
        public ResponsePacket PotentialGrowthHandler(CharacterPotentialGrowthRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var targetCharacter = account.Characters.FirstOrDefault(x => x.ServerId == req.TargetCharacterDBId);

            foreach (var growthReq in req.PotentialGrowthRequestDBs)
            {
                targetCharacter.PotentialStats[(int)growthReq.Type] = growthReq.Level;
            }

            context.SaveChanges();

            return new CharacterPotentialGrowthResponse()
            {
                CharacterDB = targetCharacter
            };
        }

        [ProtocolHandler(Protocol.Character_ExpGrowth)]
        public ResponsePacket ExpGrowthHandler(CharacterExpGrowthRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var targetCharacter = account.Characters.FirstOrDefault(x => x.ServerId == req.TargetCharacterServerId);
            var dataList = excelTableService.GetTable<CharacterLevelExcelTable>().UnPack().DataList;

            long addExp = 0;
            long previousExp = 0;
            long exp = targetCharacter.Exp;
            var consumeResult = new ConsumeResultDB
            {
                UsedItemServerIdAndRemainingCounts = [],
            };
            var accountCurrency = account.Currencies.First();
            
            var itemData = new Dictionary<long, (int gold, int exp)>
            {
                { 10, (350, 50) },
                { 11, (3500, 500) },
                { 12, (14000, 2000) },
                { 13, (70000, 10000) }
            };

            for (int i = 0; i < req.ConsumeRequestDB.ConsumeItemServerIdAndCounts.Count; i++)
            {
                var item = account.Items.FirstOrDefault(x => x.ServerId == req.ConsumeRequestDB.ConsumeItemServerIdAndCounts.ElementAt(i).Key);
                if (itemData.TryGetValue(item.UniqueId, out var data))
                {
                    addExp += req.ConsumeRequestDB.ConsumeItemServerIdAndCounts.ElementAt(i).Value * data.exp;
                    accountCurrency.CurrencyDict[CurrencyTypes.Gold] -= req.ConsumeRequestDB.ConsumeItemServerIdAndCounts.ElementAt(i).Value * data.gold;
                    accountCurrency.UpdateTimeDict[CurrencyTypes.Gold] = DateTime.Now;
                    item.StackCount -= req.ConsumeRequestDB.ConsumeItemServerIdAndCounts.ElementAt(i).Value;
                    consumeResult.UsedItemServerIdAndRemainingCounts.Add(item.ServerId, item.StackCount);
                }
            }

            if (targetCharacter.Level == 1) previousExp = exp + addExp;
            else previousExp = dataList[targetCharacter.Level - 2].TotalExp + exp + addExp;

            foreach(var data in dataList)
            {
                if (previousExp > data.TotalExp && targetCharacter.Level < dataList.Count) targetCharacter.Level++;
                else if (previousExp == data.TotalExp)
                {
                    targetCharacter.Level = data.Level;
                    break;
                }
                else if (previousExp < data.TotalExp)
                {
                    targetCharacter.Level = data.Level;
                    targetCharacter.Exp = previousExp - data.TotalExp + data.Exp;
                    break;
                }
            }
            context.Entry(accountCurrency).State = EntityState.Modified;
            context.SaveChanges();
            
            return new CharacterExpGrowthResponse()
            {
                CharacterDB = targetCharacter,
                ConsumeResultDB = consumeResult,
                AccountCurrencyDB = accountCurrency
            };
        }

        [ProtocolHandler(Protocol.Character_Transcendence)]
        public ResponsePacket TranscendenceHandler(CharacterTranscendenceRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var targetCharacter = account.Characters.FirstOrDefault(x => x.ServerId == req.TargetCharacterServerId);
            var item = account.Items.FirstOrDefault(x => x.AccountServerId == req.SessionKey.AccountServerId && x.UniqueId == targetCharacter.UniqueId);
            var currency = account.Currencies.First();

            var itemNeeded = targetCharacter.StarGrade switch
            {
                1 => 30,
                2 => 80,
                3 => 100,
                4 => 120,
                _ => 0
            };

            var currencyNeeded = targetCharacter.StarGrade switch
            {
                1 => 10000,
                2 => 40000,
                3 => 200000,
                4 => 1000000,
                _ => 0
            };

            targetCharacter.StarGrade++;
            item.StackCount -= itemNeeded;
            currency.CurrencyDict[CurrencyTypes.Gold] -= currencyNeeded;
            currency.UpdateTimeDict[CurrencyTypes.Gold] = DateTime.Now;
            context.Entry(currency).State = EntityState.Modified;
            context.SaveChanges();

            return new CharacterTranscendenceResponse()
            {
                CharacterDB = targetCharacter,
                ParcelResultDB = new()
                {
                    AccountCurrencyDB = currency,
                    ItemDBs = new Dictionary<long, ItemDB> { { item.UniqueId, item } }
                }
            };
        }

        [ProtocolHandler(Protocol.Character_WeaponExpGrowth)]
        public ResponsePacket WeaponExpGrowthHandler(CharacterWeaponExpGrowthRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var targetCharacter = account.Characters.FirstOrDefault(x => x.ServerId == req.TargetCharacterServerId);
            var weapon = account.Weapons.FirstOrDefault(x => x.AccountServerId == req.SessionKey.AccountServerId && x.UniqueId == targetCharacter.UniqueId);
            var levelTable = excelTableService.GetTable<CharacterWeaponLevelExcelTable>().UnPack().DataList;
            var characterTable = excelTableService.GetTable<CharacterExcelTable>().UnPack().DataList;

            long addExp = 0;
            long previousExp = 0;
            long exp = weapon.Exp;
            var charWeaponType = characterTable.Find(x => x.Id == targetCharacter.UniqueId).WeaponType;
            var accountCurrency = account.Currencies.First();
            Dictionary<long, EquipmentDB> equipmentData = [];

            var eqData = new Dictionary<long, (int gold, int exp, int expBonus, WeaponType[] weaponType)>
            {
                { 10, (2700, 10, 15, [WeaponType.SMG, WeaponType.SG, WeaponType.HG, WeaponType.FT]) },
                { 20, (2700, 10, 15, [WeaponType.AR, WeaponType.GL, WeaponType.RL]) },
                { 30, (2700, 10, 15, [WeaponType.MG, WeaponType.SR, WeaponType.RG, WeaponType.MT]) },
                { 40, (2700, 10, 15, [WeaponType.None]) },

                { 11, (13500, 50, 75, [WeaponType.SMG, WeaponType.SG, WeaponType.HG, WeaponType.FT]) },
                { 21, (13500, 50, 75, [WeaponType.AR, WeaponType.GL, WeaponType.RL]) },
                { 31, (13500, 50, 75, [WeaponType.MG, WeaponType.SR, WeaponType.RG, WeaponType.MT]) },
                { 41, (13500, 50, 75, [WeaponType.None]) },

                { 12, (36000, 200, 300, [WeaponType.SMG, WeaponType.SG, WeaponType.HG, WeaponType.FT]) },
                { 22, (36000, 200, 300, [WeaponType.AR, WeaponType.GL, WeaponType.RL]) },
                { 32, (36000, 200, 300, [WeaponType.MG, WeaponType.SR, WeaponType.RG, WeaponType.MT]) },
                { 42, (36000, 200, 300, [WeaponType.None]) },

                { 13, (270000, 1000, 1500, [WeaponType.SMG, WeaponType.SG, WeaponType.HG, WeaponType.FT]) },
                { 23, (270000, 1000, 1500, [WeaponType.AR, WeaponType.GL, WeaponType.RL]) },
                { 33, (270000, 1000, 1500, [WeaponType.MG, WeaponType.SR, WeaponType.RG, WeaponType.MT]) },
                { 43, (270000, 1000, 1500, [WeaponType.None]) }
            };

            for (int i = 0; i < req.ConsumeUniqueIdAndCounts.Count; i++)
            {
                var eq = account.Equipment.FirstOrDefault(x => x.UniqueId == req.ConsumeUniqueIdAndCounts.ElementAt(i).Key);
                if(eqData.TryGetValue(eq.UniqueId, out var data))
                {
                    if (data.weaponType.Contains(charWeaponType) || data.weaponType.Contains(WeaponType.None)) addExp += req.ConsumeUniqueIdAndCounts.ElementAt(i).Value * data.expBonus;
                    else addExp += req.ConsumeUniqueIdAndCounts.ElementAt(i).Value * data.exp;
                    accountCurrency.CurrencyDict[CurrencyTypes.Gold] -= req.ConsumeUniqueIdAndCounts.ElementAt(i).Value * data.gold;
                    accountCurrency.UpdateTimeDict[CurrencyTypes.Gold] = DateTime.Now;
                    eq.StackCount -= req.ConsumeUniqueIdAndCounts.ElementAt(i).Value;
                    equipmentData.Add(eq.UniqueId, eq);
                }
            }

            if (weapon.Level == 1) previousExp = exp + addExp;
            else previousExp = levelTable[weapon.Level - 2].TotalExp + exp + addExp;

            foreach(var data in levelTable)
            {
                if (previousExp > data.TotalExp && weapon.Level < 50) weapon.Level++;
                else if (previousExp == data.TotalExp)
                {
                    weapon.Level = data.Level;
                    break;
                }
                else if (previousExp < data.TotalExp)
                {
                    weapon.Level = data.Level;
                    weapon.Exp = previousExp - data.TotalExp + data.Exp;
                    break;
                }
            }
            context.Entry(accountCurrency).State = EntityState.Modified;
            context.SaveChanges();

            return new CharacterWeaponExpGrowthResponse()
            {
                ParcelResultDB = new()
                {
                    AccountCurrencyDB = accountCurrency,
                    WeaponDBs = new List<WeaponDB> { weapon },
                    EquipmentDBs = equipmentData
                }
            };
        }

        [ProtocolHandler(Protocol.Character_WeaponTranscendence)]
        public ResponsePacket WeaponTranscendenceHandler(CharacterWeaponTranscendenceRequest req)
        {
            var account = sessionKeyService.GetAccount(req.SessionKey);
            var targetCharacter = account.Characters.FirstOrDefault(x => x.ServerId == req.TargetCharacterServerId);
            var weapon = account.Weapons.FirstOrDefault(x => x.AccountServerId == req.SessionKey.AccountServerId && x.UniqueId == targetCharacter.UniqueId);
            var item = account.Items.FirstOrDefault(x => x.AccountServerId == req.SessionKey.AccountServerId && x.UniqueId == targetCharacter.UniqueId);
            var currency = account.Currencies.First();

            var itemNeeded = weapon.StarGrade switch
            {
                1 => 120,
                2 => 180,
                _ => 0
            };

            var currencyNeeded = weapon.StarGrade switch
            {
                1 => 1000000,
                2 => 1500000,
                _ => 0
            };

            weapon.StarGrade++;
            item.StackCount -= itemNeeded;
            currency.CurrencyDict[CurrencyTypes.Gold] -= currencyNeeded;
            currency.UpdateTimeDict[CurrencyTypes.Gold] = DateTime.Now;
            context.Entry(currency).State = EntityState.Modified;
            context.SaveChanges();

            return new CharacterWeaponTranscendenceResponse()
            {
                ParcelResultDB = new()
                {
                    WeaponDBs = new List<WeaponDB> { weapon },
                    ItemDBs = new Dictionary<long, ItemDB> { { item.UniqueId, item } }
                }
            };
        }
    }
}
