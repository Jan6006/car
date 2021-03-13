﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RevoltApi;
using RevoltBot.Attributes;

namespace RevoltBot
{
    public static class CommandHandler
    {
        public static List<CommandInfo> Commands = new();
        public static List<ModuleInfo> ModuleInfos = new();

        public static void LoadCommands()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                if (type.IsInterface)
                    continue;
                // https://stackoverflow.com/questions/4963160/how-to-determine-if-a-type-implements-an-interface-with-c-sharp-reflection
                if (typeof(ModuleBase).IsAssignableFrom(type))
                {
                    if(type == typeof(ModuleBase))
                        continue;
                    var module = new ModuleInfo()
                    {
                        Type = type,
                        Summary = type.GetCustomAttribute<SummaryAttribute>()?.Text,
                        Names = type.GetCustomAttribute<ModuleNameAttribute>()
                    };
                    ModuleInfos.Add(module);
                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                    {
                        var att = method.CustomAttributes.FirstOrDefault(
                            a => a.AttributeType.Name == "CommandAttribute");
                        if (att == null)
                            continue;
                        // todo: just use .GetCustomAttribute retard?????
                        var aliases = new List<string>();
                        foreach (var arg in att.ConstructorArguments)
                        {
                            var val = arg.Value as IReadOnlyCollection<CustomAttributeTypedArgument>;
                            foreach (var h in val)
                            {
                                aliases.Add((string) h.Value);
                            }
                        }

                        var command = new CommandInfo
                        {
                            Aliases = aliases.ToArray(), AttributeType = att.AttributeType, Method = method,
                            BarePreconditions = method.GetCustomAttributes<BarePreconditionAttribute>(true).ToArray(),
                            Summary = method.GetCustomAttribute<SummaryAttribute>()?.Text
                        };
                        Commands.Add(command);
                        module.Commands.Add(command);
                    }
                }
            }
        }

        public static async Task ExecuteCommandAsync(Message message, int prefixLength)
        {
            var relevant = message.Content.Remove(0, prefixLength);
            // get command
            var commands = Commands.Where(c => c.Aliases.Any(alias => relevant.ToLower() == alias.ToLower()))
                .Concat(Commands.Where(c => c.Aliases.Any(alias => relevant.ToLower().StartsWith(alias.ToLower()))));
            CommandInfo command = null;
            int longest = 0;
            foreach (var cmd in commands)
            {
                foreach (var cmdAlias in cmd.Aliases.Where(alias => relevant.ToLower().StartsWith(alias.ToLower())))
                {
                    if (longest < cmdAlias.Length)
                    {
                        longest = cmdAlias.Length;
                        command = cmd;
                    }
                }
            }

            if (command == null)
                throw new Exception("COMMAND_NOT_FOUND");
            var alias = command.Aliases.First(alias => relevant.ToLower().StartsWith(alias.ToLower()));
            var args = relevant.Remove(0, alias.Length + (alias.Length == relevant.Length ? 0 : 1));
            var passedPreconditions = true;
            foreach (var precondition in command.BarePreconditions)
            {
                if (!await precondition.Evaluate(message))
                {
                    passedPreconditions = false;
                }
            }

            if (passedPreconditions)
                await command.ExecuteAsync(message, args);
        }
    }
}