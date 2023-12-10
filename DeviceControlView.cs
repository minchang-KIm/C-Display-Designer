using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using DeviceControl;
using TBaseLightChannel;
using SystemData.SetupData;
using CommonDefine.Interface;
using System.IO;
using CommonDefine.Abstract;
using CommonDefine;
using SystemData;
using System.Text;
using InsepctionMethods;
using static InsepctionMethods.InspectionSupport;
using DefectParts;
using static TInspSolutionPlatForm.NavigatorControlView;
using FuncTool;
using static CommonDefine.DefEnums;
using System.Drawing;
using Microsoft.VisualBasic;
using System.Text.RegularExpressions;

namespace TInspSolutionPlatForm
{
    public partial class DeviceControlView : DoubleBufferUserControl
    {
        public enum Layer
        {
            Layer_0,//unit
            Layer_1,//image,camera
            Layer_2,//align,inspection,defect
            Layer_3,//imageprocessing
            Layer_4,
        }
        //public enum enDeviceTarget { eRunning, eModel};
        public TreeNode PreviousSelectedNode = null;//2021.08.18_3 kimgun
        public TreeNode PreviousSelectedNode2 = null;//2023.01.20_1 kimgun
        public delegate void deleDeviceSelected(UnitDevice unit, IDeviceItem device);
        public event deleDeviceSelected evtSeletected;
        private List<UnitDevice> lstUnitDevice;//{ get => SystemManager.SystemManager.Instance.Units; }
        private CommonDevice commonDevice;
        private CimDevice cimDevice;//2021.07.26_2 jhyun
        private AlignerDevice pasteAlign = null;//2021.08.25_1 kimgun
        private InspectionDevice pasteInsp = null;//2021.08.25_1 kimgun
        private DefectDevice pasteDefect = null;//2022.08.11_1_1 kimgun
        private int UnitCount = 0;
        private bool AddDeleteDevice = false;//2021.07.26_1 jhyun
        private ToolStripMenuItem subMenuItem = null;//2022.08.11_1 kimgun
        private bool isAllowNodeRenaming = false;
        private TreeView TreeViewCache = new TreeView();
        private bool IsFilter = false;
        private bool IsComboFilter = false;
        private Random random;  //2023.02.17 mck
        private bool allignDup = false; //2023.02.17 mck
        private bool inspDup = false;   //2023.02.17 mck
        public bool onlyNode = false;   //2023.02.17 mck

        //private enDeviceTarget eDeviceTarget;
        public DeviceControlView()
        {
            InitializeComponent();
            treeView.AfterLabelEdit += TreeView_AfterLabelEdit;
        }

        private void TreeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            isAllowNodeRenaming = false;
            if (e.Label == null || e.Label.Length == 0)
            {
                e.CancelEdit = true;
                return;
            }
            object propertyData = null;
            string[] tmp = treeView.SelectedNode.FullPath.Split('\\');
            var SelectedNode = treeView.SelectedNode;
            if (IsFilter || IsComboFilter)
            {//2023.01.27_1 kimgun
                var nodes = TreeViewCache.Nodes.Find(treeView.SelectedNode.Name, true);
                if (nodes == null || nodes.Length == 0)
                    return;
                foreach (var nd in nodes)
                {
                    if (nd.FullPath.Contains(SelectedNode.FullPath))
                    {
                        SelectedNode = nd;
                        break;
                    }
                }
                tmp = SelectedNode.FullPath.Split('\\');
            }
            string[] sKey = new string[tmp.Length];
            bool contextHide = false;
            
            TreeNode node1 = SelectedNode;// e.Node;
            for (int i = tmp.Length - 1; i >= 0; i--)
            {
                sKey[i] = node1.Name as string;
                node1 = node1.Parent;
            }
            string[] node = e.Node.Name.Split('.');
            UnitDevice unitDevice = null;// lstUnitDevice.Where(p => p.Id == sKey[0]).FirstOrDefault() as UnitDevice;
            
            if (node.Last().Contains("Common"))
            {
                propertyData = commonDevice;
            }
            else if (node.Last().Contains("Cim"))
            {//2021.07.26_2 jhyun
                propertyData = cimDevice;
            }
            else if (node.Last().Contains("Unit"))
            {
                unitDevice = lstUnitDevice.Where(p => p.Id == e.Node.Text).FirstOrDefault() as UnitDevice;
                propertyData = unitDevice;
            }
            else if (node.Last().Contains("Camera"))
            {
                unitDevice = lstUnitDevice.Where(p => p.Id == sKey[0]).FirstOrDefault() as UnitDevice;
                if (unitDevice == null) return;
                propertyData = unitDevice.CameraScenarioItems.Where(p => p.Key == sKey.Last()).FirstOrDefault();
            }
            else if (node.Last().Contains("Calibration"))
            {
                unitDevice = lstUnitDevice.Where(p => p.Id == sKey[0]).FirstOrDefault() as UnitDevice;
                if (unitDevice == null) return;
                propertyData = unitDevice.CalibrationSenarioItems.Where(p => p.Key == sKey.Last()).FirstOrDefault();
            }
            else if (node.Last().Contains("Result"))
            {//2022.08.11_1 kimgun
                unitDevice = lstUnitDevice.Where(p => p.Id == sKey[0]).FirstOrDefault() as UnitDevice;
                if (unitDevice == null) return;
                propertyData = (unitDevice as UnitDevice).ResultScenarioItems.Where(p => p.Key == sKey.Last()).FirstOrDefault();
            }
            else if (node.Last().Contains("Light"))
            {
                unitDevice = lstUnitDevice.Where(p => p.Id == sKey[0]).FirstOrDefault() as UnitDevice;
                if (unitDevice == null) return;
                CameraDevice camera = unitDevice.CameraScenarioItems.Where(p => p.Key == sKey[1]).FirstOrDefault();
                if (camera == null) return;
                propertyData = camera.LightScenarioItems.Where(p => p.Id == sKey[2]).FirstOrDefault();
            }
            else if (node.Last().Contains("Image"))
            {
                unitDevice = lstUnitDevice.Where(p => p.Id == sKey[0]).FirstOrDefault() as UnitDevice;
                if (unitDevice == null) return;
                propertyData = unitDevice.ImageScenarioItems.Where(p => p.Key == sKey.Last()).FirstOrDefault();
            }
            else if (node.Last().Contains("Aligner"))
            {
                unitDevice = lstUnitDevice.Where(p => p.Id == sKey[0]).FirstOrDefault() as UnitDevice;
                if (unitDevice == null) return;
                var Image = unitDevice.ImageScenarioItems.Where(p => p.Key == sKey[1]).FirstOrDefault();
                var result = unitDevice.ResultScenarioItems.Where(p => p.Key == sKey[1]).FirstOrDefault();
                if (Image != null)
                {
                    propertyData = (Image as ImageDevice).AlignerScenarioItems.Where(p => p.Key == sKey.Last()).FirstOrDefault();
                }
                else
                {
                    propertyData = (result as ResultDevice).AlignerScenarioItems.Where(p => p.Key == sKey.Last()).FirstOrDefault();
                }
            }
            else if (node.Last().Contains("Inspection"))
            {
                unitDevice = lstUnitDevice.Where(p => p.Id == sKey[0]).FirstOrDefault() as UnitDevice;
                if (unitDevice == null) return;
                var Image = unitDevice.ImageScenarioItems.Where(p => p.Key == sKey[1]).FirstOrDefault();
                var result = unitDevice.ResultScenarioItems.Where(p => p.Key == sKey[1]).FirstOrDefault();
                if (Image != null)
                {
                    propertyData = (Image as ImageDevice).InspScenarioItems.Where(p => p.Key == sKey.Last()).FirstOrDefault();
                }
                else if (result != null)
                {//2022.08.11_1 kimgun
                    if (result != null)
                        propertyData = (result as ResultDevice).InspScenarioItems.Where(p => p.Key == sKey.Last()).FirstOrDefault();
                }
            }
            else if (node.Last().Contains("Defect"))
            {
                if (node[1].Contains("Calibration"))
                {
                    unitDevice = lstUnitDevice.Where(p => p.Id == sKey[0]).FirstOrDefault() as UnitDevice;
                    if (unitDevice == null) return;
                    var Cal = unitDevice.CalibrationSenarioItems.Where(p => p.Key == sKey[1]).FirstOrDefault();
                    if (Cal != null)
                    {
                        propertyData = (Cal as CalibrationDevice).DefectScenarioItems.Where(p => p.Key == sKey[2]).FirstOrDefault();
                    }
                }
                else
                {
                    unitDevice = lstUnitDevice.Where(p => p.Id == sKey[0]).FirstOrDefault() as UnitDevice;
                    if (unitDevice == null) return;

                    var Image = unitDevice.ImageScenarioItems.Where(p => p.Key == sKey[1]).FirstOrDefault();
                    var result = unitDevice.ResultScenarioItems.Where(p => p.Key == sKey[1]).FirstOrDefault();
                    var Cam = unitDevice.CameraScenarioItems.Where(p => p.Key == sKey[1]).FirstOrDefault();//2022.12.16_2 cgun
                    if (Image != null)
                    {
                        propertyData = (Image as ImageDevice).DefectScenarioItems.Where(p => p.Key == sKey.Last()).FirstOrDefault();//2022.08.11_1 kimgun                        
                    }
                    else if (result != null)
                    {
                        var tmpKey = sKey[2].Split('.');
                        string key2 = tmpKey[0] + "." + tmpKey[1];
                        Image = unitDevice.ImageScenarioItems.Where(p => p.Key == key2).FirstOrDefault();
                        propertyData = (Image as ImageDevice).DefectScenarioItems.Where(p => p.Key == sKey.Last()).FirstOrDefault();//2022.08.11_1 kimgun                        
                    }
                    if (Cam != null)//2022.12.16_2 cgun
                    {
                        propertyData = (Cam as CameraDevice).DefectScenarioItems.Where(p => p.Key == sKey.Last()).FirstOrDefault();
                    }
                }
            }
            else if (node.Last().Contains("Processing"))
            {
                unitDevice = lstUnitDevice.Where(p => p.Id == sKey[0]).FirstOrDefault() as UnitDevice;
                if (unitDevice == null) return;

                if (sKey[1].Contains("Camera"))
                {
                    //2022.12.16_2 cgun
                    var Cam = unitDevice.CameraScenarioItems.Where(p => p.Key == sKey[1]).FirstOrDefault();
                    DefectDevice defect = null;
                    defect = (Cam as CameraDevice).DefectScenarioItems.Where(p => p.Key == sKey[2]).FirstOrDefault();
                    if (defect == null)
                    {
                        defect = (Cam as CameraDevice).DefectScenarioItems.Where(p => p.Key == sKey[3]).FirstOrDefault();
                        if (defect != null)
                            propertyData = (defect as DefectDevice).ImageProcessingScenarioItems.Where(p => p.Key == sKey[(int)Layer.Layer_3]).FirstOrDefault();
                    }
                    else
                        propertyData = (defect as DefectDevice).ImageProcessingScenarioItems.Where(p => p.Key == sKey[(int)Layer.Layer_3]).FirstOrDefault();
                }
                else
                {
                    var Image = unitDevice.ImageScenarioItems.Where(p => p.Key == sKey[1]).FirstOrDefault();
                    if (Image == null)
                    {
                        var result = unitDevice.ResultScenarioItems.Where(p => p.Key == sKey[1]).FirstOrDefault();
                        if (result != null)
                        {
                            var tmpKey = sKey[2].Split('.');
                            string resultkey = tmpKey[0] + "." + tmpKey[1];
                            Image = unitDevice.ImageScenarioItems.Where(p => p.Key == resultkey).FirstOrDefault();
                        }
                    }
                    var align = (Image as ImageDevice).AlignerScenarioItems.Where(p => p.Key == sKey[2]).FirstOrDefault();
                    var Insp = (Image as ImageDevice).InspScenarioItems.Where(p => p.Key == sKey[2]).FirstOrDefault();
                    DefectDevice defect = null;
                    defect = (Image as ImageDevice).DefectScenarioItems.Where(p => p.Key == sKey[2]).FirstOrDefault();
                    if (defect == null)
                    {
                        if (align != null)
                            defect = (align as AlignerDevice).DefectScenarioItems.Where(p => p.Key == sKey[3]).FirstOrDefault();
                        else if (Insp != null)
                            defect = (Insp as InspectionDevice).DefectScenarioItems.Where(p => p.Key == sKey[3]).FirstOrDefault();
                        else return;
                        propertyData = (defect as DefectDevice).ImageProcessingScenarioItems.Where(p => p.Key == sKey[(int)Layer.Layer_4]).FirstOrDefault();
                    }
                    else
                        propertyData = (defect as DefectDevice).ImageProcessingScenarioItems.Where(p => p.Key == sKey[(int)Layer.Layer_3]).FirstOrDefault();
                }
            }
            if (propertyData != null)
                (propertyData as IId).Title = e.Label;

                
            var nodes2 = treeView.Nodes.Find(sKey.Last(), true);//2023.01.27_1 kimgun
                
            if (nodes2 != null)
            {//2023.01.27_1 kimgun
                foreach (var nd in nodes2)
                    nd.Text = e.Label;
            }
            var nodes3 = TreeViewCache.Nodes.Find(sKey.Last(), true);//2023.01.27_1 kimgun
            if (nodes3 != null)
            {//2023.01.27_1 kimgun
                foreach (var nd in nodes3)
                    nd.Text = e.Label;
            }
        }

        private void DeviceControlView_Load(object sender, EventArgs e)
        {
        }
        public void UnitSync(enDeviceTarget deviceTarget)
        {
            SystemState.DeviceTarget = deviceTarget;
            EnableControl(deviceTarget);

            commonDevice = SystemManager.SystemManager.Instance.CommonUnit;
            cimDevice = SystemManager.SystemManager.Instance.CimUnit;//2021.07.26_2 jhyun
            lstUnitDevice = SystemManager.SystemManager.Instance.Units;//2021.08.20 kimxxxxxx.ToList();
            UnitCount = lstUnitDevice.Count;
            UpdateTreeView();

            //if (deviceTarget == enDeviceTarget.eRunning)
            //{
            //commonDevice = SystemManager.SystemManager.Instance.CommonUnit;
            //lstUnitDevice = SystemManager.SystemManager.Instance.Units.ToList();
            //}
            //else
            //{
            //ClearTempUnit();
            //SystemManager.SystemManager.Instance.InitDeviceforModel();
            //SystemManager.SystemManager.Instance.CreateDeviceforModel();
            //SystemManager.SystemManager.Instance.LoadUnitDeviceforModel(false);
            //commonDevice = SystemManager.SystemManager.Instance.CommonUnit;
            //lstUnitDevice = SystemManager.SystemManager.Instance.Units.ToList();
            //SystemManager.SystemManager.Instance.Assign(lstUnitDevice);
            //UpdateTreeView();

            //}
            //UnitCount = lstUnitDevice.Count;

            //SystemManager.SystemManager.Instance.InitDevice();
            //SystemManager.SystemManager.Instance.CreateDevice();
            //SystemManager.SystemManager.Instance.LoadUnitDevice();
            //commonDevice = SystemManager.SystemManager.Instance.CommonUnit;
            //lstUnitDevice = SystemManager.SystemManager.Instance.Units.ToList();
            //SystemManager.SystemManager.Instance.Assign(lstUnitDevice);
            //SystemManager.SystemManager.Instance.ThreadStart();
            //UpdateTreeView();
        }

        public void ClearTempUnit()
        {
            if (lstUnitDevice != null)
            {
                foreach (var unit in lstUnitDevice)
                {
                    unit.Dispose();
                }
                lstUnitDevice.Clear();
                lstUnitDevice = null;
            }
            UnitCount = 0;
        }

        public void Initialize(enDeviceTarget deviceTarget)
        {
            SystemState.DeviceTarget = deviceTarget;

            UnitSync(deviceTarget);

            contextMenuStrip.ItemClicked += ContextMenuStrip_ItemClicked;
            contextMenuStrip.Opened += ContextMenuStrip_Opened;

            UnitCount = lstUnitDevice.Count;
            if (lstUnitDevice.Count != 0)
                UpdateTreeView();

            if (SystemState.DeviceTarget == enDeviceTarget.eRunning)
            {
                btnBackup.Visible = false;
                btnBackupLoad.Visible = false;
            }
        }

        private void ContextMenuStrip_Opened(object sender, EventArgs e)
        {

        }

        private void UpdateTreeView()
        {
            if (treeView.SelectedNode != null)
            {
                if(PreviousSelectedNode2 != null)
                PreviousSelectedNode2.BeginEdit();
                PreviousSelectedNode2 = treeView.SelectedNode;//.Clone() as TreeNode;
                PreviousSelectedNode2.EndEdit(false);

            }

            treeView.Nodes.Clear();
            int iUnitIndex = 0;
            foreach (var unitDevice in lstUnitDevice)
            {//Unit
                treeView.Nodes.Add(unitDevice.Id, unitDevice.Title);//2021.07.20_1 kimgun
                int iCameraIndex = 0;
                foreach (var camDevice in unitDevice.CameraScenarioItems)
                {//Camera
                    treeView.Nodes[iUnitIndex].Nodes.Add(camDevice.Id, camDevice.Title);//2021.07.20_1 kimgun
                    int ilightIndex = 0;
                    foreach (var lightDevice in camDevice.LightScenarioItems)
                    {//Light
                        treeView.Nodes[iUnitIndex].Nodes[iCameraIndex].Nodes.Add(lightDevice.Id, lightDevice.Title);
                        ilightIndex++;
                    }

                    //2022.12.16_2 cgun
                    int iDefIndex = 0;
                    foreach (var defectDevice in camDevice.DefectScenarioItems)
                    {//Defect
                        treeView.Nodes[iUnitIndex].Nodes[iCameraIndex].Nodes.Add(defectDevice.Id, defectDevice.Title);
                        foreach (var procDevice in defectDevice.ImageProcessingScenarioItems)
                        {
                            treeView.Nodes[iUnitIndex].Nodes[iCameraIndex].Nodes[iDefIndex].Nodes.Add(procDevice.Id, procDevice.Title);
                        }
                        iDefIndex++;
                    }

                    //2023.01.31 cgun
                    foreach (var imageProcessDevice in camDevice.ImageProcessingScenarioItems)
                    {
                        treeView.Nodes[iUnitIndex].Nodes[iCameraIndex].Nodes.Add(imageProcessDevice.Id, imageProcessDevice.Title);
                    }

                    iCameraIndex++;
                }

                int iImageIndex = unitDevice.CameraScenarioItems.Count;
                foreach (var ImageDevice in unitDevice.ImageScenarioItems)
                {//Image
                    treeView.Nodes[iUnitIndex].Nodes.Add(ImageDevice.Id, ImageDevice.Title);//2021.07.20_1 kimgun


                    //2022.08.11_1 kimgun
                    int iDefIndex = 0;
                    foreach (var defectDevice in ImageDevice.DefectScenarioItems)
                    {//Defect
                        treeView.Nodes[iUnitIndex].Nodes[iImageIndex].Nodes.Add(defectDevice.Id, defectDevice.Title);//2021.07.20_1 kimgun
                        foreach (var procDevice in defectDevice.ImageProcessingScenarioItems)
                        {//Processing
                            treeView.Nodes[iUnitIndex].Nodes[iImageIndex].Nodes[iDefIndex].Nodes.Add(procDevice.Id, procDevice.Title);//2021.07.20_1 kimgun
                        }
                        iDefIndex++;
                    }

                    int alignIndex = ImageDevice.DefectScenarioItems.Count;
                    foreach (var alignDevice in ImageDevice.AlignerScenarioItems)
                    {//aligner
                        treeView.Nodes[iUnitIndex].Nodes[iImageIndex].Nodes.Add(alignDevice.Id, alignDevice.Title);//2021.07.20_1 kimgun
                        int irefDefIndex = 0;
                        foreach (var defectDevice in alignDevice.DefectScenarioItems)
                        {//Defect
                            treeView.Nodes[iUnitIndex].Nodes[iImageIndex].Nodes[alignIndex].Nodes.Add(defectDevice.Id, defectDevice.Title);//2021.07.20_1 kimgun
                            foreach (var procDevice in defectDevice.ImageProcessingScenarioItems)
                            {//Processing
                                treeView.Nodes[iUnitIndex].Nodes[iImageIndex].Nodes[alignIndex].Nodes[irefDefIndex].Nodes.Add(procDevice.Id, procDevice.Title);//2021.07.20_1 kimgun
                            }
                            irefDefIndex++;
                        }
                        alignIndex++;
                    }


                    int iInspIndex = ImageDevice.DefectScenarioItems.Count + ImageDevice.AlignerScenarioItems.Count;
                    foreach (var inspDevice in ImageDevice.InspScenarioItems)
                    {//Inspection
                        treeView.Nodes[iUnitIndex].Nodes[iImageIndex].Nodes.Add(inspDevice.Id, inspDevice.Title);//2021.07.20_1 kimgun
                        int irefDefIndex = 0;
                        foreach (var defectDevice in inspDevice.DefectScenarioItems)
                        {//Defect
                            treeView.Nodes[iUnitIndex].Nodes[iImageIndex].Nodes[iInspIndex].Nodes.Add(defectDevice.Id, defectDevice.Title);//2021.07.20_1 kimgun
                            foreach (var procDevice in defectDevice.ImageProcessingScenarioItems)
                            {//Processing
                                treeView.Nodes[iUnitIndex].Nodes[iImageIndex].Nodes[iInspIndex].Nodes[irefDefIndex].Nodes.Add(procDevice.Id, procDevice.Title);//2021.07.20_1 kimgun
                            }
                            irefDefIndex++;
                        }
                        //2023.01.19_1 kimgun
                        int irefInspIndex = irefDefIndex;
                        foreach (var refInspDevice in inspDevice.InspectionScenarioItems)
                        {
                            treeView.Nodes[iUnitIndex].Nodes[iImageIndex].Nodes[iInspIndex].Nodes.Add(refInspDevice.Id, refInspDevice.Title);//2021.07.20_1 kimgun
                            int irefDefIndex2 = 0;
                            foreach (var defectDevice in refInspDevice.DefectScenarioItems)
                            {//Defect
                                treeView.Nodes[iUnitIndex].Nodes[iImageIndex].Nodes[iInspIndex].Nodes[irefInspIndex].Nodes.Add(defectDevice.Id, defectDevice.Title);//2021.07.20_1 kimgun
                                foreach (var procDevice in defectDevice.ImageProcessingScenarioItems)
                                {//Processing
                                    treeView.Nodes[iUnitIndex].Nodes[iImageIndex].Nodes[iInspIndex].Nodes[irefInspIndex].Nodes[irefDefIndex2].Nodes.Add(procDevice.Id, procDevice.Title);//2021.07.20_1 kimgun
                                }
                                irefDefIndex2++;
                            }
                            irefInspIndex++;
                        }
                        iInspIndex++;
                    }
                    iImageIndex++;

                }
                int iCalIndex = iImageIndex;
                foreach (var CalibrationDevice in unitDevice.CalibrationSenarioItems)
                {//Calibration
                    treeView.Nodes[iUnitIndex].Nodes.Add(CalibrationDevice.Id, CalibrationDevice.Title);//2021.07.20_1 kimgun
                    foreach (var defectDevice in CalibrationDevice.DefectScenarioItems)
                    {//Defect
                        treeView.Nodes[iUnitIndex].Nodes[iCalIndex].Nodes.Add(defectDevice.Id, defectDevice.Title);//2021.07.20_1 kimgun
                    }
                    iCalIndex++;
                }

                int iResultIndex = iCalIndex;
                foreach (var resultDevice in unitDevice.ResultScenarioItems)
                {//2022.08.11_1 kimgun
                    treeView.Nodes[iUnitIndex].Nodes.Add(resultDevice.Id, resultDevice.Title);
                    int ralignIndex = 0;
                    foreach (var alignDevice in resultDevice.AlignerScenarioItems)
                    {//aligner
                        treeView.Nodes[iUnitIndex].Nodes[iResultIndex].Nodes.Add(alignDevice.Id, alignDevice.Title);
                        //int irefDefIndex = 0;
                        //foreach (var defectDevice in alignDevice.DefectScenarioItems)
                        //{//Defect
                        //    treeView.Nodes[iUnitIndex].Nodes[iResultIndex].Nodes[ralignIndex].Nodes.Add(defectDevice.Id, defectDevice.Title);
                        //    foreach (var procDevice in defectDevice.ImageProcessingScenarioItems)
                        //    {//Processing
                        //        treeView.Nodes[iUnitIndex].Nodes[iResultIndex].Nodes[ralignIndex].Nodes[irefDefIndex].Nodes.Add(procDevice.Id, procDevice.Title);
                        //    }
                        //    irefDefIndex++;
                        //}
                        ralignIndex++;
                    }


                    int riInspIndex = resultDevice.AlignerScenarioItems.Count;
                    foreach (var inspDevice in resultDevice.InspScenarioItems)
                    {//Inspection
                        treeView.Nodes[iUnitIndex].Nodes[iResultIndex].Nodes.Add(inspDevice.Id, inspDevice.Title);
                        //int irefDefIndex = 0;
                        //foreach (var defectDevice in inspDevice.DefectScenarioItems)
                        //{//Defect
                        //    treeView.Nodes[iUnitIndex].Nodes[iResultIndex].Nodes[riInspIndex].Nodes.Add(defectDevice.Id, defectDevice.Title);
                        //    foreach (var procDevice in defectDevice.ImageProcessingScenarioItems)
                        //    {//Processing
                        //        treeView.Nodes[iUnitIndex].Nodes[iResultIndex].Nodes[riInspIndex].Nodes[irefDefIndex].Nodes.Add(procDevice.Id, procDevice.Title);
                        //    }
                        //    irefDefIndex++;
                        //}
                        riInspIndex++;
                    }
                    iResultIndex++;
                }
                iUnitIndex++;
            }

            if (commonDevice != null)
                treeView.Nodes.Add(commonDevice.Id, commonDevice.Title);//2021.07.20_1 kimgun

            if (cimDevice != null)//2021.07.26_2 jhyun
                treeView.Nodes.Add(cimDevice.Id, cimDevice.Title);

            if (evtSeletected != null)
                evtSeletected.Invoke(null, null);
            treeView.Focus();
            TreeViewCache.Nodes.Clear();
            foreach (TreeNode _node in this.treeView.Nodes)
            {
                TreeViewCache.Nodes.Add((TreeNode)_node.Clone());
            }
            if (IsFilter || IsComboFilter)
            {
                TreeViewFiltering();
            }
            else
            {
                UpdateCombUnit();
                combUnit.SelectedIndex = 0;
            }
            treeView.ExpandAll();
            SetLastSelectedItem();//2023.01.25_1 kimgun
            //2323.2.17 UpdateTreeView 할때마다 scenarioItem의 연결 상태를 보고 그림을 추가하는 코드 저장 불러오기 따로 필요 없어지는 코드
            //List<DrawableTreeView.Link_Node2Node> lstNodePair = new List<DrawableTreeView.Link_Node2Node>();
            //for (int unitidx = 0; unitidx < lstUnitDevice.Count; unitidx++)
            //{
            //    for(int imageidx = 0; imageidx < lstUnitDevice[unitidx].ImageScenarioItems.Count; imageidx++)
            //    {
            //        for(int AlignerIdx = 0; AlignerIdx < lstUnitDevice[unitidx].ImageScenarioItems[imageidx].AlignerScenarioItems.Count; AlignerIdx++)
            //        {
            //            //aligner
            //            for(int defectIdx = 0; defectIdx < lstUnitDevice[unitidx].ImageScenarioItems[imageidx].AlignerScenarioItems[AlignerIdx].DefectScenarioItems.Count; defectIdx++)
            //            {
            //                //2023.02.06 mck
            //                string AlignerFullPath = lstUnitDevice[unitidx].ImageScenarioItems[imageidx].AlignerScenarioItems[AlignerIdx].Id.Replace('.', '\\');
            //                string defectFullPath = lstUnitDevice[unitidx].ImageScenarioItems[imageidx].AlignerScenarioItems[AlignerIdx].DefectScenarioItems[defectIdx].Id.Replace('.', '\\');
                            

            //                DrawableTreeView.Link_Node2Node tempInstacne = new DrawableTreeView.Link_Node2Node();
                            
            //                tempInstacne.sourceNodeID = defectFullPath;
            //                tempInstacne.targetNodeID = AlignerFullPath;
            //                random = new Random(Guid.NewGuid().GetHashCode());
            //                tempInstacne.Colorarr[0] = random.Next(0, 255);
            //                tempInstacne.Colorarr[1] = random.Next(0, 255);
            //                tempInstacne.Colorarr[2] = random.Next(0, 255);

            //                foreach (var tempPair in treeView.lstNodePair)
            //                {
            //                    if (tempPair.sourceNodeID == tempInstacne.sourceNodeID)
            //                    {
            //                        allignDup = true;
            //                    }
            //                }
            //                if(allignDup == false)
            //                {
            //                    treeView.lstNodePair.Add(tempInstacne);
            //                }
            //                else
            //                {
            //                    allignDup = false;
            //                }
            //                tempInstacne = null;
            //                GC.Collect();

            //            }

            //        }
            //        // insp
            //        for(int InspIdx = 0; InspIdx < lstUnitDevice[unitidx].ImageScenarioItems[imageidx].InspScenarioItems.Count; InspIdx++)
            //        {
            //            for (int defectIdx = 0; defectIdx < lstUnitDevice[unitidx].ImageScenarioItems[imageidx].InspScenarioItems[InspIdx].DefectScenarioItems.Count; defectIdx++)
            //            {
            //                //2023.02.06 mck
            //                string AlignerFullPath = lstUnitDevice[unitidx].ImageScenarioItems[imageidx].InspScenarioItems[InspIdx].Id.Replace('.', '\\');
            //                string defectFullPath = lstUnitDevice[unitidx].ImageScenarioItems[imageidx].InspScenarioItems[InspIdx].DefectScenarioItems[defectIdx].Id.Replace('.', '\\');


            //                DrawableTreeView.Link_Node2Node tempInstacne = new DrawableTreeView.Link_Node2Node();
            //                tempInstacne.sourceNodeID = defectFullPath;
            //                tempInstacne.targetNodeID = AlignerFullPath;
            //                random = new Random(Guid.NewGuid().GetHashCode());
            //                tempInstacne.Colorarr[0] = random.Next(0, 255);
            //                tempInstacne.Colorarr[1] = random.Next(0, 255);
            //                tempInstacne.Colorarr[2] = random.Next(0, 255);
            //                foreach(var tempPair in treeView.lstNodePair)
            //                {
            //                    if(tempPair.sourceNodeID == tempInstacne.sourceNodeID)
            //                    {
            //                        inspDup = true;
            //                    }
            //                }
            //                if(inspDup == false)
            //                {
            //                    treeView.lstNodePair.Add(tempInstacne);
            //                }
            //                else
            //                {
            //                    inspDup = false;
            //                }
            //                tempInstacne = null;
            //                GC.Collect();

            //            }
            //        }
            //    }
            //}
            //lstNodePair DrawableTreeView에 있는 lstNodePair로 보내서 DrawableTreeView의 lstNodePair가 0이면 add

        }
        private void UpdateCombUnit()
        {//2023.01.27_1 kimgun
            combUnit.Items.Clear();
            combUnit.Items.Add("All");
            foreach (var u in lstUnitDevice)
                combUnit.Items.Add(u.Id);
        }
        private void SetLastSelectedItem()
        {//2023.01.25_1 kimgun
            if (PreviousSelectedNode2 != null)
            {//2023.01.20_4 kimgun
                try
                {
                    //var fullPath = PreviousSelectedNode2.FullPath as string;
                    var nodes = treeView.Nodes.Find(PreviousSelectedNode2.Name, true);
                    if (nodes.Length > 0)
                    {
                        foreach (var node in nodes)
                        {
                            try
                            {
                                if (node.Parent.Name == PreviousSelectedNode2.Parent.Name)
                                {
                                    treeView.SelectedNode = node;
                                    break;
                                }
                            }
                            catch { SetLastSelectedItemForRemovedItem(); }
                        }
                        if (treeView.SelectedNode == null)
                        {
                            SetLastSelectedItemForRemovedItem();
                        }
                    }
                    else
                    {
                        SetLastSelectedItemForRemovedItem();
                    }
                }
                catch
                {
                    SetLastSelectedItemForRemovedItem();
                }
            }
        }

        private void SetLastSelectedItemForRemovedItem()
        {//2023.01.25_1 kimgun
            TreeNode selectedNode = null;
            try
            {
                selectedNode = PreviousSelectedNode2.PrevNode != null ? PreviousSelectedNode2.PrevNode :
                                                                    PreviousSelectedNode2.NextNode != null ? PreviousSelectedNode2.NextNode :
                                                                    PreviousSelectedNode2.Parent != null ? PreviousSelectedNode2.Parent : null;
            }
            catch(Exception ex)
            {
                if (IsFilter == false && IsComboFilter == false)
                {
                    NoticeAlarmCollection.Instance.AddNoticeAlarm("DeviceControlView", ex.ToString());
                    classLog.WriteTextLog(classLog.LogType.Exception, ex.ToString());
                }
            }
            if (selectedNode != null)
            {
                var nodes2 = treeView.Nodes.Find(selectedNode.Name, true);
                foreach (var node in nodes2)
                {
                    try
                    {
                        if (PreviousSelectedNode2.PrevNode != null || PreviousSelectedNode2.NextNode != null)
                        {
                            if (node.Parent != null && PreviousSelectedNode2.Parent != null)
                            {
                                if (node.Parent.Name == PreviousSelectedNode2.Parent.Name)
                                {
                                    treeView.SelectedNode = node;
                                    treeView.Focus();
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (node != null && PreviousSelectedNode2.Parent != null)
                            {
                                if (node.Name == PreviousSelectedNode2.Parent.Name)
                                {
                                    treeView.SelectedNode = node;
                                    treeView.Focus();
                                    break;
                                }
                            }
                        }
                    }
                    catch { continue; }
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            //if (UnitCount >= SystemOption.param.UnitCount)
            //{
            //    MessageBox.Show("SystemOption에 설정한 유닛 최대 갯수에 도달했습니다.");
            //    return;
            //}

            UnitCount++;

            //SystemOption.param.UnitCount.Value = UnitCount;//2021.07.26_1 jhyun
            //SystemOption.param.Save();//Save 시점으로 변경

            //PLC Mapping 바뀌어서 주석
            //SystemManager.SystemManager.Instance.LoadPlcComponent();

            UnitDevice device = new UnitDevice();
            device.Initialize();
            //device.Create(); //20200614 cjw thread 생성 및 Start는 저장할 때에만 

            device.Id = string.Format("Unit{0}", (UnitCount));
            device.Title = string.Format("Unit{0}", (UnitCount));

            device.Index = (UnitCount) - 1;
            treeView.Nodes.Add(device.Id, device.Title);//2021.07.20_1 kimgun
            lstUnitDevice.Add(device);
            AddDeleteDevice = true;//2021.07.26_1 jhyun
            //SystemOption.param.UnitCount.Value = UnitCount;
            //PLC Mapping 바뀌어서 주석
            //SystemManager.SystemManager.Instance.LoadPlcComponent();

            //Initialize(enDeviceTarget.eModel);

            if (commonDevice.Id == null)
            {
                CommonDevice dv = new CommonDevice();
                dv.Initialize();
                //dv.Create(); //20200614 cjw thread 생성 및 Start는 저장할 때에만 

                dv.Id = "Common";
                dv.Title = "Common";
                dv.Index = 0;
                treeView.Nodes.Add(dv.Id, dv.Title);
                commonDevice = dv;
            }

            if (cimDevice.Id == null)
            {//2021.07.26_2 jhyun
                CimDevice dv = new CimDevice();
                dv.Initialize();

                dv.Id = "Cim";
                dv.Title = "Cim";
                treeView.Nodes.Add(dv.Id, dv.Title);
                cimDevice = dv;
            }
        }
        private void AddContextMenu(params string[] menu)
        {
            foreach (string sm in menu)
                contextMenuStrip.Items.Add(sm);
        }
        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            contextMenuStrip.Items.Clear();
            if (PreviousSelectedNode != null)
            {//2021.08.18_3 kimgun
                PreviousSelectedNode.BackColor = treeView.BackColor;
                PreviousSelectedNode.ForeColor = treeView.ForeColor;
            }
           
            if (IsFilter == false && IsComboFilter == false)
            { //2023.01.27_1 kimgun treeview backup
                TreeViewCache.Nodes.Clear();
                foreach (TreeNode _node in this.treeView.Nodes)
                {
                    TreeViewCache.Nodes.Add((TreeNode)_node.Clone());
                }
            }
            //PreviousSelectedNode = treeView.SelectedNode;
            object propertyData = null;
            string[] tmp = treeView.SelectedNode.FullPath.Split('\\');
            var SelectedNode = treeView.SelectedNode;
            if (IsFilter || IsComboFilter)
            {//2023.01.27_1 kimgun
                var nodes = TreeViewCache.Nodes.Find(treeView.SelectedNode.Name,true);
                if (nodes == null || nodes.Length == 0)
                    return;
                foreach (var nd in nodes)
                {
                    if (nd.FullPath.Contains(SelectedNode.FullPath))
                    {
                        SelectedNode = nd;
                        break;
                    }
                }
                tmp = SelectedNode.FullPath.Split('\\');
            }
            string[] sKey = new string[tmp.Length];
            bool contextHide = false;

            TreeNode node1 = SelectedNode;// e.Node;
            
            for (int i = tmp.Length - 1; i >= 0; i--)
            {
                sKey[i] = node1.Name as string;
                node1 = node1.Parent;
            }
            string[] node = e.Node.Name.Split('.');
            UnitDevice unitDevice = null;
            if (node.Last().Contains("Common"))
            {
                FuncTool.GListUItype.Clear();
                propertyData = commonDevice;
            }
            else if (node.Last().Contains("Cim"))
            {//2021.07.26_2 jhyun
                FuncTool.GListUItype.Clear();
                propertyData = cimDevice;
            }
            else if (node.Last().Contains("Unit"))
            {
                AddContextMenu("Add Camera", "Add Image", "Add Calibration", "Add Result");

                FuncTool.GListUItype.Clear();

                unitDevice = lstUnitDevice.Where(p => p.Id == e.Node.Text).FirstOrDefault() as UnitDevice;
                propertyData = unitDevice;
            }
            else if (node.Last().Contains("Camera"))
            {
                AddContextMenu("Add Light", "Add Defect", "Add Processing");//2022.12.16_2 cgun //2023.01.29 cgun - camera에 processing 추가

                unitDevice = lstUnitDevice.Where(p => p.Id == sKey[0]).FirstOrDefault() as UnitDevice;
                if (unitDevice == null) return;
                propertyData = unitDevice.CameraScenarioItems.Where(p => p.Key == sKey.Last()).FirstOrDefault();

                FuncTool.GListUItype.Clear();
                var lstCam = SystemManager.SystemManager.Instance.GetCameraIdList();
                string[] listofValues = new string[lstCam.Count + 1];
                lstCam.CopyTo(listofValues);
                FuncTool.GListUItype._ListofValues.Add("CameraId", listofValues);

                string keyU = treeView.SelectedNode.Parent.Text;
                var unit = lstUnitDevice.Where(p => p.Id == keyU).FirstOrDefault();
                if (unit != null)
                {
                    var imgList = unit.ImageScenarioItems.Select(x => x.Key).ToArray();
                    FuncTool.GListUItype._ListofValues.Add("TargetId", imgList);

                    var lstdisp = SystemManager.SystemManager.Instance.GetDisplayDevieList();
                    listofValues = new string[lstdisp.Count + 1];
                    lstdisp.CopyTo(listofValues);
                    FuncTool.GListUItype._ListofValues.Add("DisplayId", listofValues);
                }
            }
            else if (node.Last().Contains("Calibration"))
            {
                contextMenuStrip.Items.Add("Add Defect");

                unitDevice = lstUnitDevice.Where(p => p.Id == sKey[0]).FirstOrDefault() as UnitDevice;
                if (unitDevice == null) return;
                propertyData = unitDevice.CalibrationSenarioItems.Where(p => p.Key == sKey.Last()).FirstOrDefault();

                FuncTool.GListUItype.Clear();
                var unit = lstUnitDevice.Where(p => p.Id == sKey[0]).FirstOrDefault();
                var camList = unit.CameraScenarioItems.Select(x => x.Key).ToList();
                string[] targetCam = new string[camList.Count + 1];
                camList.CopyTo(targetCam);
                FuncTool.GListUItype._ListofValues.Add("TargetId", targetCam);
            }
            else if (node.Last().Contains("Result"))
            {//2022.08.11_1 kimgun
                unitDevice = lstUnitDevice.Where(p => p.Id == sKey[0]).FirstOrDefault() as UnitDevice;
                if (unitDevice == null) return;
                propertyData = (unitDevice as UnitDevice).ResultScenarioItems.Where(p => p.Key == sKey.Last()).FirstOrDefault();
                ////2022.08.11_1 kimgun 
                //foreach (var sub in contextMenuStrip.Items)
                //{
                //    if (sub != null)
                //    {
                //        (sub as ToolStripMenuItem).DropDownItemClicked -= Submenu_DropDownItemClicked;
                //        (sub as ToolStripMenuItem).Dispose();
                //    }
                //}
                #region Add menuitems
                contextMenuStrip.Items.Add("Link All");//2023.01.17_1 kimgun
                contextMenuStrip.Items.Add("Unlink All");//2023.01.17_1 kimgun
                var subAligner = new ToolStripMenuItem("Link Aligner");
                var subInspection = new ToolStripMenuItem("Link Inspection");
                foreach (var Image in unitDevice.ImageScenarioItems)
                {
                    subAligner.DropDownItemClicked += Submenu_DropDownItemClicked;
                    if (Image != null)
                    {
                        foreach (AlignerDevice ad in Image.AlignerScenarioItems)
                        {
                            var item = new ToolStripMenuItem(ad.Title); //23.01.12 LSS Key -> Title
                            subAligner.DropDownItems.Add(item);
                        }
                    }
                    subInspection.DropDownItemClicked += Submenu_DropDownItemClicked;
                    if (Image != null)
                    {
                        foreach (InspectionDevice id in Image.InspScenarioItems)
                        {
                            var item = new ToolStripMenuItem(id.Title); //23.01.12 LSS Key -> Title
                            subInspection.DropDownItems.Add(item);
                        }
                    }
                }
                contextMenuStrip.Items.Add(subAligner);
                contextMenuStrip.Items.Add(subInspection);
                subAligner = new ToolStripMenuItem("UnLink Aligner");
                subAligner.DropDownItemClicked += Submenu_DropDownItemClicked;
                subInspection = new ToolStripMenuItem("UnLink Inspection");
                subInspection.DropDownItemClicked += Submenu_DropDownItemClicked;
                if (propertyData != null)
                {
                    foreach (AlignerDevice ad in (propertyData as ResultDevice).AlignerScenarioItems)
                    {
                        var item = new ToolStripMenuItem(ad.Title); //23.01.12 LSS Key -> Title
                        subAligner.DropDownItems.Add(item);
                    }
                    foreach (InspectionDevice id in (propertyData as ResultDevice).InspScenarioItems)
                    {
                        var item = new ToolStripMenuItem(id.Title); //23.01.12 LSS Key -> Title
                        subInspection.DropDownItems.Add(item);
                    }
                }

                contextMenuStrip.Items.Add(subAligner);
                contextMenuStrip.Items.Add(subInspection);
                GC.Collect(0, GCCollectionMode.Forced);
                //contextMenuStrip.Items.Add(subMenuItem);

                #endregion
            }
            else if (node.Last().Contains("Light"))
            {
                unitDevice = lstUnitDevice.Where(p => p.Id == sKey[0]).FirstOrDefault() as UnitDevice;
                if (unitDevice == null) return;
                CameraDevice camera = unitDevice.CameraScenarioItems.Where(p => p.Key == sKey[1]).FirstOrDefault();
                if (camera == null) return;
                propertyData = camera.LightScenarioItems.Where(p => p.Id == sKey[2]).FirstOrDefault();
                FuncTool.GListUItype.Clear();
                if (propertyData == null) return;

                //lightcontrol id list반환
                var arrlc = SystemManager.SystemManager.Instance.GetLightControlInforlList(SystemManager.SystemManager.enLightControlItems.enControlIdList);// lcId);
                string[] listofValues = new string[arrlc.Length];
                Array.Copy(arrlc, listofValues, arrlc.Length);
                FuncTool.GListUItype.Add("LightControlId", listofValues);

                //lightcontrol channel id 와 channel index를 반환
                string lcId = (propertyData as LightDevice).LightControlId;
                if (lcId != null && lcId.Length != 0)
                {
                    //channel  class array
                    var arrChs = SystemManager.SystemManager.Instance.GetLightControlInforlList(SystemManager.SystemManager.enLightControlItems.enChannelList, lcId);
                    if (arrChs != null)
                    {
                        //channel id
                        FuncTool.GListUItype._ListofValues.Add("LightChannelId", arrChs.Select(x => (x as TBaseLightch).Id).ToArray());
                        //chnnel index
                        FuncTool.GListUItype.Add("LightCh", arrChs.Select(x => (x as TBaseLightch).Channel as object).ToArray());
                    }
                }
            }
            else if (node.Last().Contains("Image"))
            {
                contextMenuStrip.Items.Add("Add Aligner");
                contextMenuStrip.Items.Add("Add Inspection");
                contextMenuStrip.Items.Add("Add Defect");//2022.08.11_1 kimgun
                if (pasteAlign != null)
                    contextMenuStrip.Items.Add(string.Format("Paste Aligner;{0}", pasteAlign.Title));//2021.08.25_1 kimgun
                if (pasteInsp != null)
                    contextMenuStrip.Items.Add(string.Format("Paste Inspection;{0}", pasteInsp.Title));//2021.08.25_1 kimgun
                if (pasteDefect != null)
                    contextMenuStrip.Items.Add(string.Format("Paste Defect;{0}", pasteDefect.Title));//2021.08.25_1 kimgun

                unitDevice = lstUnitDevice.Where(p => p.Id == sKey[0]).FirstOrDefault() as UnitDevice;
                if (unitDevice == null) return;
                propertyData = unitDevice.ImageScenarioItems.Where(p => p.Key == sKey.Last()).FirstOrDefault();

                FuncTool.GListUItype.Clear();
                var unit = lstUnitDevice.Where(p => p.Id == sKey[0]).FirstOrDefault();
                var camList = unit.CameraScenarioItems.Select(x => x.Key).ToList();
                string[] targetCam = new string[camList.Count + 1];
                camList.CopyTo(targetCam);
                FuncTool.GListUItype._ListofValues.Add("TargetId", targetCam);

                var lstdisp = SystemManager.SystemManager.Instance.GetDisplayDevieList();
                string[] listofValues = new string[lstdisp.Count + 1];
                lstdisp.CopyTo(listofValues);
                FuncTool.GListUItype._ListofValues.Add("DisplayId", listofValues);

                if(unit.CameraScenarioItems.Count > 0)
                {//2022.12.07_1 kimgun
                    //line scan 카메라가 한 유닛에 여러개 있더라도 frame count는 동일하니까 하나만 찾는다.
                    var cam = unit.CameraScenarioItems.Find(x =>x.Camera != null && x.Camera.IsLinescan);
                    if (cam != null)
                    {
                        List<string> lstIndex = new List<string>();
                        for (int i = -1; i < cam.FrameCount; i++)//2022.12.14_1 kimgun -1은 모든 프레임 사용을 의미한다.
                        {
                            lstIndex.Add((i).ToString());
                        }
                        listofValues = new string[lstIndex.Count];
                        lstIndex.CopyTo(listofValues);
                        FuncTool.GListUItype._ListofValues.Add("TargetIndex", listofValues);
                    }
                }                
            }
            else if (node.Last().Contains("Aligner"))
            {
                if (sKey[1].Contains("Result"))
                    contextHide = true;//2022.08.11_1 kimgun
                unitDevice = lstUnitDevice.Where(p => p.Id == sKey[0]).FirstOrDefault() as UnitDevice;
                if (unitDevice == null) return;
                var Image = unitDevice.ImageScenarioItems.Where(p => p.Key == sKey[1]).FirstOrDefault();
                var result = unitDevice.ResultScenarioItems.Where(p => p.Key == sKey[1]).FirstOrDefault();
                if (Image != null)
                {
                    propertyData = (Image as ImageDevice).AlignerScenarioItems.Where(p => p.Key == sKey.Last()).FirstOrDefault();
                    if (contextHide == false)
                    {
                        #region Add menuitems
                        //2022.08.11_1 kimgun 
                        if (subMenuItem != null)
                        {
                            subMenuItem.DropDownItemClicked -= Submenu_DropDownItemClicked;
                            subMenuItem.Dispose();
                            subMenuItem = null;
                        }

                        subMenuItem = new ToolStripMenuItem("Link Defect");
                        subMenuItem.DropDownItemClicked += Submenu_DropDownItemClicked;
                        foreach (DefectDevice dd in Image.DefectScenarioItems)
                        {
                            if (propertyData is AlignerDevice && (propertyData as AlignerDevice).DefectScenarioItems.FindIndex(x => x.Id == dd.Id) != -1) continue;
                            var item = new ToolStripMenuItem(dd.Title); //23.01.12 LSS
                            //var item = new ToolStripMenuItem(dd.Key);
                            subMenuItem.DropDownItems.Add(item);
                        }
                        contextMenuStrip.Items.Add(subMenuItem);
                        subMenuItem = new ToolStripMenuItem("UnLink Defect");
                        subMenuItem.DropDownItemClicked += Submenu_DropDownItemClicked;
                        if (propertyData != null)
                        {
                            foreach (DefectDevice dd in (propertyData as AlignerDevice).DefectScenarioItems)
                            {
                                var item = new ToolStripMenuItem(dd.Title); //23.01.12 LSS
                                //var item = new ToolStripMenuItem(dd.Key);
                                subMenuItem.DropDownItems.Add(item);
                            }
                        }
                        contextMenuStrip.Items.Add(subMenuItem);
                        var menuitems = new ToolStripMenuItem("Copy Aligner");//2021.08.25_1 kimgun
                        contextMenuStrip.Items.Add(menuitems);
                        #endregion
                    }
                    string[] arrAligner = null;
                    if (Image != null)
                    {
                        var aldev = Image.AlignerScenarioItems.Select(x => x.Id).ToList();
                        aldev = aldev.FindAll(x => x != (propertyData as AlignerDevice).Id);
                        arrAligner = new string[aldev.Count + 1];
                        aldev.CopyTo(arrAligner);
                    }
                    FuncTool.GListUItype.Clear();
                    FuncTool.GListUItype._ListofValues.Add("AlignId", arrAligner);
                }
                else
                {
                    propertyData = (result as ResultDevice).AlignerScenarioItems.Where(p => p.Key == sKey.Last()).FirstOrDefault();
                    FuncTool.GListUItype.Clear();
                }


                var unit = lstUnitDevice.Where(p => p.Id == sKey[0]).FirstOrDefault();
                var camList = unit.CameraScenarioItems.Select(x => x.Key).ToList();
                string[] targetCam = new string[camList.Count + 1];
                camList.CopyTo(targetCam);
                FuncTool.GListUItype._ListofValues.Add("TargetId", targetCam);

                var lstdisp = SystemManager.SystemManager.Instance.GetDisplayDevieList();
                string[] listofValues = new string[lstdisp.Count + 1];
                lstdisp.CopyTo(listofValues);
                FuncTool.GListUItype._ListofValues.Add("DisplayId", listofValues);
            }
            else if (node.Last().Contains("Inspection"))
            {
                if (sKey[1].Contains("Result"))
                    contextHide = true;//2022.08.11_1 kimgun
                unitDevice = lstUnitDevice.Where(p => p.Id == sKey[0]).FirstOrDefault() as UnitDevice;
                if (unitDevice == null) return;
                var Image = unitDevice.ImageScenarioItems.Where(p => p.Key == sKey[1]).FirstOrDefault();
                var result = unitDevice.ResultScenarioItems.Where(p => p.Key == sKey[1]).FirstOrDefault();
                if (Image != null)
                {
                    propertyData = (Image as ImageDevice).InspScenarioItems.Where(p => p.Key == sKey.Last()).FirstOrDefault();
                    if (contextHide == false)
                    {
                        #region Add menuitems
                        //2022.08.11_1 kimgun 
                        if (subMenuItem != null)
                        {
                            subMenuItem.DropDownItemClicked -= Submenu_DropDownItemClicked;
                            subMenuItem.Dispose();
                            subMenuItem = null;
                        }

                        subMenuItem = new ToolStripMenuItem("Link Defect");
                        subMenuItem.DropDownItemClicked += Submenu_DropDownItemClicked;
                        if (Image != null)
                        {
                            foreach (DefectDevice dd in Image.DefectScenarioItems)
                            {
                                if ((propertyData as InspectionDevice).DefectScenarioItems.FindIndex(x => x.Id == dd.Id) != -1) continue;
                                var item = new ToolStripMenuItem(dd.Title); //23.01.12 LSS
                                //var item = new ToolStripMenuItem(dd.Key);
                                subMenuItem.DropDownItems.Add(item);
                            }
                        }
                        contextMenuStrip.Items.Add(subMenuItem);
                        var soruceInsp = Image.InspScenarioItems.Where(p => p.Key == sKey[2]).FirstOrDefault();
                        if (soruceInsp.InspectionType == enInspectionCategory.DEFECT_GRID)
                        {//2023.01.24_1 kimgun
                            subMenuItem = new ToolStripMenuItem("Link Inspection");
                            subMenuItem.DropDownItemClicked += Submenu_DropDownItemClicked;
                            if (Image != null)
                            {
                                foreach (InspectionDevice insp in Image.InspScenarioItems)
                                {
                                    if ((propertyData as InspectionDevice).InspectionScenarioItems.FindIndex(x => x.Id == insp.Id) != -1) continue;
                                    var item = new ToolStripMenuItem(insp.Title); //23.01.12 LSS
                                                                                  //var item = new ToolStripMenuItem(dd.Key);
                                    subMenuItem.DropDownItems.Add(item);
                                }
                            }
                            contextMenuStrip.Items.Add(subMenuItem);
                        }                       
                        subMenuItem = new ToolStripMenuItem("UnLink Defect");
                        subMenuItem.DropDownItemClicked += Submenu_DropDownItemClicked;
                        if (propertyData != null)
                        {
                            foreach (DefectDevice dd in (propertyData as InspectionDevice).DefectScenarioItems)
                            {
                                if ((propertyData as InspectionDevice).DefectScenarioItems.FindIndex(x => x.Id == dd.Id) == -1) continue;
                                var item = new ToolStripMenuItem(dd.Title);//23.01.12 LSS
                                //var item = new ToolStripMenuItem(dd.Key);
                                subMenuItem.DropDownItems.Add(item);
                            }
                        }
                        contextMenuStrip.Items.Add(subMenuItem);
                        if (soruceInsp.InspectionType == enInspectionCategory.DEFECT_GRID)
                        {//2023.01.24_1 kimgun
                            subMenuItem = new ToolStripMenuItem("UnLink Inspection");
                            subMenuItem.DropDownItemClicked += Submenu_DropDownItemClicked;
                            if (Image != null)
                            {
                                foreach (InspectionDevice insp in Image.InspScenarioItems)
                                {
                                    if ((propertyData as InspectionDevice).InspectionScenarioItems.FindIndex(x => x.Id == insp.Id) == -1) continue;
                                    var item = new ToolStripMenuItem(insp.Title); //23.01.12 LSS
                                                                                  //var item = new ToolStripMenuItem(dd.Key);
                                    subMenuItem.DropDownItems.Add(item);
                                }
                            }
                            contextMenuStrip.Items.Add(subMenuItem);
                        }
                        var menuitems = new ToolStripMenuItem("Copy Inspection");//2021.08.25_1 kimgun
                        contextMenuStrip.Items.Add(menuitems);
                        #endregion
                    }
                    FuncTool.GListUItype.Clear();

                    string[] arrAligner = null;
                    if (Image != null)
                    {
                        var aldev = Image.AlignerScenarioItems.Select(x => x.Id).ToList();
                        arrAligner = new string[aldev.Count + 1];
                        aldev.CopyTo(arrAligner);
                    }
                    FuncTool.GListUItype._ListofValues.Add("AlignId", arrAligner);
                }
                else if (result != null)
                {//2022.08.11_1 kimgun
                    if (result != null)
                        propertyData = (result as ResultDevice).InspScenarioItems.Where(p => p.Key == sKey.Last()).FirstOrDefault();
                    FuncTool.GListUItype.Clear();
                }


                var unit = lstUnitDevice.Where(p => p.Id == sKey[0]).FirstOrDefault();
                var camList = unit.CameraScenarioItems.Select(x => x.Key).ToList();
                string[] targetCam = new string[camList.Count + 1];
                camList.CopyTo(targetCam);
                FuncTool.GListUItype._ListofValues.Add("TargetId", targetCam);

                var lstdisp = SystemManager.SystemManager.Instance.GetDisplayDevieList();
                string[] listofValues = new string[lstdisp.Count + 1];
                lstdisp.CopyTo(listofValues);
                FuncTool.GListUItype._ListofValues.Add("DisplayId", listofValues);
            }
            else if (node.Last().Contains("Defect"))
            {
                if (sKey.Length > 3)
                    contextHide = true;//2022.08.11_1 kimgun 삭제나 추가는 image device에서 하게끔 defect을 참조하는 device는 패스
                if (contextHide == false)
                    contextMenuStrip.Items.Add("Copy Defect");//2022.08.11_1 kimgun
                FuncTool.GListUItype.Clear();
                if (node[1].Contains("Calibration"))
                {
                    unitDevice = lstUnitDevice.Where(p => p.Id == sKey[0]).FirstOrDefault() as UnitDevice;
                    if (unitDevice == null) return;
                    var Cal = unitDevice.CalibrationSenarioItems.Where(p => p.Key == sKey[1]).FirstOrDefault();
                    if (Cal != null)
                    {
                        propertyData = (Cal as CalibrationDevice).DefectScenarioItems.Where(p => p.Key == sKey[2]).FirstOrDefault();
                    }
                }
                else
                {
                    if (contextHide == false)
                        contextMenuStrip.Items.Add("Add Processing");

                    unitDevice = lstUnitDevice.Where(p => p.Id == sKey[0]).FirstOrDefault() as UnitDevice;
                    if (unitDevice == null) return;

                    var Image = unitDevice.ImageScenarioItems.Where(p => p.Key == sKey[1]).FirstOrDefault();
                    var result = unitDevice.ResultScenarioItems.Where(p => p.Key == sKey[1]).FirstOrDefault();
                    var Cam = unitDevice.CameraScenarioItems.Where(p => p.Key == sKey[1]).FirstOrDefault();//2022.12.16_2 cgun
                    if (Image != null)
                    {
                        propertyData = (Image as ImageDevice).DefectScenarioItems.Where(p => p.Key == sKey.Last()).FirstOrDefault();//2022.08.11_1 kimgun
                    }
                    else if (result != null)
                    {
                        var tmpKey = sKey[2].Split('.');
                        string key2 = tmpKey[0] + "." + tmpKey[1];
                        Image = unitDevice.ImageScenarioItems.Where(p => p.Key == key2).FirstOrDefault();
                        if(Image != null)
                            propertyData = (Image as ImageDevice).DefectScenarioItems.Where(p => p.Key == sKey.Last()).FirstOrDefault();//2022.08.11_1 kimgun
                    }
                    if (Cam != null)//2022.12.16_2 cgun
                    {
                        propertyData = (Cam as CameraDevice).DefectScenarioItems.Where(p => p.Key == sKey.Last()).FirstOrDefault();
                    }
                    string[] arrAligner = null;
                    if (Image != null)
                    {
                        var aldev = Image.AlignerScenarioItems.Select(x => x.Id).ToList();
                        arrAligner = new string[aldev.Count + 1];
                        aldev.CopyTo(arrAligner);
                    }
                    FuncTool.GListUItype._ListofValues.Add("AlignId", arrAligner);
                }
                var unit = lstUnitDevice.Where(p => p.Id == sKey[0]).FirstOrDefault();
                var camList = unit.CameraScenarioItems.Select(x => x.Key).ToList();
                string[] targetCam = new string[camList.Count + 1];
                camList.CopyTo(targetCam);
                FuncTool.GListUItype._ListofValues.Add("TargetId", targetCam);

                var lstdisp = SystemManager.SystemManager.Instance.GetDisplayDevieList();
                string[] listofValues = new string[lstdisp.Count + 1];
                lstdisp.CopyTo(listofValues);
                FuncTool.GListUItype._ListofValues.Add("DisplayId", listofValues);

                if (unit.CameraScenarioItems.Count > 0)
                {//2022.12.07_1 kimgun
                    //line scan 카메라가 한 유닛에 여러개 있더라도 frame count는 동일하니까 하나만 찾는다.
                    var cam = unit.CameraScenarioItems.Find(x =>x.Camera != null && x.Camera.IsLinescan);
                    if (cam != null)
                    {
                        List<string> lstIndex = new List<string>();
                        if (cam.SubFrameCount <= 0) //2022.12.21_1 cgun SubFrame이 0일 경우는 기존과 같이 한다.
                        {
                            for (int i = -1; i < cam.FrameCount; i++)//2022.12.14_1 kimgun -1은 모든 프레임 사용을 의미한다.
                            {
                                lstIndex.Add((i).ToString());
                            }
                        }
                        else
                        {//2022.12.21_1 cgun SubFrame이 0보다 클 경우 경우 SubFrameIndex를 Defect에서 선택할 수 있도록 한다.
                            for (int i = 0; i < cam.SubFrameCount; i++)
                            {
                                lstIndex.Add((i).ToString());
                            }
                        }
                        
                        listofValues = new string[lstIndex.Count];
                        lstIndex.CopyTo(listofValues);
                        FuncTool.GListUItype._ListofValues.Add("TargetIndex", listofValues);
                    }
                }
            }
            else if (node.Last().Contains("Processing"))
            {
                //if (sKey.Length > 4) 2023.01.31 cgun - camera에 processing 추가하며 해당 조건 주석
                //    contextHide = true;//2022.08.11_1 kimgun
                FuncTool.GListUItype.Clear();

                unitDevice = lstUnitDevice.Where(p => p.Id == sKey[0]).FirstOrDefault() as UnitDevice;
                if (unitDevice == null) return;

                if (sKey[1].Contains("Camera"))
                {
                    //2022.12.16_2 cgun
                    var Cam = unitDevice.CameraScenarioItems.Where(p => p.Key == sKey[1]).FirstOrDefault();
                    DefectDevice defect = null;
                    defect = (Cam as CameraDevice).DefectScenarioItems.Where(p => p.Key == sKey[2]).FirstOrDefault();

                    //2023.01.31 cgun - Camera에 processing이 바로 있을 경우로
                    ImageProcessingDevice imageProcessing = null;
                    imageProcessing = (Cam as CameraDevice).ImageProcessingScenarioItems.Where(p => p.Key == sKey[2]).FirstOrDefault();

                    if (defect == null)
                    {
                        //defect = (Cam as CameraDevice).DefectScenarioItems.Where(p => p.Key == sKey[3]).FirstOrDefault();
                        //if (defect != null)
                        //    propertyData = (defect as DefectDevice).ImageProcessingScenarioItems.Where(p => p.Key == sKey[(int)Layer.Layer_3]).FirstOrDefault();
                        propertyData = (Cam as CameraDevice).ImageProcessingScenarioItems.Where(p => p.Key == sKey[(int)Layer.Layer_2]).FirstOrDefault();
                    }
                    else
                        propertyData = (defect as DefectDevice).ImageProcessingScenarioItems.Where(p => p.Key == sKey[(int)Layer.Layer_3]).FirstOrDefault();

                }
                else
                {
                    var Image = unitDevice.ImageScenarioItems.Where(p => p.Key == sKey[1]).FirstOrDefault();
                    if (Image == null)
                    {
                        var result = unitDevice.ResultScenarioItems.Where(p => p.Key == sKey[1]).FirstOrDefault();
                        if (result != null)
                        {
                            var tmpKey = sKey[2].Split('.');
                            string resultkey = tmpKey[0] + "." + tmpKey[1];
                            Image = unitDevice.ImageScenarioItems.Where(p => p.Key == resultkey).FirstOrDefault();
                        }
                    }
                    var align = (Image as ImageDevice).AlignerScenarioItems.Where(p => p.Key == sKey[2]).FirstOrDefault();
                    var Insp = (Image as ImageDevice).InspScenarioItems.Where(p => p.Key == sKey[2]).FirstOrDefault();
                    DefectDevice defect = null;
                    defect = (Image as ImageDevice).DefectScenarioItems.Where(p => p.Key == sKey[2]).FirstOrDefault();
                    if (defect == null)
                    {
                        if (align != null)
                            defect = (align as AlignerDevice).DefectScenarioItems.Where(p => p.Key == sKey[3]).FirstOrDefault();
                        else if (Insp != null)
                            defect = (Insp as InspectionDevice).DefectScenarioItems.Where(p => p.Key == sKey[3]).FirstOrDefault();
                        else return;
                        propertyData = (defect as DefectDevice).ImageProcessingScenarioItems.Where(p => p.Key == sKey[(int)Layer.Layer_4]).FirstOrDefault();
                    }
                    else
                        propertyData = (defect as DefectDevice).ImageProcessingScenarioItems.Where(p => p.Key == sKey[(int)Layer.Layer_3]).FirstOrDefault();
                }
                
                FuncTool.GListUItype.Clear();
                var unit = lstUnitDevice.Where(p => p.Id == sKey[0]).FirstOrDefault();
                var camList = unit.CameraScenarioItems.Select(x => x.Key).ToList();
                string[] targetCam = new string[camList.Count + 1];
                camList.CopyTo(targetCam);
                FuncTool.GListUItype._ListofValues.Add("TargetId", targetCam);

                var lstdisp = SystemManager.SystemManager.Instance.GetDisplayDevieList();
                string[] listofValues = new string[lstdisp.Count + 1];
                lstdisp.CopyTo(listofValues);
                FuncTool.GListUItype._ListofValues.Add("DisplayId", listofValues);
            }

            if (sKey.Length <= 2 || contextHide == false)
            {
                var name = treeView.SelectedNode.Name;//2023.01.18_2 kimgun LastRemoveNumber(sKey.Last());
                contextMenuStrip.Items.Add(string.Format("Delete {0}", name));  // 2023.02.17 mck name 이거 갖다 쓰기
            }
            //contextMenuStrip.Items.Add(string.Format("Delete Last {0}", e.Node.Text.Substring(0, e.Node.Text.Length - 1)));
            //if (rightbutton) return;
            if (propertyData != null)
            {
                propertyGrid.SelectedObject = propertyData;
                if(rightbutton == false)//2023.01.17_1 kimgun
                    evtSeletected.Invoke(unitDevice, propertyData as IDeviceItem);// (propertyData as IDeviceItem).Clone() as IDeviceItem);
            }
        }


        private void Submenu_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (treeView.SelectedNode == null || treeView.SelectedNode.Index < -1)
                return;
            ToolStripMenuItem tsm = sender as ToolStripMenuItem;
            var index = -1;
            var SelectedNode = treeView.SelectedNode;
            string[] tmp2 = SelectedNode.FullPath.Split('\\');
            if (IsFilter || IsComboFilter)
            {//2023.01.27_1 kimgun
                var nodes = TreeViewCache.Nodes.Find(treeView.SelectedNode.Name, true);
                if (nodes == null || nodes.Length == 0)
                    return;
                foreach (var nd in nodes)
                {
                    if (nd.FullPath.Contains(SelectedNode.FullPath))
                    {
                        SelectedNode = nd;
                        break;
                    }
                }
                tmp2 = SelectedNode.FullPath.Split('\\');
            }
            string[] unitID = new string[tmp2.Length];
            TreeNode node1 = SelectedNode;
            for (int i = tmp2.Length - 1; i >= 0; i--)
            {
                unitID[i] = node1.Name as string;
                node1 = node1.Parent;
            }
            //string[] unitID = treeView.SelectedNode.FullPath.Split('\\');
            string[] node = treeView.SelectedNode.Text.Split('.');
            if (node.Last().Contains("Unit")
                && e.ClickedItem.Text.Contains("Delete"))
            {
                index = lstUnitDevice.FindLastIndex(p => p.Id == unitID.Last());
                lstUnitDevice.RemoveAt(index);
                treeView.SelectedNode.Remove();
                UnitCount--;
                AddDeleteDevice = true;//2021.07.26_1 jhyun
            }

            string section;
            if (e.ClickedItem.Text.Contains("Delete"))
            {
                string[] tmp = e.ClickedItem.Text.Split(' ');
                string itm = tmp.Last().Split('.').Last();
                section = tmp[0] + " " + itm;
            }
            else
                section = e.ClickedItem.Text;

            foreach (var unitDevice in lstUnitDevice)
            {
                if (unitDevice.Id == unitID[0])
                {
                    if (tsm.Text == "Link Aligner")
                    {
                        var result = unitDevice.ResultScenarioItems.Find(x => x.Id == unitID[1]);
                        foreach (var image in unitDevice.ImageScenarioItems)
                        {
                            var aligner = image.AlignerScenarioItems.Find(x => x.Title == section); //23.01.12 LSS id -> Title
                            if (aligner != null)
                            {
                                int endidx = treeView.SelectedNode.Nodes.Count;
                                if (treeView.SelectedNode.Nodes.ContainsKey(aligner.Id)) continue;
                                treeView.SelectedNode.Nodes.Add(aligner.Id, aligner.Title);
                                foreach (var defect in aligner.DefectScenarioItems)
                                {//Processing
                                    int subendidx = treeView.SelectedNode.Nodes[0].Nodes.Count;
                                    if (treeView.SelectedNode.Nodes.ContainsKey(defect.Id)) continue;
                                    treeView.SelectedNode.Nodes[endidx].Nodes.Add(defect.Id, defect.Title);//2021.07.20_1 kimgun 
                                    foreach (var procDevice in defect.ImageProcessingScenarioItems)
                                    {//Processing
                                        treeView.SelectedNode.Nodes[endidx].Nodes[subendidx].Nodes.Add(procDevice.Id, procDevice.Title);//2021.07.20_1 kimgun
                                    }
                                }
                                treeView.SelectedNode.Expand();
                                if (result.AlignerScenarioItems.FindIndex(x => x.Id == aligner.Id) == -1)
                                {
                                    result.AlignerScenarioItems.Add(aligner);
                                }
                                break;
                            }
                        }
                    }
                    else if (tsm.Text == "UnLink Aligner")
                    {
                        var result = unitDevice.ResultScenarioItems.Find(x => x.Id == unitID[1]);
                        var aligner = result.AlignerScenarioItems.Find(x => x.Title == section); //23.01.12 LSS id -> Title
                        result.UnLink(aligner);  //2022.09.06 shlee
                        if (aligner != null)
                        {
                            treeView.SelectedNode.Nodes.RemoveByKey(aligner.Id);
                            treeView.SelectedNode.Expand();
                            result.AlignerScenarioItems.Remove(aligner);
                        }
                    }
                    else if (tsm.Text == "Link Inspection")
                    {
                        var result = unitDevice.ResultScenarioItems.Find(x => x.Id == unitID[1]);
                        if (result != null)
                        {
                            foreach (var image in unitDevice.ImageScenarioItems)
                            {
                                var inspection = image.InspScenarioItems.Find(x => x.Title == section); //23.01.12 LSS id -> Title
                                if (inspection != null)
                                {
                                    int endidx = treeView.SelectedNode.Nodes.Count;
                                    if (treeView.SelectedNode.Nodes.ContainsKey(inspection.Id)) continue;
                                    treeView.SelectedNode.Nodes.Add(inspection.Id, inspection.Title);
                                    foreach (var defect in inspection.DefectScenarioItems)
                                    {//Processing
                                        int subendidx = treeView.SelectedNode.Nodes[0].Nodes.Count;
                                        if (treeView.SelectedNode.Nodes.ContainsKey(defect.Id)) continue;
                                        treeView.SelectedNode.Nodes[endidx].Nodes.Add(defect.Id, defect.Title);//2021.07.20_1 kimgun
                                        foreach (var procDevice in defect.ImageProcessingScenarioItems)
                                        {//Processing
                                            treeView.SelectedNode.Nodes[endidx].Nodes[subendidx].Nodes.Add(procDevice.Id, procDevice.Title);//2021.07.20_1 kimgun
                                        }
                                    }
                                    treeView.SelectedNode.Expand();
                                    if (result.InspScenarioItems.FindIndex(x => x.Id == inspection.Id) == -1)
                                    {
                                        result.InspScenarioItems.Add(inspection);
                                    }
                                    break;
                                }
                            }
                        }
                        else
                        {// 2023.01.19_1 kimgun
                             var selectedImage = unitDevice.ImageScenarioItems.Find(x => x.Id == unitID[1]);
                            if (selectedImage == null) return;
                            var selectedImageInspection = selectedImage.InspScenarioItems.Find(x => x.Id == unitID.Last());
                            if (selectedImageInspection != null)
                            {
                                foreach (var image in unitDevice.ImageScenarioItems)
                                {
                                    var inspection = image.InspScenarioItems.Find(x => x.Title == section); //23.01.12 LSS id -> Title
                                    if (inspection != null)
                                    {
                                        int endidx = treeView.SelectedNode.Nodes.Count;
                                        if (treeView.SelectedNode.Nodes.ContainsKey(inspection.Id)) continue;
                                        treeView.SelectedNode.Nodes.Add(inspection.Id, inspection.Title);
                                        foreach (var defect in inspection.DefectScenarioItems)
                                        {//Processing
                                            int subendidx = treeView.SelectedNode.Nodes[0].Nodes.Count;
                                            if (treeView.SelectedNode.Nodes.ContainsKey(defect.Id)) continue;
                                            treeView.SelectedNode.Nodes[endidx].Nodes.Add(defect.Id, defect.Title);//2021.07.20_1 kimgun
                                            foreach (var procDevice in defect.ImageProcessingScenarioItems)
                                            {//Processing
                                                treeView.SelectedNode.Nodes[endidx].Nodes[subendidx].Nodes.Add(procDevice.Id, procDevice.Title);//2021.07.20_1 kimgun
                                            }
                                        }
                                        treeView.SelectedNode.Expand();
                                        if (selectedImageInspection.InspectionScenarioItems.FindIndex(x => x.Id == inspection.Id) == -1)
                                        {
                                            selectedImageInspection.InspectionScenarioItems.Add(inspection);
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (tsm.Text == "UnLink Inspection")
                    {
                        var result = unitDevice.ResultScenarioItems.Find(x => x.Id == unitID[1]);
                        if (result != null)
                        {
                            var inspection = result.InspScenarioItems.Find(x => x.Title == section); //22.01.12 LSS
                            result.UnLink(inspection);  //2022.09.05 shlee
                            if (inspection != null)
                            {
                                treeView.SelectedNode.Nodes.RemoveByKey(inspection.Id);
                                treeView.SelectedNode.Expand();
                                result.InspScenarioItems.Remove(inspection);
                            }
                        }
                        else
                        {//2023.01.19_1 kimgun
                            var selectedImage = unitDevice.ImageScenarioItems.Find(x => x.Id == unitID[1]);
                            if (selectedImage == null) return;
                            var selectedImageInspection = selectedImage.InspScenarioItems.Find(x => x.Id == unitID.Last());
                            if (selectedImageInspection != null)
                            {
                                var inspection = selectedImageInspection.InspectionScenarioItems.Find(x => x.Title == section); //22.01.12 LSS
                                selectedImageInspection.UnLink(inspection);  //2022.09.05 shlee
                                if (inspection != null)
                                {
                                    treeView.SelectedNode.Nodes.RemoveByKey(inspection.Id);
                                    treeView.SelectedNode.Expand();
                                    selectedImageInspection.InspectionScenarioItems.Remove(inspection);
                                }
                            }
                        }
                    }
                    else if (tsm.Text == "Link Defect")
                    {
                        var image = unitDevice.ImageScenarioItems.Find(x => x.Id == unitID[1]);
                        if (image == null) return;
                        var defect = image.DefectScenarioItems.Find(x => x.Title == section); //23.01.12 LSS
                        if (defect == null) return;
                        var align = image.AlignerScenarioItems.Find(x => x.Id == unitID[2]);
                        var insp = image.InspScenarioItems.Find(x => x.Id == unitID.Last());
                        if (align != null)
                        {
                            int endidx = treeView.SelectedNode.Nodes.Count;
                            if (treeView.SelectedNode.Nodes.ContainsKey(defect.Id)) return;
                            treeView.SelectedNode.Nodes.Add(defect.Id, defect.Title);
                            foreach (var procDevice in defect.ImageProcessingScenarioItems)
                            {//Processing
                                treeView.SelectedNode.Nodes[endidx].Nodes.Add(procDevice.Id, procDevice.Title);//2021.07.20_1 kimgun
                            }
                            treeView.SelectedNode.Expand();
                            if (align.DefectScenarioItems.FindIndex(x => x.Id == defect.Id) == -1)
                            {
                                defect.AlignAlgoType = align.AlignAlgoType;
                                align.DefectScenarioItems.Add(defect);
                            }
                        }
                        else if (insp != null)
                        {
                            int endidx = treeView.SelectedNode.Nodes.Count;
                            if (treeView.SelectedNode.Nodes.ContainsKey(defect.Id)) return;
                            treeView.SelectedNode.Nodes.Add(defect.Id, defect.Title);
                            foreach (var procDevice in defect.ImageProcessingScenarioItems)
                            {//Processing
                                treeView.SelectedNode.Nodes[endidx].Nodes.Add(procDevice.Id, procDevice.Title);//2021.07.20_1 kimgun
                            }
                            treeView.SelectedNode.Expand();
                            if (insp.DefectScenarioItems.FindIndex(x => x.Id == defect.Id) == -1)
                                insp.DefectScenarioItems.Add(defect);
                        }
                    }
                    else if (tsm.Text == "UnLink Defect")
                    {
                        var image = unitDevice.ImageScenarioItems.Find(x => x.Id == unitID[1]);
                        if (image == null) return;
                        var defect = image.DefectScenarioItems.Find(x => x.Title == section); //23.01.12 LSS
                        if (defect == null) return;
                        var align = image.AlignerScenarioItems.Find(x => x.Id == unitID.Last());
                        var insp = image.InspScenarioItems.Find(x => x.Id == unitID.Last());

                        if (align != null)
                        {
                            align.UnLink(defect);   //2022.09.06 shlee
                            treeView.SelectedNode.Nodes.RemoveByKey(defect.Id);
                            treeView.SelectedNode.Expand();
                            int idx = align.DefectScenarioItems.FindIndex(x => x.Id == defect.Id);
                            if (idx != -1)
                            {
                                if (defect.AlignAlgoType == enAlignAlgo.SimpleAlign_Fixture)
                                    defect.AlignAlgoType = enAlignAlgo.SimpleAlign;
                                align.DefectScenarioItems.Remove(defect);
                            }
                        }
                        else if (insp != null)
                        {
                            insp.UnLink(defect);    //2022.09.06 shlee
                            treeView.SelectedNode.Nodes.RemoveByKey(defect.Id);
                            treeView.SelectedNode.Expand();
                            int idx = insp.DefectScenarioItems.FindIndex(x => x.Id == defect.Id);
                            if (idx != -1)
                                insp.DefectScenarioItems.RemoveAt(idx);
                        }
                    }
                }
            }
            AddDeleteDevice = true;//2022.09.05 shlee
            UpdateTreeView();
            contextMenuStrip.Hide();
        }

        public void SelectedDevice(UnitDevice Unit, IDeviceItem device)
        {//2021.08.04_4 kimgun
            var node = treeView.Nodes.Find((device as IId).Id, true);
            treeView.SelectedNode = node[0];
            //evtSeletected.Invoke(Unit, device);// (propertyData as IDeviceItem).Clone() as IDeviceItem);
        }
        private void ContextMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (treeView.SelectedNode == null || treeView.SelectedNode.Index < -1)
                return;
            ItemFunc(e.ClickedItem.Text);
            
            //var name = treeView.SelectedNode.Name;
            //string cmd = "Delete";
            //ItemFunc(cmd + " " + name);
        }
        /// <summary>
        /// 삭제,추가등의 기능을 수행한다.
        /// </summary>
        /// <param name="clickedItem"></param>

        private void ItemFunc(string clickedItem)
        {
            var index = -1;
            int last = -1;
            string[] tmp2 = treeView.SelectedNode.FullPath.Split('\\');
            var SelectedNode = treeView.SelectedNode;
            if (IsFilter || IsComboFilter)
            {//2023.01.27_1 kimgun
                var nodes = TreeViewCache.Nodes.Find(treeView.SelectedNode.Name, true);
                if (nodes == null || nodes.Length == 0)
                    return;
                foreach(var nd in nodes)
                {
                    if (nd.FullPath.Contains(SelectedNode.FullPath))
                    {
                        SelectedNode = nd;
                        break;
                    }
                }
                //SelectedNode = nodes[0];
                tmp2 = SelectedNode.FullPath.Split('\\');
            }
            string[] unitID = new string[tmp2.Length];
            TreeNode node1 = SelectedNode;// treeView.SelectedNode;
            for (int i = tmp2.Length - 1; i >= 0; i--)
            {
                unitID[i] = node1.Name as string;
                node1 = node1.Parent;
            }
            //string[] unitID = treeView.SelectedNode.FullPath.Split('\\');
            string[] node = treeView.SelectedNode.Text.Split('.');
            int itemCount = 0;
            if (node.Last().Contains("Unit")
                && clickedItem.Contains("Delete"))
            {
                index = lstUnitDevice.FindLastIndex(p => p.Id == unitID.Last());
                lstUnitDevice.RemoveAt(index);
                treeView.SelectedNode.Remove();
                UnitCount--;
                AddDeleteDevice = true;//2021.07.26_1 jhyun

                //SystemOption.param.UnitCount.Value = UnitCount;//2021.07.26_1 jhyun
                //SystemOption.param.Save();//Save 시점으로 변경

                //PLC Mapping 바뀌어서 주석
                //SystemManager.SystemManager.Instance.LoadPlcComponent();
            }

            string section;
            if (clickedItem.Contains("Delete"))
            {
                string[] tmp = clickedItem.Split(' ');
                string itm = tmp.Last().Split('.').Last();
                section = tmp[0] + " " + itm;
            }
            else
                section = clickedItem;
            foreach (var unitDevice in lstUnitDevice)
            {
                if (unitDevice.Id == unitID[0])
                {
                    string[] sKey = unitID;// treeView.SelectedNode.FullPath.Split('\\');
                    #region Add Device
                    if (clickedItem.Contains("Add Camera"))
                    {
                        CameraDevice device = new CameraDevice();
                        if (unitDevice.CameraScenarioItems == null || unitDevice.CameraScenarioItems.Count == 0)
                            itemCount = 1;
                        else
                            itemCount = unitDevice.CameraScenarioItems.Count + 1;
                        device.Id = string.Format("{0}.Camera{1}", unitID.Last(), itemCount);
                        device.Index = itemCount - 1;
                        device.Title = string.Format("Camera{0}", itemCount);
                        //treeView.SelectedNode.Nodes.Add(device.Id, device.Title);//2021.07.20_1 kimgun
                        //treeView.SelectedNode.Expand();
                        unitDevice.CameraScenarioItems.Add(device as CameraDevice);
                        AddDeleteDevice = true;//2021.07.26_1 jhyun
                    }
                    else if (clickedItem.Contains("Add Image"))
                    {
                        ImageDevice device = new ImageDevice();
                        if (unitDevice.ImageScenarioItems == null || unitDevice.ImageScenarioItems.Count == 0)
                            itemCount = 1;
                        else
                            itemCount = unitDevice.ImageScenarioItems.Count + 1;
                        device.Id = string.Format("{0}.Image{1}", unitID.Last(), itemCount);
                        device.Index = itemCount - 1;
                        device.Title = string.Format("Image{0}", itemCount);

                        //treeView.SelectedNode.Expand();
                        //treeView.SelectedNode.Nodes.Add(device.Id, device.Title);//2021.07.20_1 kimgun

                        unitDevice.ImageScenarioItems.Add(device as ImageDevice);
                        AddDeleteDevice = true;//2021.07.26_1 jhyun
                    }
                    else if (clickedItem.Contains("Add Calibration"))
                    {
                        if (unitDevice.CalibrationSenarioItems.Count() >= unitDevice.CameraScenarioItems.Count())
                        {
                            var dlg = CommonHelper.CreateMessageBox("Information", "Camera count is not mismatching..!!\n카메라 수량하고 일치 하지 않습니다.");
                            dlg.ShowDialog();
                            dlg.Dispose();
                            return;
                        }

                        CalibrationDevice device = new CalibrationDevice();
                        if (unitDevice.CalibrationSenarioItems == null || unitDevice.CalibrationSenarioItems.Count == 0)
                            itemCount = 1;
                        else
                            itemCount = unitDevice.CalibrationSenarioItems.Count + 1;

                        device.Id = string.Format("{0}.Calibration{1}", unitID.Last(), itemCount);
                        device.Index = itemCount - 1;
                        device.Title = string.Format("Calibration{0}", itemCount);

                        //treeView.SelectedNode.Expand();
                        //treeView.SelectedNode.Nodes.Add(device.Id, device.Title);//2021.07.20_1 kimgun

                        unitDevice.CalibrationSenarioItems.Add(device);
                        AddDeleteDevice = true;//2021.07.26_1 jhyun
                    }
                    else if (clickedItem.Contains("Add Result"))
                    {//2022.08.11_1 kimgun
                        ResultDevice device = new ResultDevice();
                        if (unitDevice.ResultScenarioItems == null || unitDevice.ResultScenarioItems.Count == 0)
                            itemCount = 1;
                        else
                        {
                            var dlg = CommonHelper.CreateMessageBox("Information", "Result Device only 1ea per unit..!!\nResult Device는 Unit당 한개입니다..!!");
                            dlg.ShowDialog();
                            dlg.Dispose();
                            return;
                        }
                        device.Id = string.Format("{0}.Result{1}", unitID.Last(), itemCount);
                        device.Index = itemCount - 1;
                        device.Title = string.Format("Result{0}", itemCount);

                        //treeView.SelectedNode.Expand();
                        //treeView.SelectedNode.Nodes.Add(device.Id, device.Title);

                        unitDevice.ResultScenarioItems.Add(device as ResultDevice);
                        AddDeleteDevice = true;//2021.07.26_1 jhyun
                    }
                    else if (clickedItem.Contains("Add Light"))
                    {
                        var cam = unitDevice.CameraScenarioItems.Find(x => x.Key == unitID.Last());
                        if (cam != null)
                        {
                            if (cam.LightScenarioItems == null || cam.LightScenarioItems.Count == 0)
                                itemCount = 1;
                            else
                                itemCount = cam.LightScenarioItems.Count + 1;

                            LightDevice device = new LightDevice();
                            device.Id = string.Format("{0}.Light{1}", unitID.Last(), itemCount);
                            device.Index = itemCount - 1;
                            device.Title = string.Format("Light{0}", itemCount);
                            // treeView.SelectedNode.Nodes.Add(device.Id, device.Title);//2021.07.20_1 kimgun
                            // treeView.SelectedNode.Expand();

                            index = unitDevice.CameraScenarioItems.FindIndex(p => p.Key == SelectedNode.Name);
                            if (index == -1) return;
                            (unitDevice.CameraScenarioItems[index] as CameraDevice).LightScenarioItems.Add(device);
                            AddDeleteDevice = true;//2021.07.26_1 jhyun
                        }
                    }
                    else if (clickedItem.Contains("Add Aligner"))
                    {
                        var img = unitDevice.ImageScenarioItems.Find(x => x.Key == unitID.Last());
                        if (img != null)
                        {
                            if (img.AlignerScenarioItems == null || img.AlignerScenarioItems.Count == 0)
                                itemCount = 1;
                            else
                                itemCount = img.AlignerScenarioItems.Count + 1;
                            AlignerDevice device = new AlignerDevice();
                            device.Id = string.Format("{0}.Aligner{1}", unitID.Last(), itemCount);
                            device.Index = itemCount - 1;
                            device.Title = string.Format("Aligner{0}", itemCount);
                            // treeView.SelectedNode.Nodes.Add(device.Id, device.Title);//2021.07.20_1 kimgun
                            // treeView.SelectedNode.Expand();

                            index = unitDevice.ImageScenarioItems.FindIndex(p => p.Key == SelectedNode.Name);
                            if (index == -1) return;
                            (unitDevice.ImageScenarioItems[index] as ImageDevice).AlignerScenarioItems.Add(device);
                            AddDeleteDevice = true;//2021.07.26_1 jhyun
                        }
                    }
                    else if (clickedItem.Contains("Add Inspection"))
                    {
                        var img = unitDevice.ImageScenarioItems.Find(x => x.Key == unitID.Last());
                        if (img != null)
                        {
                            if (img.InspScenarioItems == null || img.InspScenarioItems.Count == 0)
                                itemCount = 1;
                            else
                                itemCount = img.InspScenarioItems.Count + 1;
                            InspectionDevice device = new InspectionDevice();
                            device.Id = string.Format("{0}.Inspection{1}", unitID.Last(), itemCount);
                            device.Index = itemCount - 1;
                            device.Title = string.Format("Inspection{0}", itemCount);
                            device.DisplayId = img.DisplayId;
                            //2023.01.04_1 kimgun
                            device.InspResultTitles = new string[] { device.Title };
                            device.InspSpec = new decimal[] { 0 };
                            device.InspResultSpecLo = new decimal[] { 0 };
                            device.InspResultSpecUp = new decimal[] { 0 };
                            device.InspOffset = new decimal[] { 0 };
                            //TreeNode addNode = treeView.SelectedNode.Nodes.Add(device.Id, device.Title);//2021.07.20_1 kimgun
                            // PreviousSelectedNode2 = addNode;//2023.01.29_1 kimgun
                            // treeView.SelectedNode.Expand();

                            index = unitDevice.ImageScenarioItems.FindIndex(p => p.Key == SelectedNode.Name);
                            if (index == -1) return;
                            (unitDevice.ImageScenarioItems[index] as ImageDevice).InspScenarioItems.Add(device);
                            AddDeleteDevice = true;//2021.07.26_1 jhyun
                        }
                    }
                    else if (clickedItem.Contains("Add Defect"))
                    {
                        if (unitID.Last().Contains("Calibration"))
                        {
                            var cal = unitDevice.CalibrationSenarioItems.Find(x => x.Key == unitID[unitID.Length - 1]);
                            if (cal != null)
                            {
                                if (cal.DefectScenarioItems == null || cal.DefectScenarioItems.Count == 0)
                                    itemCount = 1;
                                else
                                    itemCount = cal.DefectScenarioItems.Count + 1;

                                DefectDevice device = new DefectDevice();
                                device.Id = string.Format("{0}.Defect{1}", unitID.Last(), itemCount);
                                device.Index = itemCount - 1;
                                device.Title = string.Format("Defect{0}", itemCount);
                                //device.DisplayId = align.DisplayId;
                                //treeView.SelectedNode.Nodes.Add(device.Id, device.Title);//2021.07.20_1 kimgun
                                // treeView.SelectedNode.Expand();

                                index = unitDevice.CalibrationSenarioItems.FindIndex(p => p.Key == sKey[1]);
                                if (index == -1) return;
                                (unitDevice.CalibrationSenarioItems[index] as CalibrationDevice).DefectScenarioItems.Add(device);
                                AddDeleteDevice = true;//2021.07.26_1 jhyun
                            }
                        }
                        else if (unitID.Last().Contains("Camera"))
                        {//2022.12.16_2 cgun
                            var cam = unitDevice.CameraScenarioItems.Find(x => x.Key == unitID[unitID.Length - 1]);
                            if (cam != null)
                            {
                                if (cam.DefectScenarioItems == null || cam.DefectScenarioItems.Count == 0)
                                    itemCount = 1;
                                else
                                    itemCount = cam.DefectScenarioItems.Count + 1;

                                DefectDevice device = new DefectDevice();
                                device.Id = string.Format("{0}.Defect{1}", unitID.Last(), itemCount);
                                device.Index = itemCount - 1;
                                device.Title = string.Format("Defect{0}", itemCount);
                                device.DisplayId = cam.DisplayId;
                                device.TargetId = unitID.Last();
                                if (unitDevice.UseLineScanMode)
                                    device.TargetIndex = cam.TargetIndex;
                                else
                                    device.TargetIndex = 0;

                                //treeView.SelectedNode.Nodes.Add(device.Id, device.Title);
                                // treeView.SelectedNode.Expand();

                                index = unitDevice.CameraScenarioItems.FindIndex(p => p.Key == SelectedNode.Name);
                                if (index == -1) return;
                                (unitDevice.CameraScenarioItems[index] as CameraDevice).DefectScenarioItems.Add(device);
                                AddDeleteDevice = true;//2021.07.26_1 jhyun
                            }
                        }
                        else
                        {
                            var img = unitDevice.ImageScenarioItems.Find(x => x.Key == unitID.Last());//2022.08.11_1 kimgun [unitID.Length - 2]);
                            if (img != null)
                            {
                                //2022.08.11_1 kimgun
                                if (img.DefectScenarioItems == null || img.DefectScenarioItems.Count == 0)
                                    itemCount = 1;
                                else
                                    itemCount = img.DefectScenarioItems.Count + 1;
                                DefectDevice device = new DefectDevice();
                                device.Id = string.Format("{0}.Defect{1}", unitID.Last(), itemCount);
                                device.Index = itemCount - 1;
                                device.Title = string.Format("Defect{0}", itemCount);
                                device.DisplayId = img.DisplayId;
                                //2022.08.30 shlee
                                if (unitDevice.UseLineScanMode)//kimxxxxxxxxxxxxxxx수정
                                    device.TargetIndex = -1;//2022.12.26_3 kimgun img.TargetIndex;
                                else
                                    device.TargetIndex = -1;

                                //treeView.SelectedNode.Nodes.Add(device.Id, device.Title);//2021.07.20_1 kimgun
                                // treeView.SelectedNode.Expand();

                                index = unitDevice.ImageScenarioItems.FindIndex(p => p.Key == SelectedNode.Name);
                                if (index == -1) return;
                                (unitDevice.ImageScenarioItems[index] as ImageDevice).DefectScenarioItems.Add(device);
                                AddDeleteDevice = true;//2021.07.26_1 jhyun
                            }
                        }
                    }
                    else if (clickedItem.Contains("Add Processing"))
                    {
                        var img = unitDevice.ImageScenarioItems.Find(x => x.Key == unitID[(int)Layer.Layer_1]);// unitID.Length - 2]);
                        var result = unitDevice.ResultScenarioItems.Find(x => x.Key == unitID[(int)Layer.Layer_1]);// unitID.Length - 2]);
                        var cam = unitDevice.CameraScenarioItems.Find(x => x.Key == unitID[(int)Layer.Layer_1]);//2022.12.16_2 cgun
                        if (img == null)
                        {
                            if (result != null)
                            {
                                var tmpResult = unitID[(int)Layer.Layer_3].Split('.');
                                string keyResult = tmpResult[0] + "." + tmpResult[1];//unit, image
                                img = unitDevice.ImageScenarioItems.Find(x => x.Key == keyResult);
                            }
                        }
                        DefectDevice defect = null;
                        if (img != null)
                        {//2022.08.11_1 kimgun
                            defect = img.DefectScenarioItems.Find(x => x.Key == unitID.Last());
                        }
                        else if (cam != null)
                        {
                            defect = cam.DefectScenarioItems.Find(x => x.Key == unitID.Last());

                            if (defect == null)
                            {//2023.01.31 cgun - camera에 ImageProcessing 추가
                                if (cam.ImageProcessingScenarioItems == null || cam.ImageProcessingScenarioItems.Count == 0)
                                    itemCount = 1;
                                else
                                    itemCount = cam.ImageProcessingScenarioItems.Count + 1;

                                ImageProcessingDevice device = new ImageProcessingDevice();
                                device.Id = string.Format("{0}.Processing{1}", unitID.Last(), itemCount);
                                device.Index = itemCount - 1;
                                (device as IScenarioItem).ScIStep = itemCount - 1;
                                device.DisplayId = cam.DisplayId;
                                device.TargetId = cam.TargetId;
                                device.Title = string.Format("Processing{0}", itemCount);

                                index = unitDevice.CameraScenarioItems.FindIndex(p => p.Key == sKey[(int)Layer.Layer_1]);
                                if (index == -1) return;
                                cam.ImageProcessingScenarioItems.Add(device);
                            }
                        }
                        if (defect != null)
                        {
                            if (defect.ImageProcessingScenarioItems == null || defect.ImageProcessingScenarioItems.Count == 0)
                                itemCount = 1;
                            else
                                itemCount = defect.ImageProcessingScenarioItems.Count + 1;
                            ImageProcessingDevice device = new ImageProcessingDevice();
                            device.Id = string.Format("{0}.Processing{1}", unitID.Last(), itemCount);
                            device.Index = itemCount - 1;
                            (device as IScenarioItem).ScIStep = itemCount - 1;
                            device.Title = string.Format("Processing{0}", itemCount);
                            device.DisplayId = defect.DisplayId;
                            device.TargetId = defect.TargetId;
                            //treeView.SelectedNode.Nodes.Add(device.Id, device.Title);//2021.07.20_1 kimgun
                            //treeView.SelectedNode.Expand();

                            if (img != null)
                                index = unitDevice.ImageScenarioItems.FindIndex(p => p.Key == sKey[(int)Layer.Layer_1]);
                            else if (result != null)
                            {//2022.08.11 kimgun
                                var tmpResult = unitID[(int)Layer.Layer_3].Split('.');
                                string keyResult = tmpResult[0] + "." + tmpResult[1];//unit, image
                                index = unitDevice.ImageScenarioItems.FindIndex(p => p.Key == keyResult);
                                img = unitDevice.ImageScenarioItems.Find(x => x.Key == keyResult);
                            }
                            else if (cam != null)
                            {
                                index = unitDevice.CameraScenarioItems.FindIndex(p => p.Key == sKey[(int)Layer.Layer_1]);
                            }
                            if (index == -1) return;
                            if (img != null)
                            {//2022.08.11 kimgun
                                defect.ImageProcessingScenarioItems.Add(device);
                            }
                            else if (cam != null)
                                defect.ImageProcessingScenarioItems.Add(device);
                            AddDeleteDevice = true;//2021.07.26_1 jhyun
                        }

                    }
                    #endregion
                
                    #region Copy & Paste
                    else if (clickedItem.Contains("Copy Aligner"))
                    {
                        var img = unitDevice.ImageScenarioItems.Find(x => x.Key == sKey[1]);
                        if (img != null)
                        {
                            var align = img.AlignerScenarioItems.Find(x => x.Key == sKey.Last());
                            if (align == null)
                                pasteAlign = null;
                            else
                                pasteAlign = align.Clone() as AlignerDevice;
                        }
                    }
                    else if (clickedItem.Contains("Copy Inspection"))
                    {
                        var img = unitDevice.ImageScenarioItems.Find(x => x.Key == sKey[1]);
                        if (img != null)
                        {
                            var insp = img.InspScenarioItems.Find(x => x.Key == sKey.Last());
                            if (insp == null)
                                pasteInsp = null;
                            else
                                pasteInsp = insp.Clone() as InspectionDevice;
                        }
                    }
                    else if (clickedItem.Contains("Copy Defect"))
                    {
                        var img = unitDevice.ImageScenarioItems.Find(x => x.Key == sKey[1]);
                        if (img != null)
                        {
                            var defect = img.DefectScenarioItems.Find(x => x.Key == sKey.Last());
                            if (defect == null)
                                pasteDefect = null;
                            else
                                pasteDefect = defect.Clone() as DefectDevice;
                        }
                    }
                    if (clickedItem.Contains("Paste Aligner"))
                    {
                        var img = unitDevice.ImageScenarioItems.Find(x => x.Key == sKey[1]);
                        if (img != null)
                        {
                            CreateAlignerDevice(unitDevice, img, unitID.Last(), SelectedNode);//2023.01.27_1 kimgun
                            //if (img.AlignerScenarioItems == null || img.AlignerScenarioItems.Count == 0)
                            //    itemCount = 1;
                            //else
                            //    itemCount = img.AlignerScenarioItems.Count + 1;
                            //AlignerDevice device = new AlignerDevice();
                            //device = pasteAlign.Clone() as AlignerDevice;
                            //device.Id = string.Format("{0}.Aligner{1}", unitID.Last(), itemCount);
                            //device.Index = itemCount - 1;
                            //device.Title = string.Format("Aligner{0}", itemCount);
                            //device.DisplayId = pasteAlign.DisplayId;
                            //device.TargetId = pasteAlign.TargetId;
                           
                            //device.DefectScenarioItems = device.DefectScenarioItems.ToList();
                            

                            //index = unitDevice.ImageScenarioItems.FindIndex(p => p.Key == SelectedNode.Name);
                            //if (index == -1) return;
                            //(unitDevice.ImageScenarioItems[index] as ImageDevice).AlignerScenarioItems.Add(device);
                        }
                    }
                    else if (clickedItem.Contains("Paste Inspection"))
                    {
                        var img = unitDevice.ImageScenarioItems.Find(x => x.Key == sKey[1]);
                        if (img != null)
                        {
                            CreateInspectionDevice(unitDevice, img, unitID.Last(), SelectedNode);//2023.01.27_1 kimgun
                            //if (img.InspScenarioItems == null || img.InspScenarioItems.Count == 0)
                            //    itemCount = 1;
                            //else
                            //    itemCount = img.InspScenarioItems.Count + 1;
                            //InspectionDevice device = new InspectionDevice();
                            //device = pasteInsp.Clone() as InspectionDevice;
                            //device.Id = string.Format("{0}.Inspection{1}", unitID.Last(), itemCount);
                            //device.Index = itemCount - 1;
                            //device.Title = string.Format("Inspection{0}", itemCount);
                            //device.DisplayId = device.DisplayId;
                            //device.TargetId = device.TargetId;
                            //device.DefectScenarioItems = device.DefectScenarioItems.ToList();
                            
                            //index = unitDevice.ImageScenarioItems.FindIndex(p => p.Key == SelectedNode.Name);
                            //if (index == -1) return;
                            //(unitDevice.ImageScenarioItems[index] as ImageDevice).InspScenarioItems.Add(device);
                        }
                    }
                    else if (clickedItem.Contains("Paste Defect"))
                    {//2023.01.02_1 kimgun
                        var img = unitDevice.ImageScenarioItems.Find(x => x.Key == sKey[1]);
                        if (img != null)
                        {
                            CreateDefectDevice(unitDevice, img, unitID.Last(), SelectedNode);//2023.01.27_1 kimgun
                            //if (img.DefectScenarioItems == null || img.DefectScenarioItems.Count == 0)
                            //    itemCount = 1;
                            //else
                            //    itemCount = img.DefectScenarioItems.Count + 1;
                            //DefectDevice device = new DefectDevice();
                            //device = pasteDefect.Clone() as DefectDevice;
                            //device.Id = string.Format("{0}.Defect{1}", unitID.Last(), itemCount);
                            //device.Index = itemCount - 1;
                            //device.Title = string.Format("Defect{0}", itemCount);
                            //device.DisplayId = device.DisplayId;
                            //device.TargetId = device.TargetId;
                            //device.ImageProcessingScenarioItems = device.ImageProcessingScenarioItems.ToList();
                            
                            //index = unitDevice.ImageScenarioItems.FindIndex(p => p.Key == SelectedNode.Name);
                            //if (index == -1) return;
                            //(unitDevice.ImageScenarioItems[index] as ImageDevice).DefectScenarioItems.Add(device);
                        }
                    }
                    #endregion
                    #region Link & UnLink All
                    else if (clickedItem.Contains("Link All"))
                    {
                        var result = unitDevice.ResultScenarioItems.Find(x => x.Key == sKey[1]);
                        foreach(var img in unitDevice.ImageScenarioItems)
                        {
                            result.AlignerScenarioItems.Clear();
                            foreach (var al in img.AlignerScenarioItems)
                                result.AlignerScenarioItems.Add(al);
                            result.InspScenarioItems.Clear();
                            foreach (var insp in img.InspScenarioItems)
                                result.InspScenarioItems.Add(insp);
                        }
                    }
                    else if (clickedItem.Contains("Unlink All"))
                    {
                        var result = unitDevice.ResultScenarioItems.Find(x => x.Key == sKey[1]);
                        foreach (var img in unitDevice.ImageScenarioItems)
                        {
                            result.AlignerScenarioItems.Clear();
                            result.InspScenarioItems.Clear();
                        }
                    }
                    #endregion
                    #region 삭제
                    /*문제 없으면 지우자
                    if (clickedItem.Contains("Reference Defect"))
                    {
                        var img = unitDevice.ImageScenarioItems.Find(x => x.Key == unitID.Last());//2022.08.11_1 kimgun [unitID.Length - 2]);
                        if (img != null)
                        {
                            //2022.08.11_1 kimgun
                            if (img.DefectScenarioItems == null || img.DefectScenarioItems.Count == 0)
                            {
                                var wnd = CommonHelper.CreateMessageBox("Warning", "Image Device에 먼저 Defect Device를 추가해 주세요");
                                wnd.ShowDialog();
                                wnd.Dispose();
                                return;
                            }

                            var align = img.AlignerScenarioItems.Find(x => x.Key == unitID.Last());
                            var insp = img.InspScenarioItems.Find(x => x.Key == unitID.Last());
                            if (align != null)
                            {
                                if (align.DefectScenarioItems == null || align.DefectScenarioItems.Count == 0)
                                    itemCount = 1;
                                else
                                    itemCount = align.DefectScenarioItems.Count + 1;
                                DefectDevice device = new DefectDevice();
                                device.Id = string.Format("{0}.Defect{1}", unitID.Last(), itemCount);
                                device.Index = itemCount - 1;
                                device.Title = string.Format("Defect{0}", itemCount);
                                device.DisplayId = align.DisplayId;
                               // treeView.SelectedNode.Nodes.Add(device.Id, device.Title);//2021.07.20_1 kimgun
                               // treeView.SelectedNode.Expand();

                                index = unitDevice.ImageScenarioItems.FindIndex(p => p.Key == sKey[1]);
                                if (index == -1) return;
                                var index1 = (unitDevice.ImageScenarioItems[index] as ImageDevice).AlignerScenarioItems.FindIndex(p => p.Key == sKey[2]);
                                if (index1 == -1) return;
                                ((unitDevice.ImageScenarioItems[index] as ImageDevice).AlignerScenarioItems[index1] as AlignerDevice).DefectScenarioItems.Add(device);
                            }
                            else if (insp != null)
                            {
                                if (insp.DefectScenarioItems == null || insp.DefectScenarioItems.Count == 0)
                                    itemCount = 1;
                                else
                                    itemCount = insp.DefectScenarioItems.Count + 1;
                                DefectDevice device = new DefectDevice();
                                device.Id = string.Format("{0}.Defect{1}", unitID.Last(), itemCount);
                                device.Index = itemCount - 1;
                                device.Title = string.Format("Defect{0}", itemCount);
                                device.DisplayId = insp.DisplayId;
                                device.TargetId = insp.TargetId;
                                //treeView.SelectedNode.Nodes.Add(device.Id, device.Title);//2021.07.20_1 kimgun
                                //treeView.SelectedNode.Expand();

                                index = unitDevice.ImageScenarioItems.FindIndex(p => p.Key == sKey[1]);
                                if (index == -1) return;
                                var index1 = (unitDevice.ImageScenarioItems[index] as ImageDevice).InspScenarioItems.FindIndex(p => p.Key == sKey[2]);
                                if (index1 == -1) return;
                                ((unitDevice.ImageScenarioItems[index] as ImageDevice).InspScenarioItems[index1] as InspectionDevice).DefectScenarioItems.Add(device);
                            }
                        }
                    }
                    */
                    #endregion
                    #region Delete
                    else if (section.Contains("Delete"))
                    {
                        int delIndex = -1;
                        if (section.Contains("Delete Camera"))
                        {//2022.08.11_1 kimgun
                            string key = clickedItem.Split(' ')[1];
                            delIndex = unitDevice.CameraScenarioItems.FindIndex(x => x.Id == key);
                            if (delIndex < 0) return;
                            unitDevice.CameraScenarioItems.RemoveAt(delIndex);
                            foreach(var imgD in unitDevice.ImageScenarioItems)
                            {
                                if (imgD.TargetId == key)
                                    imgD.TargetId = "";
                                foreach(var defD in imgD.DefectScenarioItems)
                                {
                                    if (defD.TargetId == key)
                                        defD.TargetId = "";
                                }
                                foreach (var alD in imgD.AlignerScenarioItems)
                                {
                                    if (alD.TargetId == key)
                                        alD.TargetId = "";
                                }
                                foreach (var inpsD in imgD.InspScenarioItems)
                                {
                                    if (inpsD.TargetId == key)
                                        inpsD.TargetId = "";
                                }
                            }
                            string[] seperate = new string[] { "Camera" };
                            for (int i = delIndex; i < unitDevice.CameraScenarioItems.Count; i++)
                            {
                                var tmp3 = unitDevice.CameraScenarioItems[i].Id.Split(seperate, StringSplitOptions.RemoveEmptyEntries);
                                int idx = (int.Parse(tmp3[1]) - 1);
                                //int idx = (int.Parse(tmp3[1])); //2023.02.17 mck 이유가 있어서 이렇게 했겠지
                                string id = tmp3[0] + "Camera" + idx;
                                unitDevice.CameraScenarioItems[i].Id = id;
                                unitDevice.CameraScenarioItems[i].Index = idx -1;
                            }
                            treeView.cngdrawid(key);//2023.02.17 mck
                        }
                        else if (section.Contains("Delete Image"))
                        {
                            string key = clickedItem.Split(' ')[1];
                            delIndex = unitDevice.ImageScenarioItems.FindIndex(x => x.Id == key);
                            if (delIndex < 0) return;
                            unitDevice.ImageScenarioItems.RemoveAt(delIndex);
                            string[] seperate = new string[] { "Image" };
                            for (int i = delIndex; i < unitDevice.ImageScenarioItems.Count; i++)
                            {
                                var tmp3 = unitDevice.ImageScenarioItems[i].Id.Split(seperate, StringSplitOptions.RemoveEmptyEntries);
                                int idx = (int.Parse(tmp3[1]) - 1);
                                string id = tmp3[0] + "Image" + idx;
                                unitDevice.ImageScenarioItems[i].Id = id;
                                unitDevice.ImageScenarioItems[i].Index = idx -1;
                                treeView.cngdrawid(key);//2023.02.17 mck
                            }
                        }
                        else if (section.Contains("Delete Calibration"))
                        {
                            string key = clickedItem.Split(' ')[1];
                            delIndex = unitDevice.CalibrationSenarioItems.FindIndex(x => x.Id == key);
                            if (delIndex < 0) return;
                            unitDevice.CalibrationSenarioItems.RemoveAt(delIndex);
                            string[] seperate = new string[] { "Calibration" };
                            for (int i = delIndex; i < unitDevice.CalibrationSenarioItems.Count; i++)
                            {
                                var tmp3 = unitDevice.CalibrationSenarioItems[i].Id.Split(seperate, StringSplitOptions.RemoveEmptyEntries);
                                int idx = (int.Parse(tmp3[1]) - 1);
                                string id = tmp3[0] + "Calibration" + idx;
                                unitDevice.CalibrationSenarioItems[i].Id = id;
                                unitDevice.CalibrationSenarioItems[i].Index = idx - 1;
                                treeView.cngdrawid(key);//2023.02.17 mck
                            }
                        }
                        else if (section.Contains("Delete Result"))
                        {
                            string key = clickedItem.Split(' ')[1];
                            delIndex = unitDevice.ResultScenarioItems.FindIndex(x => x.Id == key);
                            if (delIndex < 0) return;
                            unitDevice.ResultScenarioItems.RemoveAt(delIndex);
                            string[] seperate = new string[] { "Result" };
                            for (int i = delIndex; i < unitDevice.ResultScenarioItems.Count; i++)
                            {
                                var tmp3 = unitDevice.ResultScenarioItems[i].Id.Split(seperate, StringSplitOptions.RemoveEmptyEntries);
                                int idx = (int.Parse(tmp3[1]) - 1);
                                string id = tmp3[0] + "Result" + idx;
                                unitDevice.CalibrationSenarioItems[i].Id = id;
                                unitDevice.CalibrationSenarioItems[i].Index = idx - 1;
                                treeView.cngdrawid(key);//2023.02.17 mck
                            }
                        }
                        else if (section.Contains("Delete Light"))
                        {
                            index = unitDevice.CameraScenarioItems.FindIndex(p => p.Key == sKey[1]);
                            if (index == -1) return;
                            string key = clickedItem.Split(' ')[1];
                            delIndex = unitDevice.CameraScenarioItems[index].LightScenarioItems.FindIndex(x => x.Id == key);
                            if (delIndex < 0) return;
                            unitDevice.CameraScenarioItems[index].LightScenarioItems.RemoveAt(delIndex);
                            string[] seperate = new string[] { "Light" };
                            for (int i = delIndex; i < unitDevice.CameraScenarioItems[index].LightScenarioItems.Count; i++)
                            {
                                var tmp3 = unitDevice.CameraScenarioItems[index].LightScenarioItems[i].Id.Split(seperate, StringSplitOptions.RemoveEmptyEntries);
                                string id = tmp3[0] + "LIght" + (int.Parse(tmp3[1]) - 1);
                                unitDevice.CameraScenarioItems[index].LightScenarioItems[i].Id = id;
                                unitDevice.CameraScenarioItems[index].LightScenarioItems[i].Index = (int.Parse(tmp3[1]) - 2);
                                treeView.cngdrawid(key);//2023.02.17 mck
                            }
                        }
                        else if (section.Contains("Delete Aligner"))
                        {
                            index = unitDevice.ImageScenarioItems.FindIndex(p => p.Key == sKey[1]);
                            if (index != -1)
                            {
                                string key = clickedItem.Split(' ')[1];
                                delIndex = unitDevice.ImageScenarioItems[index].AlignerScenarioItems.FindIndex(x => x.Id == key);
                                if (delIndex < 0) return;
                                unitDevice.ImageScenarioItems[index].AlignerScenarioItems.RemoveAt(delIndex);
                                //unlink
                                string[] seperate = new string[] { "Aligner" };
                                foreach (var result in unitDevice.ResultScenarioItems)
                                    result.AlignerScenarioItems.RemoveAll(x => x.Id == key);

                                for (int i = delIndex; i < unitDevice.ImageScenarioItems[index].AlignerScenarioItems.Count; i++)
                                {
                                    var tmp3 = unitDevice.ImageScenarioItems[index].AlignerScenarioItems[i].Id.Split(seperate, StringSplitOptions.RemoveEmptyEntries);
                                    string id = tmp3[0] + "Aligner" + (int.Parse(tmp3[1]) - 1);
                                    unitDevice.ImageScenarioItems[index].AlignerScenarioItems[i].Id = id;
                                    unitDevice.ImageScenarioItems[index].AlignerScenarioItems[i].Index = (int.Parse(tmp3[1]) - 2);
                                    treeView.cngdrawid(key);//2023.02.17 mck
                                }
                            }
                            else
                            {//result unlink
                                index = unitDevice.ResultScenarioItems.FindIndex(p => p.Key == sKey[1]);
                                if (index == -1) return;
                                string key = clickedItem.Split(' ')[1];
                                delIndex = unitDevice.ResultScenarioItems[index].AlignerScenarioItems.FindIndex(x => x.Id == key);
                                if (delIndex < 0) return;
                                unitDevice.ResultScenarioItems[index].AlignerScenarioItems.RemoveAt(delIndex);                                
                            }
                        }
                        else if (section.Contains("Delete Inspection"))
                        {
                            index = unitDevice.ImageScenarioItems.FindIndex(p => p.Key == sKey[1]);
                            if (index != -1)
                            {
                                string key = clickedItem.Split(' ')[1];
                                int index1 = unitDevice.ImageScenarioItems[index].InspScenarioItems.FindIndex(x => x.Id == sKey[2]);
                                if (index1 != -1)
                                {//2023.01.24_2 kimgun
                                    delIndex = unitDevice.ImageScenarioItems[index].InspScenarioItems[index1].InspectionScenarioItems.FindIndex(x => x.Id == key);
                                    if (delIndex != -1)
                                        unitDevice.ImageScenarioItems[index].InspScenarioItems[index1].InspectionScenarioItems.RemoveAt(delIndex);
                                    else
                                    {
                                        delIndex = unitDevice.ImageScenarioItems[index].InspScenarioItems.FindIndex(x => x.Id == key);
                                        if (delIndex < 0) return;
                                        unitDevice.ImageScenarioItems[index].InspScenarioItems.RemoveAt(delIndex);

                                        //unlink
                                        string[] seperate = new string[] { "Inspection" };
                                        foreach (var result in unitDevice.ResultScenarioItems)
                                            result.InspScenarioItems.RemoveAll(x => x.Id == key);

                                        for (int i = delIndex; i < unitDevice.ImageScenarioItems[index].InspScenarioItems.Count; i++)
                                        {
                                            var tmp3 = unitDevice.ImageScenarioItems[index].InspScenarioItems[i].Id.Split(seperate, StringSplitOptions.RemoveEmptyEntries);
                                            string id = tmp3[0] + "Inspection" + (int.Parse(tmp3[1]) - 1);
                                            unitDevice.ImageScenarioItems[index].InspScenarioItems[i].Id = id;
                                            unitDevice.ImageScenarioItems[index].InspScenarioItems[i].Index = (int.Parse(tmp3[1]) - 2);
                                            treeView.cngdrawid(key);//2023.02.17 mck
                                        }
                                    }
                                }
                                else
                                {
                                    delIndex = unitDevice.ImageScenarioItems[index].InspScenarioItems.FindIndex(x => x.Id == key);
                                    if (delIndex < 0) return;
                                    unitDevice.ImageScenarioItems[index].InspScenarioItems.RemoveAt(delIndex);

                                    //unlink
                                    string[] seperate = new string[] { "Inspection" };
                                    foreach (var result in unitDevice.ResultScenarioItems)
                                        result.InspScenarioItems.RemoveAll(x => x.Id == key);

                                    for (int i = delIndex; i < unitDevice.ImageScenarioItems[index].InspScenarioItems.Count; i++)
                                    {
                                        var tmp3 = unitDevice.ImageScenarioItems[index].InspScenarioItems[i].Id.Split(seperate, StringSplitOptions.RemoveEmptyEntries);
                                        string id = tmp3[0] + "Inspection" + (int.Parse(tmp3[1]) - 1);
                                        unitDevice.ImageScenarioItems[index].InspScenarioItems[i].Id = id;
                                        unitDevice.ImageScenarioItems[index].InspScenarioItems[i].Index = (int.Parse(tmp3[1]) - 2);
                                        treeView.cngdrawid(key);//2023.02.17 mck
                                    }
                                }
                            }
                            else
                            {//result unlink
                                index = unitDevice.ResultScenarioItems.FindIndex(p => p.Key == sKey[1]);
                                if (index == -1) return;
                                string key = clickedItem.Split(' ')[1];
                                delIndex = unitDevice.ResultScenarioItems[index].InspScenarioItems.FindIndex(x => x.Id == key);
                                if (delIndex < 0) return;
                                unitDevice.ResultScenarioItems[index].InspScenarioItems.RemoveAt(delIndex);
                            }
                        }
                        else if (section.Contains("Delete Defect"))
                        {
                            if (sKey[2].Contains("Calibration"))
                            {
                                index = unitDevice.CalibrationSenarioItems.FindIndex(p => p.Key == sKey[1]);
                                if (index == -1) return;
                                string key = clickedItem.Split(' ')[1];
                                delIndex = unitDevice.CalibrationSenarioItems[index].DefectScenarioItems.FindIndex(x => x.Id == key);
                                if (delIndex < 0) return;
                                unitDevice.CalibrationSenarioItems[index].DefectScenarioItems.RemoveAt(delIndex);
                                string[] seperate = new string[] { "Defect" };
                                for (int i = delIndex; i < unitDevice.CalibrationSenarioItems[index].DefectScenarioItems.Count; i++)
                                {
                                    var tmp3 = unitDevice.CalibrationSenarioItems[index].DefectScenarioItems[i].Id.Split(seperate, StringSplitOptions.RemoveEmptyEntries);
                                    string id = tmp3[0] + "Defect" + (int.Parse(tmp3[1]) - 1);
                                    unitDevice.CalibrationSenarioItems[index].DefectScenarioItems[i].Id = id;
                                    unitDevice.CalibrationSenarioItems[index].DefectScenarioItems[i].Index = (int.Parse(tmp3[1]) - 2);
                                    treeView.cngdrawid(key);//2023.02.17 mck
                                }
                            }
                            else
                            {
                                //기본 : sKey = unit.image.defect
                                //insp : skey = 
                                int layer = sKey.Length - 1;
                                if (layer == 3)
                                {
                                    index = unitDevice.ImageScenarioItems.FindIndex(p => p.Key == sKey[1]);
                                    if (index != -1)
                                    {
                                        string key = clickedItem.Split(' ')[1];
                                        int index1 = unitDevice.ImageScenarioItems[index].InspScenarioItems.FindIndex(x => x.Id == sKey[2]);
                                        if (index1 != -1)
                                        {
                                            delIndex = unitDevice.ImageScenarioItems[index].InspScenarioItems[index1].DefectScenarioItems.FindIndex(x => x.Id == key);
                                            if(delIndex != -1)
                                                unitDevice.ImageScenarioItems[index].InspScenarioItems[index1].DefectScenarioItems.RemoveAt(delIndex);
                                        }
                                        else
                                        {
                                            index1 = unitDevice.ImageScenarioItems[index].AlignerScenarioItems.FindIndex(x => x.Id == sKey[2]);
                                            if (index1 == -1) return;
                                            delIndex = unitDevice.ImageScenarioItems[index].AlignerScenarioItems[index1].DefectScenarioItems.FindIndex(x => x.Id == key);
                                            if(delIndex != -1)
                                                unitDevice.ImageScenarioItems[index].AlignerScenarioItems[index1].DefectScenarioItems.RemoveAt(delIndex);
                                        }
                                    }
                                    else
                                    {
                                        index = unitDevice.ResultScenarioItems.FindIndex(p => p.Key == sKey[1]);
                                        if (index == -1) return;
                                        string key = clickedItem.Split(' ')[1];
                                        int index1 = unitDevice.ResultScenarioItems[index].InspScenarioItems.FindIndex(x => x.Id == sKey[2]);
                                        if (index1 != -1)
                                        {
                                            delIndex = unitDevice.ResultScenarioItems[index].InspScenarioItems[index1].DefectScenarioItems.FindIndex(x => x.Id == key);
                                            if (delIndex != -1)
                                                unitDevice.ResultScenarioItems[index].InspScenarioItems[index1].DefectScenarioItems.RemoveAt(delIndex);
                                        }
                                        else
                                        {
                                            index1 = unitDevice.ResultScenarioItems[index].AlignerScenarioItems.FindIndex(x => x.Id == sKey[2]);
                                            if (index1 == -1) return;
                                            delIndex = unitDevice.ResultScenarioItems[index].AlignerScenarioItems[index1].DefectScenarioItems.FindIndex(x => x.Id == key);
                                            if (delIndex != -1)
                                                unitDevice.ResultScenarioItems[index].AlignerScenarioItems[index1].DefectScenarioItems.RemoveAt(delIndex);
                                        }
                                    }
                                }
                                else if (sKey[1].Contains("Camera"))//2022.12.16_2 cgun
                                {
                                    index = unitDevice.CameraScenarioItems.FindIndex(p => p.Key == sKey[1]);
                                    if (index == -1) return;

                                    string key = clickedItem.Split(' ')[1];
                                    delIndex = unitDevice.CameraScenarioItems[index].DefectScenarioItems.FindIndex(x => x.Id == key);
                                    if (delIndex < 0) return;
                                    unitDevice.CameraScenarioItems[index].DefectScenarioItems.RemoveAt(delIndex);
                                    string[] seperate = new string[] { "Defect" };
                                    for (int i = delIndex; i < unitDevice.CameraScenarioItems[index].DefectScenarioItems.Count; i++)
                                    {
                                        var tmp3 = unitDevice.CameraScenarioItems[index].DefectScenarioItems[i].Id.Split(seperate, StringSplitOptions.RemoveEmptyEntries);
                                        string id = tmp3[0] + "Defect" + (int.Parse(tmp3[1]) - 1);
                                        unitDevice.CameraScenarioItems[index].DefectScenarioItems[i].Id = id;
                                        unitDevice.CameraScenarioItems[index].DefectScenarioItems[i].Index = (int.Parse(tmp3[1]) - 2);
                                        treeView.cngdrawid(key);//2023.02.17 mck
                                    }
                                }
                                else
                                {
                                    index = unitDevice.ImageScenarioItems.FindIndex(p => p.Key == sKey[1]);
                                    if (index == -1)
                                    {
                                        return;
                                    }

                                    string key = clickedItem.Split(' ')[1];
                                    delIndex = unitDevice.ImageScenarioItems[index].DefectScenarioItems.FindIndex(x => x.Id == key);
                                    if (delIndex < 0) return;
                                    unitDevice.ImageScenarioItems[index].DefectScenarioItems.RemoveAt(delIndex);
                                    string[] seperate = new string[] { "Defect" };
                                    //unlink
                                    foreach (var align in (unitDevice.ImageScenarioItems[index]).AlignerScenarioItems)
                                    {
                                        align.DefectScenarioItems.RemoveAll(x => x.Id == key);
                                    }
                                    foreach (var insp in (unitDevice.ImageScenarioItems[index]).InspScenarioItems)
                                    {
                                        insp.DefectScenarioItems.RemoveAll(x => x.Id == key);
                                    }
                                    for (int i = delIndex; i < unitDevice.ImageScenarioItems[index].DefectScenarioItems.Count; i++)
                                    {
                                        var tmp3 = unitDevice.ImageScenarioItems[index].DefectScenarioItems[i].Id.Split(seperate, StringSplitOptions.RemoveEmptyEntries);
                                        string id = tmp3[0] + "Defect" + (int.Parse(tmp3[1]) - 1);
                                        unitDevice.ImageScenarioItems[index].DefectScenarioItems[i].Id = id;
                                        unitDevice.ImageScenarioItems[index].DefectScenarioItems[i].Index = (int.Parse(tmp3[1]) - 2);
                                        treeView.cngdrawid(key);//2023.02.17 mck
                                    }
                                    /*Defect Parameter 파일 삭제및 리네임.순서(index)가 무조건 이어지도록하기 위해*/
                                    string parse2 = clickedItem.Split(' ')[1];
                                    string paramPath = Path.Combine(SystemData.SetupData.SystemOption.param.ModelBasePath.Value as string, SystemData.SetupData.SystemOption.param.CurrentModel.Value as string);//, pdef.DefectMethod.GetDataFileName(i));
                                    DirectoryInfo di = new DirectoryInfo(paramPath);
                                    var delFiles = di.GetFiles("*" + parse2 + "*");
                                    foreach (var d in delFiles)
                                    {
                                        d.Delete();
                                    }
                                    /*Rename*/
                                    var tmp = parse2.Split(seperate, StringSplitOptions.RemoveEmptyEntries);
                                    string renameKey = tmp[0] + "Defect";
                                    int idx = int.Parse(tmp[1]);
                                    var fis = di.GetFiles("*" + renameKey + "*");
                                    var fileNames = fis.Select(x => x.FullName).ToArray();
                                    Array.Sort(fileNames, new StringAsNumericComparer());

                                    foreach (var f in fileNames)
                                    {
                                        string fName = f.Split('\\').Last();
                                        var parse = fName.Split('.');
                                        int no = -1;
                                        if (int.TryParse(parse[2].Replace("Defect", ""), out no))
                                        {
                                            no = int.Parse(parse[2].Replace("Defect", ""));
                                            if (no > idx)
                                            {
                                                //var parse = f.Name.Split(seperate, StringSplitOptions.RemoveEmptyEntries);
                                                int newIndex = no - 1;
                                                StringBuilder newName = new StringBuilder();
                                                for (int i = 0; i < parse.Length; i++)
                                                {
                                                    if (parse[i].Contains("Defect") && parse[i].Contains("eDefect") == false)
                                                        parse[i] = "Defect" + newIndex;
                                                    if (i == parse.Length - 1)
                                                        newName.AppendFormat("{0}", parse[i]);
                                                    else
                                                        newName.AppendFormat("{0}.", parse[i]);
                                                }

                                                Microsoft.VisualBasic.FileIO.FileSystem.RenameFile(f, newName.ToString());
                                            }
                                        }
                                    }
                                }
                            }

                        }
                        else if (section.Contains("Delete Processing"))
                        {//2022.08.11_1 kimgun
                            if (sKey[1].Contains("Camera"))//2022.12.16_2 cgun
                            {
                                index = unitDevice.CameraScenarioItems.FindIndex(p => p.Key == sKey[1]);
                                if (index == -1) return;
                                int index1 = unitDevice.CameraScenarioItems[index].DefectScenarioItems.FindIndex(p => p.Key == sKey[2]);
                                //2023.01.31 cgun - camera 에 Processing이 생기며 아래 조건 변경
                                //if (index1 == -1) return;
                                //int index2 = unitDevice.CameraScenarioItems[index].DefectScenarioItems[index1].ImageProcessingScenarioItems.FindIndex(p => p.Key == sKey[3]);
                                //if (index2 == -1) return;
                                //delIndex = unitDevice.CameraScenarioItems[index].DefectScenarioItems[index1].ImageProcessingScenarioItems.FindIndex(x => x.Id == sKey[3]);
                                //if (delIndex < 0) return;
                                if (index1 == -1)
                                {
                                    delIndex = unitDevice.CameraScenarioItems[index].ImageProcessingScenarioItems.FindIndex(p => p.Key == sKey[2]);
                                    if (delIndex < 0) return;

                                    unitDevice.CameraScenarioItems[index].ImageProcessingScenarioItems.RemoveAt(delIndex);
                                    string[] seperate = new string[] { "Processing" };
                                    for (int i = index; i < unitDevice.CameraScenarioItems[index].ImageProcessingScenarioItems.Count; i++)
                                    {
                                        var tmp3 = unitDevice.CameraScenarioItems[index].ImageProcessingScenarioItems[i].Id.Split(seperate, StringSplitOptions.RemoveEmptyEntries);
                                        string id = tmp3[0] + "Processing" + (int.Parse(tmp3[1]) - 1);
                                        unitDevice.CameraScenarioItems[index].ImageProcessingScenarioItems[i].Id = id;
                                        unitDevice.CameraScenarioItems[i].ImageProcessingScenarioItems[i].Index = (int.Parse(tmp3[1]) - 2);
                                    }
                                }
                                else
                                {//camera에 Defect 시나리오가 있을 경우
                                    int index2 = unitDevice.CameraScenarioItems[index].DefectScenarioItems[index1].ImageProcessingScenarioItems.FindIndex(p => p.Key == sKey[3]);
                                    if (index2 == -1) return;
                                    delIndex = unitDevice.CameraScenarioItems[index].DefectScenarioItems[index1].ImageProcessingScenarioItems.FindIndex(x => x.Id == sKey[3]);
                                    if (delIndex < 0) return;

                                    unitDevice.CameraScenarioItems[index].DefectScenarioItems[index1].ImageProcessingScenarioItems.RemoveAt(delIndex);
                                    string[] seperate = new string[] { "Processing" };
                                    for (int i = index1; i < unitDevice.CameraScenarioItems[index].DefectScenarioItems[index1].ImageProcessingScenarioItems.Count; i++)
                                    {
                                        var tmp3 = unitDevice.CameraScenarioItems[index].DefectScenarioItems[index1].ImageProcessingScenarioItems[i].Id.Split(seperate, StringSplitOptions.RemoveEmptyEntries);
                                        string id = tmp3[0] + "Processing" + (int.Parse(tmp3[1]) - 1);
                                        unitDevice.CameraScenarioItems[index].DefectScenarioItems[index1].ImageProcessingScenarioItems[i].Id = id;
                                        unitDevice.CameraScenarioItems[i].DefectScenarioItems[index1].ImageProcessingScenarioItems[i].Index = (int.Parse(tmp3[1]) - 2);
                                    }
                                }
                            }
                            else
                            {
                                index = unitDevice.ImageScenarioItems.FindIndex(p => p.Key == sKey[1]);
                                if (index == -1) return;
                                int index1 = unitDevice.ImageScenarioItems[index].DefectScenarioItems.FindIndex(p => p.Key == sKey[2]);
                                if (index1 == -1) return;
                                int index2 = unitDevice.ImageScenarioItems[index].DefectScenarioItems[index1].ImageProcessingScenarioItems.FindIndex(p => p.Key == sKey[3]);
                                if (index2 == -1) return;
                                delIndex = unitDevice.ImageScenarioItems[index].DefectScenarioItems[index1].ImageProcessingScenarioItems.FindIndex(x => x.Id == sKey[3]);
                                if (delIndex < 0) return;
                                
                                unitDevice.ImageScenarioItems[index].DefectScenarioItems[index1].ImageProcessingScenarioItems.RemoveAt(delIndex);
                                string[] seperate = new string[] { "Processing" };
                                for (int i = index1; i < unitDevice.ImageScenarioItems[index].DefectScenarioItems[index1].ImageProcessingScenarioItems.Count; i++)
                                {
                                    var tmp3 = unitDevice.ImageScenarioItems[index].DefectScenarioItems[index1].ImageProcessingScenarioItems[i].Id.Split(seperate, StringSplitOptions.RemoveEmptyEntries);
                                    string id = tmp3[0] + "Processing" + (int.Parse(tmp3[1]) - 1);
                                    unitDevice.ImageScenarioItems[index].DefectScenarioItems[index1].ImageProcessingScenarioItems[i].Id = id;
                                        unitDevice.ImageScenarioItems[i].DefectScenarioItems[index1].ImageProcessingScenarioItems[i].Index = (int.Parse(tmp3[1]) - 2);
                                }
                            }
                        }

                        //kimxxxxxxxxxxxxxxxxxxxxxxxxx2023.01.17_1 if (last < 0 || last >= treeView.SelectedNode.Parent.Nodes.Count) return;

                        //if (section.Contains("Aligner") == false
                        //    && section.Contains("Inspection") == false &&
                        //    section.Contains("Defect") == false)//2022.08.11_1 kimgun
                        //    treeView.SelectedNode.Parent.Nodes.RemoveAt(delIndex);
                    }
                    #endregion
                }
                AddDeleteDevice = true;//2021.07.26_1 jhyun
                
            }
            UpdateTreeView();
        }

        private bool IsAbleSave()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var unitDevice in lstUnitDevice)
            {
                if (unitDevice.Title == null || unitDevice.Title.Length == 0)
                    sb.AppendFormat("{0}의 Title이 공백입니다.\r\n", unitDevice.Id);
                if (unitDevice.ScanTime == 0)
                    sb.AppendFormat("{0}의 Scan Time 0입니다. 0보다 크게 설정 해주세요.\r\n", unitDevice.Id);
                if (unitDevice.Title.Contains(','))
                {//2021.08.24_1 kimgun
                    if (CommonHelper.MyMessageBox(Application.OpenForms[0], "Device Data", string.Format("{0}의 {1}에 comma가 포함되어 있습니다.'_'로 자동 변경 할까요?", unitDevice.Id, nameof(unitDevice.Title)), MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        unitDevice.Title = unitDevice.Title.Replace(',', '_');
                    }
                    else
                        sb.AppendFormat(string.Format("{0}의 {1}에 comma가 포함되어 있습니다.다른 단어로 교체해 주세요.", unitDevice.Id, nameof(unitDevice.Title)));
                }
                int preFrame = 0;
                bool bDiff = false;
                foreach (CameraDevice cam in unitDevice.CameraScenarioItems)
                {
                    if (cam.Title == null || cam.Title.Length == 0)
                        sb.AppendFormat("{0}의 Title이 공백입니다.\r\n", cam.Id);
                    if (cam.CameraId == null || cam.CameraId.Length == 0)
                        sb.AppendFormat("{0}Device에서 사용할 Camera ID를 선택해 주세요.\r\n", cam.Id);
                    if (cam.DisplayId == null || cam.DisplayId.Length == 0)
                        sb.AppendFormat("{0}Device에서 사용할 Display ID를 선택해 주세요.\r\n", cam.Id);
                    if (cam.ScanTime == 0)
                        sb.AppendFormat("{0}의 Scan Time 0입니다. 0보다 크게 설정 해주세요.\r\n", cam.Id);
                    if (cam.Title.Contains(','))
                    {//2021.08.24_1 kimgun
                        if (CommonHelper.MyMessageBox(Application.OpenForms[0], "Device Data", string.Format("{0}의 {1}에 comma가 포함되어 있습니다.'_'로 자동 변경 할까요?", cam.Id, nameof(cam.Title)), MessageBoxButtons.YesNo) == DialogResult.Yes)
                            cam.Title = cam.Title.Replace(',', '_');
                        else
                            sb.AppendFormat(string.Format("{0}의 {1}에 comma가 포함되어 있습니다.다른 단어로 교체해 주세요.", cam.Id, nameof(cam.Title)));
                    }
                    if(cam.Camera != null && cam.Camera.IsLinescan)
                    {
                        if (preFrame == 0)
                        {
                            preFrame = cam.FrameCount;
                        }
                        else if (preFrame != cam.FrameCount)
                            bDiff = true;
                    }
                    foreach (LightDevice light in cam.LightScenarioItems)
                    {
                        if (light.Title == null || light.Title.Length == 0)
                            sb.AppendFormat("{0}의 Title이 공백입니다.\r\n", light.Id);
                        if (light.LightControlId == null || light.LightControlId.Length == 0)
                            sb.AppendFormat("{0}Device에서 사용할 Light Control ID를 선택해 주세요.\r\n", light.Id);
                        if (light.LightChannelId == null || light.LightChannelId.Length == 0)
                            sb.AppendFormat("{0}Device에서 사용할 Light Channel ID를 선택해 주세요.\r\n", light.Id);
                        if (light.Title.Contains(','))
                        {//2021.08.24_1 kimgun
                            if (CommonHelper.MyMessageBox(Application.OpenForms[0], "Device Data", string.Format("{0}의 {1}에 comma가 포함되어 있습니다.'_'로 자동 변경 할까요?", light.Id, nameof(light.Title)), MessageBoxButtons.YesNo) == DialogResult.Yes)
                                light.Title = light.Title.Replace(',', '_');
                            else
                                sb.AppendFormat(string.Format("{0}의 {1}에 comma가 포함되어 있습니다.다른 단어로 교체해 주세요.", light.Id, nameof(light.Title)));
                        }
                        //2021.08.09_2 kimgun
                        //if (light.ScanTime == 0)
                        //sb.AppendFormat("{0}의 Scan Time 0입니다. 0보다 크게 설정 해주세요.\r\n", light.Id);
                    }
                }
                if (bDiff)
                {//2022.12.09_1 kimgun
                    sb.AppendFormat(string.Format("Line Scan 카메라의 Frame 수량이 다릅니다.."));
                }

                foreach (ImageDevice img in unitDevice.ImageScenarioItems)
                {
                    if (img.Title == null || img.Title.Length == 0)
                        sb.AppendFormat("{0}의 Title이 공백입니다.\r\n", img.Id);
                    if (img.DisplayId == null || img.DisplayId.Length == 0)
                        sb.AppendFormat("{0}Device에서 사용할 Display ID를 선택해 주세요.\r\n", img.Id);
                    if (img.ScanTime == 0)
                        sb.AppendFormat("{0}의 Scan Time 0입니다. 0보다 크게 설정 해주세요.\r\n", img.Id);
                    if (img.TargetId == null || img.TargetId.Length == 0)
                        sb.AppendFormat("{0}Device에서 사용할 TargetId를 선택해 주세요.\r\n", img.Id); //23.01.11 LSS 없으면 갇힘
                    if (img.Title.Contains(','))
                    {//2021.08.24_1 kimgun
                        if (CommonHelper.MyMessageBox(Application.OpenForms[0], "Device Data", string.Format("{0}의 {1}에 comma가 포함되어 있습니다.'_'로 자동 변경 할까요?", img.Id, nameof(img.Title)), MessageBoxButtons.YesNo) == DialogResult.Yes)
                            img.Title = img.Title.Replace(',', '_');
                        else
                            sb.AppendFormat(string.Format("{0}의 {1}에 comma가 포함되어 있습니다.다른 단어로 교체해 주세요.", img.Id, nameof(img.Title)));
                    }

                    foreach (DefectDevice def in img.DefectScenarioItems)
                    {//2022.12.09_1 kimgun
                        var cam = unitDevice.CameraScenarioItems.Find(x => x.Id == def.TargetId);
                        if (cam != null)
                        {
                            if (cam.Camera != null && cam.Camera.IsLinescan)
                            {//2022.12.09_1 kimgun
                                if (cam.SubFrameCount <= 0)
                                {
                                    if (cam.FrameCount <= def.TargetIndex)
                                        sb.AppendFormat("{0}의 FrameCount보다 {1}의 TargetIndex가 더 작아야 합니다.[FrameCount => {2} TargetIndex => {3}]\r\n", cam.Id, def.Id, cam.FrameCount, def.TargetIndex);
                                }
                                else    //2022.12.21_1 cgun SubFrame 사용 시 FrameCount가 아닌 SubFrameCount를 보게 변경
                                {
                                    if (cam.SubFrameCount <= def.TargetIndex)
                                        sb.AppendFormat("{0}의 SubFrameCount보다 {1}의 TargetIndex가 더 작아야 합니다.[SubFrameCount => {2} TargetIndex => {3}]\r\n", cam.Id, def.Id, cam.SubFrameCount, def.TargetIndex);
                                }  
                            }
                        }
                        if (def.Title == null || def.Title.Length == 0)
                            sb.AppendFormat("{0}의 Title이 공백입니다.\r\n", def.Id);
                        if (def.TargetId == null || def.TargetId.Length == 0)
                            sb.AppendFormat("{0}Device에서 사용할 TargetId를 선택해 주세요.\r\n", def.Id);
                        if (def.DisplayId == null || def.DisplayId.Length == 0)
                            sb.AppendFormat("{0}Device에서 사용할 Display ID를 선택해 주세요.\r\n", def.Id);
                        //if (def.ScanTime == 0)
                        //    sb.AppendFormat("{0}의 Scan Time 0입니다. 0보다 크게 설정 해주세요.\r\n", def.Id);
                        if (def.Title != null && def.Title.Contains(','))
                        {//2021.08.24_1 kimgun
                            if (CommonHelper.MyMessageBox(Application.OpenForms[0], "Device Data", string.Format("{0}의 {1}에 comma가 포함되어 있습니다.'_'로 자동 변경 할까요?", def.Id, nameof(def.Title)), MessageBoxButtons.YesNo) == DialogResult.Yes)
                                def.Title = def.Title.Replace(',', '_');
                            else
                                sb.AppendFormat(string.Format("{0}의 {1}에 comma가 포함되어 있습니다.다른 단어로 교체해 주세요.", def.Id, nameof(def.Title)));
                        }
                        if(def.UseCrop && def.TargetIndex != -1)
                        {//2022.12.26_3 kimgun
                            sb.AppendFormat(string.Format("{0}의 TargetIndex가 -1일 때만 Crop을 사용할 수 있습니다.\r\n", def.Id, nameof(def.Title)));
                            if(def.CropHeight == 0 || def.CropWidth == 0)
                            {
                                sb.AppendFormat(string.Format("{0}의 crop의 크기는 반드시 0보다 커야 합니다.\r\n", def.Id, nameof(def.Title)));
                            }
                        }
                        foreach (ImageProcessingDevice proc in def.ImageProcessingScenarioItems)
                        {
                            if (proc.Title == null || proc.Title.Length == 0)
                                sb.AppendFormat("{0}의 Title이 공백입니다.\r\n", proc.Id);
                            if (proc.TargetId == null || proc.TargetId.Length == 0)
                                sb.AppendFormat("{0}Device에서 사용할 TargetId를 선택해 주세요.\r\n", proc.Id);
                            if (proc.DisplayId == null || proc.DisplayId.Length == 0)
                                sb.AppendFormat("{0}Device에서 사용할 Display ID를 선택해 주세요.\r\n", proc.Id);
                            if (proc.Title.Contains(','))
                            {//2021.08.24_1 kimgun
                                if (CommonHelper.MyMessageBox(Application.OpenForms[0], "Device Data", string.Format("{0}의 {1}에 comma가 포함되어 있습니다.'_'로 자동 변경 할까요?", proc.Id, nameof(proc.Title)), MessageBoxButtons.YesNo) == DialogResult.Yes)
                                    proc.Title = proc.Title.Replace(',', '_');
                                else
                                    sb.AppendFormat(string.Format("{0}의 {1}에 comma가 포함되어 있습니다.다른 단어로 교체해 주세요.", proc.Id, nameof(proc.Title)));
                            }
                        }
                    }
                    foreach (AlignerDevice align in img.AlignerScenarioItems)
                    {
                        if (align.Title == null || align.Title.Length == 0)
                            sb.AppendFormat("{0}의 Title이 공백입니다.\r\n", align.Id);
                        if (align.TargetId == null || align.TargetId.Length == 0)
                            sb.AppendFormat("{0}Device에서 사용할 TargetId를 선택해 주세요.\r\n", align.Id);
                        if (align.DisplayId == null || align.DisplayId.Length == 0)
                            sb.AppendFormat("{0}Device에서 사용할 Display ID를 선택해 주세요.\r\n", align.Id);
                        //if (align.ScanTime == 0)
                        //    sb.AppendFormat("{0}의 Scan Time 0입니다. 0보다 크게 설정 해주세요.\r\n", align.Id);
                        if (align.AlignResultUpperLImit == null || align.AlignResultUpperLImit.Length == 0)
                            sb.AppendFormat("{0}의 Upper Limit 0입니다. 0보다 크게 설정 해주세요.\r\n", align.Id, nameof(align.AlignResultUpperLImit));
                        if (align.AlignResultLowerLImit == null || align.AlignResultLowerLImit.Length == 0)
                            sb.AppendFormat("{0}의 Lower Limit 0입니다. 0보다 크게 설정 해주세요.\r\n", align.Id, nameof(align.AlignResultLowerLImit));

                        if (align.Title.Contains(','))
                        {//2021.08.24_1 kimgun
                            if (CommonHelper.MyMessageBox(Application.OpenForms[0], "Device Data", string.Format("{0}의 {1}에 comma가 포함되어 있습니다.'_'로 자동 변경 할까요?", align.Id, nameof(align.Title)), MessageBoxButtons.YesNo) == DialogResult.Yes)
                                align.Title = align.Title.Replace(',', '_');
                            else
                                sb.AppendFormat(string.Format("{0}의 {1}에 comma가 포함되어 있습니다.다른 단어로 교체해 주세요.", align.Id, nameof(align.Title)));
                        }
                        if (align.AlignResultTitles != null)
                        {//2021.08.24_1 kimgun
                            for (int i = 0; i < align.AlignResultTitles.Length; i++)
                            {
                                if (align.AlignResultTitles[i].Contains(','))
                                {//2021.08.24_1 kimgun
                                    if (CommonHelper.MyMessageBox(Application.OpenForms[0], "Device Data", string.Format("{0}의 {1}에 comma가 포함되어 있습니다.'_'로 자동 변경 할까요?", align.Id, nameof(align.AlignResultTitles)), MessageBoxButtons.YesNo) == DialogResult.Yes)
                                        align.AlignResultTitles[i] = align.AlignResultTitles[i].Replace(',', '_');
                                    else
                                        sb.AppendFormat(string.Format("{0}의 {1}에 comma가 포함되어 있습니다.다른 단어로 교체해 주세요.", align.Id, nameof(align.AlignResultTitles)));
                                }
                            }
                        }
                        sb.Append(align.IsAbleDefectTool());

                        if (align.AlignAlgoTool == null)
                            sb.AppendFormat("{0}의 AlignAlgoTool Link 되지 않았습니다. 소프트웨어 엔지니어에게 전달 바랍니다.\r\n", align.Id);

                        //2022.12.14_1 kimgun
                        int all = align.DefectScenarioItems.FindIndex(x => x.TargetIndex == -1);
                        int select = align.DefectScenarioItems.FindIndex(x => x.TargetIndex != -1);
                        if(all != -1 && select != -1)
                            sb.AppendFormat("{0}에 연결된 Defect Device는 반드시 TargetIndex가 모두 -1이거나 지정 frame 번호여야 합니다.\r\n", align.Id);

                        //foreach (DefectDevice def in align.DefectScenarioItems)
                        //{
                        //    if (def.Title.Length == 0)
                        //        sb.AppendFormat("{0}의 Title이 공백입니다.\r\n", def.Id);
                        //    if (def.TargetId == null || def.TargetId.Length == 0)
                        //        sb.AppendFormat("{0}Device에서 사용할 TargetId를 선택해 주세요.\r\n", def.Id);
                        //    if (def.DisplayId == null || def.DisplayId.Length == 0)
                        //        sb.AppendFormat("{0}Device에서 사용할 Display ID를 선택해 주세요.\r\n", def.Id);
                        //    //if (def.ScanTime == 0)
                        //    //    sb.AppendFormat("{0}의 Scan Time 0입니다. 0보다 크게 설정 해주세요.\r\n", def.Id);
                        //    if (def.Title.Contains(','))
                        //    {//2021.08.24_1 kimgun
                        //        if (CommonHelper.MyMessageBox(Application.OpenForms[0], "Device Data", string.Format("{0}의 {1}에 comma가 포함되어 있습니다.'_'로 자동 변경 할까요?", def.Id, nameof(def.Title)), MessageBoxButtons.YesNo) == DialogResult.Yes)
                        //            def.Title = def.Title.Replace(',', '_');
                        //        else
                        //            sb.AppendFormat(string.Format("{0}의 {1}에 comma가 포함되어 있습니다.다른 단어로 교체해 주세요.", def.Id, nameof(def.Title)));
                        //    }
                        //    foreach (ImageProcessingDevice proc in def.ImageProcessingScenarioItems)
                        //    {
                        //        if (proc.Title.Length == 0)
                        //            sb.AppendFormat("{0}의 Title이 공백입니다.\r\n", proc.Id);
                        //        if (proc.TargetId == null || proc.TargetId.Length == 0)
                        //            sb.AppendFormat("{0}Device에서 사용할 TargetId를 선택해 주세요.\r\n", proc.Id);
                        //        if (proc.DisplayId == null || proc.DisplayId.Length == 0)
                        //            sb.AppendFormat("{0}Device에서 사용할 Display ID를 선택해 주세요.\r\n", proc.Id);
                        //        if (proc.Title.Contains(','))
                        //        {//2021.08.24_1 kimgun
                        //            if (CommonHelper.MyMessageBox(Application.OpenForms[0], "Device Data", string.Format("{0}의 {1}에 comma가 포함되어 있습니다.'_'로 자동 변경 할까요?", proc.Id, nameof(proc.Title)), MessageBoxButtons.YesNo) == DialogResult.Yes)
                        //                proc.Title = proc.Title.Replace(',', '_');
                        //            else
                        //                sb.AppendFormat(string.Format("{0}의 {1}에 comma가 포함되어 있습니다.다른 단어로 교체해 주세요.", proc.Id, nameof(proc.Title)));
                        //        }
                        //    }
                        //}
                    }
                    foreach (InspectionDevice insp in img.InspScenarioItems)
                    {
                        if (insp.Title == null || insp.Title.Length == 0)
                            sb.AppendFormat("{0}의 Title이 공백입니다.\r\n", insp.Id);
                        if (insp.TargetId == null || insp.TargetId.Length == 0)
                            sb.AppendFormat("{0}Device에서 사용할 TargetId를 선택해 주세요.\r\n", insp.Id);
                        if (insp.DisplayId == null || insp.DisplayId.Length == 0)
                            sb.AppendFormat("{0}Device에서 사용할 Display ID를 선택해 주세요.\r\n", insp.Id);
                        //if (insp.ScanTime == 0)
                        //    sb.AppendFormat("{0}의 Scan Time 0입니다. 0보다 크게 설정 해주세요.\r\n", insp.Id);
                        if (insp.InspResultTitles == null || insp.InspResultTitles.Length == 0)
                            sb.AppendFormat("{0}의 {1}을 설정 해주세요.\r\n", insp.Id, nameof(insp.InspResultTitles));
                        if (insp.InspResultSpecUp == null || insp.InspResultSpecUp.Length == 0)
                            sb.AppendFormat("{0}의 {1}을 설정 해주세요.\r\n", insp.Id, nameof(insp.InspResultSpecUp));
                        if (insp.InspResultSpecLo == null || insp.InspResultSpecLo.Length == 0)
                            sb.AppendFormat("{0}의 {1}을 설정 해주세요.\r\n", insp.Id, nameof(insp.InspResultSpecLo));
                        if (insp.InspSpec == null || insp.InspSpec.Length == 0)
                            sb.AppendFormat("{0}의 {1}을 설정 해주세요.\r\n", insp.Id, nameof(insp.InspSpec));
                        if (insp.InspOffset == null || insp.InspOffset.Length == 0)
                            sb.AppendFormat("{0}의 {1}을 설정 해주세요.\r\n", insp.Id, nameof(insp.InspOffset));
                        if (insp.Title != null && insp.Title.Contains(','))
                        {//2021.08.24_1 kimgun
                            if (CommonHelper.MyMessageBox(Application.OpenForms[0], "Device Data", string.Format("{0}의 {1}에 comma가 포함되어 있습니다.'_'로 자동 변경 할까요?", insp.Id, nameof(insp.Title)), MessageBoxButtons.YesNo) == DialogResult.Yes)
                                insp.Title = insp.Title.Replace(',', '_');
                            else
                                sb.AppendFormat(string.Format("{0}의 {1}에 comma가 포함되어 있습니다.다른 단어로 교체해 주세요.", insp.Id, nameof(insp.Title)));
                        }
                        if (insp.InspResultTitles != null)
                        {//2021.08.24_1 kimgun
                            for (int i = 0; i < insp.InspResultTitles.Length; i++)
                            {
                                if (insp.InspResultTitles[i].Contains(','))
                                {//2021.08.24_1 kimgun
                                    if (CommonHelper.MyMessageBox(Application.OpenForms[0], "Device Data", string.Format("{0}의 {1}에 comma가 포함되어 있습니다.'_'로 자동 변경 할까요?", insp.Id, nameof(insp.InspResultTitles)), MessageBoxButtons.YesNo) == DialogResult.Yes)
                                        insp.InspResultTitles[i] = insp.InspResultTitles[i].Replace(',', '_');
                                    else
                                        sb.AppendFormat(string.Format("{0}의 {1}에 comma가 포함되어 있습니다.다른 단어로 교체해 주세요.", insp.Id, nameof(insp.InspResultTitles)));
                                }
                            }
                        }
                        if (insp.InspResultCimReportTitles != null)
                        {//2021.08.24_1 kimgun
                            for (int i = 0; i < insp.InspResultCimReportTitles.Length; i++)
                            {
                                if (insp.InspResultCimReportTitles[i].Contains(','))
                                {//2021.08.24_1 kimgun
                                    if (CommonHelper.MyMessageBox(Application.OpenForms[0], "Device Data", string.Format("{0}의 {1}에 comma가 포함되어 있습니다.'_'로 자동 변경 할까요?", insp.Id, nameof(insp.InspResultCimReportTitles)), MessageBoxButtons.YesNo) == DialogResult.Yes)
                                        insp.InspResultCimReportTitles[i] = insp.InspResultCimReportTitles[i].Replace(',', '_');
                                    else
                                        sb.AppendFormat(string.Format("{0}의 {1}에 comma가 포함되어 있습니다.다른 단어로 교체해 주세요.", insp.Id, nameof(insp.InspResultCimReportTitles)));
                                }
                            }
                        }
                        //2022.12.14_1 kimgun
                        int all = insp.DefectScenarioItems.FindIndex(x => x.TargetIndex == -1);
                        int select = insp.DefectScenarioItems.FindIndex(x => x.TargetIndex != -1);
                        if (all != -1 && select != -1)
                            sb.AppendFormat("{0}에 연결된 Defect Device는 반드시 TargetIndex가 모두 -1이거나 지정 frame 번호여야 합니다.\r\n", insp.Id);

                        sb.Append(insp.IsAbleDefectTool());
                        //foreach (DefectDevice def in insp.DefectScenarioItems)
                        //{
                        //    if (def.Title.Length == 0)
                        //        sb.AppendFormat("{0}의 Title이 공백입니다.\r\n", def.Id);
                        //    if (def.TargetId == null || def.TargetId.Length == 0)
                        //        sb.AppendFormat("{0}Device에서 사용할 TargetId를 선택해 주세요.\r\n", def.Id);
                        //    if (def.DisplayId == null || def.DisplayId.Length == 0)
                        //        sb.AppendFormat("{0}Device에서 사용할 Display ID를 선택해 주세요.\r\n", def.Id);
                        //    //if (def.ScanTime == 0)
                        //    //    sb.AppendFormat("{0}의 Scan Time 0입니다. 0보다 크게 설정 해주세요.\r\n", def.Id);
                        //    if (def.Title.Contains(','))
                        //    {//2021.08.24_1 kimgun
                        //        if (CommonHelper.MyMessageBox(Application.OpenForms[0], "Device Data", string.Format("{0}의 {1}에 comma가 포함되어 있습니다.'_'로 자동 변경 할까요?", def.Id, nameof(def.Title)), MessageBoxButtons.YesNo) == DialogResult.Yes)
                        //            def.Title = def.Title.Replace(',', '_');
                        //        else
                        //            sb.AppendFormat(string.Format("{0}의 {1}에 comma가 포함되어 있습니다.다른 단어로 교체해 주세요.", def.Id, nameof(def.Title)));
                        //    }
                        //    foreach (ImageProcessingDevice proc in def.ImageProcessingScenarioItems)
                        //    {
                        //        if (proc.Title.Length == 0)
                        //            sb.AppendFormat("{0}의 Title이 공백입니다.\r\n", proc.Id);
                        //        if (proc.TargetId == null || proc.TargetId.Length == 0)
                        //            sb.AppendFormat("{0}Device에서 사용할 TargetId를 선택해 주세요.\r\n", proc.Id);
                        //        if (proc.DisplayId == null || proc.DisplayId.Length == 0)
                        //            sb.AppendFormat("{0}Device에서 사용할 Display ID를 선택해 주세요.\r\n", proc.Id);
                        //        if (proc.Title.Contains(','))
                        //        {//2021.08.24_1 kimgun
                        //            if (CommonHelper.MyMessageBox(Application.OpenForms[0], "Device Data", string.Format("{0}의 {1}에 comma가 포함되어 있습니다.'_'로 자동 변경 할까요?", proc.Id, nameof(proc.Title)), MessageBoxButtons.YesNo) == DialogResult.Yes)
                        //                proc.Title = proc.Title.Replace(',', '_');
                        //            else
                        //                sb.AppendFormat(string.Format("{0}의 {1}에 comma가 포함되어 있습니다.다른 단어로 교체해 주세요.", proc.Id, nameof(proc.Title)));
                        //        }
                        //    }
                        //}
                    }
                }
                foreach (CalibrationDevice cal in unitDevice.CalibrationSenarioItems)
                {
                    if (cal.MovePointCount == 0 || cal.MoveThetaCount == 0)
                        sb.AppendFormat("{0} MovePoint는 0이 될수 없습니다.", cal.Id);

                    if (cal.PitchX.Count() != cal.MovePointCount)
                        sb.AppendFormat("{0} X Pitch 갯수와 MovePoint 갯수는 동일 해야 합니다.", cal.Id);

                    if (cal.PitchY.Count() != cal.MovePointCount)
                        sb.AppendFormat("{0} Y Pitch 갯수와 MovePoint 갯수는 동일 해야 합니다.", cal.Id);

                    if (cal.PitchT.Count() != cal.MoveThetaCount)
                        sb.AppendFormat("{0} T Pitch 갯수와 ThetaPoint 갯수는 동일 해야 합니다.", cal.Id);

                    if (cal.TargetId == null || cal.TargetId.Length == 0)
                        sb.AppendFormat("{0} Device에서 사용할 TargetId를 선택해 주세요", cal.Id);
                    if (cal.Title.Contains(','))
                    {//2021.08.24_1 kimgun
                        if (CommonHelper.MyMessageBox(Application.OpenForms[0], "Device Data", string.Format("{0}의 {1}에 comma가 포함되어 있습니다.'_'로 자동 변경 할까요?", cal.Id, nameof(cal.Title)), MessageBoxButtons.YesNo) == DialogResult.Yes)
                            cal.Title = cal.Title.Replace(',', '_');
                        else
                            sb.AppendFormat(string.Format("{0}의 {1}에 comma가 포함되어 있습니다.다른 단어로 교체해 주세요.", cal.Id, nameof(cal.Title)));
                    }

                    foreach (DefectDevice def in cal.DefectScenarioItems)
                    {
                        if (def.Title == null || def.Title.Length == 0)
                            sb.AppendFormat("{0}의 Title이 공백입니다.\r\n", def.Id);
                        if (def.TargetId == null || def.TargetId.Length == 0)
                            sb.AppendFormat("{0}Device에서 사용할 TargetId를 선택해 주세요.\r\n", def.Id);
                        if (def.DisplayId == null || def.DisplayId.Length == 0)
                            sb.AppendFormat("{0}Device에서 사용할 Display ID를 선택해 주세요.\r\n", def.Id);
                        //if (def.ScanTime == 0)
                        //    sb.AppendFormat("{0}의 Scan Time 0입니다. 0보다 크게 설정 해주세요.\r\n", def.Id);
                        if (def.Title.Contains(','))
                        {//2021.08.24_1 kimgun
                            if (CommonHelper.MyMessageBox(Application.OpenForms[0], "Device Data", string.Format("{0}의 {1}에 comma가 포함되어 있습니다.'_'로 자동 변경 할까요?", def.Id, nameof(def.Title)), MessageBoxButtons.YesNo) == DialogResult.Yes)
                                def.Title = def.Title.Replace(',', '_');
                            else
                                sb.AppendFormat(string.Format("{0}의 {1}에 comma가 포함되어 있습니다.다른 단어로 교체해 주세요.", def.Id, nameof(def.Title)));
                        }
                    }
                }
            }

            if (sb.Length != 0)
                NoticeAlarmCollection.Instance.AddNoticeAlarm(this.Name, sb.ToString());

            return sb.ToString().Length == 0 ? true : false;
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            MessageWindow window = null;
            if(IsFilter || IsComboFilter)
            {
                window = CommonHelper.CreateMessageBox("Information", "Clear the filter and try again.\r\n Filter 해제 후 저장해주세요.");
                window.ShowDialog();
                window.Dispose();
                return;
            }
            if (SystemState.ManualMode == false)
            {
                window = CommonHelper.CreateMessageBox("Information", "Manual Mode가 아닙니다.");
                window.ShowDialog();
                window.Dispose();
                return;
            }
            if (AutoMainScreen.MainScreen.BatchRun != null && AutoMainScreen.MainScreen.BatchRun.Visible)
            {
                window = CommonHelper.CreateMessageBox("Information", "Batch Run창을 닫은 후 재시도 해주세요.");
                window.ShowDialog();
                window.Dispose();
                return;
            }
            if (CommonHelper.MyMessageBox(Application.OpenForms[0], "Device Data", string.Format("변경 된 data를 저장 하시겟습니까?"), MessageBoxButtons.YesNo) == DialogResult.No) return;

            if (IsAbleSave() == false) return;

            bool bReturn = true;
            SystemState.SaveSyncTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            int rv = 0;
            bReturn = commonDevice.Save();
            bReturn &= cimDevice.Save();//2021.07.26_2 jhyun
            //commonDevice.Dispose();
            //commonDevice = null;
            foreach (var unitDevice in lstUnitDevice)
            {
                foreach (CameraDevice cam in unitDevice.CameraScenarioItems)
                {
                    if(cam.CameraParameter != null)//2023.01.10_1 kimgun 예외처리
                     cam.CameraParameter.Save();//2022.12.27_1 kimgun
                }
                    //bReturn &= unitDevice.Save();2021.08.18_2 kimgun 아래로 이동
                foreach (ImageDevice img in unitDevice.ImageScenarioItems)
                {
                    //foreach (AlignerDevice align in img.AlignerScenarioItems)
                    //{
                    //    foreach (DefectDevice def in align.DefectScenarioItems)
                    //    {
                    //        if (def.DefectMethod == null) continue;//kimxxxx
                    //    }
                    //}
                    //foreach (InspectionDevice insp in img.InspScenarioItems)
                    //{
                    //    foreach (DefectDevice def in insp.DefectScenarioItems)
                    //    {
                    //    }
                    //}
                    foreach (DefectDevice def in img.DefectScenarioItems)
                    {
                        if (def.DefectMethod == null) continue;//kimxxxx
                        var cam = unitDevice.CameraScenarioItems.Find(x => x.Id == def.TargetId); 
                        if(cam.Camera != null && cam.Camera.IsLinescan)
                        {
                            if (cam.SubFrameCount <= 0) //2022.12.21_1 cgun SubFrameCount 미사용일 경우만 해당 상황 적용
                            {
                                if (def.TargetIndex >= cam.FrameCount)
                                    def.TargetIndex = cam.FrameCount;//kimxxxx 자동 바꿈보다는 알림이 맞지
                            }
                        }
                        foreach (ImageProcessingDevice proc in def.ImageProcessingScenarioItems)
                        {
                            if (proc.Params == null)
                            {
                                var procType = proc.ProcType;
                                proc.Params = CommonAdaptorTool.AImageProcessingParamManager.CreateImageProcParams(ref procType, proc.Id);
                                proc.ProcType = procType;
                            }
                            //2021.07.21_2 cjw 
                            proc.Params.Save(Path.Combine(SystemOption.param.ModelBasePath.Value as string, SystemOption.param.CurrentModel.Value as string, string.Format("{0}.{1}.obj", proc.Id, proc.ProcType)));
                        }
                    }
                }
                foreach (CalibrationDevice cal in unitDevice.CalibrationSenarioItems)
                {
                    foreach (DefectDevice def in cal.DefectScenarioItems)
                    {
                        if (def.DefectMethod == null) continue;

                        string filename = Path.Combine(SystemOption.param.ModelBasePath.Value as string, SystemOption.param.CurrentModel.Value as string, string.Format("{0}", def.DefectMethod.DataFileName));
                        if (def.DefectTool != null)
                        {
                            rv = def.DefectTool.SaveParam(filename);
                            if (rv != 0)
                            {
                                NoticeAlarmCollection.Instance.AddNoticeAlarm(unitDevice.Id, string.Format("{0} Save Fail", def.Title));
                            }
                        }
                    }
                }
                bReturn &= unitDevice.Save();

                //unitDevice.Dispose();
            }
            if (bReturn == false)
                return;

            if ((int)SystemOption.param.UnitCount.Value != lstUnitDevice.Count)
            {
                SystemOption.param.UnitCount.Value = lstUnitDevice.Count;
                SystemOption.param.Save();
                AutoMainScreen.MainScreen.evhResultAssign(null, EventArgs.Empty); //2021.07.17_1 kimgun
            }

            //2021.07.26_1 jhyun
            if (AddDeleteDevice == true || SystemState.RebootReq)
            { //Device가 하나라도 추가, 삭제되면 할 수 있도록..
                SystemState.RebootReq = false;//2021.08.18_2 kimgun
                SystemManager.SystemManager.Instance.InitDevice();
                SystemManager.SystemManager.Instance.CreateDevice();
                SystemManager.SystemManager.Instance.LoadUnitDevice();
                commonDevice = SystemManager.SystemManager.Instance.CommonUnit;
                cimDevice = SystemManager.SystemManager.Instance.CimUnit;//2021.07.26_2 jhyun
                lstUnitDevice = SystemManager.SystemManager.Instance.Units.ToList();
                SystemManager.SystemManager.Instance.Assign(lstUnitDevice);
                AutoMainScreen.MainScreen.evhDiplayAssign(null, EventArgs.Empty);
                if (AutoMainScreen.MainScreen.BatchRun != null)
                    AutoMainScreen.MainScreen.BatchRun.CreateVariable();//2021.08.03_2 kimgun
                AutoMainScreen.MainScreen.evhResultAssign(null, EventArgs.Empty); //2021.07.17_1 kimgun
                AutoMainScreen.MainScreen.evhMonitorAssign(null, EventArgs.Empty); //2021.08.18_1 kimgun
                SystemManager.SystemManager.Instance.ThreadStart();

                AddDeleteDevice = false;
            }
            //(AutoMainScreen.MainScreen.NavigatorControl[(int)enNavigator.MONITOR] as MonitorMainScreen).Initialize();//PLC Mapping 형식으로 바뀌어 주석

            //저장이면 이미 테스트 완료한 상태라 굳이 forModel을 다시 어싸인 할 필요가 없을 것 같아 주석...
            //if (eDeviceTarget != enDeviceTarget.eRunning)
            //{
            //    SystemManager.SystemManager.Instance.InitDeviceforModel();
            //    SystemManager.SystemManager.Instance.CreateDeviceforModel();
            //    SystemManager.SystemManager.Instance.LoadUnitDeviceforModel();

            //    lstUnitDevice = SystemManager.SystemManager.Instance.UnitsforModel.ToList();
            //    SystemManager.SystemManager.Instance.Assign(lstUnitDevice);
            //}

            window = CommonHelper.CreateMessageBox("Information", "SAVE OK");
            window.ShowDialog();
            window.Dispose();

            UpdateTreeView();
            // GC.Collect();
            GC.Collect(0, GCCollectionMode.Forced);//LBG 20211111
                                                   // GC.Collect(0, GCCollectionMode.Forced); //LBG 20211111
                                                   //GC.WaitForFullGCComplete(); //LBG 20211111

        }

        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (e.ChangedItem.Label.Contains("Title") || (e.ChangedItem.Label.Contains("[") && e.ChangedItem.Parent.Label.Contains("Title")))
            {//2021.08.24_1 kimgun
                if (e.ChangedItem.Value is string)
                {
                    if (e.ChangedItem.Value.ToString().Contains(','))
                    {
                        NoticeAlarmCollection.Instance.AddNoticeAlarm(this.Name, "Title에는 comma(,)를 사용 불가합니다.\r\n다른 단어로 대체 바랍니다.");
                        return;
                    }
                }
                else if (e.ChangedItem.Value is string[])
                {
                    foreach (string sv in e.ChangedItem.Value as string[])
                    {
                        if (sv.Contains(','))
                        {
                            NoticeAlarmCollection.Instance.AddNoticeAlarm(this.Name, "Title에는 comma(,)를 사용 불가합니다.\r\n다른 단어로 대체 바랍니다.");
                            return;
                        }
                    }

                }
            }
            else if (e.ChangedItem.Label.Contains("SpecUp") || (e.ChangedItem.Label.Contains("[") && e.ChangedItem.Parent.Label.Contains("SpecUp")))
            {//2021.09.06_4 kimgun
                var device = ((s as PropertyGrid).SelectedObject as InspectionDevice);
                if (device == null) return;
                if (device.Display == null)
                {//2022.08.11_1 kimgun
                    MessageWindow window = CommonHelper.CreateMessageBox("Warning", string.Format("선택한 Device에 Display를 지정후 다시 시도해 주세요."));// lstUnitDevice[idx].ImageScenarioItems[idx1].AlignerScenarioItems[idx2].IsAbleDefectTool(method).ToString());
                    window.ShowDialog();
                    window.Dispose();
                    return;
                }
                if (device.Display.Option is AdaptorDisplay.ProfileDisplayOption)
                {
                    if (e.ChangedItem.Value is decimal)
                    {
                        (device.Display.Option as AdaptorDisplay.ProfileDisplayOption).SpecUpper = (double)((decimal)e.ChangedItem.Value);
                    }
                    else if (e.ChangedItem.Value is decimal[])
                    {
                        foreach (var upper in e.ChangedItem.Value as decimal[])
                        {
                            (device.Display.Option as AdaptorDisplay.ProfileDisplayOption).SpecUpper = (double)upper;
                            break;
                        }
                    }
                }
            }

            else if (e.ChangedItem.Label.Contains("SpecLo") || (e.ChangedItem.Label.Contains("[") && e.ChangedItem.Parent.Label.Contains("SpecLo")))
            {//2021.09.06_4 kimgun
                var device = ((s as PropertyGrid).SelectedObject as InspectionDevice);
                if (device == null) return;
                if (device.Display == null)
                {//2022.08.11_1 kimgun
                    MessageWindow window = CommonHelper.CreateMessageBox("Warning", string.Format("선택한 Device에 Display를 지정후 다시 시도해 주세요."));// lstUnitDevice[idx].ImageScenarioItems[idx1].AlignerScenarioItems[idx2].IsAbleDefectTool(method).ToString());
                    window.ShowDialog();
                    window.Dispose();
                    return;
                }
                if (device.Display.Option is AdaptorDisplay.ProfileDisplayOption)
                {
                    if (e.ChangedItem.Value is decimal)
                    {
                        (device.Display.Option as AdaptorDisplay.ProfileDisplayOption).SpecLower = (double)((decimal)e.ChangedItem.Value);
                    }
                    else if (e.ChangedItem.Value is decimal[])
                    {
                        foreach (var lower in e.ChangedItem.Value as decimal[])
                        {
                            (device.Display.Option as AdaptorDisplay.ProfileDisplayOption).SpecLower = (double)lower;
                            break;
                        }
                    }
                }
            }
            if (e.ChangedItem.Label == "LightControlId")
            {
                string lcId = ((s as PropertyGrid).SelectedObject as LightDevice).LightControlId;
                if (lcId != null && lcId.Length != 0)
                {
                    //channel  class array
                    var arrChs = SystemManager.SystemManager.Instance.GetLightControlInforlList(SystemManager.SystemManager.enLightControlItems.enChannelList, lcId);
                    if (arrChs != null)
                    {
                        //channel id
                        GListUItype.Add("LightChannelId", arrChs.Select(x => (x as TBaseLightch).Id).ToArray());
                        //chnnel index
                        GListUItype.Add("LightCh", arrChs.Select(x => (x as TBaseLightch).Channel as object).ToArray());
                    }
                }
            }
            else if (e.ChangedItem.Label == "LightChannelId")
            {
                string dvId = ((s as PropertyGrid).SelectedObject as LightDevice).Id;
                var dvtmp = dvId.Split('.');
                if (dvtmp.Length < 3) goto UNIT_ASSIGN;
                var tmp = e.ChangedItem.Value.ToString().Split('_');
                int idx = lstUnitDevice.FindIndex(x => x.Id == dvtmp[0]); if (idx == -1) goto UNIT_ASSIGN;
                int idx1 = lstUnitDevice[idx].CameraScenarioItems.FindIndex(x => x.Id == (dvtmp[0] + "." + dvtmp[1])); if (idx1 == -1) goto UNIT_ASSIGN;
                int idx2 = lstUnitDevice[idx].CameraScenarioItems[idx1].LightScenarioItems.FindIndex(x => x.Id == dvId); if (idx2 == -1) goto UNIT_ASSIGN;
                lstUnitDevice[idx].CameraScenarioItems[idx1].LightScenarioItems[idx2].LightCh = int.Parse(tmp[1]);
            }
            else if (e.ChangedItem.Label == "LightCh")
            {
                string dvId = ((s as PropertyGrid).SelectedObject as LightDevice).Id;
                string newChId = ((s as PropertyGrid).SelectedObject as LightDevice).LightControlId + "_" + e.ChangedItem.Value.ToString();
                var dvtmp = dvId.Split('.');
                if (dvtmp.Length < 3) goto UNIT_ASSIGN;
                var tmp = e.ChangedItem.Value.ToString().Split('_');
                int idx = lstUnitDevice.FindIndex(x => x.Id == dvtmp[0]); if (idx == -1) goto UNIT_ASSIGN;
                int idx1 = lstUnitDevice[idx].CameraScenarioItems.FindIndex(x => x.Id == (dvtmp[0] + "." + dvtmp[1])); if (idx1 == -1) goto UNIT_ASSIGN;
                int idx2 = lstUnitDevice[idx].CameraScenarioItems[idx1].LightScenarioItems.FindIndex(x => x.Id == dvId); if (idx2 == -1) goto UNIT_ASSIGN;
                lstUnitDevice[idx].CameraScenarioItems[idx1].LightScenarioItems[idx2].LightChannelId = newChId;
            }
            else if (e.ChangedItem.Label == "AlignAlgoTypeIndex")
            {

            }
            else if (e.ChangedItem.Label == "InspectionWay")
            {//2021.07.20_4 kimgun 지원하지 않는 것은 선택 불가
                InspectionDevice insp = ((s as PropertyGrid).SelectedObject as InspectionDevice);
                if (insp != null)
                {
                    var method = InspectionManager.Instance.GetMethod(insp.InspectionType, insp.InspectionWay);
                    if (method == null)
                    {
                        insp.InspectionWay = (enInspectionSubClass)e.OldValue;
                        return;
                    }
                }
            }
            else if (e.ChangedItem.Label == "DefectType")
            {
                string dvId = ((s as PropertyGrid).SelectedObject as DefectDevice).Id;
                var dvtmp = dvId.Split('.');
                if (dvtmp.Length < 3) goto UNIT_ASSIGN;

                int idx = lstUnitDevice.FindIndex(x => x.Id == dvtmp[0]); if (idx == -1) goto UNIT_ASSIGN;
                int idx1 = lstUnitDevice[idx].ImageScenarioItems.FindIndex(x => x.Id == (dvtmp[0] + "." + dvtmp[1])); if (idx1 == -1) goto UNIT_ASSIGN;
                int idx2 = 0;

                if (idx2 == -1) goto UNIT_ASSIGN;


                if (lstUnitDevice[idx].ImageScenarioItems[idx1].AlignerScenarioItems.Count != 0)
                {
                    idx2 = lstUnitDevice[idx].ImageScenarioItems[idx1].AlignerScenarioItems.FindIndex(x => x.Id == (dvtmp[0] + "." + dvtmp[1] + "." + dvtmp[2]));
                    if (idx2 != -1)
                    {
                        var method = DefectManager.Instance.GetMethod(((s as PropertyGrid).SelectedObject as DefectDevice).DefectType);
                        ((s as PropertyGrid).SelectedObject as DefectDevice).DefectType = (DefEnums.enDefect)e.ChangedItem.Value;
                        string msg;
                        if ((msg = lstUnitDevice[idx].ImageScenarioItems[idx1].AlignerScenarioItems[idx2].IsAbleDefectTool(method).ToString()).Length != 0)
                        {
                            //MessageWindow window = CommonHelper.CreateMessageBox("Information", string.Format("해당 Align은 {0} 선택이 불가능합니다.", e.ChangedItem.Value.ToString()));
                            MessageWindow window = CommonHelper.CreateMessageBox("Information", msg);// lstUnitDevice[idx].ImageScenarioItems[idx1].AlignerScenarioItems[idx2].IsAbleDefectTool(method).ToString());
                            window.ShowDialog();
                            window.Dispose(); //lbg 2010909
                            ((s as PropertyGrid).SelectedObject as DefectDevice).DefectType = (DefEnums.enDefect)e.OldValue;
                            goto UNIT_ASSIGN;
                        }
                    }
                }

                if (lstUnitDevice[idx].ImageScenarioItems[idx1].InspScenarioItems.Count != 0)
                {
                    idx2 = lstUnitDevice[idx].ImageScenarioItems[idx1].InspScenarioItems.FindIndex(x => x.Id == (dvtmp[0] + "." + dvtmp[1] + "." + dvtmp[2]));
                    if (idx2 != -1)
                    {
                        if (lstUnitDevice[idx].ImageScenarioItems[idx1].InspScenarioItems[idx2].InspTool == null) goto UNIT_ASSIGN;
                        ((s as PropertyGrid).SelectedObject as DefectDevice).DefectType = (DefEnums.enDefect)e.ChangedItem.Value;
                        string msg;
                        ((s as PropertyGrid).SelectedObject as DefectDevice).DefectType = (DefEnums.enDefect)e.ChangedItem.Value;//2021.07.24_1 kimgun
                        if ((msg = lstUnitDevice[idx].ImageScenarioItems[idx1].InspScenarioItems[idx2].IsAbleDefectTool((DefEnums.enDefect)e.ChangedItem.Value).ToString()).Length != 0)
                        {
                            MessageWindow window = null;
                            window = CommonHelper.CreateMessageBox("Information", msg);
                            window.ShowDialog();
                            window.Dispose(); //lbg 20210909
                            ((s as PropertyGrid).SelectedObject as DefectDevice).DefectType = (DefEnums.enDefect)e.OldValue;
                            goto UNIT_ASSIGN;
                        }
                    }
                }
            }
            else if (e.ChangedItem.Label == "CameraId")
            {//2021.07.27_4 kimgun camera parameter 다를 경우 카메라 parameter로 변경하도록 수정..kimxxx 같은 형식이지만 내용이 다를 경우 제대로 동작하는지 확인 요망
                CameraDevice cam = ((s as PropertyGrid).SelectedObject as CameraDevice);
                if (cam != null)
                {
                    if (cam.Camera != null && cam.CameraParameter.GetType().Equals(cam.Camera.m_CamParam.GetType()) == false)
                    {
                        cam.CameraParameter = cam.Camera.m_CamParam.Clone() as TBaseCamera.CCameraParameter;
                    }
                }
            }
            else if (e.ChangedItem.Label == "IndependentMode")
            {
                if ((s as PropertyGrid).SelectedObject is InspectionDevice)
                {
                    InspectionDevice insp = ((s as PropertyGrid).SelectedObject as InspectionDevice);
                    if (insp != null)
                    {
                        SystemState.RebootReq = true;
                    }
                }
                else if ((s as PropertyGrid).SelectedObject is AlignerDevice)
                {
                    AlignerDevice align = ((s as PropertyGrid).SelectedObject as AlignerDevice);
                    if (align != null)
                    {
                        SystemState.RebootReq = true;
                    }
                }

            }
            else if (e.ChangedItem.Label == "AlignId")
            {//2021.08.18_2 kimgun 
                InspectionDevice insp = ((s as PropertyGrid).SelectedObject as InspectionDevice);
                if (insp != null)
                {
                    //if(insp.AlignId != null & insp.AlignId.Length > 0)
                    {
                        foreach (DefectDevice dd in insp.DefectScenarioItems)
                            dd.AlignId = insp.AlignId;
                    }
                }
            }
            //else if (e.ChangedItem.Label == "ScIStep")
            //{//2021.08.10_5 kimgun
            //    if (((s as PropertyGrid).SelectedObject) is UnitDevice)
            //    {
            //        AddDeleteDevice = true;//unit의 시나리오 스텝이 바꼈다면 Assign을 새로 해야 한다.
            //    }
            //}
            else if (e.ChangedItem.Label == "ImageIndex")
            {
                //2022.08.30 shlee
                InspectionDevice insp = ((s as PropertyGrid).SelectedObject as InspectionDevice);
                if (insp != null)
                {
                    foreach (DefectDevice dd in insp.DefectScenarioItems)
                        dd.AlignId = insp.AlignId;
                }
            }
        UNIT_ASSIGN:
            SystemManager.SystemManager.Instance.Assign(lstUnitDevice);
        }

        private void btnBackupLoad_Click(object sender, EventArgs e)
        {//kimxxxxx 점검해보고 사용 여부 판단 하자. 점검 전까지 차단
            foreach (var unitDevice in lstUnitDevice)
            {
                unitDevice.LoadforModel();
            }
        }

        private void btnBackup_Click(object sender, EventArgs e)
        {
            SystemState.SaveSyncTime = DateTime.Now.ToString("yyyyMMddHHmmss");

            foreach (var unitDevice in lstUnitDevice)
            {
                unitDevice.SaveforModel();
                //camera parameter는 공용이라 따로 저장하지  않는다.
                //foreach(CameraDevice cam in unitDevice.CameraScenarioItems)
                //{
                //    if (cam.CameraParameter != null)
                //        cam.CameraParameter.Save();
                //}
                foreach (CameraDevice cam in unitDevice.CameraScenarioItems)
                {
                    foreach (DefectDevice def in cam.DefectScenarioItems)
                    {
                        string filename = Path.Combine(SystemOption.param.ModelBasePath.Value as string, SystemOption.param.CurrentModel.Value as string, string.Format("Defect.{0}.model.obj", def.Id));
                        if (def.DefectTool != null)
                            def.DefectTool.SaveParam(filename);
                        foreach (ImageProcessingDevice proc in def.ImageProcessingScenarioItems)
                        {
                            if (proc.Params == null)
                            {
                                var procType = proc.ProcType;
                                proc.Params = CommonAdaptorTool.AImageProcessingParamManager.CreateImageProcParams(ref procType, proc.Id);
                                proc.ProcType = procType;
                            }
                            proc.Params.Save(Path.Combine(SystemOption.param.ModelBasePath.Value as string, SystemOption.param.CurrentModel.Value as string, string.Format("{0}.{1}.model.obj", proc.Id, proc.ProcType)));
                        }
                    }
                }
                foreach (ImageDevice img in unitDevice.ImageScenarioItems)
                {
                    foreach (DefectDevice def in img.DefectScenarioItems)
                    {
                        string filename = Path.Combine(SystemOption.param.ModelBasePath.Value as string, SystemOption.param.CurrentModel.Value as string, string.Format("Defect.{0}.model.obj", def.Id));
                        if (def.DefectTool != null)
                            def.DefectTool.SaveParam(filename);
                        foreach (ImageProcessingDevice proc in def.ImageProcessingScenarioItems)
                        {
                            if (proc.Params == null)
                            {
                                var procType = proc.ProcType;
                                proc.Params = CommonAdaptorTool.AImageProcessingParamManager.CreateImageProcParams(ref procType, proc.Id);
                                proc.ProcType = procType;
                            }
                            proc.Params.Save(Path.Combine(SystemOption.param.ModelBasePath.Value as string, SystemOption.param.CurrentModel.Value as string, string.Format("{0}.{1}.model.obj", proc.Id, proc.ProcType)));
                        }
                    }

                }
                foreach (CalibrationDevice cal in unitDevice.CalibrationSenarioItems)
                {
                    foreach (DefectDevice def in cal.DefectScenarioItems)
                    {
                        string filename = Path.Combine(SystemOption.param.ModelBasePath.Value as string, SystemOption.param.CurrentModel.Value as string, string.Format("Defect.{0}.model.obj", def.Id));
                        if (def.DefectTool != null)
                            def.DefectTool.SaveParam(filename);
                        foreach (ImageProcessingDevice proc in def.ImageProcessingScenarioItems)
                        {
                            if (proc.Params == null)
                            {
                                var procType = proc.ProcType;
                                proc.Params = CommonAdaptorTool.AImageProcessingParamManager.CreateImageProcParams(ref procType, proc.Id);
                                proc.ProcType = procType;
                            }
                            proc.Params.Save(Path.Combine(SystemOption.param.ModelBasePath.Value as string, SystemOption.param.CurrentModel.Value as string, string.Format("{0}.{1}.model.obj", proc.Id, proc.ProcType)));
                        }
                    }
                }

            }
        }

        private void btnRecovery_Click(object sender, EventArgs e)
        {
            //if (SystemState.AutoMode == true) return;//2021.08.20_1 kimgun
            //UnitSync(SystemState.DeviceTarget);
        }

        private void treeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            treeView.SelectedNode = e.Node;
        }

        private string LastRemoveNumber(string value)
        {
            StringBuilder sb = new StringBuilder();
            string[] items = value.Split('.');
            if (items.Length == 1)
                return value;

            for (int i = 0; i < items.Length; i++)
            {
                if (i == (items.Length - 1))
                    sb.AppendFormat("{0}", items[i].RemoveNumber());
                else
                    sb.AppendFormat("{0}.", items[i]);
            }

            return sb.ToString();
        }

        public void EnableControl(enDeviceTarget deviceTarget)
        {
            propertyGrid.Enabled = deviceTarget == enDeviceTarget.eModel ? true : false;
            btnAddUnit.Enabled = deviceTarget == enDeviceTarget.eModel ? true : false;
            btnRecovery.Enabled = deviceTarget == enDeviceTarget.eModel ? true : false;
            btnBackupLoad.Enabled = deviceTarget == enDeviceTarget.eModel ? true : false;
            btnBackup.Enabled = deviceTarget == enDeviceTarget.eModel ? true : false;
        }

        private void treeView_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {//2021.08.18_3 kimgun
            if (treeView.SelectedNode == null) return;
            treeView.SelectedNode.BackColor = SystemColors.Highlight;
            treeView.SelectedNode.ForeColor = Color.White;
            PreviousSelectedNode = treeView.SelectedNode;
        }
        private void treeView_KeyDown(object sender, KeyEventArgs e)
        {
            string cmd = "";
            if(e.KeyCode == Keys.Delete)
            {
                var treeView = sender as TreeView;
                var name = treeView.SelectedNode.Name;
                cmd = "Delete";
                ItemFunc(cmd + " " + name);
            }
            else if (e.KeyCode == Keys.F2)
            {
                isAllowNodeRenaming = true;
                treeView.LabelEdit = true;
                treeView.SelectedNode.BeginEdit();
            }
            else if(e.Control && e.KeyCode == Keys.C)
            {
                var SelectedNode = treeView.SelectedNode;
                if (IsFilter || IsComboFilter)
                {//2023.01.27_1 kimgun
                    var nodes = TreeViewCache.Nodes.Find(treeView.SelectedNode.Name, true);
                    if (nodes == null || nodes.Length == 0)
                        return;
                    foreach (var nd in nodes)
                    {
                        if (nd.FullPath.Contains(SelectedNode.FullPath))
                        {
                            SelectedNode = nd;
                            break;
                        }
                    }
                }
                if (SelectedNode == null) return;
                if (lstUnitDevice == null || lstUnitDevice.Count == 0) return;
                string target = SelectedNode.Name;
                string parent = SelectedNode.Parent.Name;
                string[] sKey = target.Split('.');
                UnitDevice unitDevice = lstUnitDevice.Find(x => x.Id == sKey[0]);
                if (unitDevice == null) return;
                var img = unitDevice.ImageScenarioItems.Find(x => x.Key == parent);
                if (img != null)
                {
                    var align = img.AlignerScenarioItems.Find(x => x.Key == target);
                    var inspection = img.InspScenarioItems.Find(x => x.Key == target);
                    var defect = img.DefectScenarioItems.Find(x => x.Key == target);
                    if (align != null)
                    {
                        pasteAlign = align.Clone() as AlignerDevice;
                        pasteInsp = null;
                        pasteDefect = null;
                    }
                    else if (inspection != null)
                    {
                        pasteInsp = inspection.Clone() as InspectionDevice;
                        pasteAlign = null;
                        pasteDefect = null;
                    }
                    else if (defect != null)
                    {
                        pasteDefect = defect.Clone() as DefectDevice;
                        pasteAlign = null;
                        pasteInsp = null;
                    }
                    else
                    {
                        pasteAlign = null;
                        pasteInsp = null;
                        pasteDefect = null;
                    }
                }
            }
            else if(e.Control && e.KeyCode == Keys.V)
            {
                var SelectedNode = treeView.SelectedNode;
                if (IsFilter || IsComboFilter)
                {//2023.01.27_1 kimgun
                    var nodes = TreeViewCache.Nodes.Find(treeView.SelectedNode.Name, true);
                    if (nodes == null || nodes.Length == 0)
                        return;
                    foreach (var nd in nodes)
                    {
                        if (nd.FullPath.Contains(SelectedNode.FullPath))
                        {
                            SelectedNode = nd;
                            break;
                        }
                    }
                }
                if (SelectedNode == null) return;
                if (lstUnitDevice == null || lstUnitDevice.Count == 0) return;
                string target = SelectedNode.Name;
                string parent = SelectedNode.Parent.Name;
                string[] sKey = target.Split('.');
                UnitDevice unitDevice = lstUnitDevice.Find(x => x.Id == sKey[0]);
                if (unitDevice == null) return;
                var img = unitDevice.ImageScenarioItems.Find(x => x.Key == parent);
                if (img != null)
                {
                    if (pasteAlign != null)
                    {
                        CreateAlignerDevice(unitDevice, img,parent,SelectedNode);
                        
                        //(unitDevice.ImageScenarioItems[index] as ImageDevice).AlignerScenarioItems.Add(device);
                        UpdateTreeView();
                    }
                    else if (pasteInsp != null)
                    {
                        CreateInspectionDevice(unitDevice, img, parent, SelectedNode);
                        
                        //(unitDevice.ImageScenarioItems[index] as ImageDevice).InspScenarioItems.Add(device);
                        UpdateTreeView();
                    }
                    else if (pasteDefect != null)
                    {
                        CreateDefectDevice(unitDevice, img, parent, SelectedNode);
                        
                        UpdateTreeView();
                    }
                }
            }
        }

        private void CreateDefectDevice(UnitDevice unitDevice, ImageDevice img, string parentId, TreeNode selectedNode)
        {//2023.01.27_1 kimgun
            int itemCount = 0;
            if (img.DefectScenarioItems == null || img.DefectScenarioItems.Count == 0)
                itemCount = 1;
            else
                itemCount = img.DefectScenarioItems.Count + 1;
            DefectDevice device = pasteDefect.Clone() as DefectDevice;
            //device = pasteDefect.Clone() as DefectDevice;
            device.Id = string.Format("{0}.Defect{1}", parentId, itemCount);
            device.Index = itemCount - 1;
            device.Title = string.Format("Defect{0}", itemCount);
            device.DisplayId = device.DisplayId;
            device.TargetId = device.TargetId;
            device.AlignId = device.AlignId;
            device.ImageProcessingScenarioItems = device.ImageProcessingScenarioItems.ToList();

            int index = unitDevice.ImageScenarioItems.FindIndex(p => selectedNode.Name.Contains(p.Key));
            if (index == -1) return;
            (unitDevice.ImageScenarioItems[index] as ImageDevice).DefectScenarioItems.Add(device);
        }

        private void CreateInspectionDevice(UnitDevice unitDevice, ImageDevice img, string parentId, TreeNode selectedNode)
        {//2023.01.27_1 kimgun
            int itemCount = 0;
            if (img.InspScenarioItems == null || img.InspScenarioItems.Count == 0)
                itemCount = 1;
            else
                itemCount = img.InspScenarioItems.Count + 1;
            InspectionDevice device = pasteInsp.Clone() as InspectionDevice;
            //device = pasteInsp.Clone() as InspectionDevice;
            device.Id = string.Format("{0}.Inspection{1}", parentId, itemCount);
            device.Index = itemCount - 1;
            device.Title = string.Format("Inspection{0}", itemCount);
            device.DisplayId = pasteInsp.DisplayId;
            device.TargetId = pasteInsp.TargetId;
            device.AlignId = pasteInsp.AlignId;
            device.DefectScenarioItems = pasteInsp.DefectScenarioItems.ToList();


            int index = unitDevice.ImageScenarioItems.FindIndex(p => selectedNode.Name.Contains(p.Key));
            if (index == -1) return;
            img.InspScenarioItems.Add(device);
        }

        private void CreateAlignerDevice(UnitDevice unitDevice, ImageDevice img, string parentId,TreeNode selectedNode)
        {//2023.01.27_1 kimgun
            int itemCount = 0;
            if (img.AlignerScenarioItems == null || img.AlignerScenarioItems.Count == 0)
                itemCount = 1;
            else
                itemCount = img.AlignerScenarioItems.Count + 1;
            AlignerDevice device = pasteAlign.Clone() as AlignerDevice;
            //device = pasteAlign.Clone() as AlignerDevice;
            device.Id = string.Format("{0}.Aligner{1}", parentId, itemCount);
            device.Index = itemCount - 1;
            device.Title = string.Format("Aligner{0}", itemCount);
            device.DisplayId = pasteAlign.DisplayId;
            device.TargetId = pasteAlign.TargetId;
            device.DefectScenarioItems = pasteAlign.DefectScenarioItems.ToList();


            int index = unitDevice.ImageScenarioItems.FindIndex(p => selectedNode.Name.Contains(p.Key));
            if (index == -1) return;
            img.AlignerScenarioItems.Add(device);
        }

        private void treeView_KeyUp(object sender, KeyEventArgs e)
        {

        }
        bool rightbutton = false;
        private void treeView_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) rightbutton = true;
            else
                rightbutton = false;
        }

        private void btnMerge_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "xml file |*.xml";
            ofd.Multiselect = true;
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                string id = "Unit1.Camera1";
                int len = ofd.FileNames.Length;
                UnitDevice[] unit = new UnitDevice[len];
                lstUnitDevice[0].ImageScenarioItems.Clear();
                int defectIndex = 1;
                int inspectionIndex = 1;

                ImageDevice imgD = new ImageDevice();
                imgD.Title = "Image1";
                imgD.TargetId = id;
                imgD.Id = "Unit1.Image1";
                imgD.DisplayId = "Disp1";
                imgD.Index = 0;
                
                for (int i = 0; i < len; i++)
                {
                    unit[i] = new UnitDevice();
                    unit[i] = unit[i].Load(ofd.FileNames[i]) as UnitDevice;
                    int pathIndex = 1;
                    foreach (var img in unit[i].ImageScenarioItems)
                    {
                        foreach(var insp in img.InspScenarioItems)
                        {
                            bool bSlaveDefect = false;
                            if (insp.DefectScenarioItems.Count > 0)
                            {
                                foreach (var def in insp.DefectScenarioItems)
                                {
                                    if (def.MasterPath.Length > 0 && def.MasterPath != "")
                                    {
                                        string originId = def.Id;
                                        def.AlignId = "";
                                        def.TargetId = id;
                                        def.DisplayId = id == "Unit1.Camera1" ? "Disp1" : "Disp2";
                                        def.MasterPath = "";
                                        //def.ResultPath = def.ResultFileName;
                                        def.Index = defectIndex - 1;
                                        def.Id = string.Format("{0}.Defect{1}", "Unit1.Image1", defectIndex);
                                        string dispID = insp.DisplayId;
                                        string pcID = i == 0 ? "PC2" : "PC3";
                                        //def.ResultPath = string.Format(@"D:\SlavePC\{0}.Defect{1}_{2}_{3}", "Unit1.Image1", pathIndex, pcID, dispID);
                                        def.ResultPath = string.Format(@"D:\SlavePC\{0}_{1}_{2}", originId, pcID, dispID);
                                        def.UseCropPixelToGraphic = true;
                                        imgD.DefectScenarioItems.Add(def);

                                        bSlaveDefect = true;
                                    }
                                    defectIndex++;
                                    pathIndex++;
                                }
                            }

                            if (bSlaveDefect == true)
                            {
                                insp.TargetId = id;
                                insp.DisplayId = id == "Unit1.Camera1" ? "Disp1" : "Disp2";
                                insp.Index = inspectionIndex - 1;
                                insp.Id = string.Format("{0}.Inspection{1}", "Unit1.Image1", inspectionIndex);
                                insp.IsShowGraphics = true;
                                insp.SyncColor = true;
                                insp.TextGraphicMode = enTextGraphicMode.IMAGE;
                                imgD.InspScenarioItems.Add(insp);
                            }
                            inspectionIndex++;
                        }
                    }
                    //pc++;
                }
                //unitDevice.ImageScenarioItems.Add(device as ImageDevice);
                lstUnitDevice[0].ImageScenarioItems.Add(imgD as ImageDevice);
                UpdateTreeView();
            }
        }

        private void treeView_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
        }

        
        private void tbTreeViewFilter_TextChanged(object sender, EventArgs e)
        {//2023.01.26_1 kimgunss
            TreeViewFiltering();
        }

        private void TreeViewFiltering()
        {//2023.01.26_1 kimgun
            this.treeView.BeginUpdate();
            this.treeView.Nodes.Clear();
            if (this.tbTreeViewFilter.Text != string.Empty)
            {
                IsFilter = true;
                combUnit.SelectedIndex = 0;
                combUnit.Enabled = false;
                combDevice.SelectedIndex = 0;
                combDevice.Enabled = false;
                TreeNodeFiltering(TreeViewCache.Nodes);
            }
            else if (this.combUnit.SelectedIndex > 0 || this.combDevice.SelectedIndex > 0)
            {
                IsComboFilter = true;
                tbTreeViewFilter.Enabled = false;
                
                TreeNodeCombFiltering(TreeViewCache.Nodes);
            }
            else
            {
                foreach (TreeNode _node in this.TreeViewCache.Nodes)
                {
                    treeView.Nodes.Add((TreeNode)_node.Clone());
                }
                combUnit.Enabled = true;
                combDevice.Enabled = true;
                tbTreeViewFilter.Enabled = true;
                IsComboFilter = false;
                IsFilter = false;
            }
            //enables redrawing tree after all objects have been added
            this.treeView.EndUpdate();
        }
        private void TreeNodeFiltering(TreeNodeCollection treeNodes)
        {//2023.01.26_1 kimgun
            if (treeNodes.Count == 0) return;
            foreach (TreeNode node in treeNodes)
            {
                TreeNodeFiltering(node.Nodes);
                if (node.Text.StartsWith(this.tbTreeViewFilter.Text))
                {
                    this.treeView.Nodes.Add((TreeNode)node.Clone());
                    IsFilter = true;
                }
            }
        }
        private void TreeNodeCombFiltering(TreeNodeCollection treeNodes)
        {//2023.01.26_1 kimgun
            if (treeNodes.Count == 0) return;        
            foreach (TreeNode node in treeNodes)
            {
                string unit = (string)combUnit.SelectedItem;
                if (unit != "All" && node.Name.Contains(unit) == false) continue;
                string key = (string)combDevice.SelectedItem;
                bool isPathCheck = false;
                if (key == "Align" || key == "Inspection" || key == "Defect"|| key == "Image")
                    isPathCheck = true;
                string[] devices = node.Name.Split('.');
                int level = node.Level;
                if ((string)combDevice.SelectedItem == "All"|| combDevice.SelectedItem == null)
                {
                    this.treeView.Nodes.Add((TreeNode)node.Clone());
                    continue;
                }
                else if (isPathCheck && devices.Length > level && devices[level].Contains(key) && level - 1 >= 0 && devices[level - 1].Contains("Image")) 
                {
                    var a = treeView.Nodes.Find(node.Name, false);
                    if(a.Length == 0)
                    this.treeView.Nodes.Add((TreeNode)node.Clone());
                    continue;
                }
                else //if (node.Name.Contains((string)combDevice.SelectedItem) && (isPathCheck == false || node.FullPath.Contains("Image") || (node.Parent != null && node.Parent.FullPath.Contains("Image"))))//.Text.StartsWith(this.tbTreeViewFilter.Text)))
                {
                    bool bAllow = node.Name.Contains((string)combDevice.SelectedItem);
                    bool bAllow2 = isPathCheck == false;
                    bAllow2 |= node.FullPath.Contains("Image");
                    bAllow2 |= node.Parent != null && node.Parent.FullPath.Contains("Image");
                    if (bAllow && bAllow2)
                    {
                        var a = treeView.Nodes.Find(node.Name, false);
                        if (a.Length == 0)
                            this.treeView.Nodes.Add((TreeNode)node.Clone());
                        continue;
                    }
                }
                TreeNodeCombFiltering(node.Nodes);
            }
        }
        private void combUnit_SelectedIndexChanged(object sender, EventArgs e)
        {//2023.01.26_1 kimgun
            TreeViewFiltering();
        }

        private void combDevice_SelectedIndexChanged(object sender, EventArgs e)
        {//2023.01.26_1 kimgun
            TreeViewFiltering();
        }

        private void btnFilterClear_Click(object sender, EventArgs e)
        {//2023.01.29_1 kimgun
            combDevice.SelectedIndex = 0;
            combUnit.SelectedIndex = 0;
            tbTreeViewFilter.Text = "";
        }

    }
}
