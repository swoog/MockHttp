﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using MockHttp.Http;
using Newtonsoft.Json;

namespace MockHttp.FluentAssertions
{
	public class ResponseAssertions : ObjectAssertions
	{
		public ResponseAssertions(HttpResponseMessage responseMessage)
			: base(responseMessage)
		{
		}

		public AndConstraint<ResponseAssertions> BeSameAs(
			HttpResponseMessage expectedResponseMessage,
			string because = "",
			params object[] becauseArgs)
		{
			new ObjectAssertions(Subject).BeSameAs(expectedResponseMessage, because, becauseArgs);

			return new AndConstraint<ResponseAssertions>(this);
		}

		public AndConstraint<ResponseAssertions> HaveStatusCode(
			HttpStatusCode expectedStatusCode,
			string because = "",
			params object[] becauseArgs)
		{
			var subject = (HttpResponseMessage)Subject;

			var assertionScope = ((IAssertionScope)Execute.Assertion.BecauseOf(because, becauseArgs).UsingLineBreaks);
			assertionScope
				.ForCondition(subject != null)
				.FailWith("The subject is null.")
				.Then
				.ForCondition(subject.StatusCode == expectedStatusCode)
				.FailWith("Expected response with status code {0}{reason}, but found {1} instead.", expectedStatusCode, subject.StatusCode)
				;

			return new AndConstraint<ResponseAssertions>(this);
		}

		public AndConstraint<ResponseAssertions> HaveContentType
		(
			string expectedMediaType,
			string because = "",
			params object[] becauseArgs)
		{
			return HaveContentType(MediaTypeHeaderValue.Parse(expectedMediaType), because, becauseArgs);
		}

		public AndConstraint<ResponseAssertions> HaveContentType(
			MediaTypeHeaderValue expectedMediaType,
			string because = "",
			params object[] becauseArgs)
		{
			var subject = (HttpResponseMessage)Subject;

			var assertionScope = ((IAssertionScope)Execute.Assertion.BecauseOf(because, becauseArgs).UsingLineBreaks);
			assertionScope
				.ForCondition(subject != null)
				.FailWith("The subject is null.")
				.Then
				.ForCondition(subject.Content != null)
				.FailWith("Expected response with content {0}{reason}, but found none.")
				.Then
				.ForCondition(subject.Content.Headers.ContentType.Equals(expectedMediaType) || expectedMediaType.CharSet == null && subject.Content.Headers.ContentType.MediaType.Equals(expectedMediaType.MediaType))
				.FailWith("Expected response with content type {0}{reason}, but found {1} instead.", expectedMediaType, subject.Content.Headers.ContentType)
				;

			return new AndConstraint<ResponseAssertions>(this);
		}

		public AndConstraint<ResponseAssertions> HaveJsonContent<T>
		(
			T expectedContent,
			string because = "",
			params object[] becauseArgs)
		{
			return HaveContent(JsonConvert.SerializeObject(expectedContent), because, becauseArgs);
		}

		public AndConstraint<ResponseAssertions> HaveContent
		(
			string expectedContent,
			string because = "",
			params object[] becauseArgs)
		{
			return HaveContent(new ByteArrayContent(Encoding.UTF8.GetBytes(expectedContent)), because, becauseArgs);
		}

		public AndConstraint<ResponseAssertions> HaveContent(
			HttpContent expectedContent,
			string because = "",
			params object[] becauseArgs)
		{
			if (expectedContent == null)
			{
				throw new ArgumentNullException(nameof(expectedContent));
			}

			if (expectedContent.Headers.ContentType != null)
			{
				HaveContentType(expectedContent.Headers.ContentType);
			}

			var subject = (HttpResponseMessage)Subject;

			((IAssertionScope)Execute.Assertion.BecauseOf(because, becauseArgs).UsingLineBreaks)
				.ForCondition(subject != null)
				.FailWith("The subject is null.")
				.Then
				.ForCondition(subject.Content != null)
				.FailWith("Expected response with content {reason}, but has no content.")
				.Then
				.Given(() => new
				{
					currentContent = subject.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult(),
					expectedContent = expectedContent.ReadAsByteArrayAsync().GetAwaiter().GetResult()
				})
				.ForCondition(x => x.currentContent.SequenceEqual(x.expectedContent))
				// Using UTF-8 for fail msg, but this will not produce correct result for other encodings or binary responses.
				// Since this is a private test helper, we accept this for now.
				.FailWith("Expected response with content {0} to match {1}{reason}, but it did not.", x => Encoding.UTF8.GetString(x.expectedContent), x => Encoding.UTF8.GetString(x.currentContent));

			return new AndConstraint<ResponseAssertions>(this);
		}

		public AndConstraint<ResponseAssertions> HaveHeader(
			string key,
			string value,
			string because = "",
			params object[] becauseArgs)
		{
			return HaveHeader(key, value == null ? null : new[] { value }, because, becauseArgs);
		}

		public AndConstraint<ResponseAssertions> HaveHeader(
			string key,
			IEnumerable<string> values,
			string because = "",
			params object[] becauseArgs)
		{
			if (key == null)
			{
				throw new ArgumentNullException(nameof(key));
			}

			var subject = (HttpResponseMessage)Subject;
			var expectedHeader = new KeyValuePair<string, IEnumerable<string>>(key, values);
			var equalityComparer = new HttpHeaderEqualityComparer();

			var assertionScope = ((IAssertionScope)Execute.Assertion.BecauseOf(because, becauseArgs).UsingLineBreaks);
			assertionScope
				.ForCondition(subject != null)
				.FailWith("The subject is null.")
				.Then
				.ForCondition(subject.Headers.Contains(key) || (subject.Content?.Headers.Contains(key) ?? false))
				.FailWith("Expected response to have header {0}{reason}, but found none.", key)
				.Then
				.ForCondition(
					subject.Headers.Any(h => equalityComparer.Equals(h, expectedHeader))
					|| (subject.Content?.Headers.Any(h => equalityComparer.Equals(h, expectedHeader)) ?? false))
				.FailWith(() =>
				{
					if (subject.Headers.TryGetValues(key, out IEnumerable<string> headerValues))
					{
						return new FailReason("Expected response to have header {0} with value {1}{reason}, but found value {2}.", key, values, headerValues);
					}

					if (!subject.Content.Headers.TryGetValues(key, out headerValues))
					{
						headerValues = new List<string>();
					}

					return new FailReason("Expected response to have header {0} with value {1}{reason}, but found value {2}.", key, values, headerValues);
				})
				;

			return new AndConstraint<ResponseAssertions>(this);
		}
	}
}