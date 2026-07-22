using System;
using System.Net.Http;
using System.Threading.Tasks;
using Celigo.NetSuite.ConnectionGuard.Abstractions;
using Celigo.NetSuite.ConnectionGuard.Decorators;

namespace Celigo.ServiceManager.NetSuite.REST
{
    public sealed class GuardedRestClient : IRestClient
    {
        private readonly IRestClient _inner;
        private readonly GuardPipeline _guard;
        private readonly INsCallContextAccessor _contextAccessor;

        public GuardedRestClient(IRestClient inner, GuardPipeline guard, INsCallContextAccessor contextAccessor)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _guard = guard ?? throw new ArgumentNullException(nameof(guard));
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        }

        public Task<HttpResponseMessage> Get(string account, Uri requestUri, string token, string tokenSecret)
        {
            return RunThroughGuardAsync(() => _inner.Get(account, requestUri, token, tokenSecret));
        }

        public Task<HttpResponseMessage> Post<T>(string account, Uri requestUri, string token, string tokenSecret, T content)
        {
            return RunThroughGuardAsync(() => _inner.Post(account, requestUri, token, tokenSecret, content));
        }

        public Task<HttpResponseMessage> Put<T>(string account, Uri requestUri, string token, string tokenSecret, T content)
        {
            return RunThroughGuardAsync(() => _inner.Put(account, requestUri, token, tokenSecret, content));
        }

        public Task<HttpResponseMessage> Patch<T>(string account, Uri requestUri, string token, string tokenSecret, T content)
        {
            return RunThroughGuardAsync(() => _inner.Patch(account, requestUri, token, tokenSecret, content));
        }

        public Task<HttpResponseMessage> Delete(string account, Uri requestUri, string token, string tokenSecret)
        {
            return RunThroughGuardAsync(() => _inner.Delete(account, requestUri, token, tokenSecret));
        }

        private async Task<HttpResponseMessage> RunThroughGuardAsync(Func<Task<HttpResponseMessage>> netSuiteCall)
        {
            var currContext = _contextAccessor.Current;
            if (currContext is null)
            {
                return await netSuiteCall();
            }

            var restContext = currContext with { Surface = NetSuiteSurface.Rest };
            return await _guard.ExecuteAsync(restContext, async () =>
            {
                var response = await netSuiteCall();

                var body = string.Empty;
                if (!response.IsSuccessStatusCode && response.Content is not null)
                {
                    await response.Content.LoadIntoBufferAsync();
                    body = await response.Content.ReadAsStringAsync();
                }

                var (outcome, reason) = NsResponseClassifier.ClassifyRest(response.StatusCode, body);
                if (outcome is NsOutcomeClass.Transient or NsOutcomeClass.Throttle)
                {
                    throw new NetSuiteTransientException(reason ?? response.StatusCode.ToString());
                }
                return (response, outcome, reason);
            });
        }
    }
}
