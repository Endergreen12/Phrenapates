using Plana.Utils;
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

            switch (Op.ToLower())
            {
                case "addall":
                    InventoryUtils.AddAllCharacters(connection, Options);
                    InventoryUtils.AddAllWeapons(connection, Options);
                    InventoryUtils.AddAllEquipment(connection, Options);
                    InventoryUtils.AddAllItems(connection);
                    InventoryUtils.AddAllGears(connection, Options);
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
