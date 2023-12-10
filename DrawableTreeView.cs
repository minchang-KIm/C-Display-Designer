using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Linq;

namespace FuncTool
{
    public class DrawableTreeView : TreeView
    {

        public class Link_Node2Node
        {
            public string sourceNodeID { get; set; }
            public string targetNodeID { get; set; }
            public int[] Colorarr = new int[3];
            public int SEROLINE = 350;
            public int ARROWLINESIZE = 3;

        };



        #region constructor

        public DrawableTreeView()//non override
        {
            // double buffering
            this.SetStyle(ControlStyles.UserPaint, false);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            // no overide method method
            this.DrawNode += DrawableTreeView_DrawNode;
            this.ItemDrag += DrawableTreeView_ItemDrag;
            this.DragEnter += DrawableTreeView_DragEnter;
            this.DragLeave += DrawableTreeView_DragLeave;
            this.DragOver += DrawableTreeView_DragOver;
            this.DrawNode += new DrawTreeNodeEventHandler(DrawText_treeview);
            this.MouseUp += DrawableTreeView_MouseUp;
            this.AfterExpand += DrawableTreeView_AfterExpand;
            this.AfterCollapse += DrawableTreeView_AfterCollapse;
            // overide method
            this.DragDrop += DrawableTreeView_DragDrop;
            this.MouseDoubleClick += DrawableTreeView_MouseDoubleClick;
            this.MouseClick += DrawableTreeView_MouseClick;
        }

        public DrawableTreeView(string overide)//override
        {
            // double buffering
            this.SetStyle(ControlStyles.UserPaint, false);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            // no overide method method
            this.DrawNode += DrawableTreeView_DrawNode;
            this.ItemDrag += DrawableTreeView_ItemDrag;
            this.DragEnter += DrawableTreeView_DragEnter;
            this.DragLeave += DrawableTreeView_DragLeave;
            this.DragOver += DrawableTreeView_DragOver;
            this.DrawNode += new DrawTreeNodeEventHandler(DrawText_treeview);
            this.MouseUp += DrawableTreeView_MouseUp;
            this.AfterExpand += DrawableTreeView_AfterExpand;
            this.AfterCollapse += DrawableTreeView_AfterCollapse;
        }


        private void DrawableTreeView_MouseClick(object sender, MouseEventArgs e)
        {
            Put_in_treeView_MouseClick(sender, e);
        }

        private void DrawableTreeView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Put_in_MouseDoubleClick(sender, e);
        }

        private void DrawableTreeView_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            Put_in_AfterCollapse(sender, e);
        }

        private void DrawableTreeView_AfterExpand(object sender, TreeViewEventArgs e)
        {
            Put_in_AfterExpand(sender, e);
        }

        private void DrawableTreeView_MouseUp(object sender, MouseEventArgs e)
        {
            Put_in_MouseUp(sender, e);
        }

        private void DrawableTreeView_DragOver(object sender, DragEventArgs e)
        {
            Put_in_DragOver(sender, e);
        }

        private void DrawableTreeView_DragLeave(object sender, EventArgs e)
        {
            Put_in_DragLeave(sender, e);
        }

        private void DrawableTreeView_DragEnter(object sender, DragEventArgs e)
        {

        }

        private void DrawableTreeView_DragDrop(object sender, DragEventArgs e)
        {
            Put_in_DragDrop(sender, e);
        }

        private void DrawableTreeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            Put_in_itemDrag(sender, e);
        }

        private void DrawableTreeView_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            DrawText_treeview(sender, e);
        }

        #endregion


        #region field
        protected TreeNode sourceNode;
        protected TreeNode targetNode;
        protected Random random;
        protected bool onlyNode;
        protected bool allignDup = false;
        protected bool inspDup = false;
        protected bool activate = false;
        #endregion


        
        protected List<Link_Node2Node> lstNodePair = new List<Link_Node2Node>();
        protected List<Link_Node2Node> tempDrawList = new List<Link_Node2Node>();
        protected List<Link_Node2Node> onlyDrawList = new List<Link_Node2Node>();
        protected Link_Node2Node arrow = new Link_Node2Node();

        //DrawableTreeView this = new DrawableTreeView();

        public virtual void cngdrawid(string key)   //한화 TIS 트리뷰 delete하면 노드 아이디 바뀌어서 전체적으로 밀리는 현상 방지 메소드
        {

        }

        private async void DrawText_treeview(object sender, DrawTreeNodeEventArgs e)
        {
            Rectangle rc2 = e.Node.Bounds;
            e.Graphics.DrawString(e.Node.Text, new Font("굴림체", 10), new SolidBrush(Color.Black), rc2.Left, rc2.Top);
            DrawLine_treeview(sender, e);

        }

        private TreeNode findNodeFromID(string Name)
        {
            string NodeName = Name;
            TreeNode[] Nodes = this.Nodes.Find(NodeName, true);    //Unit1 key = Unit1 title = Unit1


            if (Nodes == null)
            {
                return null;
            }

            foreach (TreeNode Node in Nodes)
            {
                if (Node.Name == Name)
                {
                    return Node;
                }
            }
            return null;

        }

        private async void DrawLine_treeview(object sender, DrawTreeNodeEventArgs e)    //색깔 화살표 변경하기.
        {
            if (onlyNode == true)
            {

                int seroLine = arrow.SEROLINE;
                foreach (var arrowPair in onlyDrawList)    //노드가 없을 경우에는?
                {
                    TreeNode startnode = findNodeFromID(arrowPair.sourceNodeID);
                    TreeNode endNode = findNodeFromID(arrowPair.targetNodeID);
                    if (startnode == null || endNode == null)
                    { }
                    else
                    {
                        Pen pen = new Pen(Color.FromArgb(arrowPair.Colorarr[0], arrowPair.Colorarr[1], arrowPair.Colorarr[2]), arrow.ARROWLINESIZE);
                        Rectangle src = startnode.Bounds;
                        Rectangle erc = endNode.Bounds;
                        Point[] points =
                        {
                            new Point(src.Right+10, src.Top+5),
                            new Point(seroLine, src.Top+5),
                            new Point(seroLine, erc.Top+5),
                            new Point(erc.Right+10, erc.Top+5),
                            new Point(erc.Right+20, erc.Top+10),
                            new Point(erc.Right+20, erc.Top),
                            new Point(erc.Right+10, erc.Top+5)
                        };
                        e.Graphics.FillPolygon(new SolidBrush(pen.Color), points);
                    }
                    seroLine -= 10;
                }
                return;
            }

            if (lstNodePair != null)
            {
                int seroLine = arrow.SEROLINE;
                foreach (var arrowPair in lstNodePair)    
                {
                    TreeNode startnode = findNodeFromID(arrowPair.sourceNodeID);
                    TreeNode endNode = findNodeFromID(arrowPair.targetNodeID);
                    if (startnode == null || endNode == null)
                    { }
                    else
                    {
                        Pen pen = new Pen(Color.FromArgb(arrowPair.Colorarr[0], arrowPair.Colorarr[1], arrowPair.Colorarr[2]), arrow.ARROWLINESIZE);
                        Rectangle src = startnode.Bounds;
                        Rectangle erc = endNode.Bounds;
                        Point[] line_points =       // Draw line Node to Node
                        {
                            new Point(src.Right+10, src.Top+5),
                            new Point(seroLine, src.Top+5),
                            new Point(seroLine, erc.Top+5),
                            new Point(erc.Right+15, erc.Top+5),
                        };
                        Point[] arrow_points =  // Draw Arrow at the end of the line
                       {
                            new Point(erc.Right+20, erc.Top+10),
                            new Point(erc.Right+20, erc.Top),
                            new Point(erc.Right+10, erc.Top+5)
                        };
                        e.Graphics.DrawLines(pen, line_points);
                        e.Graphics.FillPolygon(new SolidBrush(pen.Color), arrow_points);

                    }
                    seroLine -= 10;
                }

                if (tempDrawList.Count != 0)
                {
                    var arrowPair = tempDrawList[tempDrawList.Count - 1];
                    TreeNode startnode = findNodeFromID(arrowPair.sourceNodeID);
                    TreeNode endNode = findNodeFromID(arrowPair.targetNodeID);
                    if (startnode == null || endNode == null)
                    { }
                    else
                    {
                        Pen pen = new Pen(Color.FromArgb(arrowPair.Colorarr[0], arrowPair.Colorarr[1], arrowPair.Colorarr[2]), arrow.ARROWLINESIZE);
                        Rectangle src = startnode.Bounds;
                        Rectangle erc = endNode.Bounds;
                        Point[] line_points =
                        {
                            new Point(src.Right+15, src.Top+5),
                            new Point(seroLine, src.Top+5),
                            new Point(seroLine, erc.Top+5),
                            new Point(erc.Right+10, erc.Top+5),
                        };
                        Point[] arrow_points =
                       {
                            new Point(erc.Right+20, erc.Top+10),
                            new Point(erc.Right+20, erc.Top),
                            new Point(erc.Right+10, erc.Top+5)
                        };
                        e.Graphics.DrawLines(pen, line_points);
                        e.Graphics.FillPolygon(new SolidBrush(pen.Color), arrow_points);

                    }
                    seroLine -= 10;
                }

            }

        }
        protected void addNodeInLst()
        {
            if (arrow.sourceNodeID != null && arrow.targetNodeID != null && arrow.sourceNodeID != arrow.targetNodeID)
            {//null 제외
                if (lstNodePair.Count == 0)
                {
                    Link_Node2Node tempInstacne = new Link_Node2Node();
                    tempInstacne.sourceNodeID = arrow.sourceNodeID;
                    tempInstacne.targetNodeID = arrow.targetNodeID;
                    random = new Random(Guid.NewGuid().GetHashCode());
                    tempInstacne.Colorarr[0] = random.Next(0, 255);
                    tempInstacne.Colorarr[1] = random.Next(0, 255);
                    tempInstacne.Colorarr[2] = random.Next(0, 255);

                    lstNodePair.Add(tempInstacne);
                    tempInstacne = null;
                    GC.Collect();
                }
                if (lstNodePair[lstNodePair.Count - 1].sourceNodeID == arrow.sourceNodeID && lstNodePair[lstNodePair.Count - 1].targetNodeID == arrow.targetNodeID)
                {/*전과 똑같은거는 안되게*/}
                else
                {
                    Link_Node2Node tempInstance = new Link_Node2Node();
                    tempInstance.sourceNodeID = arrow.sourceNodeID;
                    tempInstance.targetNodeID = arrow.targetNodeID;
                    random = new Random(Guid.NewGuid().GetHashCode());
                    tempInstance.Colorarr[0] = random.Next(0, 255);
                    tempInstance.Colorarr[1] = random.Next(0, 255);
                    tempInstance.Colorarr[2] = random.Next(0, 255);
                    lstNodePair.Add(tempInstance);
                    tempInstance = null;
                    GC.Collect();
                }
            }
            else
            {
                arrow.sourceNodeID = null;
                arrow.targetNodeID = null;
            }
            this.Invalidate();
        }

        #region Put_in_method

        private void Put_in_itemDrag(object sender, ItemDragEventArgs e)
        {
            sourceNode = e.Item as TreeNode;
            arrow.sourceNodeID = sourceNode.Name;

            DoDragDrop(e.Item.ToString(), DragDropEffects.Move);
        }
        /// <summary>
        /// source node와 target node의 텍스트 값으로 두 노드 사이를 연결할 때 실행시켜야 할 메소드들을 정의할 수 있다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Put_in_DragDrop(object sender, DragEventArgs e)
        {
            tempDrawList.Clear();
            this.Invalidate();
            Point dropPoint = this.PointToClient(new Point(e.X, e.Y));
            TreeNode targetNode = this.GetNodeAt(dropPoint);
            if (targetNode == null) { return; }     //타겟 노드 널일 경우 리턴
            string[] sourceNodeSplitArr = sourceNode.FullPath.Split('\\');  
            string[] targetNodeSplitArr = targetNode.FullPath.Split('\\');
            try
            {
                if (targetNodeSplitArr[targetNodeSplitArr.Length - 2] == sourceNodeSplitArr[sourceNodeSplitArr.Length - 2]) { return; }   //형제노드일 경우 리턴
            }
            catch { }
            arrow.targetNodeID = targetNode.Name;
            foreach (var a in lstNodePair)//이미 연결되어 있을 때 중복 그리기 방지
            {
                if (a.sourceNodeID == arrow.sourceNodeID && a.targetNodeID == arrow.targetNodeID)
                {
                    return;
                }
            }
            addNodeInLst();
        }
        private void Put_in_DragOver(object sender, DragEventArgs e)
        {
            //TreeView this = new TreeView();
            e.Effect = DragDropEffects.Move;
            Point dropPoint = this.PointToClient(new Point(e.X, e.Y));
            TreeNode node = this.GetNodeAt(dropPoint);
            if (node == null || sourceNode.Name == node.Name)
            {
                return;
            }
            try
            {
                if (tempDrawList[tempDrawList.Count - 1].targetNodeID == node.Name && tempDrawList[tempDrawList.Count - 1].sourceNodeID == sourceNode.Name)
                {
                    return;
                }
            }
            catch { }
            Link_Node2Node tempInstacne = new Link_Node2Node();
            tempInstacne.sourceNodeID = sourceNode.Name;
            tempInstacne.targetNodeID = node.Name;
            tempInstacne.Colorarr[0] = 125;
            tempInstacne.Colorarr[1] = 0;
            tempInstacne.Colorarr[2] = 0;
            tempDrawList.Add(tempInstacne);
            this.Invalidate();
        }
        private void Put_in_DragLeave(object sender, EventArgs e)
        {
            //마우스가 객체를 떠날 때

            DialogResult dialogResult = MessageBox.Show("리스트뷰의 범위를 넘어섰습니다.");
        }

        private void Put_in_MouseUp(object sender, MouseEventArgs e)
        {
            tempDrawList.Clear();
        }

        private void Put_in_AfterExpand(object sender, TreeViewEventArgs e)
        {
            this.Invalidate();
        }
        private void Put_in_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            this.Invalidate();
        }

        protected virtual void Put_in_MouseDoubleClick(object sender, MouseEventArgs e) //타겟 노드를 더블클릭하여 연결된 노드들 전부 삭제
        {
            Point dropPoint = e.Location;
            TreeNode deleteNode = this.GetNodeAt(dropPoint);
            if (deleteNode == null) { }
            else
            {
                string delNodeFullPath = deleteNode.Name;
                lstNodePair.RemoveAll(lstNodePair => delNodeFullPath == lstNodePair.targetNodeID);
                this.Invalidate();
            }
            //삭제할 때 발생할 메소드들 구현하는 부분
        }

        protected virtual void Put_in_treeView_MouseClick(object sender, MouseEventArgs e)
        {}

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // DrawableTreeView
            // 
            this.AllowDrop = true;
            this.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this.ResumeLayout(false);

        }

        #endregion
        // itemDrag DragDrop DragOver DragLeave MouseUp AfterExpand AfterCollapse MouseDoubleClick MouseClick

    }
}
