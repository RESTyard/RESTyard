using System;
using System.Text;

namespace RESTyard.HtoSourceGenerators;

/// <summary>
/// StringBuilder wrapper for emitting C# source with tracked indentation (4 spaces per level).
/// <see cref="Block"/> opens a brace scope that closes itself (with a configurable closing token)
/// on dispose, so the shape of the generated code is visible in the emitter source:
/// <code>
/// using (w.Block(close: "},"))
/// {
///     w.Line("Name = \"...\",");
/// }
/// </code>
/// </summary>
internal sealed class CodeWriter
{
    private const string IndentUnit = "    ";

    private readonly StringBuilder sb = new();
    private int indentLevel;

    /// <summary>Appends an empty line (no indentation).</summary>
    public CodeWriter Line()
    {
        sb.AppendLine();
        return this;
    }

    /// <summary>Appends one line at the current indentation. Empty text emits a blank line.</summary>
    public CodeWriter Line(string text)
    {
        if (text.Length > 0)
        {
            AppendIndent();
            sb.Append(text);
        }

        sb.AppendLine();
        return this;
    }

    /// <summary>
    /// Appends pre-formatted multi-line text (e.g. an XML doc comment),
    /// indenting each non-empty line at the current level.
    /// </summary>
    public CodeWriter Lines(string text)
    {
        foreach (var line in text.Split('\n'))
        {
            Line(line.TrimEnd('\r'));
        }

        return this;
    }

    /// <summary>
    /// Emits <paramref name="open"/>, indents one level, and returns a scope that
    /// un-indents and emits <paramref name="close"/> when disposed.
    /// </summary>
    public IndentScope Block(string open = "{", string close = "}")
    {
        Line(open);
        indentLevel++;
        return new IndentScope(this, close);
    }

    /// <summary>Indents one level without emitting braces; un-indents on dispose.</summary>
    public IndentScope Indent()
    {
        indentLevel++;
        return new IndentScope(this, close: null);
    }

    public override string ToString() => sb.ToString();

    private void AppendIndent()
    {
        for (var i = 0; i < indentLevel; i++)
        {
            sb.Append(IndentUnit);
        }
    }

    internal readonly struct IndentScope : IDisposable
    {
        private readonly CodeWriter writer;
        private readonly string? close;

        public IndentScope(CodeWriter writer, string? close)
        {
            this.writer = writer;
            this.close = close;
        }

        public void Dispose()
        {
            writer.indentLevel--;
            if (close != null)
            {
                writer.Line(close);
            }
        }
    }
}
