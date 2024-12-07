using Plana.FlatData;
using Plana.MX.GameLogic.DBModel;

namespace Phrenapates.Services
{
    public class ArenaService
    {
        public static ArenaTeamSettingDB DummyTeamFormation = new()
        {
            EchelonType = EchelonType.ArenaDefence,
            LeaderCharacterId = 10065,
            MainCharacters =
            [
                new ArenaCharacterDB()
                {
                    UniqueId = 10065,
                    StarGrade = 3,
                    Level = 90,
                    PublicSkillLevel = 1,
                    ExSkillLevel = 1,
                    PassiveSkillLevel = 1,
                    ExtraPassiveSkillLevel = 1,
                    LeaderSkillLevel = 1
                }
            ],
            MapId = 1001,
        };

        public static List<ArenaUserDB> DummyOpponent(ArenaTeamSettingDB? team)
        {
            return
            [
                new ArenaUserDB()
                {
                    RepresentCharacterUniqueId = 20024,
                    NickName = "Your",
                    Rank = 2,
                    Level = 90,
                    TeamSettingDB = team ?? DummyTeamFormation
                },
                new ArenaUserDB()
                {
                    RepresentCharacterUniqueId = 10059,
                    NickName = "Defense",
                    Rank = 3,
                    Level = 90,
                    TeamSettingDB = team ?? DummyTeamFormation
                },
                new ArenaUserDB()
                {
                    RepresentCharacterUniqueId = 10065,
                    NickName = "Team",
                    Rank = 4,
                    Level = 90,
                    TeamSettingDB = team ?? DummyTeamFormation
                }
            ];
        }
    }
}