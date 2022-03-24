

console.log("Validating...");

// Validation returns true if the string includes one of our bad words.
const Validation = (x) => {

    // Convert to lower case
    let input = x.toLowerCase();


    const badWords = [
        "123",
        " add ",
        "add ",
        " add",
        "alert",
        "alter",
        "begin",
        "body",
        "cast",
        "char",
        "checkpoint",
        "click",
        "cookie",
        "commit",
        "create",
        "cursor",
        "database",
        "delete",
        "describe",
        "deny",
        "document",
        "drop",
        "error",
        "exec",
        "execute",
        "focus",
        "footer",
        "fetch",
        "from",
        "form",
        "grant ",
        " grant",
        "group",
        "header",
        "href",
        "html",
        "img",
        "index",
        "insert",
        "json",
        "join",
        "kill",
        "like",
        "link",
        "load",
        "localhost",
        "null",
        "onmouse",
        "onload",
        "onchange",
        "open",
        "order",
        "pass",
        "password",
        "replace",
        "rollback",
        "savepoint",
        "script",
        "select",
        "section",
        "set",
        "show",
        "string",
        "storage",
        "submit",
        "svg",
        "table",
        "test",
        "then",
        "truncate",
        "update",
        "use",
        "value",
        "where",
        ".css",
        ".exe",
        ".htm",
        ".js",
        ".ps",
        ".py",
        "fuck",
        "shit",
        "cunt",
        "bitch",
        "whore",
        "slut",
        "bastard"
    ];


    for(let i=0; i < badWords.length; i++){
        if( input.includes(badWords[i])){
            return true;
        }
    }

    return false;


}

export default Validation;