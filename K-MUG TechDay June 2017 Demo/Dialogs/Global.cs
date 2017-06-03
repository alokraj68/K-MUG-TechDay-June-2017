using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace K_MUG_TechDay_June_2017_Demo.Dialogs
{
    public class Global
    {
        public static async Task TypingDelay(int seconds)
        {
            await Task.Delay(seconds);
        }
    }
}