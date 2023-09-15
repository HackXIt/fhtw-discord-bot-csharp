using System.Threading.Tasks;
using BIC_FHTW.Shared;

namespace BIC_FHTW.WebClient.Services.Core;

public interface IAuthorizeApi
{
    Task<DiscordUserDTO> GetUserInfo();
    Task Login();
    Task Logout();
}