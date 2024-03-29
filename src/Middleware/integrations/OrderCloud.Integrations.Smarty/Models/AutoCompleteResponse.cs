﻿using System.Collections.Generic;

namespace OrderCloud.Integrations.Smarty.Models
{
    public class AutoCompleteResponse
    {
        public List<AutoCompleteSuggestion> suggestions { get; set; }
    }

    public class AutoCompleteSuggestion
    {
        public string street_line { get; set; }

        public string secondary { get; set; }

        public string city { get; set; }

        public string state { get; set; }

        public string zipcode { get; set; }

        public int entries { get; set; }
    }
}
