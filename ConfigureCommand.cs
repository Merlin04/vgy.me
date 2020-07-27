using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;

namespace vgy.me
{
    [Command("configure", Description = "Configure this tool.")]
    public class ConfigureCommand : ICommand
    {
        [CommandParameter(0, Description = "API key/userkey. To generate one go to https://vgy.me/account/details.")]
        public string Userkey { get; set; }

        private readonly ConfigurationService _configuration;
        
        public ConfigureCommand(ConfigurationService configuration)
        {
            _configuration = configuration;
        }

        public ValueTask ExecuteAsync(IConsole console)
        {
            if (Userkey is null)
            {
                throw new CommandException("Error: No parameter specified.");
            }

            _configuration.Userkey = Userkey;

            return default;
        }
    }

    [Command("configure reset", Description = "Delete the .vgy.me.json file in your home folder.")]
    public class ConfigureResetCommand : ICommand
    {
        private readonly ConfigurationService _configuration;
        
        public ConfigureResetCommand(ConfigurationService configuration)
        {
            _configuration = configuration;
        }
        
        public ValueTask ExecuteAsync(IConsole console)
        {
            if (!_configuration.DeleteConfig())
            {
                throw new CommandException(
                    "Error: Failed to delete configuration file, maybe it didn't exist in the first place?");
            }

            return default;
        }
    }
}