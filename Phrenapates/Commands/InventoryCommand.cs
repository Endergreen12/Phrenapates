using Phrenapates.Utils;
using Phrenapates.Services.Irc;

namespace Phrenapates.Commands
{
    [CommandHandler("inventory", "Command to manage inventory (chars, weapons, equipment, items)", "/inventory <addall|removeall> [basic|ue30|ue50|max]")]
    internal class InventoryCommand : Command
    {
        public InventoryCommand(IrcConnection connection, string[] args, bool validate = true) : base(connection, args, validate) { }

        [Argument(0, @"^addall|^removeall$", "The operation selected (addall, removeall)", ArgumentFlags.IgnoreCase)]
        public string Op { get; set; } = string.Empty;

        [Argument(1, @"^basic|^ue30$|^ue50$|^max$", "The options selected (basic, ue30, ue50, max)", ArgumentFlags.Optional)]
        public string Options { get; set; } = string.Empty;

        public override void Execute()
        {
            var context = connection.Context;
            var options = Options.ToLower();
            List<string> optionList = ["basic", "ue30", "ue50", "max"];

            if (!optionList.Contains(options) && options.Length > 0)
            {
                connection.SendChatMessage("Unknown options!");
                connection.SendChatMessage("Usage: /inventory addall ue50");
                return;
            }

            switch (Op.ToLower())
            {
                case "addall":
                    InventoryUtils.AddAllCharacters(connection, options);
                    InventoryUtils.AddAllWeapons(connection, options);
                    InventoryUtils.AddAllEquipment(connection, options);
                    InventoryUtils.AddAllItems(connection);
                    InventoryUtils.AddAllGears(connection, options);
                    InventoryUtils.AddAllMemoryLobbies(connection);
                    InventoryUtils.AddAllScenarios(connection);
                    InventoryUtils.AddAllFurnitures(connection);

                    connection.SendChatMessage("Added Everything!");
                    break;
                case "removeall":
                    InventoryUtils.RemoveAllCharacters(connection);
                    context.Weapons.RemoveRange(context.Weapons.Where(x => x.AccountServerId == connection.AccountServerId));
                    context.Equipment.RemoveRange(context.Equipment.Where(x => x.AccountServerId == connection.AccountServerId));
                    context.Items.RemoveRange(context.Items.Where(x => x.AccountServerId == connection.AccountServerId));
                    context.Gears.RemoveRange(context.Gears.Where(x => x.AccountServerId == connection.AccountServerId));
                    context.MemoryLobbies.RemoveRange(context.MemoryLobbies.Where(x => x.AccountServerId == connection.AccountServerId));
                    context.Scenarios.RemoveRange(context.Scenarios.Where(x => x.AccountServerId == connection.AccountServerId));
                    InventoryUtils.RemoveAllFurnitures(connection);

                    connection.SendChatMessage("Removed Everything!");
                    break;
            }

            context.SaveChanges();
        }
    }
}
