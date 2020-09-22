using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace GameAt
{
    public partial class GameFrm : Form
    {
        // セル状態
        public enum eCELL_STT{
            NONE,
            BLACK,
            WHITE,
            LMT
        };

        public class sCELL_POS
        {
            public int m_idRow;
            public int m_idCol;

            public sCELL_POS(int idRow1, int idCol1)
            {
                m_idRow = idRow1;
                m_idCol = idCol1;
            }
        };

        // 学習用データ
        public class sTRN_DATA
        {
            public float m_dtPnt;
            public int[] m_adtStt;
            public sTRN_DATA()
            {
                m_adtStt = new int[Define.BOARD_ROW * Define.BOARD_COL];
            }
        };

        public class sPLAY_HIS
        {
            public eCELL_STT m_nbPlyr;
            public int m_idRow;
            public int m_idCol;

            public sPLAY_HIS(eCELL_STT nbPlyr1, int idRow1, int idCol1)
            {
                m_nbPlyr = nbPlyr1;
                m_idRow = idRow1;
                m_idCol = idCol1;
            }
        };

        public eCELL_STT[,] m_anbCellStt;
        Random m_sRand;
        const int DIR_LMT = 8;
        int[,] m_aidDirOfs;

        public GameFrm()
        {
            InitializeComponent();

            int idCol1;
            for (idCol1 = 0; idCol1 < Define.BOARD_COL; idCol1++)
            {
                DataGridViewImageColumn imgCol1 = new DataGridViewImageColumn();
                imgCol1.ImageLayout = DataGridViewImageCellLayout.Zoom;
                m_sDgvBoard1.Columns.Add(imgCol1);
                m_sDgvBoard1.Columns[idCol1].Width = (m_sDgvBoard1.Width) / Define.BOARD_COL;
            }

            int idRow1;
            for (idRow1 = 0; idRow1 < Define.BOARD_ROW; idRow1++)
            {
                m_sDgvBoard1.Rows.Add();
                m_sDgvBoard1.Rows[idRow1].Height = (m_sDgvBoard1.Height) / Define.BOARD_ROW;
            }

            m_anbCellStt = new eCELL_STT[1 + Define.BOARD_ROW + 1, 1 + Define.BOARD_COL + 1];

            m_aidDirOfs = new int[DIR_LMT, 2] {
                { -1, -1},
                { -1, 0},
                { -1, 1},
                { 0, -1},
                { 0, 1},
                { 1, -1},
                { 1, 0},
                { 1, 1}
            };

            m_sRand = new System.Random();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<sTRN_DATA> asTrnData1 = new List<sTRN_DATA>();
            SimGame(ref asTrnData1);
        }

        public void InitBoard()
        {
            int idRow1;
            for (idRow1 = 0; idRow1 < 1 + Define.BOARD_ROW + 1; idRow1++)
            {
                int idCol1;
                for (idCol1 = 0; idCol1 < 1 + Define.BOARD_COL + 1; idCol1++)
                {
                    m_anbCellStt[idRow1, idCol1] = eCELL_STT.NONE;
                }
            }

            m_anbCellStt[1 + Define.BOARD_ROW / 2 - 1, 1 + Define.BOARD_COL / 2 - 1] = eCELL_STT.BLACK;
            m_anbCellStt[1 + Define.BOARD_ROW / 2, 1 + Define.BOARD_COL / 2] = eCELL_STT.BLACK;
            m_anbCellStt[1 + Define.BOARD_ROW / 2 - 1, 1 + Define.BOARD_COL / 2] = eCELL_STT.WHITE;
            m_anbCellStt[1 + Define.BOARD_ROW / 2, 1 + Define.BOARD_COL / 2 - 1] = eCELL_STT.WHITE;
        }

        public void SimGame(ref List<sTRN_DATA> asTrnData1)
        {
            List<sPLAY_HIS> anbPlayHis1 = new List<sPLAY_HIS>();

            InitBoard();

            UpdBoardDsp();
            // 黒から始まり
            eCELL_STT nbPlyr1 = eCELL_STT.BLACK;
            while (true)
            {
                if (!PlayGame(nbPlyr1, ref anbPlayHis1))
                {
                    // おけない場合交代
                    nbPlyr1 = GetOptPlyr(nbPlyr1);
                    if (!PlayGame(nbPlyr1, ref anbPlayHis1))
                    {
                        // 両方おけない場合終了
                        break;
                    }
                }

                nbPlyr1 = GetOptPlyr(nbPlyr1);
            }

            eCELL_STT nbWinPlyr1 = GetWinPlyr();

            if(nbWinPlyr1 == eCELL_STT.NONE)
            {
                // 引き分け時はサンプルに含めない
            }
            else
            {
                RgsTrnData(ref asTrnData1, nbWinPlyr1, anbPlayHis1);
            }
        }

        public void RgsTrnData(ref List<sTRN_DATA> asTrnData1, eCELL_STT nbWinPlyr1, List<sPLAY_HIS> anbPlayHis1)
        {
            InitBoard();

            int idHis1;
            for(idHis1 = 0; idHis1 < anbPlayHis1.Count; idHis1++)
            {
                PutCell(anbPlayHis1[idHis1].m_nbPlyr, anbPlayHis1[idHis1].m_idRow, anbPlayHis1[idHis1].m_idCol);

                asTrnData1.Add(GetTrnData(anbPlayHis1[idHis1].m_nbPlyr, nbWinPlyr1));
            }
        }

        public sTRN_DATA GetTrnData(eCELL_STT nbPlyr1, eCELL_STT nbWinPlyr1)
        {
            sTRN_DATA sTrnData1 = new sTRN_DATA();
            sTrnData1.m_dtPnt = nbPlyr1 == nbWinPlyr1 ? 1 : -1;

            int idRow1;
            for (idRow1 = 0; idRow1 < Define.BOARD_ROW; idRow1++)
            {
                int idCol1;
                for (idCol1 = 0; idCol1 < Define.BOARD_COL; idCol1++)
                {
                    sTrnData1.m_adtStt[idRow1 * Define.BOARD_COL + idCol1] = m_anbCellStt[1 + idRow1, 1 + idCol1] == eCELL_STT.NONE ? 0 : (m_anbCellStt[1 + idRow1, 1 + idCol1] == nbPlyr1 ? 1 : -1);
                }
            }

            return (sTrnData1);
        }

        public eCELL_STT GetWinPlyr()
        {
            eCELL_STT nbPlyr1 = eCELL_STT.NONE;
            int ctCell1 = GetPlyrCell(eCELL_STT.BLACK);
            int ctCell2 = GetPlyrCell(eCELL_STT.WHITE);

            if(ctCell1 > ctCell2)
            {
                nbPlyr1 = eCELL_STT.BLACK;
            }
            else if (ctCell2 > ctCell1)
            {
                nbPlyr1 = eCELL_STT.WHITE;
            }

            return (nbPlyr1);
        }

        public int GetPlyrCell(eCELL_STT nbPlyr1)
        {
            int ctCell1 = 0;
            int idRow1;
            for (idRow1 = 0; idRow1 < Define.BOARD_ROW; idRow1++)
            {
                int idCol1;
                for (idCol1 = 0; idCol1 < Define.BOARD_COL; idCol1++)
                {
                    if (m_anbCellStt[1 + idRow1, 1 + idCol1] == nbPlyr1)
                    {
                        ctCell1++;
                    }
                }
            }

            return (ctCell1);
        }

        public bool PlayGame(eCELL_STT nbStt1, ref List<sPLAY_HIS> anbPlayHis1)
        {
            List<sCELL_POS> asCellPos1 = GetOkCell(nbStt1);

            if(asCellPos1.Count == 0)
            {
                // 置くとこない
                return (false);
            }

            sCELL_POS sCellPos1 = GetPutCell(nbStt1, asCellPos1);
            PutCell(nbStt1, sCellPos1.m_idRow, sCellPos1.m_idCol);

            // 履歴を更新
            anbPlayHis1.Add(new sPLAY_HIS(nbStt1, sCellPos1.m_idRow, sCellPos1.m_idCol));

            UpdBoardDsp();
          
            return (true);
        }

        public void PutCell(eCELL_STT nbStt1, int idRow1, int idCol1)
        {
            eCELL_STT nbOpsStt1 = GetOptPlyr(nbStt1);

            int idDir1;
            for (idDir1 = 0; idDir1 < DIR_LMT; idDir1++)
            {
                int idRow2 = idRow1;
                int idCol2 = idCol1;
                int ctCell1 = 0;

                while (true)
                {
                    idRow2 += m_aidDirOfs[idDir1, 0];
                    idCol2 += m_aidDirOfs[idDir1, 1];
                    if (m_anbCellStt[1 + idRow2, 1 + idCol2] != nbOpsStt1)
                    {
                        // 敵の色でなければ終了
                        break;
                    }
                    ctCell1++;
                }

                if (m_anbCellStt[1 + idRow2, 1 + idCol2] == nbStt1)
                {
                    // 自分の色なら挟んでいる可能性があるので、元の位置まで戻ってる途中の敵の色を反転
                    while (true)
                    {
                        idRow2 -= m_aidDirOfs[idDir1, 0];
                        idCol2 -= m_aidDirOfs[idDir1, 1];
                        if (m_anbCellStt[1 + idRow2, 1 + idCol2] != nbOpsStt1)
                        {
                            // 敵の色でなければ終了
                            break;
                        }
                        m_anbCellStt[1 + idRow2, 1 + idCol2] = nbStt1;
                    }
                }
            }

            // 最後に置く
            m_anbCellStt[1 + idRow1, 1 + idCol1] = nbStt1;
        }


        public sCELL_POS GetPutCell(eCELL_STT nbStt1, List<sCELL_POS> asCellPos1)
        {
            sCELL_POS sCellPos1 = null;

            sCellPos1 = asCellPos1[m_sRand.Next(0, asCellPos1.Count - 1)];

            return (sCellPos1);

        }

        public List<sCELL_POS> GetOkCell(eCELL_STT nbStt1)
        {
            List<sCELL_POS> asCellPos1 = new List<sCELL_POS>();
            int idRow1;
            for (idRow1 = 0; idRow1 < Define.BOARD_ROW; idRow1++)
            {
                int idCol1;
                for (idCol1 = 0; idCol1 < Define.BOARD_COL; idCol1++)
                {
                    if(IsOkCell(nbStt1, idRow1, idCol1)){
                        asCellPos1.Add(new sCELL_POS(idRow1, idCol1));
                    }
                }
            }

            return (asCellPos1);
        }

        // 敵プレイヤーを取得
        public eCELL_STT GetOptPlyr(eCELL_STT nbStt1)
        {
            return (nbStt1 == eCELL_STT.BLACK ? eCELL_STT.WHITE : eCELL_STT.BLACK);
        }

        public bool IsOkCell(eCELL_STT nbStt1, int idRow1, int idCol1)
        {
            bool flRes1 = false;
            if (m_anbCellStt[1 + idRow1, 1 + idCol1] != eCELL_STT.NONE)
            {
                // 空白じゃない
                return (flRes1);
            }


            eCELL_STT nbOpsStt1 = GetOptPlyr(nbStt1); 

            int idDir1;
            for(idDir1 = 0; idDir1 < DIR_LMT; idDir1++)
            {
                int idRow2 = idRow1;
                int idCol2 = idCol1;
                int ctCell1 = 0;
                
                while(true)
                {
                    idRow2 += m_aidDirOfs[idDir1, 0];
                    idCol2 += m_aidDirOfs[idDir1, 1];
                    if(m_anbCellStt[1 + idRow2, 1 + idCol2] != nbOpsStt1)
                    {
                        // 敵の色でなければ終了
                        break;
                    }
                    ctCell1++;
                }

                if(m_anbCellStt[1 + idRow2, 1 + idCol2] == nbStt1)
                {
                    // 自分の色で終了
                    if(ctCell1 > 0)
                    {
                        // 間に敵の色が1つ以上ある
                        flRes1 = true;
                    }
                }
            }

            return (flRes1);
        }

        private void UpdBoardDsp()
        {
            int idRow1;
            for (idRow1 = 0; idRow1 < Define.BOARD_ROW; idRow1++)
            {
                int idCol1;
                for (idCol1 = 0; idCol1 < Define.BOARD_COL; idCol1++)
                {
                    switch (m_anbCellStt[1 + idRow1, 1 + idCol1])
                    {
                    case eCELL_STT.NONE:
                        m_sDgvBoard1[idCol1, idRow1].Value = Properties.Resources.non;
                        break;
                    case eCELL_STT.BLACK:
                        m_sDgvBoard1[idCol1, idRow1].Value = Properties.Resources.black;
                        break;
                    case eCELL_STT.WHITE:
                        m_sDgvBoard1[idCol1, idRow1].Value = Properties.Resources.white;
                        break;
                    case eCELL_STT.LMT:
                        break;
                    default:
                        break;
                    }
                    
                }
            }

        }
    }
}
