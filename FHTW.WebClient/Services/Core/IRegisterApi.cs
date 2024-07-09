using System.Threading.Tasks;

namespace FHTW.WebClient.Services.Core;

public interface IRegisterApi
{
    Task Register(string token);
}