using Plana.Database.ModelExtensions;
using Phrenapates.Utils;
using Phrenapates.Services.Irc;

namespace Phrenapates.Commands
{
    [CommandHandler("character", "Command to interact with user's characters", "/character <add|remove> [all|characterId] [basic|ue30|ue50|max]")]
    internal class CharacterCommand : Command
    {
        public CharacterCommand(IrcConnection connection, string[] args, bool validate = true) : base(connection, args, validate) { }

        [Argument(0, @"^add$|^remove$", "The operation selected (add, remove, clear)", ArgumentFlags.IgnoreCase)]
        public string Op { get; set; } = string.Empty;

        [Argument(1, @"^[0-9]+$|^all$", "The target character, value is item id or 'all'", ArgumentFlags.Optional)]
        public string Target { get; set; } = string.Empty;

        [Argument(2, @"^basic|^ue30$|^ue50$|^max$", "The options selected (basic, ue30, ue50, max)", ArgumentFlags.Optional)]
        public string Options { get; set; } = string.Empty;

        public override void Execute()
        {
            var characterDB = connection.Context.Characters;
            var options = Options.ToLower();
            List<string> optionList = ["basic", "ue30", "ue50", "max"];

            if (!optionList.Contains(options) && options.Length > 0 && !Op.ToLower().Equals("add"))
            {
                connection.SendChatMessage("Unknown options!");
                connection.SendChatMessage("Usage: /character add 10000 basic");
                return;
            }

            switch (Op.ToLower())
            {
                case "add":
                    if (Target == "all")
                    {
                        InventoryUtils.AddAllCharacters(connection, options);

                        connection.SendChatMessage("All Characters Added!");
                    }
                    else if (uint.TryParse(Target, out uint characterId))
                    {
                        
                        if (characterDB.Any(x => x.UniqueId == characterId))
                        {
                            connection.SendChatMessage($"{characterId} already exists!");
                            return;
                        }
                        
                        CharacterUtils.AddCharacter(connection, characterId, options);
                        connection.SendChatMessage($"{characterId} added!");
                    }
                    else
                    {
                        throw new ArgumentException("Invalid Target / Amount!");
                    }

                    break;
                case "remove":
                    if (Target == "all")
                    {
                        InventoryUtils.RemoveAllCharacters(connection);

                        connection.SendChatMessage("All Characters Removed!");
                    }
                    else if (uint.TryParse(Target, out uint characterId))
                    {
                        CharacterUtils.RemoveCharacter(connection, characterId);
                    }
                    else
                    {
                        throw new ArgumentException("Invalid Target / Amount!");
                    }
                    break;
                default:
                    connection.SendChatMessage($"Usage: /character <add|remove> add|remove=[characterId] options=[basic|ue30|ue50|max]");
                    throw new InvalidOperationException("Invalid operation!");
            }

            connection.Context.SaveChanges();
        }
    }
}
