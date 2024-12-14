using Plana.MX.GameLogic.DBModel;
using Plana.Database.ModelExtensions;
using Plana.FlatData;
using Phrenapates.Services.Irc;
using System.Data;

namespace Plana.Utils
{
    public static class InventoryUtils
    {
        public static void AddAllCharacters(IrcConnection connection, bool maxed = true)
        {
            var account = connection.Account;
            var context = connection.Context;

            var characterExcel = connection.ExcelTableService.GetTable<CharacterExcelTable>().UnPack().DataList;
            var defaultCharacterExcel = connection.ExcelTableService.GetTable<DefaultCharacterExcelTable>().UnPack().DataList;
            var characterLevelExcel = connection.ExcelTableService.GetTable<CharacterLevelExcelTable>().UnPack().DataList;
            var favorLevelExcel = connection.ExcelTableService.GetTable<FavorLevelExcelTable>().UnPack().DataList;

            var allCharacters = characterExcel.Where(x =>
                x is
                {
                    IsPlayable: true,
                    IsPlayableCharacter: true,
                    IsNPC: false,
                    ProductionStep: ProductionStep.Release,
                }
            )
            .Where(x => !account.Characters.Any(y => y.UniqueId == x.Id))
            .Select(x => {
                return new CharacterDB()
                {
                    UniqueId = x.Id,
                    StarGrade = maxed ? x.MaxStarGrade : x.DefaultStarGrade,
                    Level = maxed ? characterLevelExcel.Count : 1,
                    Exp = 0,
                    PublicSkillLevel = maxed ? 10 : 1,
                    ExSkillLevel = maxed ? 5 : 1,
                    PassiveSkillLevel = maxed ? 10 : 1,
                    ExtraPassiveSkillLevel = maxed ? 10 : 1,
                    LeaderSkillLevel = 1,
                    FavorRank = maxed ? favorLevelExcel.Count : 1,
                    IsNew = true,
                    IsLocked = true,
                    PotentialStats = maxed ? new Dictionary<int, int> { { 1, 25 }, { 2, 25 }, { 3, 25 } } : new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 } },
                    EquipmentServerIds = [0, 0, 0]
                };
            }).ToList();

            foreach (var character in account.Characters.Where(x => characterExcel.Any(y => y.Id == x.UniqueId)))
            {
                var updateCharacter = character;
                updateCharacter.StarGrade = maxed ? characterExcel.FirstOrDefault(y => y.Id == character.UniqueId).MaxStarGrade : characterExcel.FirstOrDefault(y => y.Id == character.UniqueId).DefaultStarGrade;
                updateCharacter.PublicSkillLevel = maxed ? 10 : 1;
                updateCharacter.ExSkillLevel = maxed ? 5 : 1;
                updateCharacter.PassiveSkillLevel = maxed ? 10 : 1;
                updateCharacter.ExtraPassiveSkillLevel = maxed ? 10 : 1;
                updateCharacter.Level = maxed ? characterLevelExcel.Count : 1;
                updateCharacter.Exp = 0;
                updateCharacter.FavorRank = maxed ? favorLevelExcel.Count : 1;
                updateCharacter.PotentialStats = maxed ? new Dictionary<int, int> { { 1, 25 }, { 2, 25 }, { 3, 25 } } : new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 } };
                updateCharacter.EquipmentServerIds = [0, 0, 0];
                connection.Context.Characters.Update(updateCharacter);
            }

            account.AddCharacters(context, [.. allCharacters]);
            context.SaveChanges();

            connection.SendChatMessage("Added all characters!");
        }

        public static void AddAllEquipment(IrcConnection connection, bool maxed = true)
        {
            var equipmentExcel = connection.ExcelTableService.GetTable<EquipmentExcelTable>().UnPack().DataList;
            
            if(!maxed)
            {
                connection.Context.Equipment.RemoveRange(connection.Context.Equipment.Where(x => x.AccountServerId == connection.AccountServerId));
                var allEquipment = equipmentExcel.Select(x =>
                {
                    return new EquipmentDB()
                    {
                        UniqueId = x.Id,
                        Level = 1,
                        StackCount = (long)Math.Floor((double)x.StackableMax / 2), // ~ 90,000 cap, auto converted if over
                    };
                }).ToList();
                connection.Account.AddEquipment(connection.Context, [.. allEquipment]);
                connection.Context.SaveChanges();
                connection.SendChatMessage("Added/Reset all equipment!");
                return;
            }

            var characterExcel = connection.ExcelTableService.GetTable<CharacterExcelTable>().UnPack().DataList;
            var allCharacterEquipment = characterExcel.FindAll(x => connection.Account.Characters.Any(y => y.UniqueId == x.Id)).ToList();
            foreach (var characterEquipmentData in allCharacterEquipment)
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
            }
            connection.Context.SaveChanges();

            connection.SendChatMessage("Added all equipment!");
        }

        public static void AddAllItems(IrcConnection connection)
        {
            var itemExcel = connection.ExcelTableService.GetTable<ItemExcelTable>().UnPack().DataList;
            var allItems = itemExcel.Select(x =>
            {
                return new ItemDB()
                {
                    IsNew = true,
                    UniqueId = x.Id,
                    StackCount = x.StackableMax - 100 <= 0 ? 1 : (long)Math.Floor((double)x.StackableMax / 2)
                };
            }).ToList();

            connection.Account.AddItems(connection.Context, [.. allItems]);
            connection.Context.SaveChanges();

            connection.SendChatMessage("Added all items!");
        }

        public static void AddAllWeapons(IrcConnection connection, bool maxed = true)
        {
            var account = connection.Account;
            var context = connection.Context;

            if(!maxed)
            {
                context.Weapons.RemoveRange(context.Weapons.Where(x => x.AccountServerId == connection.AccountServerId));
                context.SaveChanges();
                return;
            }

            var weaponExcel = connection.ExcelTableService.GetTable<CharacterWeaponExcelTable>().UnPack().DataList;
            // only for current characters
            var allWeapons = account.Characters.Select(x =>
            {
                return new WeaponDB()
                {
                    UniqueId = x.UniqueId,
                    BoundCharacterServerId = x.ServerId,
                    IsLocked = false,
                    StarGrade = weaponExcel.FirstOrDefault(y => y.Id == x.UniqueId).Unlock.TakeWhile(y => y).Count(),
                    Level = 50
                };
            });

            account.AddWeapons(context, [.. allWeapons]);
            

            connection.SendChatMessage("Added all weapons!");
        }

        public static void AddAllGears(IrcConnection connection, bool maxed = true)
        {
            var account = connection.Account;
            var context = connection.Context;
            if(!maxed)
            {
                context.Gears.RemoveRange(context.Gears.Where(x => x.AccountServerId == connection.AccountServerId));
                context.SaveChanges();
                return;
            }

            var uniqueGearExcel = connection.ExcelTableService.GetTable<CharacterGearExcelTable>().UnPack().DataList;

            var uniqueGear = uniqueGearExcel.Where(x => x.Tier == (maxed ? 2 : 1) && context.Characters.Any(y => y.UniqueId == x.CharacterId)).Select(x => 
                new GearDB()
                {
                    UniqueId = x.Id,
                    Level = 1,
                    SlotIndex = 4,
                    BoundCharacterServerId = context.Characters.FirstOrDefault(z => z.UniqueId == x.CharacterId).ServerId,
                    Tier = (int)x.Tier,
                    Exp = 0,
                }
            );

            account.AddGears(context, [.. uniqueGear]);
            context.SaveChanges();

            connection.SendChatMessage("Added all gears!");
        }

        public static void AddAllMemoryLobbies(IrcConnection connection)
        {
            var account = connection.Account;
            var context = connection.Context;

            var memoryLobbyExcel = connection.ExcelTableService.GetExcelList<MemoryLobbyExcel>("MemoryLobbyDBSchema");
            var allMemoryLobbies = memoryLobbyExcel
            .Where(x => !account.MemoryLobbies.Any(y => y != null && y.MemoryLobbyUniqueId == x.Id))
            .Select(x =>
            {
                return new MemoryLobbyDB()
                {
                    MemoryLobbyUniqueId = x.Id,
                };
            }).ToList();
            account.AddMemoryLobbies(context, [.. allMemoryLobbies]);
            context.SaveChanges();

            connection.SendChatMessage("Added all Memory Lobbies!");
        }

        public static void AddAllScenarios(IrcConnection connection)
        {
            var account = connection.Account;
            var context = connection.Context;

            var scenarioModeExcel = connection.ExcelTableService.GetExcelList<ScenarioModeExcel>("ScenarioModeDBSchema");
            var normalScenario = scenarioModeExcel
            .Where(x => !account.Scenarios.Any(y => y != null && y.ScenarioUniqueId == x.ModeId))
            .Select(x =>
            {
                return new ScenarioHistoryDB()
                {
                    ScenarioUniqueId = x.ModeId,
                };
            }).ToList();
            
            account.AddScenarios(context, [.. normalScenario]);
            context.SaveChanges();

            connection.SendChatMessage("Added all Scenarios!");
        }

        public static void AddAllFurnitures(IrcConnection connection)
        {
            var account = connection.Account;
            var context = connection.Context;

            var furnitureExcel = connection.ExcelTableService.GetTable<FurnitureExcelTable>().UnPack().DataList;
            var defaultFurnitureExcel = connection.ExcelTableService.GetTable<DefaultFurnitureExcelTable>().UnPack().DataList;
            
            var allFurnitures = furnitureExcel.Where(x => !account.Furnitures.Any(y => y != null && y.UniqueId == x.Id))
            .Select(x =>
            {
                return new FurnitureDB()
                {
                    //Furniture on Inventory doesn't have data about its furniture owner
                    CafeDBId = account.Cafes.FirstOrDefault().CafeDBId,
                    Location = FurnitureLocation.Inventory,
                    UniqueId = x.Id,
                    StackCount = x.StackableMax
                };
            }).ToList();
            
            account.AddFurnitures(context, [.. allFurnitures]);
            context.SaveChanges();

            connection.SendChatMessage("Added all Furnitures!");
        }

        public static void RemoveAllCharacters(IrcConnection connection) // removing default characters breaks game
        {
            var characterDB = connection.Context.Characters;
            var characterExcel = connection.ExcelTableService.GetTable<CharacterExcelTable>().UnPack().DataList;
            var defaultCharacterExcel = connection.ExcelTableService.GetTable<DefaultCharacterExcelTable>().UnPack().DataList;

            var removed = characterDB.Where(x => x.AccountServerId == connection.AccountServerId && !defaultCharacterExcel.Select(x => x.CharacterId).ToList().Contains(x.UniqueId));

            characterDB.RemoveRange(removed);
            foreach (var character in connection.Account.Characters.Where(x => defaultCharacterExcel.Any(y => y.CharacterId == x.UniqueId)))
            {
                var defaultChar = character;
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
            }

            connection.SendChatMessage("Removed all characters!");
        }

        public static void RemoveAllFurnitures(IrcConnection connection)
        {
            var account = connection.Account;
            var context = connection.Context;

            var defaultFurnitureExcel = connection.ExcelTableService.GetTable<DefaultFurnitureExcelTable>().UnPack().DataList;

            var removed = context.Furnitures.Where(x => x.AccountServerId == connection.AccountServerId);
            context.RemoveRange(removed);
            context.SaveChanges();

            /*var cafeFurnitures = defaultFurnitureExcel.GetRange(0, 3).Select((x, index) => {
                return new FurnitureDB()
                {
                    CafeDBId = 0,
                    UniqueId = x.Id,
                    Location = x.Location,
                    PositionX = x.PositionX,
                    PositionY = x.PositionY,
                    Rotation = x.Rotation,
                    ItemDeploySequence = index + 1,
                    StackCount = 1
                };
            }).ToList();
            var secondCafeFurnitures = defaultFurnitureExcel.GetRange(0, 3).Select((x, index) => {
                return new FurnitureDB()
                {
                    CafeDBId = 1,
                    UniqueId = x.Id,
                    Location = x.Location,
                    PositionX = x.PositionX,
                    PositionY = x.PositionY,
                    Rotation = x.Rotation,
                    ItemDeploySequence = index + 4,
                    StackCount = 1
                };
            }).ToList();
            var combinedFurnitures = cafeFurnitures.Concat(secondCafeFurnitures).ToList();
            context.Furnitures.AddRange(combinedFurnitures);
            context.SaveChanges();*/

            foreach (var cafeDB in account.Cafes)
            {
                cafeDB.FurnitureDBs.Clear();
                /*var furnitures = account.Furnitures
                    .Where(x => x.CafeDBId == cafeDB.CafeDBId)
                    .Select(x => {
                        return new FurnitureDB()
                        {
                            CafeDBId = x.CafeDBId,
                            UniqueId = x.UniqueId,
                            Location = x.Location,
                            PositionX = x.PositionX,
                            PositionY = x.PositionY,
                            Rotation = x.Rotation,
                            StackCount = x.StackCount,
                            ItemDeploySequence = x.ItemDeploySequence
                        };
                    }).ToList();
                cafeDB.FurnitureDBs.AddRange(furnitures);*/
            };
            connection.Context.SaveChanges();
            connection.SendChatMessage("Removed all furnitures!");
        }

        public static CharacterDB CreateMaxCharacterFromId(uint characterId)
        {
            return new CharacterDB()
            {
                UniqueId = characterId,
                StarGrade = 5,
                Level = 90,
                Exp = 0,
                PublicSkillLevel = 10,
                ExSkillLevel = 5,
                PassiveSkillLevel = 10,
                ExtraPassiveSkillLevel = 10,
                LeaderSkillLevel = 1,
                FavorRank = 100,
                IsNew = true,
                IsLocked = true,
                PotentialStats = { { 1, 25 }, { 2, 25 }, { 3, 25 } },
                EquipmentServerIds = [0, 0, 0]
            };
        }
    }
}
