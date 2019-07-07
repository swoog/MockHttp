﻿using System;
using System.Net.Http;
using FluentAssertions;
using MockHttp.FluentAssertions;
using Xunit;

namespace MockHttp.Matchers
{
	public class UrlMatcherTests
	{
		private UrlMatcher _sut;

		[Theory]
		[InlineData("relative.htm", UriKind.Relative, "http://127.0.0.1/relative.htm", true)]
		[InlineData("/folder/relative.htm", UriKind.Relative, "http://127.0.0.1/relative.htm", false)]
		[InlineData("relative.htm", UriKind.Relative, "http://127.0.0.1/folder/relative.htm", false)]
		[InlineData("folder/relative.htm", UriKind.Relative, "http://127.0.0.1/folder/relative.htm", true)]
		[InlineData("/folder/relative.htm", UriKind.Relative, "http://127.0.0.1/folder/relative.htm", true)]
		[InlineData("http://127.0.0.1/absolute.htm", UriKind.Absolute, "http://127.0.0.1/absolute.htm", true)]
		[InlineData("http://127.0.0.1/absolute.htm", UriKind.Absolute, "http://127.0.0.1/folder/absolute.htm", false)]
		public void Given_uri_when_matching_should_match(string matchUri, UriKind uriKind, string requestUri, bool isMatch)
		{
			_sut = new UrlMatcher(new Uri(matchUri, uriKind));

			// Act & assert
			_sut.IsMatch(new HttpRequestMessage
			{
				RequestUri = new Uri(requestUri, UriKind.Absolute)
			}).Should().Be(isMatch);
		}

		[Theory]
		[InlineData("relative.htm", true, "http://127.0.0.1/relative.htm", true)]
		[InlineData("/folder/relative.htm", true, "http://127.0.0.1/relative.htm", false)]
		[InlineData("relative.htm", true, "http://127.0.0.1/folder/relative.htm", false)]
		[InlineData("folder/relative.htm", true, "http://127.0.0.1/folder/relative.htm", true)]
		[InlineData("/folder/relative.htm", true, "http://127.0.0.1/folder/relative.htm", true)]
		[InlineData("http://127.0.0.1/absolute.htm", true, "http://127.0.0.1/absolute.htm", true)]
		[InlineData("http://127.0.0.1/absolute.htm", true, "http://127.0.0.1/folder/absolute.htm", false)]
		[InlineData("*.htm", true, "http://127.0.0.1/relative.htm", true)]
		[InlineData("*/relative.htm", true, "http://127.0.0.1/relative.htm", true)]
		[InlineData("/*/relative.htm", true, "http://127.0.0.1/folder/relative.htm", false)]
		[InlineData("/*/relative.htm", true, "http://127.0.0.1/relative.htm", false)]
		[InlineData("/folder/*.htm", true, "http://127.0.0.1/folder/relative.htm", false)]
		[InlineData("*/folder/*.htm", true, "http://127.0.0.1/folder/relative.htm", true)]
		[InlineData("/folder/*.htm", true, "http://127.0.0.1/relative.htm", false)]
		[InlineData("/*/*/relative.*", true, "http://127.0.0.1/folder1/folder2/relative.htm", false)]
		[InlineData("*/folder1/*/relative.*", true, "http://127.0.0.1/folder1/folder2/relative.htm", true)]
		[InlineData("/*/*/relative.*", true, "http://127.0.0.1/folder1/relative.htm", false)]
		[InlineData("http://127.0.0.1/*.htm", true, "http://127.0.0.1/absolute.htm", true)]
		[InlineData("http://127.0.0.1/*.htm", true, "http://127.0.0.1/folder/absolute.htm", true)]
		public void Given_uriString_when_matching_should_match(string uriString, bool hasWildcard, string requestUri, bool isMatch)
		{
			_sut = new UrlMatcher(uriString, hasWildcard);

			// Act & assert
			_sut.IsMatch(new HttpRequestMessage
			{
				RequestUri = new Uri(requestUri, UriKind.Absolute)
			}).Should().Be(isMatch);
		}

		[Fact]
		public void Given_null_uri_when_creating_matcher_should_throw()
		{
			// Act
			// ReSharper disable once ObjectCreationAsStatement
			Action act = () => new UrlMatcher(null);

			// Assert
			act.Should().Throw<ArgumentNullException>().WithParamName("uri");
		}

		[Fact]
		public void Given_null_uriString_when_creating_matcher_should_throw()
		{
			// Act
			// ReSharper disable once ObjectCreationAsStatement
			Action act = () => new UrlMatcher(null, false);

			// Assert
			act.Should().Throw<ArgumentNullException>().WithParamName("uriString");
		}
	}
}