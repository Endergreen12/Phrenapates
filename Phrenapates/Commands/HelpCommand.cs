using Phrenapates.Services.Irc;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Phrenapates.Commands
{

    [CommandHandler("help", "Show this help.", "/help [command]")]
    internal class HelpCommand : Command
    {
        [Argument(0, @"^[a-zA-Z]+$", "The command to display the help message", ArgumentFlags.IgnoreCase | ArgumentFlags.Optional)]
        public string Command { get; set; } = string.Empty;

        public HelpCommand(IrcConnection connection, string[] args, bool validate = true) : base(connection, args, validate) { }

        public override void Execute()
        {   
            if (Command != string.Empty)
            {
                if (CommandFactory.commands.ContainsKey(Command))
                {
                    var cmdAtr = (CommandHandlerAttribute?)Attribute.GetCustomAttribute(CommandFactory.commands[Command], typeof(CommandHandlerAttribute));
                    Command? cmd = CommandFactory.CreateCommand(Command, connection, args, false);

                    if (cmd is not null)
                    {
                        connection.SendChatMessage($"{Command} - {cmdAtr.Hint} (Usage: {cmdAtr.Usage})");

                        List<PropertyInfo> argsProperties = cmd.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.GetCustomAttribute(typeof(ArgumentAttribute)) is not null).ToList();
                        
                        foreach (var argProp in argsProperties)
                        {
                            ArgumentAttribute attr = (ArgumentAttribute)argProp.GetCustomAttribute(typeof(ArgumentAttribute))!;
                            var arg = Regex.Replace(attr.Pattern.ToString(), @"[\^\$\+]", "");

                            connection.SendChatMessage($"<{arg}> - {attr.Description}");
                        }
                    }
                } else
                {
                    throw new ArgumentException("Invalid Argument.");
                }

                return;
            }

            foreach (var command in CommandFactory.commands.Keys)
            {
                var cmdAtr = (CommandHandlerAttribute?)Attribute.GetCustomAttribute(CommandFactory.commands[command], typeof(CommandHandlerAttribute));

                Command? cmd = CommandFactory.CreateCommand(command, connection, args, false);

                if (cmd is not null)
                {
                    connection.SendChatMessage($"{command} - {cmdAtr.Hint} (Usage: {cmdAtr.Usage})");
                }
            }
        }
    }

}
