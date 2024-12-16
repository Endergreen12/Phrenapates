using Phrenapates.Services.Irc;
using Plana.Database.ModelExtensions;
using Plana.FlatData;
using Plana.MX.GameLogic.DBModel;
using Microsoft.IdentityModel.Tokens;

namespace Phrenapates.Utils
{
    public static class CharacterUtils
    {
        public static void AddCharacter(IrcConnection connection, uint characterId, string addOption)
        {
            var characterExcel = connection.ExcelTableService.GetTable<CharacterExcelTable>().UnPack().DataList;
            var characterLevelExcel = connection.ExcelTableService.GetTable<CharacterLevelExcelTable>().UnPack().DataList;

            var weaponExcel = connection.ExcelTableService.GetTable<CharacterWeaponExcelTable>().UnPack().DataList;
            var equipmentExcel = connection.ExcelTableService.GetTable<EquipmentExcelTable>().UnPack().DataList;
            var uniqueGearExcel = connection.ExcelTableService.GetTable<CharacterGearExcelTable>().UnPack().DataList;

            bool useOptions = false;
            int starGrade = 3;
            int favorRank = 1;
            bool breakLimit = false;
            int weaponLevel = 1;
            bool useEquipment = false;
            bool useGear = false;

            if (!addOption.IsNullOrEmpty()) useOptions = true;

            switch(addOption)
            {
                case "basic":
                    starGrade = 3;
                    favorRank = 20;
                    useEquipment = true;
                    useGear = true;
                    break;
                case "ue30":
                    starGrade = 5;
                    favorRank = 20;
                    weaponLevel = 30;
                    useEquipment = true;
                    useGear = true;
                    break;
                case "ue50":
                    starGrade = 5;
                    favorRank = 50;
                    weaponLevel = 50;
                    useEquipment = true;
                    useGear = true;
                    break;
                case "max":
                    starGrade = 5;
                    favorRank = 100;
                    breakLimit = true;
                    weaponLevel = 50;
                    useEquipment = true;
                    useGear = true;
                    break;
            }

            // Character
            var characterDB = new CharacterDB()
            {
                UniqueId = characterId,
                StarGrade = useOptions ? starGrade : characterExcel.FirstOrDefault(x => x.Id == characterId).DefaultStarGrade,
                Level = useOptions ? characterLevelExcel.Count : 1,
                Exp = 0,
                ExSkillLevel = useOptions ? 5 : 1,
                PublicSkillLevel = useOptions ? 10 : 1,
                PassiveSkillLevel = useOptions ? 10 : 1,
                ExtraPassiveSkillLevel = useOptions ? 10 : 1,
                LeaderSkillLevel = 1,
                FavorRank = useOptions ? favorRank : 1,
                IsNew = true,
                IsLocked = true,
                PotentialStats = breakLimit ? 
                    new Dictionary<int, int> { { 1, 25 }, { 2, 25 }, { 3, 25 } } :
                    new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 } },
                EquipmentServerIds = [0, 0, 0]
            };

            if(connection.Account.Characters.Any(x => x.UniqueId == characterId))
            {
                var character = connection.Account.Characters.FirstOrDefault(x => x.UniqueId == characterId);
                character.StarGrade = useOptions ? starGrade : characterExcel.FirstOrDefault(x => x.Id == characterId).DefaultStarGrade;
                character.Level = useOptions ? characterLevelExcel.Count : 1;
                character.Exp = 0;
                character.ExSkillLevel = useOptions ? 5 : 1;
                character.PublicSkillLevel = useOptions ? 10 : 1;
                character.PassiveSkillLevel = useOptions ? 10 : 1;
                character.ExtraPassiveSkillLevel = useOptions ? 10 : 1;
                character.LeaderSkillLevel = 1;
                character.FavorRank = useOptions ? favorRank : 1;
                character.IsNew = true;
                character.IsLocked = true;
                character.PotentialStats = breakLimit ? 
                    new Dictionary<int, int> { { 1, 25 }, { 2, 25 }, { 3, 25 } } :
                    new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 } };
                connection.Context.SaveChanges();
            }
            else
            {
                connection.Account.AddCharacters(connection.Context, [characterDB]);
                connection.Context.SaveChanges();
            }

            
            //Weapon
            if(useOptions && weaponLevel != 1)
            {
                var weaponOwner = connection.Account.Characters.FirstOrDefault(x => x.UniqueId == characterId);
                if(weaponOwner != null)
                {
                    var weapon = new WeaponDB()
                    {
                        UniqueId = weaponOwner.UniqueId,
                        BoundCharacterServerId = weaponOwner.ServerId,
                        IsLocked = false,
                        StarGrade = weaponExcel.FirstOrDefault(y => y.Id == weaponOwner.UniqueId).Unlock.TakeWhile(y => y).Count(),
                        Level = weaponLevel
                    };
                    connection.Account.AddWeapons(connection.Context, [weapon]);
                    connection.Context.SaveChanges();
                }
            }

            //Equipment
            if(useOptions && useEquipment)
            {
                var characterEquipmentData = characterExcel.FirstOrDefault(x => x.Id == characterId);
                if (characterEquipmentData != null)
                {
                    var characterEquipment = characterEquipmentData.EquipmentSlot.Select(x =>
                    {
                        var equipmentData = equipmentExcel.FirstOrDefault(
                            y => y.EquipmentCategory == x && y.MaxLevel == 65
                        );
                        return new EquipmentDB()
                        {
                            UniqueId = equipmentData.Id,
                            Level = equipmentData.MaxLevel,
                            Tier = (int)equipmentData.TierInit,
                            StackCount = 1,
                            BoundCharacterServerId = connection.Account.Characters.FirstOrDefault(y => y.UniqueId == characterEquipmentData.Id).ServerId
                        };                
                    }).ToList();
                    connection.Account.AddEquipment(connection.Context, [.. characterEquipment]);
                    connection.Context.SaveChanges();    
                    connection.Account.Characters.FirstOrDefault(x => x.UniqueId == characterEquipmentData.Id).EquipmentServerIds.Clear();
                    connection.Account.Characters.FirstOrDefault(x => x.UniqueId == characterEquipmentData.Id).EquipmentServerIds.AddRange(characterEquipment.Select(x => x.ServerId));
                    connection.Context.SaveChanges();
                }
            }

            // Unique Gear
            if(useOptions && useGear)
            {
                var uniqueGear = uniqueGearExcel.First(x => x.Tier == 2 && x.CharacterId == characterId);
                if (uniqueGear != null)
                {
                    var uniqueGearDB = new GearDB()
                    {
                        UniqueId = uniqueGear.Id,
                        Level = 1,
                        SlotIndex = 4,
                        BoundCharacterServerId = connection.Context.Characters.FirstOrDefault(z => z.UniqueId == uniqueGear.CharacterId).ServerId,
                        Tier = (int)uniqueGear.Tier,
                        Exp = 0,
                    };
                    connection.Account.AddGears(connection.Context, [uniqueGearDB]);
                    connection.Context.SaveChanges();
                }
            }
            
            /*var characterData = connection.Context.Characters.FirstOrDefault(x => x.AccountServerId == connection.AccountServerId &&
                x.UniqueId == characterId);
            var weaponData = connection.Context.Weapons.FirstOrDefault(x => x.AccountServerId == connection.AccountServerId && 
                x.BoundCharacterServerId == characterData.ServerId);
            var equipmentData = connection.Context.Equipment.Where(x => x.AccountServerId == connection.AccountServerId &&
                x.BoundCharacterServerId == characterData.ServerId);
            var gearData = connection.Context.Gears.FirstOrDefault(x => x.AccountServerId == connection.AccountServerId &&
                x.BoundCharacterServerId == characterData.ServerId);
            Console.WriteLine($"Add Character: {JsonSerializer.Serialize(characterData)}");
            Console.WriteLine($"Add Weapon: {JsonSerializer.Serialize(weaponData)}");
            Console.WriteLine($"Add Equipment: {JsonSerializer.Serialize(equipmentData)}");
            Console.WriteLine($"Add Gear: {JsonSerializer.Serialize(gearData)}");*/
        }

        public static void RemoveCharacter(IrcConnection connection, uint characterId)
        {
            var characterExcel = connection.ExcelTableService.GetTable<CharacterExcelTable>().UnPack().DataList;
            var defaultCharacterExcel = connection.ExcelTableService.GetTable<DefaultCharacterExcelTable>().UnPack().DataList;
            var weaponExcel = connection.ExcelTableService.GetTable<CharacterWeaponExcelTable>().UnPack().DataList;

            var character = connection.Context.Characters.FirstOrDefault(x => x.AccountServerId == connection.AccountServerId &&
                x.UniqueId == characterId);
            var weapon = connection.Context.Weapons.FirstOrDefault(x => x.AccountServerId == connection.AccountServerId && 
                x.BoundCharacterServerId == character.ServerId);
            var equipment = connection.Context.Equipment.Where(x => x.AccountServerId == connection.AccountServerId &&
                x.BoundCharacterServerId == character.ServerId);
            var gear = connection.Context.Gears.FirstOrDefault(x => x.AccountServerId == connection.AccountServerId &&
                x.BoundCharacterServerId == character.ServerId);
            /*Console.WriteLine($"Remove Character: {JsonSerializer.Serialize(character)}");
            Console.WriteLine($"Remove Weapon: {JsonSerializer.Serialize(weapon)}");
            Console.WriteLine($"Remove Equipment: {JsonSerializer.Serialize(equipment)}");
            Console.WriteLine($"Remove Gear: {JsonSerializer.Serialize(gear)}");*/

            if (character == null)
            {
                connection.SendChatMessage($"{characterId} does not exist!");
                return;
            };

            if (character != null && !defaultCharacterExcel.Select(x => x.CharacterId).ToList().Contains(character.UniqueId))
            {
                connection.Context.Characters.Remove(character);
                connection.SendChatMessage($"Character {characterId} successfully removed!");
            }
            else
            {
                var defaultChar = connection.Context.Characters.FirstOrDefault(x => x.UniqueId == characterId);
                defaultChar.StarGrade = characterExcel.FirstOrDefault(x => x.Id == character.UniqueId).DefaultStarGrade;
                defaultChar.PublicSkillLevel = 1;
                defaultChar.ExSkillLevel = 1;
                defaultChar.PassiveSkillLevel = 1;
                defaultChar.ExtraPassiveSkillLevel = 1;
                defaultChar.Level = 1;
                defaultChar.Exp = 0;
                defaultChar.FavorRank = 1;
                defaultChar.PotentialStats = new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 } };
                defaultChar.EquipmentServerIds = [0, 0, 0];
                connection.Context.Characters.Update(defaultChar);
                connection.SendChatMessage($"Default character cannot be removed!");
            }

            if(weapon != null) connection.Context.Weapons.Remove(weapon);
            if(equipment != null) connection.Context.Equipment.RemoveRange(equipment);
            if(gear != null) connection.Context.Gears.Remove(gear);
            connection.Context.SaveChanges();
        }
    }
}