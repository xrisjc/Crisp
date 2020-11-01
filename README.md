# Crisp
A toy programming language interpreter.

# Grammar

    program -> expr_sequence ;
    expr_sequence -> expr* ;

    expr -> let
          | function
          | if
          | while
          | for
          | block
          | assignment ;

    let -> "let" id "=" expr+ ;

    block -> "{" expr_sequence "}" ;

    function -> "fn" params block ;
    params -> "(" id_list? ")" ;
    id_list -> id ( "," id )* ;

    if -> "if" expr block ( "else" "if" expr block )* ("else" block)? ;
    while -> "while" expr block ;

    assignment -> logical_or ( "=" expr )* ;
    logical_or -> logical_and ( "||" logical_and )* ;
    logical_and -> equality ( "&&" equality )* ;
    equality -> relation ( ( "==" | "!=" ) relation )* ;
    relation -> addition ( ( "<" | "<=" | ">" | ">=" | "is" ) addition )* ;
    addition -> multiplication ( ( "+" | "-" ) multiplication )* ;
    multiplication -> unary ( ( "*" | "/" | "mod" ) unary )* ;
    unary -> ( ( "-" | "!" ) unary ) | call ; 
    call -> primary ( "(" arguments? ")" | "[" expr "]" | "." id )* ;
    arguments -> expr ( "," expr )* ;
    primary -> number
             | string
             | "true"
             | "false"
             | id
             | "null"
             | "[" listItems? "]"
             | "(" expr ")" ;
             | "write" arguments
             | "#" expr ;
    listItems -> expr ( "," expr )* ;
    number -> digit+ ( "." digit+ )? ;
    string -> "\"" ([^"])* "\"" ;
    id -> alpha ( alpha | digit )* ;
    alpha -> 'a' .. 'z' | 'A' .. 'Z' | '_' ;
    digit -> '0' .. '9' ;
