using Plana.Database;
using Plana.FlatData;
using Plana.Utils;
using Phrenapates.Services;
using Phrenapates.Services.Irc;

namespace Phrenapates.Commands
{
    [CommandHandler("inventory", "Command to manage inventory (chars, weapons, equipment, items)", "/inventory <addall|addallmax|removeall>")]
    internal class InventoryCommand : Command
    {
        public InventoryCommand(IrcConnection connection, string[] args, bool validate = true) : base(connection, args, validate) { }

        [Argument(0, @"^addall|^addallmax$|^removeall$", "The operation selected (addall, addallmax, removeall)", ArgumentFlags.IgnoreCase)]
        public string Op { get; set; } = string.Empty;

        public override void Execute()
        {
            var context = connection.Context;

            switch (Op.ToLower())
            {
                case "addall":
                    InventoryUtils.AddAllCharacters(connection, false);
                    InventoryUtils.AddAllWeapons(connection, false);
                    InventoryUtils.AddAllEquipment(connection, false);
                    InventoryUtils.AddAllItems(connection);
                    InventoryUtils.AddAllGears(connection, false);
                    InventoryUtils.AddAllMemoryLobbies(connection);
                    InventoryUtils.AddAllScenarios(connection);

                    connection.SendChatMessage("Added Everything!");
                    break;

                case "addallmax":
                    InventoryUtils.AddAllCharacters(connection);
                    InventoryUtils.AddAllWeapons(connection);
                    InventoryUtils.AddAllEquipment(connection);
                    InventoryUtils.AddAllItems(connection);
                    InventoryUtils.AddAllGears(connection);
                    InventoryUtils.AddAllMemoryLobbies(connection);
                    InventoryUtils.AddAllScenarios(connection);

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

                    connection.SendChatMessage("Removed Everything!");
                    break;
            }

            context.SaveChanges();
        }
    }
}
