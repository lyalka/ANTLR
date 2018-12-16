using System;
using Antlr4.Runtime;
using AntlrTest;
using Microsoft.Extensions.Logging;
using SimpleGrammar;

namespace ScriptExecutor.Client.Infrastructure
{
    public class ScriptHost
    {
        private readonly string _code;
        private readonly ILogger<ScriptHost> _logger;

        public ScriptHost(string code, ILogger<ScriptHost> logger)
        {
            _code = code;
            _logger = logger;
        }

        public void ExecuteCode()
        {
            
            var inputStream = new AntlrInputStream(_code);
            var speakLexer = new SimpleGrammarLexer(inputStream);
            var commonTokenStream = new CommonTokenStream(speakLexer);
            var parser = new SimpleGrammarParser(commonTokenStream);

            parser.AddErrorListener(new EL(_logger));

            var chatContext = parser.calc();

            var visitor = new SampleVisitor(_logger);
            try
            {
                visitor.Visit(chatContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            _logger.LogDebug(visitor.ReturnResult.Value?.ToString() ?? "");

        }

        public class EL : IAntlrErrorListener<IToken>
        {
            private readonly ILogger _logger;

            public EL(ILogger logger)
            {
                _logger = logger;
            }

            public void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine,
                string msg,
                RecognitionException e)
            {
                LoggerExtensions.LogError(_logger, string.Format("Error at {0}:{1} - {2}", line, charPositionInLine, msg));

            }
        }
    }
}