using System.Threading.Tasks;
using FHTW.Shared;

namespace FHTW.WebClient.Services.Core;

public interface IAuthorizeApi
{
    Task<DiscordUserDTO> GetUserInfo();
    Task Login();
    Task Logout();
}