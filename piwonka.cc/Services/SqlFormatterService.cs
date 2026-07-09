namespace Piwonka.CC.Services
{
    public class SqlFormatterService
    {
        private static readonly HashSet<string> MajorClauses = new(StringComparer.OrdinalIgnoreCase)
        {
            "SELECT", "FROM", "WHERE", "ORDER BY", "GROUP BY", "HAVING",
            "INSERT INTO", "INSERT", "UPDATE", "DELETE FROM", "DELETE",
            "SET", "VALUES", "UNION ALL", "UNION", "EXCEPT", "INTERSECT",
            "WITH", "MERGE", "USING", "WHEN MATCHED", "WHEN NOT MATCHED"
        };

        private static readonly HashSet<string> SubClauses = new(StringComparer.OrdinalIgnoreCase)
        {
            "INNER JOIN", "LEFT JOIN", "RIGHT JOIN", "FULL JOIN", "CROSS JOIN",
            "LEFT OUTER JOIN", "RIGHT OUTER JOIN", "FULL OUTER JOIN",
            "JOIN", "ON", "AND", "OR", "CASE", "END",
            "WHEN", "THEN", "ELSE", "LIMIT", "OFFSET", "FETCH", "TOP",
            "INTO", "RETURNING", "OUTPUT", "OPTION", "CROSS APPLY", "OUTER APPLY"
        };

        private static readonly HashSet<string> AllKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            "SELECT", "DISTINCT", "TOP", "FROM", "WHERE", "AND", "OR", "NOT",
            "IN", "EXISTS", "BETWEEN", "LIKE", "IS", "NULL", "AS", "ON",
            "JOIN", "INNER", "LEFT", "RIGHT", "FULL", "OUTER", "CROSS",
            "ORDER", "BY", "GROUP", "HAVING", "UNION", "ALL", "EXCEPT", "INTERSECT",
            "INSERT", "INTO", "VALUES", "UPDATE", "SET", "DELETE",
            "CREATE", "ALTER", "DROP", "TABLE", "INDEX", "VIEW", "PROCEDURE",
            "BEGIN", "END", "IF", "ELSE", "WHILE", "RETURN", "DECLARE",
            "CASE", "WHEN", "THEN", "ASC", "DESC", "WITH", "NOLOCK",
            "COUNT", "SUM", "AVG", "MIN", "MAX", "CAST", "CONVERT", "COALESCE",
            "ISNULL", "NULLIF", "ROW_NUMBER", "RANK", "DENSE_RANK", "OVER",
            "PARTITION", "OFFSET", "FETCH", "NEXT", "ROWS", "ONLY", "FIRST",
            "LIMIT", "MERGE", "USING", "MATCHED", "OUTPUT", "APPLY",
            "OPTION", "RECOMPILE", "RETURNING", "CASCADE", "RESTRICT",
            "PRIMARY", "KEY", "FOREIGN", "REFERENCES", "UNIQUE", "CHECK",
            "DEFAULT", "CONSTRAINT", "IDENTITY", "NONCLUSTERED", "CLUSTERED",
            "INCLUDE", "GO", "EXEC", "EXECUTE", "PRINT", "TRIGGER", "AFTER",
            "BEFORE", "FOR", "EACH", "ROW", "SCHEMA", "DATABASE", "USE",
            "GRANT", "REVOKE", "DENY", "TRUNCATE", "ROLLBACK", "COMMIT",
            "TRANSACTION", "SAVE", "SAVEPOINT"
        };

        public string Format(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return sql;

            var tokens = Tokenize(sql);
            var formatted = BuildFormatted(tokens);
            return formatted.TrimEnd();
        }

        private List<Token> Tokenize(string sql)
        {
            var tokens = new List<Token>();
            int i = 0;
            int len = sql.Length;

            while (i < len)
            {
                // Skip whitespace
                if (char.IsWhiteSpace(sql[i]))
                {
                    i++;
                    continue;
                }

                // Single-line comment
                if (i + 1 < len && sql[i] == '-' && sql[i + 1] == '-')
                {
                    int end = sql.IndexOf('\n', i);
                    if (end < 0) end = len;
                    tokens.Add(new Token(TokenType.Comment, sql[i..end].TrimEnd()));
                    i = end;
                    continue;
                }

                // Multi-line comment
                if (i + 1 < len && sql[i] == '/' && sql[i + 1] == '*')
                {
                    int end = sql.IndexOf("*/", i + 2);
                    if (end < 0) end = len - 2;
                    tokens.Add(new Token(TokenType.Comment, sql[i..(end + 2)]));
                    i = end + 2;
                    continue;
                }

                // Quoted string
                if (sql[i] == '\'' || sql[i] == 'N' && i + 1 < len && sql[i + 1] == '\'')
                {
                    int start = i;
                    if (sql[i] == 'N') i++;
                    i++; // skip opening quote
                    while (i < len)
                    {
                        if (sql[i] == '\'' && i + 1 < len && sql[i + 1] == '\'')
                        {
                            i += 2; // escaped quote
                            continue;
                        }
                        if (sql[i] == '\'') { i++; break; }
                        i++;
                    }
                    tokens.Add(new Token(TokenType.String, sql[start..i]));
                    continue;
                }

                // Quoted identifier [...]
                if (sql[i] == '[')
                {
                    int end = sql.IndexOf(']', i + 1);
                    if (end < 0) end = len - 1;
                    tokens.Add(new Token(TokenType.Identifier, sql[i..(end + 1)]));
                    i = end + 1;
                    continue;
                }

                // Quoted identifier "..."
                if (sql[i] == '"')
                {
                    int start = i;
                    i++;
                    while (i < len && sql[i] != '"') i++;
                    if (i < len) i++;
                    tokens.Add(new Token(TokenType.Identifier, sql[start..i]));
                    continue;
                }

                // Parentheses
                if (sql[i] == '(')
                {
                    tokens.Add(new Token(TokenType.OpenParen, "("));
                    i++;
                    continue;
                }
                if (sql[i] == ')')
                {
                    tokens.Add(new Token(TokenType.CloseParen, ")"));
                    i++;
                    continue;
                }

                // Comma
                if (sql[i] == ',')
                {
                    tokens.Add(new Token(TokenType.Comma, ","));
                    i++;
                    continue;
                }

                // Semicolon
                if (sql[i] == ';')
                {
                    tokens.Add(new Token(TokenType.Semicolon, ";"));
                    i++;
                    continue;
                }

                // Operators
                if (sql[i] == '=' || sql[i] == '<' || sql[i] == '>' || sql[i] == '!' || sql[i] == '+' || sql[i] == '-' || sql[i] == '*' || sql[i] == '/' || sql[i] == '%')
                {
                    int start = i;
                    i++;
                    if (i < len && (sql[i] == '=' || sql[i] == '>'))
                        i++;
                    tokens.Add(new Token(TokenType.Operator, sql[start..i]));
                    continue;
                }

                // Dot
                if (sql[i] == '.')
                {
                    tokens.Add(new Token(TokenType.Dot, "."));
                    i++;
                    continue;
                }

                // @ variable
                if (sql[i] == '@')
                {
                    int start = i;
                    i++;
                    while (i < len && (char.IsLetterOrDigit(sql[i]) || sql[i] == '_')) i++;
                    tokens.Add(new Token(TokenType.Variable, sql[start..i]));
                    continue;
                }

                // Number
                if (char.IsDigit(sql[i]))
                {
                    int start = i;
                    while (i < len && (char.IsDigit(sql[i]) || sql[i] == '.')) i++;
                    tokens.Add(new Token(TokenType.Number, sql[start..i]));
                    continue;
                }

                // Word (keyword or identifier)
                if (char.IsLetter(sql[i]) || sql[i] == '_' || sql[i] == '#')
                {
                    int start = i;
                    while (i < len && (char.IsLetterOrDigit(sql[i]) || sql[i] == '_' || sql[i] == '#')) i++;
                    var word = sql[start..i];
                    var type = AllKeywords.Contains(word) ? TokenType.Keyword : TokenType.Identifier;
                    tokens.Add(new Token(type, word));
                    continue;
                }

                // Anything else
                tokens.Add(new Token(TokenType.Other, sql[i].ToString()));
                i++;
            }

            return tokens;
        }

        private string BuildFormatted(List<Token> tokens)
        {
            var sb = new System.Text.StringBuilder();
            int indent = 0;
            int parenDepth = 0;
            bool newLineNeeded = false;
            bool isFirstToken = true;
            bool lineStarted = false; // true wenn bereits indent/newline geschrieben wurde
            int afterCommentIndent = -1; // >= 0: nächstes Token soll auf dieser Ebene eingerückt werden

            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                var upper = token.Value.ToUpperInvariant();

                // Comments: eigene Zeile, danach neue Zeile für nächsten Befehl
                if (token.Type == TokenType.Comment)
                {
                    int commentIndent = indent + 1;
                    if (lineStarted)
                    {
                        // Nach Komma/Zeilenumbruch bereits eingerückt - Kommentar direkt schreiben
                    }
                    else if (isFirstToken)
                    {
                        commentIndent = indent;
                        sb.Append(new string(' ', commentIndent * 4));
                    }
                    else
                    {
                        sb.AppendLine();
                        sb.Append(new string(' ', commentIndent * 4));
                    }
                    sb.Append(token.Value);
                    sb.AppendLine();
                    // Nächstes Token soll auf der gleichen Ebene weitermachen
                    afterCommentIndent = commentIndent;
                    newLineNeeded = false;
                    lineStarted = false;
                    isFirstToken = false;
                    continue;
                }

                // Check for multi-word keywords (look ahead)
                string multiWord = TryMatchMultiWord(tokens, i);

                // Nach Kommentar steht der Cursor bereits am Zeilenanfang
                bool atLineStart = afterCommentIndent >= 0;

                if (multiWord != null)
                {
                    var multiUpper = multiWord.ToUpperInvariant();

                    if (MajorClauses.Contains(multiUpper))
                    {
                        if (!isFirstToken && !lineStarted && !atLineStart) sb.AppendLine();
                        sb.Append(new string(' ', Math.Max(0, indent * 4)));
                        sb.Append(multiUpper);
                        newLineNeeded = true;
                        afterCommentIndent = -1;
                        lineStarted = false;

                        // Skip consumed tokens
                        int wordCount = multiUpper.Split(' ').Length;
                        i += wordCount - 1;
                        isFirstToken = false;
                        continue;
                    }

                    if (SubClauses.Contains(multiUpper))
                    {
                        if (!isFirstToken && !lineStarted && !atLineStart) sb.AppendLine();
                        sb.Append(new string(' ', (indent + 1) * 4));
                        sb.Append(multiUpper);
                        newLineNeeded = false;
                        afterCommentIndent = -1;
                        lineStarted = false;

                        int wordCount = multiUpper.Split(' ').Length;
                        i += wordCount - 1;
                        isFirstToken = false;
                        continue;
                    }
                }

                // Single-word clause check
                if (token.Type == TokenType.Keyword)
                {
                    if (MajorClauses.Contains(upper))
                    {
                        if (!isFirstToken && !lineStarted && !atLineStart) sb.AppendLine();
                        sb.Append(new string(' ', Math.Max(0, indent * 4)));
                        sb.Append(upper);
                        newLineNeeded = true;
                        afterCommentIndent = -1;
                        lineStarted = false;
                        isFirstToken = false;
                        continue;
                    }

                    if (SubClauses.Contains(upper) && parenDepth == 0)
                    {
                        if (!isFirstToken && !lineStarted && !atLineStart) sb.AppendLine();
                        sb.Append(new string(' ', (indent + 1) * 4));
                        sb.Append(upper);
                        newLineNeeded = false;
                        afterCommentIndent = -1;
                        lineStarted = false;
                        isFirstToken = false;
                        continue;
                    }
                }

                if (token.Type == TokenType.OpenParen)
                {
                    sb.Append(" (");
                    parenDepth++;
                    isFirstToken = false;
                    continue;
                }

                if (token.Type == TokenType.CloseParen)
                {
                    sb.Append(")");
                    parenDepth--;
                    isFirstToken = false;
                    continue;
                }

                if (token.Type == TokenType.Comma)
                {
                    sb.Append(",");
                    if (parenDepth == 0)
                    {
                        sb.AppendLine();
                        sb.Append(new string(' ', (indent + 1) * 4));
                        lineStarted = true;
                    }
                    else
                    {
                        sb.Append(" ");
                    }
                    isFirstToken = false;
                    continue;
                }

                if (token.Type == TokenType.Semicolon)
                {
                    sb.Append(";");
                    sb.AppendLine();
                    sb.AppendLine();
                    newLineNeeded = false;
                    isFirstToken = true;
                    continue;
                }

                if (token.Type == TokenType.Dot)
                {
                    sb.Append(".");
                    isFirstToken = false;
                    continue;
                }

                // Default: add space before token
                if (afterCommentIndent >= 0)
                {
                    // Nach Kommentar: auf gemerkter Einrückungsebene weitermachen
                    sb.Append(new string(' ', afterCommentIndent * 4));
                    afterCommentIndent = -1;
                    lineStarted = false;
                }
                else if (isFirstToken)
                {
                    sb.Append(new string(' ', indent * 4));
                }
                else if (newLineNeeded)
                {
                    sb.AppendLine();
                    sb.Append(new string(' ', (indent + 1) * 4));
                    newLineNeeded = false;
                }
                else if (lineStarted)
                {
                    // Bereits am Zeilenanfang mit korrektem Indent (nach Komma) - kein extra Space
                    lineStarted = false;
                }
                else
                {
                    // Don't add space after dot or before dot
                    var prev = i > 0 ? tokens[i - 1] : null;
                    if (prev?.Type != TokenType.Dot)
                        sb.Append(" ");
                }

                if (token.Type == TokenType.Keyword)
                    sb.Append(upper);
                else
                    sb.Append(token.Value);

                isFirstToken = false;
            }

            return sb.ToString();
        }

        private string? TryMatchMultiWord(List<Token> tokens, int start)
        {
            // Try matching 3-word, then 2-word compound keywords
            string?[] candidates = { TryWords(tokens, start, 3), TryWords(tokens, start, 2) };

            foreach (var candidate in candidates)
            {
                if (candidate != null && (MajorClauses.Contains(candidate) || SubClauses.Contains(candidate)))
                    return candidate;
            }

            return null;
        }

        private string? TryWords(List<Token> tokens, int start, int count)
        {
            if (start + count > tokens.Count) return null;

            var parts = new string[count];
            for (int j = 0; j < count; j++)
            {
                if (tokens[start + j].Type != TokenType.Keyword)
                    return null;
                parts[j] = tokens[start + j].Value;
            }

            return string.Join(" ", parts);
        }

        private enum TokenType
        {
            Keyword, Identifier, String, Number, Operator,
            OpenParen, CloseParen, Comma, Semicolon, Dot,
            Comment, Variable, Other
        }

        private record Token(TokenType Type, string Value);
    }
}
