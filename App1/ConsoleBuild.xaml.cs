﻿using System;
using System.Collections.Generic;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YetAnotherScriptingLanguage;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace App1
{
    public sealed partial class ConsoleBuild : UserControl
    {
        List<String> inputTokens = new List<String>();
        public ConsoleBuild()
        {
            this.InitializeComponent();
            this.InputTokens.Items.VectorChanged += TokenItems_VectorChanged;
        }

        private void TokenItems_VectorChanged(IObservableVector<object> sender, IVectorChangedEventArgs @event)
        {
            if(@event.CollectionChange == CollectionChange.ItemRemoved && inputTokens.Count>0)
            {
                this.inputTokens.RemoveAt((int)@event.Index);
            }
            else if(@event.CollectionChange == CollectionChange.Reset)
            {
                this.inputTokens.Clear();
            }
        }

        public string Token
        {
            get
            {
                string token = inputTokens[0];
                inputTokens.RemoveAt(0);
                return token;
            }
        }

        public async void clear()
        {
            await this.InputTokens.ClearAsync();
        }

        public void HookEvents()
        {
            ((PrintProcess)Interpreter.Functions["PRINT"]).PrintHandler += Interpreter_PrintHandler1;
            ((ReadProcess)Interpreter.Functions["READ"]).ReadHandler += Interpreter_ReadHandler;
        }

        public void addText(string s)
        {
            this.BuiltConsole.Text += s;
        }

        

        private YetAnotherScriptingLanguage.variables.Variable Interpreter_ReadHandler(ReadProcess sender, YetAnotherScriptingLanguage.variables.Variable Argument)
        {
            var token = this.Token;
            addText(Argument.Value + (Argument.Value == "" ? "" : " "));
            addText(token + Environment.NewLine);
            var r = new YetAnotherScriptingLanguage.variables.Variable(token);
            return r;
        }



        private void Interpreter_PrintHandler1(PrintProcess sender, List<YetAnotherScriptingLanguage.variables.Variable> Arguments)
        {
            for (int i = 0; i < Arguments.Count; i++)
                addText(Arguments[i].Value + (i < Arguments.Count - 1 ? " " : Environment.NewLine));
        }

        private void InputTokens_TokenItemAdded(Microsoft.Toolkit.Uwp.UI.Controls.TokenizingTextBox sender, object args)
        {
            this.inputTokens.Add(args as string);
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            this.BuiltConsole.Text = "**********************************************************************" +  Environment.NewLine +
                                     "** AYSL CLI Runtime                                                                       **" + Environment.NewLine +
                                     "** Where every bug is shroedinger's bug                                      **" + Environment.NewLine +
                                     "**********************************************************************" + Environment.NewLine +
                                     "Entry Point >>" + Environment.NewLine;
            clear();
        }

    }
}
