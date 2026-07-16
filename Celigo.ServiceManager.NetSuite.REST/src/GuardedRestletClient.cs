using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Celigo.NetSuite.ConnectionGuard.Abstractions;
using Celigo.NetSuite.ConnectionGuard.Decorators;

namespace Celigo.ServiceManager.NetSuite.REST
{
    public sealed class GuardedRestletClient : IRestletClient
    {
        private readonly IRestletClient _inner;
        private readonly GuardPipeline _guard;
        private readonly INsCallContextAccessor _contextAccessor;

        public GuardedRestletClient(IRestletClient inner, GuardPipeline guard, INsCallContextAccessor contextAccessor)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _guard = guard ?? throw new ArgumentNullException(nameof(guard));
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
        }

        public Task<HttpResponseMessage> Get(in string account, in string token, in string tokenSecret, IReadOnlyDictionary<string, string> queryParams = null)
        {
            string capturedAccount = account, capturedToken = token, capturedTokenSecret = tokenSecret;
            var capturedQueryParams = queryParams;
            return RunThroughGuardAsync(() => _inner.Get(capturedAccount, capturedToken, capturedTokenSecret, capturedQueryParams));
        }

        public Task<HttpResponseMessage> Post<T>(in string account, in string token, in string tokenSecret, in T message, IReadOnlyDictionary<string, string> queryParams = null)
        {
            string capturedAccount = account, capturedToken = token, capturedTokenSecret = tokenSecret;
            T capturedMessage = message;
            var capturedQueryParams = queryParams;
            return RunThroughGuardAsync(() => _inner.Post(capturedAccount, capturedToken, capturedTokenSecret, capturedMessage, capturedQueryParams));
        }

        public Task<HttpResponseMessage> Delete(in string account, in string token, in string tokenSecret, IReadOnlyDictionary<string, string> queryParams = null)
        {
            string capturedAccount = account, capturedToken = token, capturedTokenSecret = tokenSecret;
            var capturedQueryParams = queryParams;
            return RunThroughGuardAsync(() => _inner.Delete(capturedAccount, capturedToken, capturedTokenSecret, capturedQueryParams));
        }

        public Task<HttpResponseMessage> Put<T>(in string account, in string token, in string tokenSecret, in T message, IReadOnlyDictionary<string, string> queryParams = null)
        {
            string capturedAccount = account, capturedToken = token, capturedTokenSecret = tokenSecret;
            T capturedMessage = message;
            var capturedQueryParams = queryParams;
            return RunThroughGuardAsync(() => _inner.Put(capturedAccount, capturedToken, capturedTokenSecret, capturedMessage, capturedQueryParams));
        }

        public Task<HttpResponseMessage> Get(in Passport passport, IReadOnlyDictionary<string, string> queryParams = null)
        {
            Passport capturedPassport = passport;
            var capturedQueryParams = queryParams;
            return RunThroughGuardAsync(() => _inner.Get(capturedPassport, capturedQueryParams));
        }

        public Task<HttpResponseMessage> Post<T>(in Passport passport, in T message, IReadOnlyDictionary<string, string> queryParams = null)
        {
            Passport capturedPassport = passport;
            T capturedMessage = message;
            var capturedQueryParams = queryParams;
            return RunThroughGuardAsync(() => _inner.Post(capturedPassport, capturedMessage, capturedQueryParams));
        }

        [Obsolete]
        public Task<HttpResponseMessage> Get(in string account, in string token, in string tokenSecret, (string key, string value) queryParam, params (string key, string value)[] queryParams)
        {
            string capturedAccount = account, capturedToken = token, capturedTokenSecret = tokenSecret;
            var capturedQueryParam = queryParam;
            var capturedQueryParams = queryParams;
            return RunThroughGuardAsync(() => _inner.Get(capturedAccount, capturedToken, capturedTokenSecret, capturedQueryParam, capturedQueryParams));
        }

        [Obsolete]
        public Task<HttpResponseMessage> Get(in Passport passport, (string key, string value) queryParam, params (string key, string value)[] queryParams)
        {
            Passport capturedPassport = passport;
            var capturedQueryParam = queryParam;
            var capturedQueryParams = queryParams;
            return RunThroughGuardAsync(() => _inner.Get(capturedPassport, capturedQueryParam, capturedQueryParams));
        }

        [Obsolete]
        public Task<HttpResponseMessage> Post<T>(in string account, in string token, in string tokenSecret, in T message, (string key, string value) queryParam, params (string key, string value)[] queryParams)
        {
            string capturedAccount = account, capturedToken = token, capturedTokenSecret = tokenSecret;
            T capturedMessage = message;
            var capturedQueryParam = queryParam;
            var capturedQueryParams = queryParams;
            return RunThroughGuardAsync(() => _inner.Post(capturedAccount, capturedToken, capturedTokenSecret, capturedMessage, capturedQueryParam, capturedQueryParams));
        }

        [Obsolete]
        public Task<HttpResponseMessage> Post<T>(in Passport passport, in T message, (string key, string value) queryParam, params (string key, string value)[] queryParams)
        {
            Passport capturedPassport = passport;
            T capturedMessage = message;
            var capturedQueryParam = queryParam;
            var capturedQueryParams = queryParams;
            return RunThroughGuardAsync(() => _inner.Post(capturedPassport, capturedMessage, capturedQueryParam, capturedQueryParams));
        }

        private async Task<HttpResponseMessage> RunThroughGuardAsync(Func<Task<HttpResponseMessage>> netSuiteCall)
        {
            var currContext = _contextAccessor.Current;
            if (currContext is null)
            {
                // rejecting guarded calls that have missing connId.
                return await netSuiteCall();
            }

            var restletContext = currContext with { Surface = NetSuiteSurface.Restlet };
            return await _guard.ExecuteAsync(restletContext, async () =>
            {
                var response = await netSuiteCall();

                var body = string.Empty;
                if (!response.IsSuccessStatusCode && response.Content is not null)
                {
                    await response.Content.LoadIntoBufferAsync();
                    body = await response.Content.ReadAsStringAsync();
                }

                var (outcome, reason) = NsResponseClassifier.ClassifyRestlet(response.StatusCode, body);
                return (response, outcome, reason);
            });
        }
    }
}
