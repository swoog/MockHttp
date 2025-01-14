﻿using System.Diagnostics;
using System.Globalization;
using System.Text;
using MockHttp.Responses;

namespace MockHttp.Matchers;

/// <summary>
/// Matches a request by the request content.
/// </summary>
public class ContentMatcher : IAsyncHttpRequestMatcher
{
    private const int MaxBytesDisplayed = 10;

    /// <summary>
    /// The default content encoding.
    /// </summary>
    public static readonly Encoding DefaultEncoding = Encoding.UTF8;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Encoding _encoding;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentMatcher" /> class.
    /// </summary>
    public ContentMatcher()
    {
        ByteContent = Array.Empty<byte>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentMatcher" /> class using specified <paramref name="content" />.
    /// </summary>
    /// <param name="content">The request content to match.</param>
    /// <param name="encoding">The content encoding.</param>
    public ContentMatcher(string content, Encoding encoding)
        : this((encoding ?? DefaultEncoding).GetBytes(content ?? throw new ArgumentNullException(nameof(content))))
    {
        _encoding = encoding ?? DefaultEncoding;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentMatcher" /> class using specified raw <paramref name="content" />.
    /// </summary>
    /// <param name="content">The request content to match.</param>
    public ContentMatcher(byte[] content)
    {
        ByteContent = content ?? throw new ArgumentNullException(nameof(content));
    }

    /// <summary>
    /// Gets the expected content in bytes.
    /// </summary>
    // ReSharper disable once MemberCanBeProtected.Global
    protected internal byte[] ByteContent { get; }

    /// <inheritdoc />
    public async Task<bool> IsMatchAsync(MockHttpRequestContext requestContext)
    {
        if (requestContext is null)
        {
            throw new ArgumentNullException(nameof(requestContext));
        }

        byte[] requestContent = null;
        if (requestContext.Request.Content is not null)
        {
            // Use of ReadAsByteArray() will use internal buffer, so we can re-enter this method multiple times.
            // In comparison, ReadAsStream() will return the underlying stream which can only be read once.
            requestContent = await requestContext.Request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        }

        if (requestContent is null)
        {
            return ByteContent.Length == 0;
        }

        if (requestContent.Length == 0 && ByteContent.Length == 0)
        {
            return true;
        }

        return IsMatch(requestContent);
    }

    /// <inheritdoc />
    public virtual bool IsExclusive => true;

    /// <summary>
    /// Checks that the request matches the specified <paramref name="requestContent" />.
    /// </summary>
    /// <param name="requestContent">The request content received.</param>
    /// <returns><see langword="true" /> if the content matches, <see langword="false" /> otherwise.</returns>
    protected virtual bool IsMatch(byte[] requestContent)
    {
        return requestContent.SequenceEqual(ByteContent);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (ByteContent.Length == 0)
        {
            return "Content: <empty>";
        }

        if (_encoding is not null)
        {
            return $"Content: {_encoding.GetString(ByteContent, 0, ByteContent.Length)}";
        }

        int charsToOutput = Math.Min(MaxBytesDisplayed, ByteContent.Length);
        var sb = new StringBuilder();
        sb.Append("Content: [");
        for (int i = 0; i < charsToOutput; i++)
        {
            sb.AppendFormat(CultureInfo.InvariantCulture, "0x{0:x2}", ByteContent[i]);
            if (i < charsToOutput - 1)
            {
                sb.Append(',');
            }
        }

        if (charsToOutput < ByteContent.Length)
        {
            sb.AppendFormat(CultureInfo.InvariantCulture, ",...](Size = {0})", ByteContent.Length);
        }
        else
        {
            sb.Append(']');
        }

        return sb.ToString();
    }
}
