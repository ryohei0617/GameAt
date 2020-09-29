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
using System.IO;
using Microsoft.ML;
using Microsoft.ML.Trainers;

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
            public eCELL_STT[,] m_anbCellStt;

            public sCELL_POS(int idRow1, int idCol1)
            {
                m_idRow = idRow1;
                m_idCol = idCol1;
                m_anbCellStt = new eCELL_STT[1 + Define.BOARD_ROW + 1, 1 + Define.BOARD_COL + 1];
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
        public eCELL_STT m_nbPlyr;
        public List<sPLAY_HIS> m_nbPlayHis;
        Random m_sRand;
        const int DIR_LMT = 8;
        int[,] m_aidDirOfs;

        PredictionEngine<GameAt.BoardData, BoardDataPrediction> predictionEngine;

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
            List<BoardData> asTrnData1 = new List<BoardData>();

            // シミュレート
            int idSim1;
            for(idSim1 = 0; idSim1 < Define.SIM_NUM; idSim1++)
            {
                SimGame(ref asTrnData1);
            }

            WrtTrnData(asTrnData1);
        }

        public void WrtTrnData(List<BoardData> asTrnData1)
        {
            string txCpt1 = "Score";
            int idRow1;
            for (idRow1 = 0; idRow1 < Define.BOARD_ROW; idRow1++)
            {
                int idCol1;
                for (idCol1 = 0; idCol1 < Define.BOARD_COL; idCol1++)
                {
                    txCpt1 += ", Stt" + idRow1.ToString() + "_" + idCol1.ToString();
                }
            }
            File.AppendAllText(Define.TRN_DATA_FILE, txCpt1 + Environment.NewLine);

            int idTrnData1;
            for(idTrnData1 = 0; idTrnData1 < asTrnData1.Count; idTrnData1++)
            {
                string txTrnData1 = GetTrnDataTxt(asTrnData1[idTrnData1]);

                File.AppendAllText(Define.TRN_DATA_FILE, txTrnData1 + Environment.NewLine);
            }
        }

        public string GetTrnDataTxt(BoardData sTrnData1)
        {
            string txRes1 = sTrnData1.m_dtScr.ToString();
            int idRow1;
            for (idRow1 = 0; idRow1 < Define.BOARD_ROW; idRow1++)
            {
                int idCol1;
                for (idCol1 = 0; idCol1 < Define.BOARD_COL; idCol1++)
                {
                    txRes1 += ", " + sTrnData1.m_adtStt[idRow1 * Define.BOARD_COL + idCol1].ToString();
                }
            }

            return (txRes1);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            m_nbPlayHis = new List<sPLAY_HIS>();

            InitBoard();
            UpdBoardDsp();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (SimTurn())
            {
                // 終了
            }
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

            m_nbPlyr = eCELL_STT.BLACK;
        }

        public void SimGame(ref List<BoardData> asTrnData1)
        {
            m_nbPlayHis = new List<sPLAY_HIS>();

            InitBoard();

            UpdBoardDsp();
            // 黒から始まり
            while (true)
            {
                if (SimTurn())
                {
                    break;
                }
            }

            eCELL_STT nbWinPlyr1 = GetWinPlyr();

            if(nbWinPlyr1 == eCELL_STT.NONE)
            {
                // 引き分け時はサンプルに含めない
            }
            else
            {
                RgsTrnData(ref asTrnData1, nbWinPlyr1);
            }
        }

        public bool SimTurn()
        {
            if (!PlayGame(m_nbPlyr))
            {
                // おけない場合交代
                m_nbPlyr = GetOptPlyr(m_nbPlyr);
                if (!PlayGame(m_nbPlyr))
                {
                    // 両方おけない場合終了
                    return (true);
                }
            }

            m_nbPlyr = GetOptPlyr(m_nbPlyr);
            return (false);
        }

        public void RgsTrnData(ref List<BoardData> asTrnData1, eCELL_STT nbWinPlyr1)
        {
            InitBoard();

            int idHis1;
            for(idHis1 = 0; idHis1 < m_nbPlayHis.Count; idHis1++)
            {
                PutCell(m_nbPlayHis[idHis1].m_nbPlyr, m_nbPlayHis[idHis1].m_idRow, m_nbPlayHis[idHis1].m_idCol, ref m_anbCellStt);

                asTrnData1.Add(GetTrnData(m_nbPlayHis[idHis1].m_nbPlyr, nbWinPlyr1));
            }
        }

        public BoardData GetTrnData(eCELL_STT nbPlyr1, eCELL_STT nbWinPlyr1)
        {
            BoardData sTrnData1 = new BoardData();
            sTrnData1.m_dtScr = nbPlyr1 == nbWinPlyr1 ? 1 : -1;

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

        public bool PlayGame(eCELL_STT nbStt1)
        {
            List<sCELL_POS> asCellPos1 = GetOkCell(nbStt1);

            if(asCellPos1.Count == 0)
            {
                // 置くとこない
                return (false);
            }

            sCELL_POS sCellPos1 = GetPutCell(nbStt1, asCellPos1);
            PutCell(nbStt1, sCellPos1.m_idRow, sCellPos1.m_idCol, ref m_anbCellStt);

            // 履歴を更新
            m_nbPlayHis.Add(new sPLAY_HIS(nbStt1, sCellPos1.m_idRow, sCellPos1.m_idCol));

            UpdBoardDsp();
          
            return (true);
        }

        public int PutCell(eCELL_STT nbStt1, int idRow1, int idCol1, ref eCELL_STT[,] anbCellStt1)
        {
            eCELL_STT nbOpsStt1 = GetOptPlyr(nbStt1);

            int ctCell1 = 0;
            int idDir1;
            for (idDir1 = 0; idDir1 < DIR_LMT; idDir1++)
            {
                int idRow2 = idRow1;
                int idCol2 = idCol1;

                while (true)
                {
                    idRow2 += m_aidDirOfs[idDir1, 0];
                    idCol2 += m_aidDirOfs[idDir1, 1];
                    if (anbCellStt1[1 + idRow2, 1 + idCol2] != nbOpsStt1)
                    {
                        // 敵の色でなければ終了
                        break;
                    }
                }

                if (anbCellStt1[1 + idRow2, 1 + idCol2] == nbStt1)
                {
                    // 自分の色なら挟んでいる可能性があるので、元の位置まで戻ってる途中の敵の色を反転
                    while (true)
                    {
                        idRow2 -= m_aidDirOfs[idDir1, 0];
                        idCol2 -= m_aidDirOfs[idDir1, 1];
                        if (anbCellStt1[1 + idRow2, 1 + idCol2] != nbOpsStt1)
                        {
                            // 敵の色でなければ終了
                            break;
                        }
                        anbCellStt1[1 + idRow2, 1 + idCol2] = nbStt1;
                        ctCell1++;
                    }
                }
            }

            // 最後に置く
            anbCellStt1[1 + idRow1, 1 + idCol1] = nbStt1;

            return (ctCell1);
        }

        // 置くセルを取得
        public sCELL_POS GetPutCell(eCELL_STT nbStt1, List<sCELL_POS> asCellPos1)
        {
            sCELL_POS sCellPos1 = null;

            if(predictionEngine != null)
            {
                // 予測パイプラインあり

            }
            else
            {
                sCellPos1 = asCellPos1[m_sRand.Next(0, asCellPos1.Count - 1)];
            }

            return (sCellPos1);

        }

        // 置けるセル配列を取得
        public List<sCELL_POS> GetOkCell(eCELL_STT nbStt1)
        {
            List<sCELL_POS> asCellPos1 = new List<sCELL_POS>();
            int idRow1;
            for (idRow1 = 0; idRow1 < Define.BOARD_ROW; idRow1++)
            {
                int idCol1;
                for (idCol1 = 0; idCol1 < Define.BOARD_COL; idCol1++)
                {

                    if (m_anbCellStt[1 + idRow1, 1 + idCol1] != eCELL_STT.NONE)
                    {
                        // 空白じゃない
                    }
                    else
                    {
                        eCELL_STT[,] anbCellStt1 = new eCELL_STT[1 + Define.BOARD_ROW + 1, 1 + Define.BOARD_COL + 1];
                        Array.Copy(m_anbCellStt, anbCellStt1, anbCellStt1.Length);
                        if (IsOkCell(nbStt1, idRow1, idCol1, ref anbCellStt1))
                        {
                            asCellPos1.Add(new sCELL_POS(idRow1, idCol1));
                        }
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

        public bool IsOkCell(eCELL_STT nbStt1, int idRow1, int idCol1, ref eCELL_STT[,] anbCellStt1)
        {
            bool flRes1 = false;
            eCELL_STT nbOpsStt1 = GetOptPlyr(nbStt1);

            if(PutCell(nbStt1, idRow1, idCol1, ref anbCellStt1) > 0)
            {
                // 敵の色をひっくり返せる
                flRes1 = true;
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

        // トレーニング
        private void button4_Click(object sender, EventArgs e)
        {
            MLContext mlContext = new MLContext();

            // トレーニング用データファイルを読み込み
            IDataView data = mlContext.Data.LoadFromTextFile<GameAt.BoardData>(Define.TRN_DATA_FILE, separatorChar: ',', hasHeader: true);

            // エスティメータ作成
            var sdcaEstimator = mlContext.Regression.Trainers.Sdca(labelColumnName: "m_dtScr", featureColumnName: "m_adtStt");

            // トレーニング
            // Build machine learning model
            var trainedModel = sdcaEstimator.Fit(data);

            // トレーニング後データをセーブ
            mlContext.Model.Save(trainedModel, data.Schema, "model.zip");

            //var trainedModelParameters = trainedModel.Model as LinearRegressionModelParameters;

            // トレーニング後データを読み込み
            DataViewSchema predictionPipelineSchema;
            ITransformer predictionPipeline = mlContext.Model.Load("model.zip", out predictionPipelineSchema);

            predictionEngine = 
                mlContext.Model.CreatePredictionEngine<GameAt.BoardData, BoardDataPrediction>(predictionPipeline);
        }
    }
}
