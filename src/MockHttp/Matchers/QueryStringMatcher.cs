﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using MockHttp.Http;

namespace MockHttp.Matchers
{
	/// <summary>
	/// Matches a request by the request URI query string.
	/// </summary>
	public class QueryStringMatcher : IHttpRequestMatcher
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly QueryString _matchQs;

		/// <summary>
		/// Initializes a new instance of the <see cref="QueryStringMatcher"/> class using specified query string parameters.
		/// </summary>
		/// <param name="parameters">The query string parameters.</param>
		public QueryStringMatcher(IEnumerable<KeyValuePair<string, IEnumerable<string>>> parameters)
		{
			if (parameters == null)
			{
				throw new ArgumentNullException(nameof(parameters));
			}

			_matchQs = new QueryString(parameters);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="QueryStringMatcher"/> class using specified query string.
		/// </summary>
		/// <param name="queryString">The query string.</param>
		public QueryStringMatcher(string queryString)
			: this(QueryString.Parse(queryString))
		{
		}

		/// <inheritdoc />
		public bool IsMatch(HttpRequestMessage request)
		{
			QueryString query = QueryString.Parse(request.RequestUri.Query);

			// When match collection is empty, behavior is flipped, and we expect no query string parameters on request.
			if (_matchQs.Count == 0 && query.Count > 0)
			{
				return false;
			}

			return _matchQs.All(q => 
				query.ContainsKey(q.Key) 
				&& (query[q.Key].Count() == q.Value.Count() && !q.Value.Any()
					|| query[q.Key].Any(qv => q.Value.Contains(qv))));
		}

		/// <inheritdoc />
		public bool IsExclusive => _matchQs.Count == 0;

		/// <inheritdoc />
		public override string ToString()
		{
			return $"Query string: '{_matchQs}'";
		}
	}
}
