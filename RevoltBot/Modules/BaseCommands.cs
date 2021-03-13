﻿using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RevoltApi.Channels;
using RevoltBot.Attributes;

namespace RevoltBot.Modules
{
    [ModuleName("base", "basic", "core")]
    public class BaseCommands : ModuleBase
    {
        [Command("info")]
        public Task BotInformation()
            => ReplyAsync($@"> ## Taco
> **Developed by:** `owouwuvu` <@01EX40TVKYNV114H8Q8VWEGBWQ>
> **Latest update at:** {new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime.ToString("dd/MM/yyyy")}
> **Groups count:** {Message.Client.ChannelsCache.OfType<GroupChannel>().Count()}");

        [Command("help")]
        [Summary("HELP ME")]
        public async Task Help()
        {
            if (Args == "")
            {
                // main help
                var description = @"Oh hey im too lazy to write a proper help command!!1!!!111!
Here's a table of shit you and I have no idea what
| Module | Description |
|:------- |:------:|
";
                foreach (var module in CommandHandler.ModuleInfos)
                {
                    description += $"{module.Name} | {module.Summary ?? "no summary"}\n";
                }

                await ReplyAsync(description);
                return;
            }
            else
            {
                {
                    var module =
                        CommandHandler.ModuleInfos.FirstOrDefault(m =>
                            m.Names != null ? m.Names.AllNames.Any(a => a.ToLower() == Args.ToLower()) : false);
                    if (module == null)
                        goto after_module;
                    var response = @$"> ## {module.Name}
> **No. of commands:** {module.Commands.Count}
> ";
                    foreach (var command in module.Commands)
                        response += $"{command.Aliases.First()}, ";
                    response = response.Remove(response.Length - 2);
                    await ReplyAsync(response);
                    return;
                }
                after_module: ;
                {
                    var command =
                        CommandHandler.Commands.FirstOrDefault(c => c.Aliases.Any(a => a.ToLower() == Args.ToLower()));
                    if (command == null)
                        goto after_command;
                    var preconditions = "";
                    foreach (var precondition in command.BarePreconditions)
                        preconditions += $"{precondition.GetType().Name.Replace("Attribute", "")}, ";
                    preconditions = preconditions.Remove(preconditions.Length - 2);
                    await ReplyAsync($@"> ## {command.Aliases.First()}
> {command.Summary}" + (preconditions != "" ? "\n> **Preconditions:** " + preconditions : ""));
                    return;
                }
                after_command: ;
                // todo: special help pages
            }

            await ReplyAsync("no");
        }
    }
}