using System;
using System.Collections.Generic;
using System.Linq;

namespace HalsteadAnalyzer.Core;

/// <summary>
/// Синтаксический анализатор, классифицирующий токены JavaScript на операторы
/// и операнды по Холстеду и вычисляющий метрики.
/// </summary>
public static class HalsteadCalculator
{
    private static readonly HashSet<string> LiteralKeywords = new(StringComparer.Ordinal)
    { "true", "false", "null" };

    private static readonly HashSet<string> NonOperatorKeywords = new(StringComparer.Ordinal)
    { "static", "as", "from", "async", "get", "set", "debugger" };

    private static readonly HashSet<string> ValueIdentifiers = new(StringComparer.Ordinal)
    { "undefined", "NaN", "Infinity" };

    public static HalsteadResult Analyze(string source)
    {
        var tokens = new JsTokenizer(source).Tokenize();
        var toks = tokens.Where(t => t.Kind != TokenKind.Eof).ToList();

        BuildBracketMaps(toks, out var matchClose, out var matchOpen);

        var operators = new Dictionary<string, int>(StringComparer.Ordinal);
        var operands = new Dictionary<string, int>(StringComparer.Ordinal);

        void AddOp(string s) => operators[s] = operators.GetValueOrDefault(s) + 1;
        void AddOperand(string s) => operands[s] = operands.GetValueOrDefault(s) + 1;

        for (int i = 0; i < toks.Count; i++)
        {
            var t = toks[i];

            switch (t.Kind)
            {
                case TokenKind.Number:
                case TokenKind.String:
                case TokenKind.Template:
                case TokenKind.Regex:
                    AddOperand(t.Text);
                    break;

                case TokenKind.Keyword:
                    if (LiteralKeywords.Contains(t.Text))
                        AddOperand(t.Text);
                    else if (t.Text == "this")
                        AddOperand(t.Text);
                    else if (NonOperatorKeywords.Contains(t.Text))
                        { /* пропускаем */ }
                    else
                        AddOp(t.Text);
                    break;

                case TokenKind.Identifier:
                {
                    var nxt = Next(toks, i);
                    bool followedByCall = nxt != null && nxt.Kind == TokenKind.Punctuator && nxt.Text == "(";

                    if (followedByCall)
                    {
                        // ИСПРАВЛЕНИЕ: ИМЯ вызываемой ИЛИ объявляемой функции учтется при обработке «(»
                        // Само имя здесь в операнды мы не добавляем ни в каком из этих случаев,
                        // чтобы не было лишних операндов при объявлении функции.
                    }
                    else
                    {
                        AddOperand(t.Text);
                    }
                    break;
                }

                case TokenKind.Punctuator:
                    HandlePunctuator(toks, i, matchClose, matchOpen, AddOp, AddOperand);
                    break;
            }
        }

        return BuildResult(operators, operands);
    }

    private static void HandlePunctuator(
        List<Token> toks, int i, 
        Dictionary<int, int> matchClose, 
        Dictionary<int, int> matchOpen,
        Action<string> addOp, Action<string> addOperand)
    {
        string s = toks[i].Text;
        switch (s)
        {
            case "(":
            {
                var prev = Prev(toks, i);
                bool prevIsName = prev != null && prev.Kind == TokenKind.Identifier;
                bool prevIsResult = prev != null && prev.Kind == TokenKind.Punctuator &&
                                    (prev.Text == ")" || prev.Text == "]");

                if (prevIsName || prevIsResult)
                {
                    // ИСПРАВЛЕНИЕ: Это вызов ИЛИ объявление функции.
                    // Отдельный оператор "( )" НЕ добавляется. Скобки становятся частью "name()".
                    string calleeName = prevIsName ? prev!.Text : "<expr>";
                    addOp(calleeName + "()");

                    // В операнды добавляем ТОЛЬКО если это вызов ВНУТРИ выражения (и это не объявление функции)
                    bool isDecl = prevIsName && IsPrecededByFunction(toks, IndexOfPrev(toks, i));
                    if (!isDecl && IsCallPartOfExpression(toks, i, matchClose, matchOpen))
                    {
                        addOperand(calleeName);
                    }
                }
                else
                {
                    // Обычные группирующие скобки или скобки конструкций (if, for, и т.д.)
                    addOp("( )");
                }
                break;
            }
            case "[": addOp("[ ]"); break;
            case "{": addOp("{ }"); break;
            case ")":
            case "]":
            case "}":
                break; // Пара учтена на открывающей скобке
            default:
                addOp(s);
                break;
        }
    }

    private static bool IsCallPartOfExpression(List<Token> toks, int lparen, 
        Dictionary<int, int> matchClose, Dictionary<int, int> matchOpen)
    {
        int calleeIdx = IndexOfPrev(toks, lparen);
        int chainStart = calleeIdx;

        while (true)
        {
            int p = IndexOfPrev(toks, chainStart);
            if (p >= 0 && toks[p].Kind == TokenKind.Punctuator &&
                (toks[p].Text == "." || toks[p].Text == "?."))
            {
                int q = IndexOfPrev(toks, p);
                if (q >= 0) 
                { 
                    if (toks[q].Kind == TokenKind.Punctuator && 
                        (toks[q].Text == ")" || toks[q].Text == "]" || toks[q].Text == "}"))
                    {
                        if (matchOpen.TryGetValue(q, out int openIdx))
                        {
                            chainStart = openIdx;
                            continue;
                        }
                    }
                    chainStart = q; 
                    continue; 
                }
            }
            break;
        }

        var before = chainStart > 0 ? PrevFrom(toks, chainStart) : null;

        Token? after = null;
        if (matchClose.TryGetValue(lparen, out int close))
            after = Next(toks, close);

        return IsValueContext(before) || ConsumesValue(after);
    }

    private static bool IsValueContext(Token? t)
    {
        if (t == null) return false;
        if (t.Kind == TokenKind.Keyword)
            return t.Text is "return" or "case" or "new" or "typeof" or "instanceof"
                or "in" or "of" or "throw" or "yield" or "await" or "delete" or "void";
        if (t.Kind == TokenKind.Punctuator)
        {
            switch (t.Text)
            {
                case "(": case "[": case ",": case ":": case "?":
                case "=>": case "...":
                case "=": case "+=": case "-=": case "*=": case "/=": case "%=":
                case "**=": case "&=": case "|=": case "^=": case "<<=": case ">>=":
                case ">>>=": case "&&=": case "||=": case "??=":
                case "+": case "-": case "*": case "/": case "%": case "**":
                case "<": case ">": case "<=": case ">=":
                case "==": case "===": case "!=": case "!==":
                case "&&": case "||": case "??":
                case "&": case "|": case "^": case "<<": case ">>": case ">>>":
                case "!": case "~":
                    return true;
                default:
                    return false;
            }
        }
        return false;
    }

    private static bool ConsumesValue(Token? t)
    {
        if (t == null) return false;
        if (t.Kind == TokenKind.Punctuator)
        {
            switch (t.Text)
            {
                case ".": case "?.": case "[": case "(":
                case ")": case "]": case ",":
                case "?": case ":":
                case "+": case "-": case "*": case "/": case "%": case "**":
                case "<": case ">": case "<=": case ">=":
                case "==": case "===": case "!=": case "!==":
                case "&&": case "||": case "??":
                case "&": case "|": case "^": case "<<": case ">>": case ">>>":
                    return true;
                default:
                    return false;
            }
        }
        if (t.Kind == TokenKind.Keyword)
            return t.Text is "instanceof" or "in";
        return false;
    }

    private static Token? Next(List<Token> toks, int i) => i + 1 < toks.Count ? toks[i + 1] : null;
    private static Token? Prev(List<Token> toks, int i) => i - 1 >= 0 ? toks[i - 1] : null;
    private static int IndexOfPrev(List<Token> toks, int i) => i - 1;
    private static Token? PrevFrom(List<Token> toks, int idx) => idx - 1 >= 0 ? toks[idx - 1] : null;

    private static bool IsPrecededByFunction(List<Token> toks, int identIdx)
    {
        var p = PrevFrom(toks, identIdx);
        return p != null && p.Kind == TokenKind.Keyword && p.Text == "function";
    }

    private static void BuildBracketMaps(List<Token> toks, 
        out Dictionary<int, int> matchClose, 
        out Dictionary<int, int> matchOpen)
    {
        matchClose = new Dictionary<int, int>();
        matchOpen = new Dictionary<int, int>();
        var stack = new Stack<int>();
        
        for (int i = 0; i < toks.Count; i++)
        {
            if (toks[i].Kind != TokenKind.Punctuator) continue;
            switch (toks[i].Text)
            {
                case "(": case "[": case "{":
                    stack.Push(i);
                    break;
                case ")": case "]": case "}":
                    if (stack.Count > 0) 
                    { 
                        int open = stack.Pop(); 
                        matchClose[open] = i; 
                        matchOpen[i] = open;
                    }
                    break;
            }
        }
    }

    private static HalsteadResult BuildResult(
        Dictionary<string, int> operators, Dictionary<string, int> operands)
    {
        if (operators.TryGetValue("else", out int elseCount))
        {
            operators.Remove("else");
            if (operators.TryGetValue("if", out int ifCount))
            {
                int remainingIf = ifCount - elseCount;
                if (remainingIf > 0)
                    operators["if"] = remainingIf;
                else
                    operators.Remove("if");

                operators["if...else"] = elseCount; 
            }
            else
            {
                operators["if...else"] = elseCount; 
            }
        }

        var res = new HalsteadResult();

        int j = 1;
        foreach (var kv in operators.OrderByDescending(k => k.Value).ThenBy(k => k.Key, StringComparer.Ordinal))
            res.Operators.Add(new FrequencyEntry(j++, kv.Key, kv.Value));

        int idx = 1;
        foreach (var kv in operands.OrderByDescending(k => k.Value).ThenBy(k => k.Key, StringComparer.Ordinal))
            res.Operands.Add(new FrequencyEntry(idx++, kv.Key, kv.Value));

        res.Eta1 = operators.Count;
        res.Eta2 = operands.Count;
        res.N1 = operators.Values.Sum();
        res.N2 = operands.Values.Sum();
        return res;
    }
}