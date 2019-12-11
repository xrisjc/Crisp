# Crisp
A toy programming language interpreter.

# Grammar

    program -> expr_sequence ;
    expr_sequence -> expr* ;

    expr -> var
          | function
          | if
          | while
          | block
          | assignment ;

    var -> "var" id ":=" expr ;

    function -> "function" params expr ;
    params -> "(" id_list? ")" ;
    id_list -> id ( "," id )* ;

    block -> "begin" expr_sequence "end" ;

    if -> "if" expr "then" expr ("else" expr)? ;

    while -> "while" expr "do" expr ;

    assignment -> logical_or ( ":=" expr )* ;
    logical_or -> logical_and ( "or" logical_and )* ;
    logical_and -> equality ( "and" equality )* ;
    equality -> relation ( ( "=" | "<>" ) relation )* ;
    relation -> addition ( ( "<" | "<=" | ">" | ">=" ) addition )* ;
    addition -> multiplication ( ( "+" | "-" ) multiplication )* ;
    multiplication -> unary ( ( "*" | "/" | "mod" ) unary )* ;
    unary -> ( ( "-" | "not" ) unary ) | call ; 
    call -> primary ( "(" arguments ")" | "[" expr "]" )* ;
    primary -> number
             | string
             | "true"
             | "false"
             | id
             | "null"
             | "(" expr ")" ;
             | "write" args ;
    arguments -> expr ( "," expr )* ;
    number -> digit+ ( "." digit+ )? ;
    string -> "'" ([^'])* "'" ;
    id -> alpha ( alpha | digit )* ;
    alpha -> 'a' .. 'z' | 'A' .. 'Z' | '_' ;
    digit -> '0' .. '9' ;
