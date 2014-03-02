using System.Collections.Generic;
using System;
using System.Linq;


using Irony.Parsing;


namespace T4ToRgen
{
    public class EmbeddedStringLiteral : StringLiteral
    {
        public EmbeddedStringLiteral(string name)
            : base(name)
        {
        }
        public EmbeddedStringLiteral(string name, string startEndSymbol, StringOptions options)
            : base(name, startEndSymbol, options)
        {
        }
        public EmbeddedStringLiteral(string name, string startEndSymbol, StringOptions options, Type astNodeType)
            : base(name, startEndSymbol, options, astNodeType)
        {
        }
        public EmbeddedStringLiteral(string name, string startEndSymbol, StringOptions options, Irony.Ast.AstNodeCreator astNodeCreator)
            : base(name, startEndSymbol, options, astNodeCreator)
        {
        }
    }
}
