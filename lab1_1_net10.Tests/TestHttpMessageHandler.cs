using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace lab1_1_net10.Tests
{
    public class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handlerFunc;

        public int CallsCount { get; private set; }
        public List<HttpRequestMessage> Requests { get; } = new();

        public TestHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handlerFunc)
        {
            _handlerFunc = handlerFunc ?? throw new ArgumentNullException(nameof(handlerFunc));
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallsCount++;
            Requests.Add(request);
            return Task.FromResult(_handlerFunc(request));
        }
    }
}
