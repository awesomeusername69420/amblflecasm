using System;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord.Interactions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Discord.Webhook;

namespace amblflecasm
{
	internal class Program
	{
		public static Task Main() => new Program().MainAsync();

		public static bool DEBUG_MODE = false;

		private IServiceProvider serviceProvider;
		private DiscordSocketConfig socketConfig;
		public static DiscordSocketClient socketClient;
		public static InteractionService interactionService;
		private static dynamic configData = null; // I don't like this being a dynamic but anything else throws errors

		public static Random rng;
		private Dictionary<ulong, string> webhookLookups = null;
		private Dictionary<int, string> userFlagLookups;
		private static List<string> denials;
		private static string randomChars;

		/*
		 * Config Setup
		 */

		public Task DeserializeConfig()
		{
			try
			{
				using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("amblflecasm.Data.Config.json"))
					using (StreamReader sr = new StreamReader(s))
						configData = JsonConvert.DeserializeObject(sr.ReadToEnd());

				return Task.CompletedTask;
			} catch (Exception)
			{
				return Task.FromResult(false);
			}
		}

#nullable enable // Dumb as hell
		public static bool ObjectIsJArray(object? obj)
		{
			if (obj == null) return false;
			return obj.GetType().IsAssignableFrom(typeof(JArray));
		}

		public static object? GetConfigData(params string[] keys)
		{
			object? obj = configData[keys[0]];
			int nextIndex = 1;

			while (ObjectIsJArray(obj) && nextIndex < keys.Length)
			{
				foreach (JObject jobject in (JArray)obj)
					foreach (JProperty jproperty in jobject.Properties())
						if (jproperty.Name.Equals(keys[nextIndex]))
						{
							obj = jproperty.Value;
							break;
						}

				nextIndex++;
			}

			return obj;
		}
#nullable disable

		/*
		 * Globals
		 */

		public static int Clamp(int x, int min = int.MinValue, int max = int.MaxValue)
		{
			if (x < min) return min;
			if (x > max) return max;
			return x;
		}

		public static double Clamp(double x, double min = double.MinValue, double max = double.MaxValue)
		{
			if (x < min) return min;
			if (x > max) return max;
			return x;
		}

		public static int FloorMod(object a, object b) // Regular modulo doesn't like negative numbers
		{
			double num1 = 1;
			double num2 = 1;

			double.TryParse(a.ToString(), out num1);
			double.TryParse(b.ToString(), out num2);

			return (int)(num1 - num2 * Math.Floor(num1 / num2));
		}

		public static string AutoPlural(string message, int x)
		{
			return x == 1 ? message : message + "s";
		}

		public static string GetRandomDenial()
		{
			return denials[rng.Next(denials.Count)];
		}

		public static string GetRandomString(int length = 10)
		{
			string s = string.Empty;

			if (length < 1)
				return s;

			for (int i = 1; i <= length; i++)
				s = s + randomChars[rng.Next(randomChars.Length)];

			return s;
		}

		public static bool IsUser(SocketUser socketUser, string userType)
		{
			if (socketUser == null) return false;

			try
			{
				JArray lemes = (JArray)GetConfigData("Users", userType);
				List<ulong> ulemes = lemes.ToObject<List<ulong>>();

				foreach (ulong id in ulemes)
					if (socketUser.Id == id) return true;

				return false;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public Task UpdateWebhooks()
		{
			if (webhookLookups == null)
				webhookLookups = new Dictionary<ulong, string>();
			else
				webhookLookups.Clear();

			JArray webhooks = (JArray)GetConfigData("Webhooks");
			foreach (JObject jobject in webhooks)
				foreach (JProperty jproperty in jobject.Properties())
				{
					ulong ID;

					if (!ulong.TryParse(jproperty.Name, out ID))
					{
						Log("Failed to register webhook '" + jproperty.Name + "'", "UpdateWebhooks", LogSeverity.Error);
						continue;
					}

					webhookLookups.Add(ID, jproperty.Value.ToString());
				}

			return Task.CompletedTask;
		}

		/*
		 * Client Tasks
		 */

		public Task ClientLogHandler(LogMessage message)
		{
			Console.WriteLine(message.ToString());
			return Task.CompletedTask;
		}

		public void Log(string message, string source = "Unknown", LogSeverity severity = LogSeverity.Info) // Quick and easy way to log something
		{
			ClientLogHandler(new LogMessage(severity, source, message));
		}

		public Task ClientReadyHandler()
		{
			if (DEBUG_MODE)
				Log("Registering commands to le land", "ClientReadyHandler");
			else
				Log("Registering commands globally", "ClientReadyHandler");

			try
			{
				if (DEBUG_MODE) // Stupid 24 hour global cache cooldown trash
				{
					string stringID = GetConfigData("Servers", "le land").ToString();
					ulong ulongID = ulong.Parse(stringID);

					interactionService.RegisterCommandsToGuildAsync(ulongID, true);
				}
				else
					interactionService.RegisterCommandsGloballyAsync(true);

				Log("Registered commands", "ClientReadyHandler");
				return Task.CompletedTask;
			} catch (Exception)
			{
				Log("Failed to register commands", "ClientReadyHandler", LogSeverity.Critical);
				return Task.FromResult(false);
			}
		}

		public async Task ClientInteractionHandler(SocketInteraction interaction)
		{
			try
			{
				IResult result = await interactionService.ExecuteCommandAsync(new SocketInteractionContext(socketClient, interaction), serviceProvider);

				if (!result.IsSuccess)
				{
					try
					{
						await interaction.RespondAsync("Something broke. Dumbfuck leme");
					}
					catch (Exception) { }

					Log(result.ErrorReason, "ClientInteractionHandler", LogSeverity.Error);
				}
			} catch (Exception ex)
			{
				try
				{
					await interaction.RespondAsync("Something seriously broke. Dumbfuck leme");
				}
				catch (Exception) { }

				Log(ex.ToString(), "ClientInteractionHandler", LogSeverity.Error);
			}
		}

		public async Task ClientMessageReceivedHandler(SocketMessage message)
		{
			try
			{
				SocketUserMessage socketUserMessage = message as SocketUserMessage;
				if (socketUserMessage == null || socketUserMessage.Author.IsBot) return;

				if (webhookLookups.ContainsKey(socketUserMessage.Channel.Id))
				{
					string content = socketUserMessage.Content;
					if (content.Equals(string.Empty) && socketUserMessage.Attachments.Count < 1 && socketUserMessage.Embeds.Count < 1) return; // If no text and no files then seethe and mald

					DiscordWebhookClient webhookClient = new DiscordWebhookClient(webhookLookups[socketUserMessage.Channel.Id]);

					if (!content.Equals(string.Empty))
						await webhookClient.SendMessageAsync(content, false, socketUserMessage.Embeds, socketUserMessage.Author.Username, socketUserMessage.Author.GetAvatarUrl(), null, AllowedMentions.None);

					if (socketUserMessage.Attachments.Count > 0) // Send another message containing attachment urls
					{
						string attachmentMessage = string.Empty;

						foreach (Attachment attachment in socketUserMessage.Attachments)
							attachmentMessage = attachmentMessage + attachment.Url + "\n";

						await webhookClient.SendMessageAsync(attachmentMessage, false, null, socketUserMessage.Author.Username, socketUserMessage.Author.GetAvatarUrl(), null, AllowedMentions.None);
					}

					await socketUserMessage.DeleteAsync();

					webhookClient.Dispose();
				}
			}
			catch (Exception) { }
		}

		public async Task ClientUserJoinedHandler(SocketUser user) // Disallow the officials, we don't run with the law
		{
			try
			{
				IGuildUser guildUser = user as IGuildUser;
				if (guildUser == null) return;

				int? nullableUserFlags = (int?)user.PublicFlags;

				if (nullableUserFlags.HasValue)
				{
					int userFlags = nullableUserFlags.Value;

					foreach (KeyValuePair<int, string> kvp in userFlagLookups)
						if ((userFlags & kvp.Key) != 0)
						{
							await guildUser.BanAsync(0, kvp.Value);
							break;
						}
				}
			}
			catch (Exception) { }
		}

		public async Task MainAsync()
		{
			await DeserializeConfig();

			if (configData == null)
			{
				Log("Failed to deserialize config", "MainAsync", LogSeverity.Critical);
				return;
			}

			rng = new Random();

			await UpdateWebhooks();

			userFlagLookups = new Dictionary<int, string>() // Access is denied
			{
				{ 0x1, "Discord Staff Member" },
				{ 0x2, "Discord Partner" },
				{ 0x1000, "Discord System" },
				{ 0x40000, "Discord Verified Moderator" }
			};

			denials = new List<string>()
			{
				"No",
				"Nuh uh",
				"No thanks",
				"Access is denied",
				"Can't make me",
				"I will do no such thing",
				"Nada",
				"Nah",
				"Nop"
			};

			randomChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890"; // Lazy

			socketConfig = new DiscordSocketConfig
			{
				GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers | GatewayIntents.GuildMessages | GatewayIntents.MessageContent, // Why do you need so many this gateway intent shit is retarded
				AlwaysDownloadUsers = true
			};

			socketClient = new DiscordSocketClient(socketConfig);
			interactionService = new InteractionService(socketClient);
			serviceProvider = new ServiceCollection()
				.AddSingleton(socketClient)
				.AddSingleton(interactionService)
				.BuildServiceProvider();

			socketClient.Log += ClientLogHandler;
			socketClient.Ready += ClientReadyHandler;
			socketClient.InteractionCreated += ClientInteractionHandler;
			socketClient.MessageReceived += ClientMessageReceivedHandler;
			socketClient.UserJoined += ClientUserJoinedHandler;

			await interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), serviceProvider);

			await socketClient.LoginAsync(TokenType.Bot, GetConfigData("Tokens", "Bot").ToString());
			await socketClient.StartAsync();

			await Task.Delay(Timeout.Infinite);
		}
	}
}
