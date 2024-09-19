using Microsoft.AspNetCore.Http.Connections;
using Plana.Database;
using Plana.Database.ModelExtensions;
using Plana.FlatData;
using Plana.Utils;
using Phrenapates.Controllers.Api.ProtocolHandlers;
using Phrenapates.Services;
using Phrenapates.Services.Irc;

namespace Phrenapates.Commands
{
    [CommandHandler("character", "Command to interact with user's characters", "/character <add|clear> [all|characterId]")]
    internal class CharacterCommand : Command
    {
        public CharacterCommand(IrcConnection connection, string[] args, bool validate = true) : base(connection, args, validate) { }

        [Argument(0, @"^add$|^clear$", "The operation selected (add, clear)", ArgumentFlags.IgnoreCase)]
        public string Op { get; set; } = string.Empty;

        [Argument(1, @"^[0-9]+$|^all$", "The target character, value is item id or 'all'", ArgumentFlags.Optional)]
        public string Target { get; set; } = string.Empty;

        public override void Execute()
        {
            var characterDB = connection.Context.Characters;

            switch (Op.ToLower())
            {
                case "add":
                    if (Target == "all")
                    {
                        InventoryUtils.AddAllCharacters(connection);

                        connection.SendChatMessage("All Characters Added!");
                    }
                    else if (uint.TryParse(Target, out uint characterId))
                    {
                        var newChar = InventoryUtils.CreateMaxCharacterFromId(characterId);
                        
                        if (characterDB.Any(x => x.UniqueId == newChar.UniqueId))
                        {
                            connection.SendChatMessage($"{newChar.UniqueId} already exists!");
                            return;
                        }

                        connection.Account.AddCharacters(connection.Context, [newChar]);
                        connection.SendChatMessage($"{newChar.UniqueId} added!");
                    }
                    else
                    {
                        throw new ArgumentException("Invalid Target / Amount!");
                    }

                    break;
                
                case "clear":
                    InventoryUtils.RemoveAllCharacters(connection);

                    connection.SendChatMessage($"Removed all characters!");
                    break;
                
                default:
                    connection.SendChatMessage($"Usage: /character unlock=<all|clear|characterId>");
                    throw new InvalidOperationException("Invalid operation!");
            }

            connection.Context.SaveChanges();
        }
    }
}
