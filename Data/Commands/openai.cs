using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI_API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace amblflecasm.Data.Commands
{
	[Group("openai", "Open AI stuff")]
	public class openai : InteractionModuleBase<SocketInteractionContext>
	{
		OpenAIAPI api;

		private async Task<bool> BuildClient()
		{
			if (api == null)
			{
				try
				{
					api = new OpenAIAPI(Program.GetConfigData("Tokens", "OpenAI").ToString());
					return true;
				}
				catch (Exception)
				{
					return false;
				}
			}
			else
				return true;
		}

		[SlashCommand("completion", "Text completion", false, RunMode.Async)]
		public async Task Completion(string query, int tokens = 200, double temperature = 1.0, double pPenalty = 0.5, double fPenalty = 0.5)
		{
			EmbedBuilder embedBuilder = new EmbedBuilder()
				.WithTitle("Working")
				.WithDescription("")
				.WithColor(Color.Red);

			await this.RespondAsync(null, new Embed[] { embedBuilder.Build() });

			embedBuilder.Title = "Finished";

			if (!await this.BuildClient())
			{
				embedBuilder.Description = "Failed to build OpenAI client";
				await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
				return;
			}

			tokens = Program.Clamp(tokens, 0, 1500);
			temperature = Program.Clamp(temperature, 0.0, 1.0);
			pPenalty = Program.Clamp(pPenalty, 0.0, 1.0);
			fPenalty = Program.Clamp(fPenalty, 0.0, 1.0);

			try
			{
				CompletionResult result = await api.Completions.CreateCompletionAsync(query, tokens, temperature, presencePenalty: pPenalty, frequencyPenalty: fPenalty);

				embedBuilder.Color = Color.Green;
				embedBuilder.Description = "```" + result.ToString() + "```";

				embedBuilder.Footer = new EmbedFooterBuilder()
				{
					Text = string.Format("OpenAPI completion for '{0}'", query)
				};
			}
			catch (Exception)
			{
				embedBuilder.Description = "Failed to generate";
			}

			await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
		}

		[SlashCommand("images", "Image generation", false, RunMode.Async)]
		public async Task Images(string query, int size = 512)
		{
			EmbedBuilder embedBuilder = new EmbedBuilder()
				.WithTitle("Working")
				.WithDescription("")
				.WithColor(Color.Red);

			await this.RespondAsync(null, new Embed[] { embedBuilder.Build() });

			embedBuilder.Title = "Finished";

			if (!await this.BuildClient())
			{
				embedBuilder.Description = "Failed to build OpenAI client";
				await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
				return;
			}

			size = Program.Clamp(Program.CeilPower(size), 256, 1024);

			try
			{
				string imageData = string.Empty;

				HttpClient client = new HttpClient();
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Program.GetConfigData("Tokens", "OpenAI").ToString());

				Dictionary<string, string> data = new Dictionary<string, string>()
				{
					{ "prompt", query },
					{ "size", "512x512" }
				};

				StringContent content = new StringContent(JsonConvert.SerializeObject(data).ToString(), Encoding.UTF8, "application/json");
				HttpResponseMessage response = await client.PostAsync("https://api.openai.com/v1/images/generations", content);
				imageData = await response.Content.ReadAsStringAsync();

				if (!imageData.Equals(string.Empty))
				{
					dynamic imageResponse = JsonConvert.DeserializeObject(imageData);

					if (Program.ObjectIsJArray(imageResponse.data))
						foreach (JObject jobject in (JArray)imageResponse.data)
							foreach (JProperty jproperty in jobject.Properties())
								embedBuilder.ImageUrl = jproperty.Value.ToString();

					if (embedBuilder.ImageUrl == null) // Likely blocked
					{
						embedBuilder.Description = "Failed to generate (Most likely blocked)";
						goto OUTPUT;
					}
						
					embedBuilder.Color = Color.Green;
					embedBuilder.Description = "";

					embedBuilder.Footer = new EmbedFooterBuilder()
					{
						Text = string.Format("OpenAPI images for '{0}'", query)
					};
				}
				else
					throw new Exception("Nice images retard");
			}
			catch (Exception)
			{
				embedBuilder.Description = "Failed to generate";
			}

			OUTPUT: ;
			await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
		}
	}
}
