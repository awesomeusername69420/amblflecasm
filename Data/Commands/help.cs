using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace amblflecasm.Data.Commands
{
	public class help : InteractionModuleBase<SocketInteractionContext>
	{
		[SlashCommand("help", "Help I'm retarded", false, RunMode.Async)]
		public async Task Help()
		{
			EmbedBuilder embedBuilder = new EmbedBuilder()
				.WithTitle("Working")
				.WithDescription("Building help text")
				.WithColor(Color.Red);

			await this.RespondAsync(null, new Embed[] { embedBuilder.Build() });

			embedBuilder.Title = "Finished";

			try // Maybe back this up somehow so it doesn't need regenerated every time?
			{
				List<SlashCommandInfo> commands = new List<SlashCommandInfo>(Program.interactionService.SlashCommands);
				Dictionary<string, SortedDictionary<string, string>> commandGroups = new Dictionary<string, SortedDictionary<string, string>>()
					{
						{ "Commands", new SortedDictionary<string, string>() }
					};

				foreach (SlashCommandInfo commandInfo in commands)
				{
					string moduleName = commandInfo.Module?.Name ?? "Unknown";

					if (moduleName.Equals(commandInfo.Name))
						moduleName = "Commands";

					if (!commandGroups.ContainsKey(moduleName))
						commandGroups[moduleName] = new SortedDictionary<string, string>();

					string moduleLookup = "`" + commandInfo.Name;

					foreach (PreconditionAttribute pa in commandInfo.Preconditions)
						if (pa.GetType() == typeof(RequireLemeAttribute))
						{
							moduleLookup = "*" + moduleLookup;
							break;
						}

					foreach (SlashCommandParameterInfo parameterInfo in commandInfo.Parameters)
						moduleLookup = moduleLookup + (parameterInfo.IsRequired ? " " : " o") + "(" + parameterInfo.Name + ")";

					moduleLookup = moduleLookup + "`";

					commandGroups[moduleName][moduleLookup] = commandInfo.Description;
				}

				foreach (KeyValuePair<string, SortedDictionary<string, string>> keyValuePair in commandGroups)
				{
					string thisSection = string.Empty;

					foreach (KeyValuePair<string, string> subValuePair in keyValuePair.Value)
						thisSection = thisSection + string.Format("{0}: {1}", subValuePair.Key, subValuePair.Value) + "\n";

					embedBuilder.AddField(keyValuePair.Key, thisSection);
				}

				embedBuilder.Color = Color.Green;
				embedBuilder.Description = "";
			}
			catch (Exception)
			{
				embedBuilder.Color = Color.Red;
				embedBuilder.Description = "Failed to build help text";
			}

			await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
		}
	}
}
