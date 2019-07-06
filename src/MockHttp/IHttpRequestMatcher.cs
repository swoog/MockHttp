﻿using System.Net.Http;

namespace MockHttp
{
	/// <summary>
	/// Represents a condition for matching a <see cref="HttpRequestMessage"/>.
	/// </summary>
	public interface IHttpRequestMatcher
	{
		/// <summary>
		/// Checks that the request matches a condition.
		/// </summary>
		/// <param name="request">The request to check.</param>
		/// <returns><see langword="true"/> if the request matches, <see langword="false"/> otherwise.</returns>
		bool IsMatch(HttpRequestMessage request);

		/// <summary>
		/// Gets whether the matcher is mutually exclusive to other matchers of the same type.
		/// </summary>
		bool IsExclusive { get; }
	}
}