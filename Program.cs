using Discord;
using Discord.Interactions;
using Discord.Webhook;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace amblflecasm
{
	internal class Program
	{
		public static bool DEBUGMODE = true;

		public static Task Main() => new Program().MainAsync();

		private IServiceProvider services;

		private DiscordSocketConfig config;
		private DiscordSocketClient client;
		public static InteractionService interactionService; // Having to make some things static is retarded!!

		private static dynamic configData;
		private Dictionary<ulong, string> tjelcLookups;

		//

		public static bool IsUserLeme(SocketUser socketUser)
		{
			try
			{
				JArray jlemes = (JArray)Program.GetConfigData("lemes");

				foreach (ulong id in jlemes.ToObject<List<ulong>>())
					if (socketUser.Id == id) return true;

				return false;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public static DateTime EpochToDateTime(dynamic etime)
		{
			return DateTimeOffset.FromUnixTimeSeconds((long)etime).DateTime;
		}

		//

		public Task DeserializeConfig()
		{
			try
			{
				using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("amblflecasm.Data.Config.json"))
					using (StreamReader sr = new StreamReader(s))
						configData = JsonConvert.DeserializeObject(sr.ReadToEnd());
			}
			catch (Exception) { }

			return Task.CompletedTask;
		}

#nullable enable // Fucking stupid

		public static bool ObjectIsJArray(object? o)
		{
			if (o == null) return false;

			return o.GetType().IsAssignableFrom(typeof(JArray));
		}

		public static object? GetConfigData(params string[] keys)
		{
			object? obj = configData[keys[0]];
			int nextIndex = 1;

			while (ObjectIsJArray(obj) && nextIndex < keys.Length)
			{
				foreach (JObject jobj in (JArray)obj)
					foreach (JProperty p in jobj.Properties())
						if (p.Name.Equals(keys[nextIndex]))
						{
							obj = p.Value;
							break;
						}

				nextIndex++;
			}

			return obj;
		}

#nullable disable

		// Client tasks

		public Task ClientLog(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}

		public async Task ClientMessageReceivedHandler(SocketMessage message)
		{
			SocketUserMessage userMessage = message as SocketUserMessage;

			if (userMessage == null) return; // Wtf
			if (userMessage.Author.IsBot) return;

			if (tjelcLookups.ContainsKey(userMessage.Channel.Id))
			{
				string content = string.Empty;

				if (!userMessage.Content.Equals(string.Empty))
					content = userMessage.Content;
				else
					foreach (Attachment a in userMessage.Attachments)
						content = content + a.Url + "\n";

				if (content.Equals(string.Empty)) return;

				DiscordWebhookClient thiswh = new DiscordWebhookClient(tjelcLookups[userMessage.Channel.Id]);

				await thiswh.SendMessageAsync(content, false, userMessage.Embeds, userMessage.Author.Username, userMessage.Author.GetAvatarUrl(), null, new AllowedMentions(AllowedMentionTypes.None));
				await userMessage.DeleteAsync();

				thiswh.Dispose();
			}
		}

		public async Task ClientUserJoinedHandler(SocketUser user)
		{
			int? userflags = (int?)user.PublicFlags;

			if (userflags == 1) // 1 << 0
			{
				await(user as IGuildUser).BanAsync(0, "Discord Staff Member", null);
				return;
			}

			if (userflags == 2) // 1 << 1
			{
				await(user as IGuildUser).BanAsync(0, "Discord Partner", null);
				return;
			}
		}

		public Task ClientReadyHandler()
		{
			// Register Commands

			if (DEBUGMODE)
				interactionService.RegisterCommandsToGuildAsync(ulong.Parse(GetConfigData("leland").ToString()), true);
			else
				interactionService.RegisterCommandsGloballyAsync();

			return Task.CompletedTask;
		}

		public async Task ClientInteractionCreatedHandler(SocketInteraction interaction)
		{
			try
			{
				await interactionService.ExecuteCommandAsync(new SocketInteractionContext(client, interaction), services);
			}
			catch (Exception) { }
		}

		public async Task MainAsync()
		{
			// Initialize Config

			await DeserializeConfig();

			if (configData == null)
			{
				Console.WriteLine("FALED TO READ CONFIG DATA");
				Console.ReadLine();
				Environment.Exit(-1);
			}

			tjelcLookups = new Dictionary<ulong, string>();

			foreach (JObject obj in (JArray)GetConfigData("TJELC"))
				foreach (JProperty p in obj.Properties())
					tjelcLookups.Add(ulong.Parse(p.Name), p.Value.ToString());

			// Setup Bot Config

			config = new DiscordSocketConfig
			{
				GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.GuildMessages,
				AlwaysDownloadUsers = true
			};

			// Setup client and services

			client = new DiscordSocketClient(config);

			interactionService = new InteractionService(client);

			services = new ServiceCollection()
				.AddSingleton(client)
				.AddSingleton(interactionService)
				.BuildServiceProvider();

			client.Log += ClientLog;
			client.MessageReceived += ClientMessageReceivedHandler;
			client.UserJoined += ClientUserJoinedHandler;
			client.Ready += ClientReadyHandler;
			client.InteractionCreated += ClientInteractionCreatedHandler;

			// Initialize Commands

			await interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), services);

			// Login

			await client.LoginAsync(TokenType.Bot, GetConfigData("Tokens", "Bot").ToString());
			await client.StartAsync();

			await Task.Delay(Timeout.Infinite);
		}
	}
}
