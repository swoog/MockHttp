﻿using System;
using System.Diagnostics;
using System.Net.Http;
using DotNet.Globbing;

namespace MockHttp.Matchers
{
	/// <summary>
	/// Matches a request by the request URI.
	/// </summary>
	public class GlobUrlMatcher : IHttpRequestMatcher
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly string _requestUri;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly Glob _glob;

		/// <summary>
		/// Initializes a new instance of the <see cref="UrlMatcher"/> class using specified <paramref name="requestUri"/>.
		/// </summary>
		/// <param name="requestUri">The request URI or a URI wildcard (based on glob).</param>
		public GlobUrlMatcher(string requestUri)
		{
			_requestUri = requestUri ?? throw new ArgumentNullException(nameof(requestUri));

			// Parse with default options, in case externally default options were set.
			var options = new GlobOptions();
			_glob = Glob.Parse(requestUri, options);
		}

		/// <inheritdoc />
		public bool IsMatch(HttpRequestMessage request)
		{
			return _glob.IsMatch(request.RequestUri.ToString());
		}

		/// <inheritdoc />
		public bool IsExclusive => false;

		/// <inheritdoc />
		public override string ToString()
		{
			return $"Uri: '{_requestUri}'";
		}
	}
}
