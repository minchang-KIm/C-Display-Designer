using FuncTool;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Schema;
using SystemData.SetupData;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TInspSolutionPlatForm
{
    public partial class DisplayDesigner : Form
    {
        private int[] gridViewBackColor = { 255, 225, 204, 184, 153, 102, 71, 50};
        private bool flag1 = false;
        private bool flag2 = false;
        private int mainScreenViewWidth = 0;
        private int mainScreenViewHeight = 0;
        static string GridPatternString;
        public int idx, ii, jj, AutoRow, AutoCol = 0;
        
        

        public DisplayDesigner(int mainW,int mainH)        
        {
            InitializeComponent();
            typeof(Control).InvokeMember("DoubleBuffered"
                , System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
                , null, dgvForm, new object[] { true });
            //LanguageConvertTool.CreateLanguage(this);

            //LanguageConvertTool lct = new LanguageConvertTool();
            //lct.ConvertLanguage("AutoMainScreen", (string)SystemOption.param.LanguageMode, this);

            mainScreenViewWidth = mainW;
            mainScreenViewHeight = mainH;
            auto_row.Text = Convert.ToString(AutoRow);
            auto_column.Text = Convert.ToString(AutoCol);
            this.Controls.Add(dgvForm);
            dgvForm.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 60, 60, 60);  //드래그 색상
            //SystemManager.SystemManager.Instance.LstDisplay.Clear();
            //new DeviceControl.DisplayDevice();
            //SystemManager.SystemManager.Instance.LstDisplay.Add();
            //그리드 gui에서 나온 값으로 Display생성
            // 생성된 디플창에서 가져오기. Disp{0}.xml형태로 저장된 값으로 가져왔더니 실시간 반영이 안됌 카운트 가져온 다음 readonly로 변경
            textBox3.Text = SystemOption.param.Size.Value.ToString();
            //textBox3.Text = SystemOption.param.DispLayoutRow.Value.ToString();
            //SystemOption.param.DispLayoutRow.Value = textBox3.Text.ToString();



            dgvForm.Rows.Clear();
            dgvForm.Columns.Clear();

            // DataGridView의 컬럼 갯수를 5개(Default)로 설정
            dgvForm.ColumnCount = Convert.ToInt32(textBox3.Text);

            // DataGridView에 빈 스트링 배열을 한 행 단위로 삽입, 이 과정이 생략되면 그리드뷰에 선이 그려지지 않음
            string[] row = { "" };
            for (int n = 0; n < Convert.ToInt32(textBox3.Text); n++)
                dgvForm.Rows.Add(row);

            // 그리드 뷰의 셀 폭과 전체 크기를 지정
            for (int i = 0; i < dgvForm.Columns.Count; i++)
                dgvForm.Columns[i].Width = (15*mainW/mainH);

            dgvForm.Width = ((15 * mainW / mainH) * Convert.ToInt32(textBox3.Text))+20;
            dgvForm.Height = (15 * Convert.ToInt32(textBox3.Text))+20;


            //// 989, 885 -- 12, 80         //크기 넘어가면 스크롤바 쓰게 조정
            //if (dgvForm.Width > 960)
            //    dgvForm.Width = 960;
            //if (dgvForm.Height > 810)
            //    dgvForm.Height = 810;

            dgvForm.ClearSelection();
            textBox1.Clear();
            textBox4.Clear();
            textBox5.Clear();
            listBox1.Items.Clear();

            //set 화면 초기화
            //resolution_label.Text = MainScreenView.
            //DispLayoutRow wa = Screen.PrimaryScreen.WorkingArea.DispLayoutRow;
            // 현재 메인스크린뷰 해상도 화면 비율 가져오기 1701, 821 tablelayoutpannel DispLayoutRow
            //Properties.Settings.Default.
            try
            {
                for (int id = 0; id < Convert.ToInt32(SystemOption.param.DisplayCount.Value); id++)
                {
                    DeviceControl.DisplayDevice disp = new DeviceControl.DisplayDevice();
                    disp.Id = string.Format("Disp{0}", id+1);
                    var device = SystemManager.SystemManager.Instance.LstDisplay[id].Load();
                    int disprow = (device as DeviceControl.DisplayDevice).Row;
                    int dispcolumn = (device as DeviceControl.DisplayDevice).Column;
                    int disprow2 = disprow + (device as DeviceControl.DisplayDevice).RowSpan-1;
                    int dispcolumn2 = dispcolumn + (device as DeviceControl.DisplayDevice).ColumnSpan-1;
                    int[] LoadItemLst = { id, dispcolumn, disprow, dispcolumn2, disprow2 };
                    string LoadItemStr = String.Join(",", LoadItemLst);
                    listBox1.Items.Add(LoadItemStr);
                }
                foreach (string item in listBox1.Items)
                {
                    Random random = new Random(Guid.NewGuid().GetHashCode());
                    int rand1 = random.Next(gridViewBackColor.Length);
                    int rand2 = random.Next(gridViewBackColor.Length);
                    int rand3 = random.Next(gridViewBackColor.Length);
                    //리스트박스 안에 있는 얘들 다 색칠 + id 에 따라서 넘버링
                    string delete_position = item;
                    int column1 = Math.Min(Convert.ToInt32(delete_position.Split(',')[1]), Convert.ToInt32(delete_position.Split(',')[3]));
                    int row1 = Math.Min(Convert.ToInt32(delete_position.Split(',')[2]), Convert.ToInt32(delete_position.Split(',')[4]));
                    int column2 = Math.Max(Convert.ToInt32(delete_position.Split(',')[1]), Convert.ToInt32(delete_position.Split(',')[3]));
                    int row2 = Math.Max(Convert.ToInt32(delete_position.Split(',')[2]), Convert.ToInt32(delete_position.Split(',')[4]));
                    dgvForm.Rows[row1].Cells[column1].Value = ' ';
                    for (int col = column1; col <= column2; col++)
                    {
                        for (int r = row1; r <= row2; r++)            //이 부분 해결하고 확인하기.
                        {
                            
                            dgvForm.Rows[r].Cells[col].Style.BackColor = Color.FromArgb(gridViewBackColor[rand1], gridViewBackColor[rand2], gridViewBackColor[rand3]);

                        }
                    }
                    dgvForm.Rows[row1].Cells[column1].Value = delete_position.Split(',')[0];//텍스트 넘버링
                    idx = Convert.ToInt32(delete_position.Split(',')[0]) + 1;
                }

            }
            catch { }
        }

        private void btnSetForm_Click(object sender, EventArgs e)
        {
            dgvForm.Rows.Clear();
            dgvForm.Columns.Clear();
            //set 폼의 값을 직접 입력하지 않고 Row Layout 과 Column Layout에서 가져오는 방법을 강구.

            // DataGridView의 컬럼 갯수를 5개(Default)로 설정
            dgvForm.ColumnCount = Convert.ToInt32(textBox3.Text);

            // DataGridView에 빈 스트링 배열을 한 행 단위로 삽입, 이 과정이 생략되면 그리드뷰에 선이 그려지지 않음
            string[] row = { "" };
            for (int n = 0; n < Convert.ToInt32(textBox3.Text); n++)
                dgvForm.Rows.Add(row);

            // 그리드 뷰의 셀 폭과 전체 크기를 지정
            for (int i = 0; i < dgvForm.Columns.Count; i++)
                dgvForm.Columns[i].Width = (15 * mainScreenViewWidth / mainScreenViewHeight);

            dgvForm.Width = ((15 * mainScreenViewWidth / mainScreenViewHeight) * Convert.ToInt32(textBox3.Text));
            dgvForm.Height = (15 * Convert.ToInt32(textBox3.Text))+20;

            // 989, 885 -- 12, 80       //최대크기 조정 (가로세로 스크롤 변경)
            //if (dgvForm.Width > 960)
            //    dgvForm.Width = 960;
            //if (dgvForm.Height > 810)
            //    dgvForm.Height = 810;

            dgvForm.ClearSelection();
            textBox1.Clear();
            textBox4.Clear();
            textBox5.Clear();
            listBox1.Items.Clear();
            idx = 0;


        }

        private void dgvForm_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            dgvForm.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = Color.FromArgb(100, 60, 60, 60);
        }

        private void dgvForm_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex == -1 || e.RowIndex == -1) return;
            //dgvForm.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = Color.FromArgb(100, 100, 100);
            textBox5.Text = Convert.ToString('0');
            textBox4.Text = "";
            textBox1.Text = e.ColumnIndex.ToString() + ',' + e.RowIndex.ToString();
            flag1= false;

        }

        private void dgvForm_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex == -1 || e.RowIndex == -1) return;
            textBox4.Text = e.ColumnIndex.ToString() + ',' + e.RowIndex.ToString();
        }

        private void save_Click(object sender, EventArgs e)
        {
            AddDisplay();
        }

        private void AddDisplay(int index = -1)
        {
            if (textBox1.Text.Length == 0 || textBox1.Text.IndexOf(',') == -1) return;
            if (textBox4.Text.Length == 0 || textBox4.Text.IndexOf(',') == -1) return;
            flag1 = false;

            Random random = new Random();
            int rand1 = random.Next(gridViewBackColor.Length);
            int rand2 = random.Next(gridViewBackColor.Length);
            int rand3 = random.Next(gridViewBackColor.Length);


            if (dgvForm.SelectedCells.Count <= 1) return;
                //박스 중복 제거처리, 칸 넘어갔을 때 예외처리
                foreach (DataGridViewCell cell in dgvForm.SelectedCells)
                {
                    if (textBox4.Text == "")
                    {
                        flag1 = true;
                        flag2 = false;
                    }

                    if (cell.Style.BackColor.ToArgb() != 0)
                    {
                        flag1 = true;
                        if (cell.Style.BackColor == Color.FromArgb(255, 255, 255))
                        {
                            flag1 = false;
                        }
                        if (flag2 == true)
                        {
                            flag1 = false;
                        }
                    }
                }
            //박스 색 변경 및 좌표 리스트박스 업로드
            if (flag1 == false)
            {
                int colStart = (int)Math.Min(Convert.ToInt32(textBox1.Text.Split(',')[0]), Convert.ToInt32(textBox4.Text.Split(',')[0]));
                int rowStart = (int)Math.Min(Convert.ToInt32(textBox1.Text.Split(',')[1]), Convert.ToInt32(textBox4.Text.Split(',')[1]));

                int colEnd = Math.Max(Convert.ToInt32(textBox1.Text.Split(',')[0]), Convert.ToInt32(textBox4.Text.Split(',')[0]));
                int rowEnd = Math.Max(Convert.ToInt32(textBox1.Text.Split(',')[1]), Convert.ToInt32(textBox4.Text.Split(',')[1]));

                foreach (DataGridViewCell cell in dgvForm.SelectedCells)
                {
                    cell.Style.BackColor = Color.FromArgb(gridViewBackColor[rand1], gridViewBackColor[rand2], gridViewBackColor[rand3]);
                }
                Text = string.Format("{0},{1},{2},{3},{4}",index, colStart, rowStart, colEnd, rowEnd);
                listBox1.Items.Add(Text);
            }
            dgvForm.ClearSelection();
            textBox4.Clear();
        }

        private void delete_Click(object sender, EventArgs e)
        {
            //idx -= 1;                                                       //리스트박스 위 왼 오 아래 좌표로 지우기
            flag1= false;
            try
            {

                string delete_position = listBox1.Text;
                int column1 = Math.Min(Convert.ToInt32(delete_position.Split(',')[1]), Convert.ToInt32(delete_position.Split(',')[3]));
                int row1 = Math.Min(Convert.ToInt32(delete_position.Split(',')[2]), Convert.ToInt32(delete_position.Split(',')[4]));
                int column2 = Math.Max(Convert.ToInt32(delete_position.Split(',')[1]), Convert.ToInt32(delete_position.Split(',')[3]));
                int row2 = Math.Max(Convert.ToInt32(delete_position.Split(',')[2]), Convert.ToInt32(delete_position.Split(',')[4]));
                dgvForm.Rows[row1].Cells[column1].Value = ' ';
                for (int col = column1; col <= column2; col++)
                {
                    for (int row = row1; row <= row2; row++)
                    {
                        dgvForm.Rows[row].Cells[col].Style.BackColor = Color.FromArgb(255, 255, 255);

                    }
                }
            }
            catch(IndexOutOfRangeException) {  }                            //리스트 uncheck후 delete버튼 누를 때의 에러처리
            finally { listBox1.Items.Remove(listBox1.SelectedItem); }

            
            }

        private void dgvForm_CellClick(object sender, DataGridViewCellEventArgs e)
        //셀을 클릭했을 때 리스트박스에 있는 사각형에 속해있는지 확인(객체 대용)
        {
            try
            {
                if (e.RowIndex == -1 || e.ColumnIndex == -1) return;
                        listBox1.ClearSelected();
                foreach (var input_items in listBox1.Items)
                {
                    string pos = input_items.ToString();
                    string[] tmp =  pos.Split(',');
                    int[] range = new int[tmp.Length];
                    for(int i = 0; i < tmp.Length; i++)
                        range[i] = int.Parse(tmp[i]);

                    int colStart = Math.Min(range[1], range[3]);
                    int rowStart = Math.Min(range[2], range[4]);
                    int colEnd = Math.Max(range[1], range[3]);
                    int rowEnd = Math.Max(range[2], range[4]);

                    if (colStart <= e.ColumnIndex && rowStart <= e.RowIndex && colEnd >= e.ColumnIndex && rowEnd >= e.RowIndex)
                    {
                        listBox1.SelectedItem = input_items;
                        break;
                    }

                }
            }
            catch(Exception ex)
            {

            }
        }
        
        private void modify_btn_Click(object sender, EventArgs e)
        {
            Random random = new Random();
            int rand1 = random.Next(gridViewBackColor.Length);
            int rand2 = random.Next(gridViewBackColor.Length);
            int rand3 = random.Next(gridViewBackColor.Length);
            if (listBox1.SelectedItem == null) return;
            string selected = listBox1.SelectedItem as string;
            var tmp = selected.Split(',');
            if (tmp[0] == "-1") return;
            int dspIndex = int.Parse(tmp[0]);
            delete_Click(sender, e);
            int column1 = Math.Min(Convert.ToInt32(textBox1.Text.Split(',')[0]), Convert.ToInt32(textBox4.Text.Split(',')[0]));
            int row1 = Math.Min(Convert.ToInt32(textBox1.Text.Split(',')[1]), Convert.ToInt32(textBox4.Text.Split(',')[1]));
            int column2 = Math.Max(Convert.ToInt32(textBox1.Text.Split(',')[0]), Convert.ToInt32(textBox4.Text.Split(',')[0]));
            int row2 = Math.Max(Convert.ToInt32(textBox1.Text.Split(',')[1]), Convert.ToInt32(textBox4.Text.Split(',')[1]));

            for (int col = column1; col <= column2; col++)
            {
                for (int row = row1; row <= row2; row++)
                {
                    dgvForm.Rows[row].Cells[col].Style.BackColor = Color.FromArgb(gridViewBackColor[rand1], gridViewBackColor[rand2], gridViewBackColor[rand3]);

                }
            }

            dgvForm.Rows[row1].Cells[column1].Value = dspIndex;

            flag2= true;
            AddDisplay(dspIndex);

            flag2= false;
        }

        private void InitNumBtn_Click(object sender, EventArgs e)
        {
            idx = 0;
            textBox5.Text = "-1";

            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                for (int j = 0; j < listBox1.Items.Count; j++)
                {
                    listBox1.SelectedItem = listBox1.Items[i];
                    num_btn_Click(sender, e);
                    listBox1.ClearSelected();
                    listBox1.SelectedItem = listBox1.Items[j];
                    num_btn_Click(sender, e);
                    listBox1.ClearSelected();
                }

            }
        }

        private void num_btn_Click(object sender, EventArgs e)
        {
            if (Convert.ToInt32(textBox5.Text.ToString()) >= listBox1.Items.Count)
            {
                MessageBox.Show("디스플레이 개수보다 더 큰 값을 입력하셨습니다.");
                return;
            }
            dgvForm.DefaultCellStyle.Font = new Font("Tahoma", 8);

            string before_change = listBox1.Text;
            int full_length = before_change.Length;
            int length = before_change.Split(',')[0].Length;


            try
            {
                int col = Convert.ToInt32(before_change.Split(',')[1]);
                int row = Convert.ToInt32(before_change.Split(',')[2]);
                dgvForm.Rows[row].Cells[col].Value = textBox5.Text;
  


                string after_change = textBox5.Text + before_change.Substring(length, full_length - length);
                listBox1.Items.Remove(before_change);
                listBox1.Items.Add(after_change);
                listBox1.ClearSelected();
            }
            catch { }
 
        }

        private void default_btn_Click(object sender, EventArgs e)
        {
            if(DPtextbox.Text.Length == 0)
            {
                MessageBox.Show("Display No는 공백이면 안 됩니다.");
                return;
            }
            int dspCount = Convert.ToInt32(DPtextbox.Text.ToString());
            int col = Convert.ToInt32(auto_column.Text);
            int row = Convert.ToInt32(auto_row.Text);
            SystemOption.param.DisplayCount.Value = dspCount;
            textBox3.Text = Convert.ToString(col * row);
            btnSetForm_Click(sender, e);
            // 디폴트 디플카운트값 가져와서 조건문으로 해결
            int dpcount = Convert.ToInt32(DPtextbox.Text.ToString()); //Convert.ToInt32(SystemOption.param.DisplayCount.Value);
            int roww = Convert.ToInt32(textBox3.Text.ToString());
            int columnn = Convert.ToInt32(textBox3.Text.ToString());
            if(dspCount > roww)
            {
                MessageBox.Show("디스플레이 개수를 다시 설정해주세요");
                return;
            }
            int id = 0;

            for (int c = 0; c < columnn; c += columnn / row)
            {
                for (int r = 0; r < roww; r += roww / col)
                {
                    if (id == dpcount)
                    {
                        break;
                    }
                    int[] itemarr = { id, r, c, (r + row - 1), (c + col - 1) };
                    string item = String.Join(",", itemarr);
                    listBox1.Items.Add(item);
                    id++;
                }

            }
            save_btn_Click(sender, e);
            
        }

        private void save_btn_Click(object sender, EventArgs e)
        {
            string GridInformation = "";
            List<string> intList = new List<string>();
            //int[] tmpintlst = { };
            List<int> tmpintlst = new List<int>();
            foreach (var input_items in listBox1.Items)
            {
                string[] tmp = input_items.ToString().Split(',');
                int a = Convert.ToInt32(tmp[0]);
                bool index = tmpintlst.Contains(a);
                if(index == true)
                {
                    MessageBox.Show("디스플레이가 중복되었습니다.");
                    
                    return;
                }//done;
                tmpintlst.Add(a);
                string item = input_items.ToString();
                intList.Add(item);
                
            }
            intList.Sort();
            foreach (var input_items in intList)
            {
                //no, 디스플에이 컨트롤 권한을 디스플레이 디자이너가 가지지 않고 밑에 있는 xml Ui 정보 수정
                //네비게이터 저장 누를 때 기존 Disp {0}.xml 포맷 저장하는폴더에 정보를 덮어쓰기 하고 다시 불러오기 저장 버튼 => 네비게이터폼
                var tmp = input_items.ToString().Split(',');
                int idx = int.Parse(tmp[0]);
                if(idx == -1)
                {
                    MessageBox.Show("넘버링을 진행해주세요.");
                    return;
                }
                int col = Math.Min(Convert.ToInt32(tmp[1]), Convert.ToInt32(tmp[3]));
                int colspan = Math.Max(Convert.ToInt32(tmp[1]), Convert.ToInt32(tmp[3])) - col;
                int row = Math.Min(Convert.ToInt32(tmp[2]), Convert.ToInt32(tmp[4]));
                int rowspan = Math.Max(Convert.ToInt32(tmp[2]), Convert.ToInt32(tmp[4])) - row;
                if (idx < SystemManager.SystemManager.Instance.LstDisplay.Count)
                {
                    SystemManager.SystemManager.Instance.LstDisplay[idx].Id = string.Format("Disp{0}", idx + 1);
                    SystemManager.SystemManager.Instance.LstDisplay[idx].Column = col;
                    SystemManager.SystemManager.Instance.LstDisplay[idx].ColumnSpan = colspan+1;
                    SystemManager.SystemManager.Instance.LstDisplay[idx].Row = row;
                    SystemManager.SystemManager.Instance.LstDisplay[idx].RowSpan = rowspan+1;
                    // 2023-01-06 mck 
                    SystemManager.SystemManager.Instance.LstDisplay[idx].Save();
                    SystemOption.param.DisplayCount.Value = (int)intList.Count;

                }
                else
                {
                    DeviceControl.DisplayDevice disp = new DeviceControl.DisplayDevice();
                    disp.Id = string.Format("Disp{0}", idx + 1);
                    disp.Column = col;
                    disp.Row = row;
                    disp.ColumnSpan = colspan+1;
                    disp.RowSpan = rowspan+1;
                    SystemManager.SystemManager.Instance.LstDisplay.Add(disp);
                    try
                    {
                        SystemManager.SystemManager.Instance.LstDisplay[idx].Save();
                    }
                    catch { }
                    SystemOption.param.DisplayCount.Value = (int)intList.Count;

                }
            }

            if(intList.Count < SystemOption.param.DisplayCount)
            {
                try
                {
                    SystemManager.SystemManager.Instance.LstDisplay.RemoveRange(intList.Count - 1, SystemOption.param.DisplayCount - intList.Count);
                    SystemOption.param.DisplayCount.Value = (int)intList.Count;

                }
                catch { }
            }
            SystemOption.param.Size.Value = Convert.ToInt32(textBox3.Text.ToString());
            DialogResult = DialogResult.OK;
            //사이즈만 변경된 경우 colspan rowspan 기존 사이즈 나누기, 새로운 사이즈 곱하기 적용

        }

        private void dgvForm_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {



            int rowindex = e.RowIndex;
            int colindex = e.ColumnIndex;
            dgvForm.CurrentCell = dgvForm.Rows[rowindex].Cells[colindex];
            Color backcolor = dgvForm.CurrentCell.Style.BackColor; 

            int row = Convert.ToInt32(textBox3.Text);
            int column = Convert.ToInt32(textBox3.Text);
            int row_d = 0;
            int column_d = 0;
            for (ii = 0; ii < row; ii++)
            {
                for (jj = 1; jj < column+1; jj++)
                {
                    Color color = dgvForm.Rows[ii].Cells[jj-1].Style.BackColor;
                    if (color == backcolor&& backcolor != Color.FromArgb(0,0,0,0) && backcolor != Color.FromArgb(255,255,255,255))  //마지막 같은색깔
                    {
                        row_d = ii;
                        column_d = jj-1; 
                    }

                }

            }
            for(int item_idx = 0;item_idx<listBox1.Items.Count;item_idx++)
            {
                listBox1.ClearSelected();
                string item = Convert.ToString(listBox1.Items[item_idx]);

                int column1 = (int)Math.Min(Convert.ToInt32(item.Split(',')[1]), Convert.ToInt32(item.Split(',')[3]));
                int row1 = (int)Math.Min(Convert.ToInt32(item.Split(',')[2]), Convert.ToInt32(item.Split(',')[4]));

                string column_dd = Math.Max(Convert.ToInt32(item.Split(',')[1]), Convert.ToInt32(item.Split(',')[3])).ToString();
                string row_dd = Math.Max(Convert.ToInt32(item.Split(',')[2]), Convert.ToInt32(item.Split(',')[4])).ToString();

                if ( row_d == int.Parse(row_dd) && column_d == int.Parse(column_dd))
                {
                    listBox1.SelectedItem = item;
                    string[] a = listBox1.Text.ToString().Split(',');
                    if (a[0] != "-1" | a[0] == null)
                    {
                            return;
                    }

                    if (listBox1.Items.Count > idx)
                    {
                        idx++;
                    }
                    textBox5.Text = Convert.ToString(idx - 1);
                    if ((idx - 1) > listBox1.Items.Count)
                    {
                        textBox5.Text = Convert.ToString(idx - 1);
                    }

                    num_btn_Click(sender, e);
                    listBox1.ClearSelected();
                }
            }
            



        }
        
    }
}