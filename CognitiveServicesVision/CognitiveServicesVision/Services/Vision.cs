using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace CognitiveServicesVision.Services
{
	public static class Vision
	{
		private const int MaximumNumberOfRetries = 10;
		private const int DelayBetweenRetriesInMilliseconds = 1500;

		public static async Task<TextOperationResult> RecognizeText(string filename, string subscriptionKey, string serviceEndpoint)
		{
			ComputerVisionClient vision = new ComputerVisionClient(new ApiKeyServiceClientCredentials(subscriptionKey), new DelegatingHandler[] { })
			{
				Endpoint = serviceEndpoint
			};

			TextRecognitionMode mode = TextRecognitionMode.Printed;

			using (Stream stream = File.OpenRead(filename))
			{
				try
				{
					RecognizeTextInStreamHeaders headers = await vision.RecognizeTextInStreamAsync(stream, mode);
					string id = headers.OperationLocation.Substring(headers.OperationLocation.LastIndexOf('/') + 1);

					int count = 0;
					TextOperationResult result;
					do
					{
						result = await vision.GetTextOperationResultAsync(id);
						if (result.Status == TextOperationStatusCodes.Succeeded || result.Status == TextOperationStatusCodes.Failed) return result;
						await Task.Delay(DelayBetweenRetriesInMilliseconds);
					}
					while ((result.Status == TextOperationStatusCodes.Running || result.Status == TextOperationStatusCodes.NotStarted) && count++ < MaximumNumberOfRetries);
				}
				catch
				{
					// TODO: handle exception here
				}
			}
			return null;
		}
	}
}
