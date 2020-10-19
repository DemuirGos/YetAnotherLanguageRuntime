﻿using System;
using System.Collections.Generic;
using System.Text;

namespace YetAnotherScriptingLanguage
{
    class Action : Function
    {
        public Action(String action)
        {
            Operator = action;
        }
        public String Operator { get;}
        public Boolean isValidAction {
            get => this.Operator == "*"  || this.Operator == "/"  || 
                   this.Operator == "+"  || this.Operator == "-"  ||
                   this.Operator == "|"  || this.Operator == "&"  ||
                   this.Operator == "<"  || this.Operator == ">"  ||
                   this.Operator == "<>" || this.Operator == "="  ||
                   this.Operator == ":=" || this.Operator == "^"  ||
                   this.Operator == "%";
        }
        public Int16 Priority
        {
            get {
                switch (this.Operator)
                {
                    case "^" : return 10; 
                    case "*" :  
                    case "%" : 
                    case "/" : return 9;
                    case "+" :
                    case "-" : return 8;
                    case "<" :
                    case ">" :
                    case "=" :
                    case "<>": return 7;
                    case "|" :
                    case "&" : return 6;
                    case ":=": return 5;
                    default  : return 0;
                }
            }
        }
    }
}
