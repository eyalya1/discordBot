using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using discordBot.config;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace discordBot
{


    internal class Program
    {

        private static Dictionary<string, Func<MessageCreatedEventArgs, string[], bool?, Task>> mapFunc =
            new Dictionary<string, Func<MessageCreatedEventArgs, string[], bool?, Task>>();
        // private Func<string,DSharpPlus.EventArgs.MessageCreatedEventArgs ,string[],bool> remind = (e,commandArgs,all) => { remind();};
        private static DiscordClient Client { get; set; }
        private static CommandsNextExtension commands { get; set; }
        private static string prefix;

        static async Task Main(string[] args)
        {
            if (mapFunc.Count == 0)
            {
                mapFunc.Add("remind", remind);
                mapFunc.Add("ping", ping);
                mapFunc.Add("echo", echo);
                mapFunc.Add("remindall", remindall);
            }

            var jsonReader = new JSONReader();

            await jsonReader.ReadJSON();
            prefix = jsonReader.prefix;

            DiscordClientBuilder builder = DiscordClientBuilder.CreateDefault(jsonReader.token,
                DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents);

            builder.ConfigureEventHandlers
            (
                b => b.HandleMessageCreated(async (s, e) =>
                {
                    if (e.Message.Content.ToLower().StartsWith(prefix))
                    {
                        var command = e.Message.Content.Substring(prefix.Length).ToLower().Split(" ");
                        if (command.Length < 2)
                        {
                            await e.Message.RespondAsync("Invalid command");
                            return;
                        }

                        var commandAction = e.Message.Content.Substring(prefix.Length).ToLower()
                            .Split(" ")[1];
                        Console.WriteLine(commandAction);
                        string[] commandArgs = command[2..];
                        string commandArgsString = string.Join(" ", commandArgs);
                        if (mapFunc.ContainsKey(commandAction))
                        {
                            mapFunc[commandAction](e, commandArgs,false);
                        }
                    }

                })
            );
        



        DiscordClient client = builder.Build();
            await client.ConnectAsync();
            await Task.Delay(-1);
        }

        public async static Task remindAll(MessageCreatedEventArgs e, string commandArgs)
        {
            string commandArgsString = string.Join(" ", commandArgs);
            // Console.WriteLine("got remind all");
            var server = e.Guild;
            IReadOnlyDictionary<ulong, DiscordMember> members = server.Members;
            foreach (var member in members.Values)
            {
                await member.SendMessageAsync("Reminder: " +commandArgsString);
            }
        }

        public async static Task Reminder(MessageCreatedEventArgs e, DateTime date, string message, bool all = false)
        {
            Console.WriteLine("got reminder");
            TimeSpan delay = date - DateTime.Now;
            Console.WriteLine(delay);
            if(TimeSpan.Zero>delay)
            {
                await e.Message.RespondAsync("Invalid date");
                return;
            }
            // Console.WriteLine("Reminder: " + message);
            await Task.Delay(delay);
            // Console.WriteLine("Reminder: " + message);

            // await Task.Delay((date - DateOnly.FromDateTime(DateTime.No
            if(all)
            {
                remindAll(e,message);
            }
            if(delay<TimeSpan.Zero)
            {
                await e.Message.RespondAsync("Invalid date");
            }
            else
            {
            await e.Message.RespondAsync("Reminder: " + message);
            }
        }
        static private Func<DSharpPlus.EventArgs.MessageCreatedEventArgs, string[], bool?, Task> remind =
            async (e, commandArgs, all) =>
            {
                if(all==null)
                {
                    all = false;
                }
                bool allFlag = (bool) all;
                Console.WriteLine("got remind");
                // Console.WriteLine(commandArgs.Length);
                if (commandArgs.Length < 3)
                {
                    await e.Message.RespondAsync(
                        $"Invalid use of remind command:{prefix} remind <deley> <hh:mm:ss> <reminder> \n {prefix} remind <date> <dd/mm/year hh:mm:ss> <reminder>");
                    ;
                    return;
                }

                bool flag = true;
                DateTime date;
                if (commandArgs[0].ToLower() == "delay")
                {
                    flag = TimeSpan.TryParse(commandArgs[1], out TimeSpan delay);
                    Console.WriteLine(delay);
                    date = DateTime.Now + delay;
                }
                else
                {
                    Console.WriteLine("got date");
                    string praseDate = commandArgs[1] + " " + commandArgs[2];

                    flag = DateTime.TryParse(praseDate, out date);
                }

                if (!flag)
                {
                    await e.Message.RespondAsync("Invalid date format");
                    return;
                }

                string commandArgsString = string.Join(" ", commandArgs);
                var reminder = string.Join(" ", commandArgs[2..]);
                // Console.WriteLine("reminder");
                Reminder(e:e, date:date,message:reminder,all:allFlag);
                // Console.WriteLine("Reminder set");
                await e.Message.RespondAsync($"Reminder {reminder} set to {date}");
                return;
            };
        static private Func<MessageCreatedEventArgs,string[]?,bool?,Task > ping = async (e,commandArgs,all) =>
        {
            await e.Message.RespondAsync("Pong!");
        };
        static private Func<MessageCreatedEventArgs,string[],bool?,Task> echo = async (e,commandArgs,all) =>
        {
            string commandArgsString = string.Join(" ", commandArgs);
            await e.Message.RespondAsync(commandArgsString);
        };
        static private Func<DSharpPlus.EventArgs.MessageCreatedEventArgs, string[], bool?, Task> remindall = 
            async (e, commandArgs, all) => { remind(e, commandArgs, true);};
    }
}
