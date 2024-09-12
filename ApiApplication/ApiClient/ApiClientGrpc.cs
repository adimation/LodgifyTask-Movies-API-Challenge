using System;
using System.Net.Http;
using System.Threading.Tasks;
using ApiApplication.Configurations;
using ApiApplication.Exceptions;
using ApiApplication.Extensions;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProtoDefinitions;
using static ProtoDefinitions.MoviesApi;

namespace ApiApplication.ApiClient
{
    public class ApiClientGrpc : IApiClientGrpc
    {
        private readonly ILogger<ApiClientGrpc> _logger;
        private readonly MoviesApiConfig _config;
        private readonly IDistributedCache _cache;
        private readonly DistributedCacheEntryOptions _cacheOptions;

        public ApiClientGrpc(ILogger<ApiClientGrpc> logger, IOptions<MoviesApiConfig> config, IDistributedCache cache)
        {
            _logger = logger;
            _config = config.Value;
            _cache = cache;
            _cacheOptions = new DistributedCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(_config.CacheExpiryInMinutes)).SetAbsoluteExpiration(TimeSpan.FromDays(1));
        }

        public async Task<showListResponse> GetAll()
        {
            try
            {
                var client = await GetClient();
                var callOptions = GetClientOptions();

                var all = await client.GetAllAsync(new ProtoDefinitions.Empty(), callOptions);
                all.Data.TryUnpack<showListResponse>(out var data);

                //setting cache
                await _cache.SetAsync<showListResponse>(Constants.Constants.MOVIES_API_GETALL_CACHE, data, _cacheOptions);

                return data;
            }
            catch (RpcException rpcEx)
            {
                _logger.LogInformation($"Exception: {rpcEx.Message}");
                throw new MoviesApiClientException("An error occurred while calling the gRPC service.", Constants.Constants.MOVIES_API_CLIENT_ERROR, rpcEx);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Exception: {ex.Message}");
                throw new MoviesApiClientException("An unexpected error occurred.", Constants.Constants.MOVIES_API_CLIENT_ERROR, ex);
            }
        }

        public async Task<showResponse> GetById(string id)
        {
            try
            {
                var client = await GetClient();
                var callOptions = GetClientOptions();

                var idRequest = new IdRequest { Id = id };

                var first = await client.GetByIdAsync(idRequest, callOptions);

                first.Data.TryUnpack<showResponse>(out var data);

                //setting cache
                await _cache.SetAsync<showResponse>(string.Format(Constants.Constants.MOVIES_API_GETBYID_CACHE, id), data, _cacheOptions);

                return data;
            }
            catch (RpcException rpcEx)
            {
                _logger.LogInformation($"Exception: {rpcEx.Message}");
                throw new MoviesApiClientException("An error occurred while calling the gRPC service.", Constants.Constants.MOVIES_API_CLIENT_ERROR, rpcEx);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Exception: {ex.Message}");
                throw new MoviesApiClientException("An unexpected error occurred.", Constants.Constants.MOVIES_API_CLIENT_ERROR, ex);
            }
        }

        public async Task<showListResponse> Search(string searchText)
        {
            try
            {
                var client = await GetClient();
                var callOptions = GetClientOptions();

                var searchRequest = new SearchRequest { Text = searchText };

                var first = await client.SearchAsync(searchRequest, callOptions);

                first.Data.TryUnpack<showListResponse>(out var data);

                //setting cache
                await _cache.SetAsync<showListResponse>(string.Format(Constants.Constants.MOVIES_API_SEARCH_CACHE, searchText), data, _cacheOptions);

                return data;
            }
            catch (RpcException rpcEx)
            {
                _logger.LogInformation($"Exception: {rpcEx.Message}");
                throw new MoviesApiClientException("An error occurred while calling the gRPC service.", Constants.Constants.MOVIES_API_CLIENT_ERROR, rpcEx);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Exception: {ex.Message}");
                throw new MoviesApiClientException("An unexpected error occurred.", Constants.Constants.MOVIES_API_CLIENT_ERROR, ex);
            }
        }

        public async Task<showListResponse> GetAllOrCached()
        {
            try
            {
                var data = await GetAll();

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Exception: {ex.Message}");

                var data = await _cache.GetAsync<showListResponse>(Constants.Constants.MOVIES_API_GETALL_CACHE);
                if (data != null)
                    return data;

                throw new MoviesApiClientException("An unexpected error occurred.", Constants.Constants.MOVIES_API_CLIENT_ERROR, ex);
            }
        }

        public async Task<showResponse> GetByIdOrCached(string id)
        {
            try
            {
                var data = await GetById(id);

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Exception: {ex.Message}");

                var data = await _cache.GetAsync<showResponse>(string.Format(Constants.Constants.MOVIES_API_GETBYID_CACHE, id));
                if (data != null)
                    return data;

                throw new MoviesApiClientException("An unexpected error occurred.", Constants.Constants.MOVIES_API_CLIENT_ERROR, ex);
            }
        }

        public async Task<showListResponse> SearchOrCached(string searchText)
        {
            try
            {
                var data = await Search(searchText);

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Exception: {ex.Message}");

                var data = await _cache.GetAsync<showListResponse>(string.Format(Constants.Constants.MOVIES_API_SEARCH_CACHE, searchText));
                if (data != null)
                    return data;
                
                throw new MoviesApiClientException("An unexpected error occurred.", Constants.Constants.MOVIES_API_CLIENT_ERROR, ex);
            }
        }

        #region Private Methods
        private async Task<MoviesApiClient> GetClient()
        {
            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            var channel = GrpcChannel.ForAddress(_config.ServiceUrl, new GrpcChannelOptions()
            {
                HttpHandler = httpHandler
            });
            var client = new MoviesApiClient(channel);

            return client;
        }

        private CallOptions GetClientOptions()
        {
            // Create metadata with API key
            var headers = new Metadata
            {
                { "X-Apikey", _config.ApiKey }
            };

            // Add the metadata to the call options
            var callOptions = new CallOptions(headers);

            return callOptions;
        }
        #endregion
    }
}