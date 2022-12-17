using Discord;
using Discord.Interactions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI_API;
using System;
using System.Collections.Generic;
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

		public enum imageSizes
		{
			Small,
			Medium,
			Large
		}

		public enum imageCount
		{
			One,
			Two,
			Three,
			Four
		}

		private int GetImageSize(imageSizes size)
		{
			switch (size)
			{
				case imageSizes.Small:
					return 256;
				case imageSizes.Medium:
					return 512;
				case imageSizes.Large:
					return 1024;
				default:
					return 256;
			}
		}

		private int GetImageCount(imageCount count)
		{
			switch (count)
			{
				case imageCount.One:
					return 1;
				case imageCount.Two:
					return 2;
				case imageCount.Three:
					return 3;
				case imageCount.Four:
					return 4;
				default:
					return 1;
			}
		}

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
					Text = string.Format("OpenAI completion for '{0}'", query)
				};
			}
			catch (Exception)
			{
				embedBuilder.Description = "Failed to generate";
			}

			await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
		}

		[SlashCommand("images", "Image generation", false, RunMode.Async)]
		public async Task Images(string query, imageSizes size = imageSizes.Medium/*, imageCount count = imageCount.One*/)
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

			int isize = GetImageSize(size);
			int icount = GetImageCount(imageCount.One); // TODO: Restore this one day

			try
			{
				HttpClient client = new HttpClient();
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Program.GetConfigData("Tokens", "OpenAI").ToString());

				Dictionary<string, object> data = new Dictionary<string, object>()
				{
					{ "prompt", query },
					{ "size", isize + "x" + isize },
					{ "n", icount }
				};

				StringContent content = new StringContent(JsonConvert.SerializeObject(data).ToString(), Encoding.UTF8, "application/json");
				HttpResponseMessage response = await client.PostAsync("https://api.openai.com/v1/images/generations", content);

				string imageData = string.Empty;
				imageData = await response.Content.ReadAsStringAsync();

				if (!imageData.Equals(string.Empty)) // Generate the embeds
				{
					dynamic imageResponse = JsonConvert.DeserializeObject(imageData);

					List<EmbedBuilder> embeds = new List<EmbedBuilder>();

					if (Program.ObjectIsJArray(imageResponse.data))
						foreach (JObject jobject in (JArray)imageResponse.data)
							foreach (JProperty jproperty in jobject.Properties())
								embeds.Add(new EmbedBuilder().WithUrl("https://openai.com/").WithImageUrl(jproperty.Value?.ToString() ?? null));

					if (embeds[0].ImageUrl == null) // Likely blocked since no image was set here
					{
						embedBuilder.Description = "Failed to generate (Most likely blocked)";
						goto OUTPUT;
					}

					embeds[0].Color = Color.Green; // Pretty it up
					embeds[0].Description = "";
					embeds[0].Footer = new EmbedFooterBuilder()
					{
						Text = string.Format("OpenAI images for '{0}'", query)
					};

					Embed[] embedsArray = new Embed[icount]; // Build the embeds into an array

					for (int i = 0; i < embeds.Count; i++)
						embedsArray[i] = embeds[i].Build();

					await this.ModifyOriginalResponseAsync(message => message.Embeds = embedsArray);

					return;
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
