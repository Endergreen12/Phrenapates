﻿using Plana.MX.GameLogic.DBModel;
using Plana.Database.ModelExtensions;
using Plana.FlatData;
using Phrenapates.Services.Irc;
using System.Data;
using Microsoft.IdentityModel.Tokens;

namespace Phrenapates.Utils
{
    public static class InventoryUtils
    {
        public static void AddAllCharacters(IrcConnection connection, string addOption)
        {
            var account = connection.Account;
            var context = connection.Context;

            var characterExcel = connection.ExcelTableService.GetTable<CharacterExcelTable>().UnPack().DataList;
            var defaultCharacterExcel = connection.ExcelTableService.GetTable<DefaultCharacterExcelTable>().UnPack().DataList;
            var characterLevelExcel = connection.ExcelTableService.GetTable<CharacterLevelExcelTable>().UnPack().DataList;
            var favorLevelExcel = connection.ExcelTableService.GetExcelDB<FavorLevelExcel>();

            bool useOptions = false;
            int starGrade = 3;
            int favorRank = 1;
            bool breakLimit = false;

            if (!addOption.IsNullOrEmpty()) useOptions = true;

            switch(addOption)
            {
                case "basic":
                    starGrade = 3;
                    favorRank = 20;
                    break;
                case "ue30":
                    starGrade = 5;
                    favorRank = 20;
                    break;
                case "ue50":
                    starGrade = 5;
                    favorRank = 50;
                    break;
                case "max":
                    starGrade = 5;
                    favorRank = 100;
                    breakLimit = true;
                    break;
            }

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
                    StarGrade = useOptions ? starGrade : x.DefaultStarGrade,
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
            }).ToList();

            foreach (var character in account.Characters.Where(x => characterExcel.Any(y => y.Id == x.UniqueId)))
            {
                var updateCharacter = character;
                updateCharacter.StarGrade = useOptions ? starGrade : characterExcel.FirstOrDefault(y => y.Id == character.UniqueId).DefaultStarGrade;;
                updateCharacter.Level = useOptions ? characterLevelExcel.Count : 1;
                updateCharacter.Exp = 0;
                updateCharacter.ExSkillLevel = useOptions ? 5 : 1;
                updateCharacter.PublicSkillLevel = useOptions ? 10 : 1;
                updateCharacter.PassiveSkillLevel = useOptions ? 10 : 1;
                updateCharacter.ExtraPassiveSkillLevel = useOptions ? 10 : 1;
                updateCharacter.LeaderSkillLevel = 1;
                updateCharacter.FavorRank = useOptions ? favorRank : 1;
                updateCharacter.IsNew = true;
                updateCharacter.IsLocked = true;
                updateCharacter.PotentialStats = breakLimit ? 
                    new Dictionary<int, int> { { 1, 25 }, { 2, 25 }, { 3, 25 } } :
                    new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 } };
                connection.Context.Characters.Update(updateCharacter);
            }

            account.AddCharacters(context, [.. allCharacters]);
            context.SaveChanges();

            connection.SendChatMessage("Added all characters!");
        }

        public static void AddAllEquipment(IrcConnection connection, string addOption)
        {
            var equipmentExcel = connection.ExcelTableService.GetTable<EquipmentExcelTable>().UnPack().DataList;
            
            var useEquipment = false;
            switch(addOption)
            {
                case "basic":
                    useEquipment = true;
                    break;
                case "ue30":
                    useEquipment = true;
                    break;
                case "ue50":
                    useEquipment = true;
                    break;
                case "max":
                    useEquipment = true;
                    break;
            }


            if(!useEquipment)
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

        public static void AddAllWeapons(IrcConnection connection, string addOption)
        {
            var account = connection.Account;
            var context = connection.Context;

            int weaponLevel = 1;
            switch(addOption)
            {
                case "ue30":
                    weaponLevel = 30;
                    break;
                case "ue50":
                    weaponLevel = 50;
                    break;
                case "max":
                    weaponLevel = 50;
                    break;
            }

            if(weaponLevel == 1)
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
                    Level = weaponLevel
                };
            });

            account.AddWeapons(context, [.. allWeapons]);
            

            connection.SendChatMessage("Added all weapons!");
        }

        public static void AddAllGears(IrcConnection connection, string addOption)
        {
            var account = connection.Account;
            var context = connection.Context;

            bool useGear = false;
            switch(addOption)
            {
                case "basic":
                    useGear = true;
                    break;
                case "ue30":
                    useGear = true;
                    break;
                case "ue50":
                    useGear = true;
                    break;
                case "max":
                    useGear = true;
                    break;
            }

            if(!useGear)
            {
                context.Gears.RemoveRange(context.Gears.Where(x => x.AccountServerId == connection.AccountServerId));
                context.SaveChanges();
                return;
            }

            var uniqueGearExcel = connection.ExcelTableService.GetExcelDB<CharacterGearExcel>();

            var uniqueGear = uniqueGearExcel.Where(x => x.Tier == (useGear ? 2 : 1) && context.Characters.Any(y => y.UniqueId == x.CharacterId)).Select(x => 
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

            var memoryLobbyExcel = connection.ExcelTableService.GetExcelDB<MemoryLobbyExcel>();
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

            var scenarioModeExcel = connection.ExcelTableService.GetExcelDB<ScenarioModeExcel>();
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
    }
}
