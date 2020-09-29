using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Data;

namespace GameAt
{
    public class BoardData
    {
        // 盤面の点数
        [LoadColumn(0)]
        public float m_dtScr { get; set; }

        // 盤面の状態
        [LoadColumn(1, Define.BOARD_ROW * Define.BOARD_COL)]
        [VectorType(Define.BOARD_ROW * Define.BOARD_COL)]
        public float[] m_adtStt { get; set; }

        public BoardData()
        {
            m_adtStt = new float[Define.BOARD_ROW * Define.BOARD_COL];
        }
    }
}
