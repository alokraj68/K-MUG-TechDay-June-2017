using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.FormFlow;

namespace K_MUG_TechDay_June_2017_Demo.Dialogs
{
    [Serializable]
    public class Question
    {
        [Prompt("Let me know how can I help?")]
        public string questionFromUser { get; set; }

    }
}