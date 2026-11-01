using BluePrint.Compiler;

var data = File.ReadAllText("test.bp");

{
    Tokenizer tokenizer = new()
    {
        Stream = new(data),
    };
    while (tokenizer.Next() is var token and not Token.EOF)
    {
        Console.WriteLine(token);
    }
}

Parser parser = new()
{
    Tokenizer = new()
    {
        Stream = new(data)
    }
};
Console.WriteLine(parser.Parse());
