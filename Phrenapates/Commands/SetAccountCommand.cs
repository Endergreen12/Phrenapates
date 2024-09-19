using Plana.Database;
using Phrenapates.Services.Irc;
using System.ComponentModel;
using System.Reflection;
using Phrenapates.Utils;

namespace Phrenapates.Commands
{
    [CommandHandler("setaccount", "Command to change player's account data (case-sensitive)", "/setaccount <|Level|Nickname|RaidSeasonId|Property|...> <Value>")]
    internal class SetAccountCommand : Command
    {
        public SetAccountCommand(IrcConnection connection, string[] args, bool validate = true) : base(connection, args, validate) { }

        [Argument(0, @"^[a-zA-Z]+$", "The Account Property you want to change. (case-sensitive)", ArgumentFlags.IgnoreCase)]
        public string Property { get; set; } = string.Empty;

        [Argument(1, @"", "The value you want to change it to, must match the property type.", ArgumentFlags.IgnoreCase)]
        public string Value { get; set; } = string.Empty;

        public override void Execute()
        {
            PropertyInfo? targetProperty = typeof(AccountDB).GetProperty(Property) ?? typeof(AccountDB).GetProperty(Property.Capitalize());

            if (targetProperty != null)
            {
                TypeConverter converter = TypeDescriptor.GetConverter(targetProperty.PropertyType);

                if (converter != null && converter.CanConvertFrom(typeof(string)))
                {
                    try
                    {
                        object targetValue = converter.ConvertFromString(Value);

                        targetProperty.SetValue(connection.Account, targetValue);
                        connection.Context.SaveChanges();

                        connection.SendChatMessage($"Set Player with UID {connection.AccountServerId}'s {Property} to {Value}");
                    } catch (Exception)
                    {
                        throw new ArgumentException("Invalid Value");
                    }
                }
            } 
            else
            {
                throw new ArgumentException("Invalid Player Property!");
            }
        }
    }
}
