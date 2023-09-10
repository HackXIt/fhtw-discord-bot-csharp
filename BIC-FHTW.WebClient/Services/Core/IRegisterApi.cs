using System.Threading.Tasks;

namespace BIC_FHTW.WebClient.Services.Core;

public interface IRegisterApi
{
    Task Register(string mailAddress);
}