Irony by default skips whitespace and the setting can only be changed grammar wide.
So a per Token setting is needed since we need to skip whitespace within embedded code, and keep all whitespace within literal text.

So the standard Irony code is tweaked in Scanner.cs and  _Terminal.cs
In _Terminal.cs a property is added
	public Boolean SkipsWhitespaceAfter = true;

While in Scanner.cs, A condition has been added to alternatively skip or not skip whitespace depending or Terminal.SkipsWhitespaceAfter
//Tweaked to use Terminal.SkipsWhitespaceAfter to alternately skip/unskip whitespace
if (Context.PreviousToken == null || Context.PreviousToken.Terminal == null || Context.PreviousToken.Terminal.SkipsWhitespaceAfter)
{
    //2. Skip whitespace.
    _grammar.SkipWhitespace(Context.Source);
}

