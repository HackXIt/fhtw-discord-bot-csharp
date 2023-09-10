using System.Threading.Tasks;
using BIC_FHTW.Shared;

namespace BIC_FHTW.WebClient.Services.Core
{
    public interface ITagApi
    {
        Task<TagMetaDataDTO[]> GetTagMetaDataAsync();
        Task<UserTagListDTO> GetUserTaglistAsync();
        Task SubscribeAsync(string[] tags);
        Task UnsubscribeAsync(string[] tags);
        Task BlacklistAsync(string[] tags);
        Task UnblacklistAsync(string[] tags);
    }
}
