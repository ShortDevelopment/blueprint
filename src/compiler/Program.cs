using BluePrint.Compiler.CodeGen;
using BluePrint.Compiler.Parser;
using System.Xml;

if (args is not [var sourceFile])
{
    Console.WriteLine("Usage: blueprint <source-file>");
    return;
}

var source = File.ReadAllText(sourceFile);

Console.OutputEncoding = System.Text.Encoding.UTF8;

using var writer = XmlWriter.Create(Console.Out);
using XmlCodeGenerator generator = new(writer);
generator.Visit(Parser.Parse(source));
