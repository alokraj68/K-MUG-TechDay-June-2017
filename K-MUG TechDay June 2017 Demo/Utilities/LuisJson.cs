using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace K_MUG_TechDay_June_2017_Demo.Utilities
{
    public class LuisJson
    {
        public string query { get; set; }
        public Topscoringintent topScoringIntent { get; set; }
        public Intent[] intents { get; set; }
        public Entity[] entities { get; set; }
        public string Json { get; set; }
    }

    public class Topscoringintent
    {
        public string intent { get; set; }
        public float score { get; set; }
    }

    public class Intent
    {
        public string intent { get; set; }
        public float score { get; set; }
    }

    public class Entity
    {
        public string entity { get; set; }
        public string type { get; set; }
        public int startIndex { get; set; }
        public int endIndex { get; set; }
        public float score { get; set; }
        public Resolution resolution { get; set; }
    }

    public class Resolution
    {
        public string unit { get; set; }
        public string value { get; set; }
    }


}