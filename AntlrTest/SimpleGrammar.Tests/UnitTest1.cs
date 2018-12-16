using System;
using Antlr4.Runtime;
using Microsoft.Extensions.Logging;
using SimpleGrammar;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace SimpleGrammar.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void ReturnStatemntParseNumberExpression()
        {
            var r = RunCode(@"
                
                return 1;
            ");

            Assert.Equal(1.0, r.Value);
        }

        [Fact]
        public void ReturnStatemntParseStringExpression()
        {
            var r = RunCode("return \"HELLO\";");

            Assert.Equal("HELLO", r.Value);
        }

        public ScopeContext RunCode(string code)
        {
            var logger = NullLogger.Instance;

            var inputStream = new AntlrInputStream(code);
            var speakLexer = new SimpleGrammarLexer(inputStream);
            var commonTokenStream = new CommonTokenStream(speakLexer);
            var parser = new SimpleGrammarParser(commonTokenStream);

            parser.AddErrorListener(new EL());

            var chatContext = parser.calc();

            var visitor = new SampleVisitor(logger);

            visitor.Visit(chatContext);

            return visitor.ReturnResult;
        }

        public class EL : IAntlrErrorListener<IToken>
        {

            public void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine,
                string msg,
                RecognitionException e)
            {
                throw new Exception(string.Format("Error at {0}:{1} - {2}", line, charPositionInLine, msg));

            }
        }
    }
}
