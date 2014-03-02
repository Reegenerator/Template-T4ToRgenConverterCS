// -------------------------------------------------------
// Automatically generated with Kodeo's Reegenerator for NON-COMMERCIAL USE
// Generator: RgenTemplate (internal)
// Generation date: 2014-03-02 01:41
// Generated by: GATSU-DEV\Tedy.Pranolo
// -------------------------------------------------------
namespace T4ToRgen
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Reegenerator", "2.0.5.0")]
    [Kodeo.Reegenerator.Generators.TemplateDisplayAttribute(DisplayName="T4ToRgenCS", Description="Converts a C# T4 Template to a Reegenator Template", HideInDialog=false)]
    [Kodeo.Reegenerator.Generators.DefaultExtensionAttribute(Extension=".txt")]
    public partial class T4ToRgenCs : CodeRendererEx
    {
        
        /// <summary>
        ///Renders the code as defined in the source script file.
        ///</summary>
        ///<param name="rgenFilename"></param>
        ///<param name="codeFilename"></param>
        ///<param name="references"></param>
        public virtual void GenReadme(string rgenFilename, string codeFilename, string[] references)
        {
            this.Output.Write("File ");
            this.Output.Write( rgenFilename );
            this.Output.Write(" and ");
            this.Output.Write( codeFilename );
            this.Output.Write(" has been generated.\r\nThese references have been added to the project\r\n");
 
	foreach(var r in references){
		
            this.Output.Write("\r\n");
            this.Output.Write(r );
            this.Output.Write("\r\n\t\t");

	}

            this.Output.WriteLine();
        }
    }
}
