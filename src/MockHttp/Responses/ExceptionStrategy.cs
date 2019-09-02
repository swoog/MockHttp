﻿using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MockHttp.Responses
{
	internal class ExceptionStrategy : IResponseStrategy
	{
		private readonly Func<Exception> _exceptionFactory;

		public ExceptionStrategy(Func<Exception> exceptionFactory)
		{
			_exceptionFactory = exceptionFactory ?? throw new ArgumentNullException(nameof(exceptionFactory));
		}

		public Task<HttpResponseMessage> ProduceResponseAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			throw _exceptionFactory() ?? new HttpMockException("The configured exception cannot be null.");
		}
	}
}
