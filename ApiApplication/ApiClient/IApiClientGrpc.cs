using ProtoDefinitions;
using System.Threading.Tasks;

namespace ApiApplication.ApiClient
{
    public interface IApiClientGrpc
    {
        Task<showListResponse> GetAll();

        Task<showResponse> GetById(string id);

        Task<showListResponse> Search(string searchText);

        Task<showListResponse> GetAllOrCached();

        Task<showResponse> GetByIdOrCached(string id);

        Task<showListResponse> SearchOrCached(string searchText);
    }
}
