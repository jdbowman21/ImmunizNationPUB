using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImmunizNation.Client
{
    public class AppSettings
    {
        /// <summary>
        /// Requried score to pass the knowledge test.
        /// </summary>
        public double RequiredKnowledgeTestScore { get; set; }

        /// <summary>
        /// Number of allowed records per table.
        /// </summary>
        public int PageSize { get; set; }
    }
}
