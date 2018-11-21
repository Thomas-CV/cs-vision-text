using CognitiveServicesVision.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CognitiveServicesVision.Pages
{
	public class IndexModel : PageModel
	{
		private static class CacheKeys
		{
			public const string Images = "img";
		}
		private const string RootFolderName = "wwwroot\\";
		private const string ImagesFolderName = RootFolderName + "images";

		private IConfiguration Configuration { get; }
		private IMemoryCache MemoryCache { get; }
		public string ImageFilename { get; private set; } = "ImagePlaceholder.png";
		public string ImageDescription { get; private set; } = "";
		public int ImageIndex { get; private set; } = 1;

		public IndexModel(IConfiguration configuration, IMemoryCache cache)
		{
			Configuration = configuration;
			MemoryCache = cache;
		}


		public async Task OnGet(int? index)
		{
			if (index.HasValue == false || index.Value < 1) return;

			string filename = null;
			var images = GetListOfImages();

			if (images != null && images.Length > 0)
			{
				if (index < images.Length) ImageIndex = index.Value + 1;
				else ImageIndex = 1;

				if (--index >= images.Length) index = 0;
				filename = images[index.Value];
				ImageFilename = filename.Replace(RootFolderName, String.Empty);
			}
			if (ImageIndex != 0 && String.IsNullOrEmpty(filename) == false)
			{
				string subscriptionKey = Configuration.GetValue<string>("SubscriptionKey");
				string serviceEndpoint = Configuration.GetValue<string>("ServiceEndpoint");
				TextOperationResult result = await Vision.RecognizeText(filename, subscriptionKey, serviceEndpoint);
				ShowExtractedText(result);
			}
		}

		private string[] GetListOfImages()
		{
			string[] images = MemoryCache.Get<string[]>(CacheKeys.Images);
			if (images == null)
			{
				images = Directory.GetFiles(ImagesFolderName);
				MemoryCache.Set(CacheKeys.Images, images);
			}
			return images;
		}

		private void ShowExtractedText(TextOperationResult result)
		{
			if (result == null || result.Status != TextOperationStatusCodes.Succeeded)
			{
				ImageDescription = "Error occurred.";
				return;
			}

			foreach (var line in result.RecognitionResult.Lines)
			{
				ImageDescription += line.Text + "<br>";
			}
		}
	}
}
