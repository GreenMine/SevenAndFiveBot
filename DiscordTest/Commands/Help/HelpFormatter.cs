using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using static DSharpPlus.Entities.DiscordEmbedBuilder;

namespace SevenAndFiveBot.Commands.Help
{
    class HelpFormatter : BaseHelpFormatter
    {
        private DiscordEmbedBuilder Builder { get; }

        public HelpFormatter(CommandContext ctx) : base(ctx)
        {
            this.Builder = new DiscordEmbedBuilder() { Color = (DiscordColor)14540124, Author = new EmbedAuthor() { Name = "7.5 бот", Url = "https://discordapp.com", IconUrl = "https://a.d-cd.net/GMAAAgP2EOA-960.jpg" } };
        }

        // this method is called first, it sets the current command's name
        // if no command is currently being processed, it won't be called
        public BaseHelpFormatter WithCommandName(string name)
        {
/*            this.MessageBuilder.Append("Command: ")
                .AppendLine(name)
                .AppendLine();*/

            return this;
        }

        // this method is called second, it sets the current command's 
        // description. if no command is currently being processed, it 
        // won't be called
        public BaseHelpFormatter WithDescription(string description)
        {
/*            this.MessageBuilder.Append("Description: ")
                .AppendLine(description)
                .AppendLine();*/

            return this;
        }

        // this method is called third, it is used when currently 
        // processed group can be executed as a standalone command, 
        // otherwise not called
        public BaseHelpFormatter WithGroupExecutable()
        {
/*            this.MessageBuilder.AppendLine("This group is a standalone command.")
                .AppendLine();*/

            return this;
        }

        // this method is called fourth, it sets the current command's 
        // aliases. if no command is currently being processed, it won't
        // be called
        public BaseHelpFormatter WithAliases(IEnumerable<string> aliases)
        {
/*            this.MessageBuilder.Append("Aliases: ")
                .AppendLine(string.Join(", ", aliases))
                .AppendLine();*/

            return this;
        }

        // this method is called fifth, it sets the current command's 
        // arguments. if no command is currently being processed, it won't 
        // be called
        public BaseHelpFormatter WithArguments(IEnumerable<CommandArgument> arguments)
        {
            this.Builder.AddField("Arguments:", string.Join(", ", arguments.Select(xarg => $"{xarg.Name}")));
/*            this.MessageBuilder.Append("Arguments: ")
                .AppendLine(string.Join(", ", arguments.Select(xarg => $"{xarg.Name}")))
                .AppendLine();*/

            return this;
        }

        // this method is called sixth, it sets the current group's subcommands
        // if no group is being processed or current command is not a group, it 
        // won't be called
        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            this.Builder.Title = "Список команд";

            foreach (Command command in subcommands)
                this.Builder.AddField("!" + command.Name, command.Description, true);

            return this;
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            this.Builder.Title = "Описание команды";

            this.Builder.AddField("``" + command.Name + "``", command.Description);

            IReadOnlyList<CommandArgument> arguments = command.Overloads.First().Arguments;

            if (arguments.Count == 0)
                this.Builder.AddField("Аргументы отсутсвуют", ":thinking::thinking::thinking:");
            else
                this.Builder.AddField($"Аргументы команды:", string.Join('\n', arguments.Select(xarg => $"{xarg.Name} ({xarg.Name}): {xarg.Description}({(xarg.IsOptional ? "Необязательный" : "Обазательный")})")));
            return this;
        }


        public override CommandHelpMessage Build()
        {
            return new CommandHelpMessage(embed: this.Builder.Build());
        }
    }
}
