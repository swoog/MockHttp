﻿using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace MockHttp
{
	public class HttpCallTests
	{
		private readonly HttpCall _sut;

		public HttpCallTests()
		{
			_sut = new HttpCall();
			_sut.SetResponse(request => Task.FromResult(new HttpResponseMessage()));
		}

		[Fact]
		public async Task When_sending_response_should_reference_request()
		{
			var request = new HttpRequestMessage();

			// Act
			HttpResponseMessage response = await _sut.SendAsync(request, CancellationToken.None);

			// Assert
			response.RequestMessage.Should().Be(request);
		}
	}
}