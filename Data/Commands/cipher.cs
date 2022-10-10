using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace amblflecasm.Data.Commands
{
	[Group("cipher", "Numerous cipher tools")]
	public class cipher : InteractionModuleBase<SocketInteractionContext>
	{
		private static List<char> alphabet = new List<char>() { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

		private static int FloorMod(object a, object b) // Regular modulo doesn't like negative numbers
		{
			double num1 = 1;
			double num2 = 1;

			double.TryParse(a.ToString(), out num1);
			double.TryParse(b.ToString(), out num2);

			return (int)(num1 - num2 * Math.Floor(num1 / num2));
		}

		[SlashCommand("caesarshift", "Caesar shift cipher", false, RunMode.Async)]
		public async Task CaesarShift(string text, int amount)
		{
			amount = FloorMod(amount, alphabet.Count);

			EmbedBuilder embedBuilder = new EmbedBuilder()
				.WithTitle("Working")
				.WithDescription("Shifting")
				.WithColor(Color.Red);

			await this.RespondAsync(null, new Embed[] { embedBuilder.Build() });

			embedBuilder.Title = "Finished";

			try
			{
				string finalText = string.Empty;
				foreach (char c in text)
				{
					bool isUpper = Char.IsUpper(c);
					int curIndex = alphabet.IndexOf(isUpper ? c : Char.ToUpper(c));

					if (curIndex < 0)
					{
						finalText = finalText + c;
						continue;
					}

					char newChar = alphabet[FloorMod(curIndex + amount, alphabet.Count)];

					if (!isUpper)
						newChar = Char.ToLower(newChar);

					finalText = finalText + newChar;
				}

				finalText = finalText.Trim();

				embedBuilder.Color = Color.Green;
				embedBuilder.Description = string.Format(@"
Original Text: `{0}`
Shifted Text: `{1}`",

				text,
				finalText);

				await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
			} catch (Exception)
			{
				embedBuilder.Color = Color.Red;
				embedBuilder.Description = "Failed to shift text";

				await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
			}
		}

		[Group("keyword", "keyword cipher")]
		public class keyword : InteractionModuleBase<SocketInteractionContext>
		{
			private static List<char> GenerateKeywordAlphabet(string keyword)
			{
				List<char> newAlphabet = new List<char>();

				foreach (char c in keyword.ToUpper())
				{
					if (newAlphabet.Contains(c))
						continue;

					if (Char.IsLetter(c))
						newAlphabet.Add(c);
				}

				foreach (char c in alphabet)
				{
					if (newAlphabet.Contains(c))
						continue;

					newAlphabet.Add(c);
				}

				return newAlphabet;
			}

			[SlashCommand("encode", "Keyword cipher encode", false, RunMode.Async)]
			public async Task KeywordEncode(string keyword, string text)
			{
				EmbedBuilder embedBuilder = new EmbedBuilder()
					.WithTitle("Working")
					.WithDescription("Encoding")
					.WithColor(Color.Red);

				await this.RespondAsync(null, new Embed[] { embedBuilder.Build() });

				embedBuilder.Title = "Finished";

				try
				{
					List<char> customAlphabet = GenerateKeywordAlphabet(keyword);

					string cipherText = string.Empty;

					foreach (char c in text)
					{
						int realIndex = alphabet.IndexOf(Char.ToUpper(c));

						if (realIndex < 0)
						{
							cipherText = cipherText + c;
							continue;
						}

						cipherText = cipherText + customAlphabet[realIndex];
					}

					embedBuilder.Color = Color.Green;
					embedBuilder.Description = string.Format("`{0}`", cipherText);
				} catch (Exception)
				{
					embedBuilder.Color = Color.Red;
					embedBuilder.Description = "Failed to encode string";
				}

				await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
			}

			[SlashCommand("decode", "Keyword cipher encode", false, RunMode.Async)]
			public async Task KeywordDecode(string keyword, string text)
			{
				EmbedBuilder embedBuilder = new EmbedBuilder()
					.WithTitle("Working")
					.WithDescription("Decoding")
					.WithColor(Color.Red);

				await this.RespondAsync(null, new Embed[] { embedBuilder.Build() });

				embedBuilder.Title = "Finished";

				try
				{
					List<char> customAlphabet = GenerateKeywordAlphabet(keyword);

					string plainText = string.Empty;

					foreach (char c in text)
					{
						int fakeIndex = customAlphabet.IndexOf(Char.ToUpper(c));

						if (fakeIndex < 0)
						{
							plainText = plainText + c;
							continue;
						}

						plainText = plainText + alphabet[fakeIndex];
					}

					embedBuilder.Color = Color.Green;
					embedBuilder.Description = string.Format("`{0}`", plainText);
				}
				catch (Exception)
				{
					embedBuilder.Color = Color.Red;
					embedBuilder.Description = "Failed to decode string";
				}

				await this.ModifyOriginalResponseAsync(message => message.Embed = embedBuilder.Build());
			}
		}
	}
}
