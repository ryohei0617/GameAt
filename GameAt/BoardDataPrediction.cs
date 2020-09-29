using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace GameAt
{
    class BoardDataPrediction
    {
        [ColumnName("Score")]
        public float m_dtScr { get; set; }
    }
}
