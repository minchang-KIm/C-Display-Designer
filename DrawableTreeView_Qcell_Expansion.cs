using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Linq;


namespace FuncTool      //DeviceControl로 조작하기 위해서는 7.Device Contorl 밑에다가 파일을 위치시켜야 한다. + 자식노드 안나오게 하는거는 UpdateTreeView코드를 손봐야 하므로 상무님께 물어보기
{
    public class DrawableTreeView_Qcell_Expansion : DrawableTreeView
    {
        #region constructor
        /// <summary>
        /// DrawableTreeView 확장판. 이 파일은 한화 qcell TIS용
        /// </summary>
        /// <param name="overide"></param>
        public DrawableTreeView_Qcell_Expansion(string overide) : base(overide)
        {
            // double buffering
            this.SetStyle(ControlStyles.UserPaint, false);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.MouseDoubleClick += DrawableTreeView_MouseDoubleClick;
            this.MouseClick += DrawableTreeView_MouseClick;
            this.DragDrop += DrawableTreeView_DragDrop;
        }

        private void DrawableTreeView_MouseClick(object sender, MouseEventArgs e)
        {
            Put_in_treeView_MouseClick(sender, e);
        }

        private void DrawableTreeView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Put_in_MouseDoubleClick(sender, e);
        }

        private void DrawableTreeView_DragDrop(object sender, DragEventArgs e)
        {
            Put_in_DragDrop(sender, e);
        }


        #endregion
        

        public override void cngdrawid(string key)   //한화 TIS 트리뷰 delete하면 노드 아이디 바뀌어서 전체적으로 밀리는 현상 방지 메소드
        {
            //mck
            for (int j = 0; j < this.lstNodePair.Count; j++)
            {
                if (this.lstNodePair[j].sourceNodeID == key)
                {
                    this.lstNodePair.RemoveAt(j);
                }
            }
            for (int j = 0; j < this.lstNodePair.Count; j++)
            {
                string strNum = Regex.Replace(this.lstNodePair[j].sourceNodeID.Split('.').Last(), @"\D", "");
                int Num = Convert.ToInt32(Regex.Replace(this.lstNodePair[j].sourceNodeID.Split('.').Last(), @"\D", ""));
                int delNum = Convert.ToInt32(Regex.Replace(key.Split('.').Last(), @"\D", ""));
                if (Num > delNum)
                {
                    this.lstNodePair[j].sourceNodeID = key.Substring(0, (key.Length - strNum.Length)) + (Num - 1);
                }
            }
            //mck
        }


        protected override void Put_in_DragDrop(object sender, DragEventArgs e)
        {
            tempDrawList.Clear();
            this.Invalidate();
            Point dropPoint = this.PointToClient(new Point(e.X, e.Y));
            TreeNode targetNode = this.GetNodeAt(dropPoint);
            if (targetNode == null) { return; }
            string[] sourceNodeSplitArr = sourceNode.FullPath.Split('\\');
            string[] targetNodeSplitArr = targetNode.FullPath.Split('\\');
            try
            {
                if (targetNodeSplitArr[targetNodeSplitArr.Length - 2] == sourceNodeSplitArr[sourceNodeSplitArr.Length - 2]) { return; }   //형제노드일 경우 리턴
            }
            catch { }
            arrow.targetNodeID = targetNode.Name;
            foreach (var a in lstNodePair)
            {
                if (a.sourceNodeID == arrow.sourceNodeID && a.targetNodeID == arrow.targetNodeID)
                {
                    return;//이미 연결되어 있으면 그리지 않기 아래 코드랑 합칠 수 도 있음
                }
            }

            //addNodeInLst();


            if (targetNode != null && sourceNode != null)
            {
                try
                {
                    int numstring = Convert.ToInt32(Regex.Replace(sourceNode.Text, @"\D", ""));       //숫자만 추출
                    string type = Regex.Replace(sourceNode.Text, @"\d", "");                           //알파벳만 추출
                    int tar_numstring = Convert.ToInt32(Regex.Replace(targetNode.Text, @"\D", ""));    //숫자만 추출
                    string tar_type = Convert.ToString(Regex.Replace(targetNode.Text, @"\d", ""));     //알파벳만 추출

                    string unitText = "";
                    string imageText = "";
                    string defectText = "";
                    int unitId = 0;

                    switch (type)//2023-01-26 mck   case == sourceNode Text  target_type == TargetNode Text
                    {
                        case "Unit":
                            addNodeInLst();
                            return;
                        case "Light":
                            addNodeInLst();
                            return;
                        case "Camera":
                            addNodeInLst();
                            //if (tar_type == "Defect")
                            //{
                            //    addNodeInLst();
                            //    treeView.SelectedNode = targetNode;
                            //    ((DeviceControl.DefectDevice)propertyGrid.SelectedObject).TargetId = (sourceNode.Parent.Text + "." + sourceNode.Text);
                            //    return;
                            //}
                            //if (tar_type == "Aligner")
                            //{
                            //    addNodeInLst();
                            //    treeView.SelectedNode = targetNode;
                            //    ((DeviceControl.AlignerDevice)propertyGrid.SelectedObject).TargetId = (sourceNode.Parent.Text + "." + sourceNode.Text);
                            //    return;
                            //}
                            //if (tar_type == "Inspection")
                            //{
                            //    addNodeInLst();
                            //    treeView.SelectedNode = targetNode;
                            //    ((DeviceControl.InspectionDevice)propertyGrid.SelectedObject).TargetId = (sourceNode.Parent.Text + "." + sourceNode.Text);
                            //    return;
                            //}
                            return;
                        case "Image":
                            addNodeInLst();
                            return;
                        case "Defect":
                            addNodeInLst();
                            //if (tar_type == "Defect")
                            //{ return; }
                            //if (tar_type == "Aligner")
                            //{
                            //    treeView.SelectedNode = targetNode;
                            //    // PropertyGrid Align의 AlignAlgoType이 simpleAlign일 경우, return
                            //    if (((DeviceControl.AlignerDevice)propertyGrid.SelectedObject).AlignAlgoType == enAlignAlgo.SimpleAlign_Fixture)
                            //    {
                            //        return;
                            //    }
                            //    addNodeInLst();
                            //    unitText = sourceNode.FullPath.ToString().Split('\\')[0];
                            //    imageText = unitText + '.' + sourceNode.FullPath.ToString().Split('\\')[1];
                            //    defectText = imageText + '.' + sourceNode.FullPath.ToString().Split('\\')[2];
                            //    //unitId = Convert.ToInt32(Regex.Replace(unitText, @"\D", ""));       //숫자만 추출
                            //    foreach (var unitDevice in lstUnitDevice)
                            //    {
                            //        if (unitDevice.Id == unitText)
                            //        {
                            //            for (int i = 0; i < unitDevice.ImageScenarioItems.Count; i++)
                            //            {
                            //                if (unitDevice.ImageScenarioItems[i].Id == imageText)
                            //                {
                            //                    for (int j = 0; j < unitDevice.ImageScenarioItems[i].DefectScenarioItems.Count; j++)
                            //                    {
                            //                        if (unitDevice.ImageScenarioItems[i].DefectScenarioItems[j].Id == defectText)
                            //                        {
                            //                            ((DeviceControl.AlignerDevice)propertyGrid.SelectedObject).DefectScenarioItems.Add(unitDevice.ImageScenarioItems[i].DefectScenarioItems[j] as DefectDevice);

                            //                        }
                            //                    }
                            //                }


                            //            }

                            //        }
                            //    }
                            //    return;
                            //}
                            //if (tar_type == "Inspection")
                            //{
                            //    addNodeInLst();
                            //    treeView.SelectedNode = targetNode;
                            //    unitText = sourceNode.FullPath.ToString().Split('\\')[0];
                            //    imageText = unitText + '.' + sourceNode.FullPath.ToString().Split('\\')[1];
                            //    defectText = imageText + '.' + sourceNode.FullPath.ToString().Split('\\')[2];
                            //    foreach (var unitDevice in lstUnitDevice)
                            //    {
                            //        if (unitDevice.Id == unitText)
                            //        {
                            //            for (int i = 0; i < unitDevice.ImageScenarioItems.Count; i++)
                            //            {
                            //                if (unitDevice.ImageScenarioItems[i].Id == imageText)
                            //                {
                            //                    for (int j = 0; j < unitDevice.ImageScenarioItems[i].DefectScenarioItems.Count; j++)
                            //                    {
                            //                        if (unitDevice.ImageScenarioItems[i].DefectScenarioItems[j].Id == defectText)
                            //                        {
                            //                            ((DeviceControl.InspectionDevice)propertyGrid.SelectedObject).DefectScenarioItems.Add(unitDevice.ImageScenarioItems[i].DefectScenarioItems[j] as DefectDevice);

                            //                        }
                            //                    }
                            //                }


                            //            }

                            //        }
                            //    }
                            //}
                            //if (tar_type == "Image")
                            //{
                            //    addNodeInLst();
                            //    treeView.SelectedNode = targetNode;
                            //    unitText = sourceNode.FullPath.ToString().Split('\\')[0];
                            //    imageText = unitText + '.' + sourceNode.FullPath.ToString().Split('\\')[1];
                            //    defectText = imageText + '.' + sourceNode.FullPath.ToString().Split('\\')[2];
                            //    foreach (var unitDevice in lstUnitDevice)
                            //    {
                            //        if (unitDevice.Id == unitText)
                            //        {
                            //            for (int i = 0; i < unitDevice.ImageScenarioItems.Count; i++)
                            //            {
                            //                if (unitDevice.ImageScenarioItems[i].Id == imageText)
                            //                {
                            //                    for (int j = 0; j < unitDevice.ImageScenarioItems[i].DefectScenarioItems.Count; j++)
                            //                    {
                            //                        if (unitDevice.ImageScenarioItems[i].DefectScenarioItems[j].Id == defectText)
                            //                        {
                            //                            ((DeviceControl.ImageDevice)propertyGrid.SelectedObject).DefectScenarioItems.Add(unitDevice.ImageScenarioItems[i].DefectScenarioItems[j] as DefectDevice);

                            //                        }
                            //                    }
                            //                }


                            //            }

                            //        }
                            //    }
                            //}
                            //if (tar_type == "Camera")
                            //{
                            //    addNodeInLst();
                            //    treeView.SelectedNode = targetNode;
                            //    unitText = sourceNode.FullPath.ToString().Split('\\')[0];           //Unit1
                            //    imageText = unitText + '.' + sourceNode.FullPath.ToString().Split('\\')[1]; //Unit1.Image1
                            //    defectText = imageText + '.' + sourceNode.FullPath.ToString().Split('\\')[2];//Unit1.Image1.Defect1
                            //    foreach (var unitDevice in lstUnitDevice)
                            //    {
                            //        if (unitDevice.Id == unitText)
                            //        {
                            //            for (int i = 0; i < unitDevice.ImageScenarioItems.Count; i++)
                            //            {
                            //                if (unitDevice.ImageScenarioItems[i].Id == imageText)
                            //                {
                            //                    for (int j = 0; j < unitDevice.ImageScenarioItems[i].DefectScenarioItems.Count; j++)
                            //                    {
                            //                        if (unitDevice.ImageScenarioItems[i].DefectScenarioItems[j].Id == defectText)
                            //                        {
                            //                            ((DeviceControl.CameraDevice)propertyGrid.SelectedObject).DefectScenarioItems.Add(unitDevice.ImageScenarioItems[i].DefectScenarioItems[j] as DefectDevice);
                            //                        }
                            //                    }
                            //                }
                            //            }
                            //        }
                            //    }
                            //}
                            //return;
                            return;
                        case "Aligner": //fixture aligner일 때 
                            addNodeInLst();
                            //if (((DeviceControl.AlignerDevice)propertyGrid.SelectedObject).AlignAlgoType == enAlignAlgo.SimpleAlign_Fixture && tar_type == "Defect")
                            //{
                            //    addNodeInLst();                                                             //2023.02.06 mck 위의 for if 쭉 나열해놓은거 이것처럼 다 바꿔놓기.
                            //    unitText = targetNode.FullPath.ToString().Split('\\')[0];
                            //    imageText = unitText + '.' + targetNode.FullPath.ToString().Split('\\')[1];
                            //    defectText = imageText + '.' + targetNode.FullPath.ToString().Split('\\')[2];

                            //    int unitIdx = lstUnitDevice.FindIndex(x => x.Id == unitText);
                            //    int ImageIdx = lstUnitDevice[unitIdx].ImageScenarioItems.FindIndex(x => x.Id == imageText);
                            //    int defectIdx = lstUnitDevice[unitIdx].ImageScenarioItems[ImageIdx].DefectScenarioItems.FindIndex(x => x.Id == defectText);
                            //    if (unitIdx != null && ImageIdx != null && defectIdx != null)
                            //    {
                            //        ((DeviceControl.AlignerDevice)propertyGrid.SelectedObject).DefectScenarioItems.Add(lstUnitDevice[unitIdx].ImageScenarioItems[ImageIdx].DefectScenarioItems[defectIdx] as DefectDevice);
                            //        ((DeviceControl.AlignerDevice)propertyGrid.SelectedObject).DefectScenarioItems = ((DeviceControl.AlignerDevice)propertyGrid.SelectedObject).DefectScenarioItems.Distinct().ToList();
                            //    }
                            //    // fixture aligner에서 defect로 옮겼을 때 defect의 alignID(Align 키)
                            //    treeView.SelectedNode = targetNode;
                            //    ((DeviceControl.DefectDevice)propertyGrid.SelectedObject).AlignId = (sourceNode.Parent.Parent.Text + '.' + sourceNode.Parent.Text + "." + sourceNode.Text);
                            //    //StandardPos 삽입하기. (데이터 보니까 Double이라는 밸류를 받던데, 어디에서 나오는 걸까?)
                            //    return;
                            //}
                            //if (tar_type == "Result")
                            //{
                            //    //addNodeInLst();       가시성 의견 수용(함수 삭제도 고려) 2023.02.07 mck
                            //    treeView.SelectedNode = targetNode;
                            //    treeView.SelectedNode = targetNode;
                            //    unitText = sourceNode.FullPath.ToString().Split('\\')[0];
                            //    imageText = unitText + '.' + sourceNode.FullPath.ToString().Split('\\')[1];
                            //    defectText = imageText + '.' + sourceNode.FullPath.ToString().Split('\\')[2];
                            //    //unitId = Convert.ToInt32(Regex.Replace(unitText, @"\D", ""));       //숫자만 추출
                            //    int unitIdx = lstUnitDevice.FindIndex(x => x.Id == unitText);
                            //    int ImageIdx = lstUnitDevice[unitIdx].ImageScenarioItems.FindIndex(x => x.Id == imageText);
                            //    int AlignerIdx = lstUnitDevice[unitIdx].ImageScenarioItems[ImageIdx].AlignerScenarioItems.FindIndex(x => x.Id == defectText);
                            //    if (unitIdx != null && ImageIdx != null && AlignerIdx != null)
                            //    {
                            //        ((DeviceControl.ResultDevice)propertyGrid.SelectedObject).AlignerScenarioItems.Add(lstUnitDevice[unitIdx].ImageScenarioItems[ImageIdx].AlignerScenarioItems[AlignerIdx] as AlignerDevice);
                            //        ((DeviceControl.ResultDevice)propertyGrid.SelectedObject).AlignerScenarioItems = ((DeviceControl.ResultDevice)propertyGrid.SelectedObject).AlignerScenarioItems.Distinct().ToList();
                            //    }

                            //}
                            return;
                        case "Inspection":
                            addNodeInLst();
                            if (tar_type == "Result")
                            {
                                //addNodeInLst(); 가시성 피드백 수용 (함수 삭제 고려) 2023.02.07 mck
                            //    treeView.SelectedNode = targetNode;
                            //    unitText = sourceNode.FullPath.ToString().Split('\\')[0];
                            //    imageText = unitText + '.' + sourceNode.FullPath.ToString().Split('\\')[1];
                            //    defectText = imageText + '.' + sourceNode.FullPath.ToString().Split('\\')[2];
                            //    //unitId = Convert.ToInt32(Regex.Replace(unitText, @"\D", ""));       //숫자만 추출
                            //    foreach (var unitDevice in lstUnitDevice)
                            //    {
                            //        if (unitDevice.Id == unitText)
                            //        {
                            //            for (int i = 0; i < unitDevice.ImageScenarioItems.Count; i++)
                            //            {
                            //                if (unitDevice.ImageScenarioItems[i].Id == imageText)
                            //                {
                            //                    for (int j = 0; j < unitDevice.ImageScenarioItems[i].InspScenarioItems.Count; j++)
                            //                    {
                            //                        if (unitDevice.ImageScenarioItems[i].InspScenarioItems[j].Id == defectText)
                            //                        {
                            //                            ((DeviceControl.ResultDevice)propertyGrid.SelectedObject).InspScenarioItems.Add(unitDevice.ImageScenarioItems[i].InspScenarioItems[j] as InspectionDevice);

                            //                        }
                            //                    }
                            //                }


                            //            }

                            //        }
                            //    }
                            //    return;
                            }
                            return;
                        case "Result":
                            addNodeInLst();
                            return;
                        case "Common":
                            addNodeInLst();
                            return;
                        case "Cim":
                            addNodeInLst();
                            return;
                    }

                    this.SelectedNode = targetNode;

                }
                catch { }
            }

        }

        protected override void Put_in_MouseDoubleClick(object sender, MouseEventArgs e) //지울 때 시나리오 아이템에 있는얘들 다 꺼내버리는 코드 구현
        {
            Point dropPoint = e.Location;
            TreeNode deleteNode = this.GetNodeAt(dropPoint);
            if (deleteNode == null) { }
            else//여기 그림 지우는 함수
            {
                string delNodeFullPath = deleteNode.Name;
                lstNodePair.RemoveAll(lstNodePair => delNodeFullPath == lstNodePair.targetNodeID);
                this.Invalidate();      
            }
        //    this.SelectedNode = deleteNode;           여기는 시나리오 아이템 싹 다 비워주는 메소드인데 종속성 문제 때문에 못씀.
        //    string deviceType = Regex.Replace(deleteNode.Text, @"\d", "");
        //    switch (deviceType)
        //    {
        //        case "Image":
        //            ((DeviceControl.ImageDevice)propertyGrid.SelectedObject).DefectScenarioItems.Clear();
        //            return;
        //        case "Aligner":
        //            ((DeviceControl.AlignerDevice)propertyGrid.SelectedObject).DefectScenarioItems.Clear();
        //            return;
        //        case "Camera":
        //            ((DeviceControl.CameraDevice)propertyGrid.SelectedObject).DefectScenarioItems.Clear();
        //            return;
        //        case "Inspection":
        //            ((DeviceControl.InspectionDevice)propertyGrid.SelectedObject).DefectScenarioItems.Clear();
        //            return;
        //    }
        }

        protected override void Put_in_treeView_MouseClick(object sender, MouseEventArgs e)
        {
            TreeNode targetNode = this.GetNodeAt(e.X, e.Y);
            if (onlyNode == false && targetNode != null && e.Button == MouseButtons.Right)
            {
                if (targetNode.Text.Contains("Aligner") || targetNode.Text.Contains("Inspection"))  //aligner하고 inspection에 마우스 우클릭을 했을 떄 연결된 애들 보여주는 메소드
                {
                    onlyNode = true;
                    foreach (var _Node2Node in lstNodePair)
                    {
                        if (_Node2Node.targetNodeID == targetNode.Name)
                        {
                            onlyDrawList.Add(_Node2Node);
                        }
                    }
                    activate = true;
                    this.Invalidate();
                }

            }
            else if (onlyNode == true && activate == true)
            {
                onlyDrawList.Clear();
                onlyNode = false;
                activate = false;
                this.CollapseAll();
                this.ExpandAll();
            }
            tempDrawList.Clear();
        }

    }
}
