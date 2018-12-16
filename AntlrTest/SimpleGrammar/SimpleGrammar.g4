grammar SimpleGrammar;

calc        : (  statement )*? EOF;

comment     : COMMENT;


expression  :
              VARIABLE LPAR RPAR                                      #functionCall
            | VARIABLE LPAR (expression ',')*?expression RPAR         #functionCall
            | OPERATOR_P1 expression                                  #unaryOperation
            | LPAR expression RPAR                                    #parent
            | expression OPERATOR_P0 expression                       #binaryOperation
            | expression OPERATOR_P1 expression                       #binaryOperation
            | expression OPERATOR_L0 expression                       #binaryOperation
            | expression OPERATOR_L1 expression                       #binaryOperation
            | VARIABLE                                                #variable
            | NUMBER                                                 #literalExpression
            | STRING                                                 #literalExpression
            
            ;

variableDefinition:
                   VAR VARIABLE ASSIGN expression
                  |VAR VARIABLE
            ;

functionPrototype: 
                  FUNC_DECL FUNC_NAME LPAR RPAR
                | FUNC_DECL FUNC_NAME LPAR (VARIABLE DELIM)+ RPAR
                ;

functionDefinition: functionPrototype BEGIN statement* END
                  ;

ifElse       :
               IF LPAR expression RPAR BEGIN statementList END
              |IF LPAR expression RPAR BEGIN statementList END ELSE BEGIN statementList END
             ;

statement   :   expression EMPTY_OP 
              | variableDefinition EMPTY_OP 
              | return EMPTY_OP 
              | ifElse 
              | comment
            ;

statementList: statement+;

return: RETURN expression; 
 
/*
LEXER RULES
*/

fragment DIGIT                    : [0-9];
fragment ALPHA                    : [a-z] | [A-Z] ;
fragment UNDERSCORE               : '_';
fragment DOLLAR                   : '$';
fragment TEXT                     : .+? ;
fragment SINLE_LINE_COMMENT_START : ('//' | '#' | '--'); 
fragment MULTI_LINE_COMMENT_START : '/*'; 
fragment MULTI_LINE_COMMENT_END   : '*/'; 

fragment IDENTIFIER_PART          : ALPHA | UNDERSCORE | DIGIT;
fragment IDENTIFIER_START         : ALPHA | UNDERSCORE | DOLLAR;
fragment IDENTIFIER               : IDENTIFIER_START IDENTIFIER_PART*;

IF                       : 'if';
ELSE                     : 'else';
RETURN                   : 'return';
DELIM                             : ',';

WHITESPACE               : (' ' | '\t') -> skip;
SPACES                   : WHITESPACE+? -> skip;
EMPTY                    : SPACES NEWLINE+ -> skip;
NEWLINE                  : ('\r'? '\n' | '\r')+ -> skip;

LPAR                     : '(' ;

RPAR                     : ')' ;

VAR                      :'var' WHITESPACE;

FUNC_DECL              : 'function' WHITESPACE; 

VARIABLE               : IDENTIFIER; 

FUNC_NAME              : IDENTIFIER; 

EMPTY_OP               :';';

LAMBDA                 :'=>';

BEGIN                  :'{';

END                    :'}';

ASSIGN                 : '=';

OPERATOR_P0            : ('*'|'/');

OPERATOR_P1            : ('+'|'-');

OPERATOR_L0            : ('>'|'<'|'>='|'<='|'=='|'!=');

OPERATOR_L1            : ('&&'|'||');

NUMBER                 : DIGIT+ ([.] DIGIT+)*;

STRING                 : '"' TEXT '"';

LITERAL                : NUMBER;

COMMENT                : (SINLE_LINE_COMMENT_START TEXT NEWLINE) | (MULTI_LINE_COMMENT_START TEXT MULTI_LINE_COMMENT_END);
