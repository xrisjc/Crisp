# Crisp
A toy programming language interpreter.

# Grammar

    program -> expr* ;
    expr -> var
          | function
          | if
          | while
          | block
          | assignment ;

    var -> "var" id ":=" expr ;

    function -> "function" params block ;
    params -> "(" id_list? ")" ;
    id_list -> id ( "," id )* ;

    block -> "{" expr* "}" ;

    if -> "if" expr block
          ("else" "if" expr block)*
          ("else" block)? ;

    while -> "while" expr block ;

    assignment -> logical_or ( ":=" expr )* ;
    logical_or -> logical_and ( "or" logical_and )* ;
    logical_and -> equality ( "and" equality )* ;
    equality -> relation ( ( "=" | "<>" ) relation )* ;
    relation -> addition ( ( "<" | "<=" | ">" | ">=" ) addition )* ;
    addition -> multiplication ( ( "+" | "-" ) multiplication )* ;
    multiplication -> unary ( ( "*" | "/" ) unary )* ;
    unary -> ( ( "-" | "not" ) expr ) | primary ;
    primary -> call
             | number
             | string
             | "true"
             | "false"
             | id
             | "null"
             | "(" expr ")" ;
             | "write" args ;
    call -> id args
    args -> "(" expr* ")" ;
    number -> digit+ ( "." digit+ )? ;
    string -> "'" ([^'])* "'" ;
    id -> alpha ( alpha | digit )* ;
    alpha -> 'a' .. 'z' | 'A' .. 'Z' | '_' ;
    digit -> '0' .. '9' ;
