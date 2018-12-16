using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.Extensions.Logging;
using Type = AntlrTest.Type;

namespace SimpleGrammar
{
    public class SampleVisitor : SimpleGrammarBaseVisitor<ScopeContext>
    {
        public ScopeContext ReturnResult { get; private set; }

        readonly ILogger _logger;

        public SampleVisitor(ILogger logger)
        {
            _logger = logger;
        }

        public Dictionary<string, ScopeContext> Variables = new Dictionary<string, ScopeContext>();

        public Dictionary<string, IParseTree> Functions = new Dictionary<string, IParseTree>();
        
        public override ScopeContext VisitLiteralExpression(SimpleGrammarParser.LiteralExpressionContext context)
        {
            if (string.IsNullOrWhiteSpace(context.NUMBER()?.GetText()) == false)
            {
                var n = double.Parse(context.NUMBER().GetText().Trim());
                return new ScopeContext() { Value = n };
            }
            else 
            {
                var n = context.STRING().GetText().Trim('"');
                return new ScopeContext() { Value = n };

            }
        }

        public override ScopeContext VisitParent(SimpleGrammarParser.ParentContext context)
        {
            return base.Visit(context.GetRuleContext<SimpleGrammarParser.ExpressionContext>(0));
        }

        public override ScopeContext VisitUnaryOperation(SimpleGrammarParser.UnaryOperationContext context)
        {
            var r = Visit(context.GetRuleContext<SimpleGrammarParser.ExpressionContext>(0)).AsDouble();

            double res = 0;

            var op = context.OPERATOR_P1();
            switch (op.GetText().Trim())
            {
                case "+":
                    res = +r;
                    break;
                case "-":
                    res = -r;
                    break;
            }

            return new ScopeContext() { Value =  res };
        }

        public override ScopeContext VisitBinaryOperation([NotNull] SimpleGrammarParser.BinaryOperationContext context)
        {
            var l = Visit(context.GetRuleContext<SimpleGrammarParser.ExpressionContext>(0)).AsDouble();
            var r = Visit(context.GetRuleContext<SimpleGrammarParser.ExpressionContext>(1)).AsDouble();


            double res = 0;

            var op = context.OPERATOR_P0() ?? context.OPERATOR_P1();
            if (op != null)
            {
                switch (op.GetText().Trim())
                {
                    case "+":
                        res = l + r;
                        break;
                    case "-":
                        res = l - r;
                        break;
                    case "*":
                        res = l * r;
                        break;
                    case "/":
                        res = l / r;
                        break;
                }

            }
            else
            {
                var lop = context.OPERATOR_L0() ?? context.OPERATOR_L1();

                switch (lop.GetText().Trim())
                {
                    case ">":
                        res = l > r ? 1 : 0;
                        break;
                    case "<":
                        res = l < r ? 1 : 0;
                        break;
                    case ">=":
                        res = l >= r ? 1 : 0;
                        break;
                    case "<=":
                        res = l <= r ? 1 : 0;
                        break;
                    case "==":
                        res = Math.Abs((double)(l - r)) < 0.0001 ? 1 : 0;
                        break;
                    case "!=":
                        res = Math.Abs((double)(l - r)) > 0.0001 ? 1 : 0;
                        break;
                }

            }

            return new ScopeContext() { Value = res };
        }

        public override ScopeContext VisitIfElse(SimpleGrammarParser.IfElseContext context)
        {
            var ex = context.expression();
            var res = Visit(ex);
            if (res.AsBool())
            {
                return Visit(context.GetRuleContext<SimpleGrammarParser.StatementListContext>(0));
            }
            else
            {
                var s = context.GetRuleContext<SimpleGrammarParser.StatementListContext>(1);
                if (s != null)
                {
                    return Visit(s);
                }
            }
            return null;
        }

        public override ScopeContext VisitFunctionCall(SimpleGrammarParser.FunctionCallContext context)
        {
            var name = context.VARIABLE().GetText().Trim();
            var parameters = context.GetRuleContexts<SimpleGrammarParser.ExpressionContext>();
            var values = parameters.Select(x => Visit(x)).ToArray();
            Action<object[]> method = null;
            switch (name)
            {
                case "log":
                    method = a => _logger.LogDebug($"{context.Start.Line}:{context.Start.Column}> {a[0]}");
                    break;
                default:
                    throw new Exception($"Unknown function {name}");
            }
            method.Invoke(values);
            IRecognizer r;
            return null;

        }


        public override ScopeContext VisitReturn(SimpleGrammarParser.ReturnContext context)
        {
            var exp = context.GetRuleContext<SimpleGrammarParser.ExpressionContext>(0);

            var res = Visit(exp);
            if(ReturnResult == null)
            ReturnResult = res;

            return res;
        }

        public override ScopeContext VisitVariable(SimpleGrammarParser.VariableContext context)
        {
            var id = context.VARIABLE().GetText().Trim();

            return Variables.ContainsKey(id) ? Variables[id] : null;
        }

        public override ScopeContext VisitVariableDefinition(SimpleGrammarParser.VariableDefinitionContext context)
        {
            var id = context.VARIABLE().GetText().Trim();
            if (context.ASSIGN() != null)
            {
                var e = Visit(context.expression());
                Variables[id] = e;
                return e;
            }
            Variables[id] = null;
            return null;
        }

    }

    public class ScopeContext
    {
        public object Value { get; set; }

        public Type Type { get; set; }

        public double AsDouble()
        {
            double.TryParse((Value ?? "").ToString(), out double res);

            return res;
        }
        
        public bool AsBool()
        {
            if (Value is bool b)
                return b;

            if (Value == null)
                return false;

            if(bool.TryParse((Value ?? "").ToString(), out bool boolRes))
                return boolRes;

            if(decimal.TryParse((Value ?? "").ToString(), out decimal res))
                return res != 0;

            return string.IsNullOrWhiteSpace(Value.ToString()) == false;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}