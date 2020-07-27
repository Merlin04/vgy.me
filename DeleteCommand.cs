using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using ScrapySharp.Html.Forms;
using ScrapySharp.Network;
using vgy.me.Models;

namespace vgy.me
{
    [Command("delete", Description = "Delete an image you previously uploaded.")]
    public class DeleteCommand : ICommand
    {
        [CommandParameter(0,
            Description =
                "The URL of the image to delete, or \"all\" to delete all images that have been uploaded that aren't albums.",
            Name = "DeleteUrl")]
        public string ParameterUrl { get; set; }

        private readonly IHttpClientFactory _clientFactory;
        private readonly ConfigurationService _configuration;

        public DeleteCommand(IHttpClientFactory clientFactory, ConfigurationService configuration)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
        }

        public async ValueTask ExecuteAsync(IConsole console)
        {
            if (ParameterUrl == "all")
            {
                await DeleteAll(console);
            }
            else
            {
                await DeleteSingleUrl();
            }
        }

        private async Task DeleteSingleUrl()
        {
            string deleteUrl;
            UploadedFile uploadedFile = _configuration.UploadedFiles
                .Find(file =>
                    file.Url == ParameterUrl || file.ImageUrl == ParameterUrl || file.DeleteUrl == ParameterUrl);

            if (uploadedFile is null)
            {
                if (!ParameterUrl.Contains("vgy.me/delete/"))
                {
                    throw new CommandException("Error: Unable to find the corresponding delete URL. " +
                                               "If the URL is for an album you need to delete that through the web interface.");
                }

                deleteUrl = ParameterUrl;
            }
            else
            {
                deleteUrl = uploadedFile.DeleteUrl;
            }

            await DeleteImage(deleteUrl);

            if (!(uploadedFile is null))
            {
                _configuration.DeleteUploadedFile(uploadedFile);
            }
        }

        private async Task DeleteAll(IConsole console)
        {
            // Copy delete URLs to a new list so they can be deleted from configuration
            var deleteUrls = _configuration.UploadedFiles
                .Select(file => new KeyValuePair<string, string>(file.Url, file.DeleteUrl)).ToList();
            foreach ((string viewerUrl, string deleteUrl) in deleteUrls)
            {
                await console.Output.WriteLineAsync("Deleting " + viewerUrl + "...");
                await DeleteImage(deleteUrl);
                _configuration.DeleteUploadedFile(
                    _configuration.UploadedFiles.Find(file => file.DeleteUrl == deleteUrl));
            }
        }
        
        private async Task DeleteImage(string deleteUrl)
        {
            ScrapingBrowser browser = new ScrapingBrowser();
            WebPage sbPage = await browser.NavigateToPageAsync(new Uri(deleteUrl));
            PageWebForm form = new PageWebForm(sbPage.Html.Descendants("form").First(), browser);
            form.Method = HttpVerb.Post;
            WebPage resultsPage = form.Submit();
            if (!resultsPage.Content.Contains("has been deleted successfully"))
            {
                throw new CommandException("Error: Deleting the image failed.");
            }
        }
    }
}