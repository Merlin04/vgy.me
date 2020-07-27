using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using Newtonsoft.Json;
using vgy.me.Models;

namespace vgy.me
{
    [Command("upload", Description = "Upload an image to vgy.me.")]
    public class UploadCommand : ICommand
    {
        [CommandParameter(0, Description = "Image file(s) to upload.")]
        public IReadOnlyList<FileInfo> Files { get; set; }

        [CommandOption("title", 't', Description = "A title that vgy.me displays on the viewer page.",
            IsRequired = false)]
        public string Title { get; set; }

        [CommandOption("description", 'd', Description = "A description that vgy.me displays on the viewer page.",
            IsRequired = false)]
        public string Description { get; set; }

        private readonly IHttpClientFactory _clientFactory;
        private readonly ConfigurationService _configuration;

        public UploadCommand(IHttpClientFactory clientFactory, ConfigurationService configuration)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
        }

        public async ValueTask ExecuteAsync(IConsole console)
        {
            // TODO: find a way to do this automatically with CliFx
            if (Files.Count == 0)
            {
                throw new CommandException("Error: No file provided.");
            }

            if (_configuration.Userkey is null)
            {
                throw new CommandException("Error: No userkey configured. To configure one, use vgy.me configure.");
            }

            using MultipartFormDataContent content = new MultipartFormDataContent
            {
                {new StringContent(_configuration.Userkey), "userkey"}
            };
            foreach (FileInfo file in Files)
            {
                content.Add(new StreamContent(file.OpenRead()), "file[]", file.Name);
            }

            if (!(Title is null))
            {
                content.Add(new StringContent(Title), "title");
            }

            if (!(Description is null))
            {
                content.Add(new StringContent(Description), "description");
            }

            HttpClient client = _clientFactory.CreateClient();
            HttpResponseMessage response = await client.PostAsync("https://vgy.me/upload", content);
            string responseJson = await response.Content.ReadAsStringAsync();
            dynamic responseContent = JsonConvert.DeserializeObject(responseJson);

            if (responseContent is null)
            {
                throw new CommandException("Error: Empty response from vgy.me.");
            }

            if ((bool) responseContent.error)
            {
                string errorText = "Errors received from vgy.me:\n";
                foreach ((string error, string message) in (Dictionary<string, string>) responseContent.messages)
                {
                    errorText += " - " + error + ": " + message + "\n";
                }

                throw new CommandException(errorText);
            }

            if (Files.Count == 1)
            {
                // vgy.me sends a different response if there's only one file
                console.Output.WriteLine("URL: " + responseContent.url);
                console.Output.WriteLine("Image URL: " + responseContent.image);
                console.Output.WriteLine("Delete: " + responseContent.delete);

                // Save in configuration file
                _configuration.AddUploadedFile(new UploadedFile
                {
                    Url = responseContent.url,
                    ImageUrl = responseContent.image,
                    DeleteUrl = responseContent.delete
                });
            }
            else
            {
                console.Output.WriteLine("Album URL: " + responseContent.url);
                await console.Output.WriteLineAsync("Images:");
                foreach (string imageUrl in responseContent.upload_list)
                {
                    await console.Output.WriteLineAsync(" - " + imageUrl);
                }
            }
        }
    }
}