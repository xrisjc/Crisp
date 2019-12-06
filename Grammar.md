
    program -> program_item* ;
    program_item -> function
                  | expr ;

    function -> "function" id params block ;
    
    params -> "(" id_list? ")" ;
    id_list -> id ( "," id )* ;

    expr -> var
          | if
          | while
          | block
          | assignment ;

    var -> "var" id ":=" expr ;

    if -> "if" expr block
          ("else" "if" expr block)*
          ("else" block)? ;

    while -> "while" expr block ;

    block -> "{" expr* "}" ;

    assignment -> logical_or ( ":=" expr )* ;
    logical_or -> logical_and ( "or" logical_and )* ;
    logical_and -> equality ( "and" equality )* ;
    equality -> relation ( ( "=" | "<>" ) relation )* ;
    relation -> addition ( ( "<" | "<=" | ">" | ">=" ) addition )* ;
    addition -> multiplication ( ( "+" | "-" ) multiplication )* ;
    multiplication -> unary ( ( "*" | "/" ) unary )* ;
    unary -> ( ( "-" | "not" ) expr ) | primary ;
    primary -> id args
             | number
             | string
             | "true"
             | "false"
             | id
             | "null"
             | "this"
             | "(" expr ")" ;
             | "write" args ;
    args -> "(" expr* ")" ;
    number -> digit+ ( "." digit+ )? ;
    string -> "'" ([^'])* "'" ;
    id -> alpha ( alpha | digit )* ;
    alpha -> 'a' .. 'z' | 'A' .. 'Z' | '_' ;
    digit -> '0' .. '9' ;
