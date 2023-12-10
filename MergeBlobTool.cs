using Emgu.CV;
using Emgu.CV.Cvb;
using sun.nio.fs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TCV;
using TCV.Blob;

namespace TCV_ToolBlob
{
    internal class MergeBlobTool
    {
        private int offsetX =100;
        private int offsetY = 100;
        private TRectAngle realRegion;
        public List<Rectangle> ListBoundingBox;
        public List<Rectangle> ListBoundingBox_Real;
        public List<int> ListArea;
        public List<PointF> ListCentroid;
        public List<PointF> ListCentroid_Real;
        public List<UserCvBlob> ListBlobInfor;
        public MergeBlobTool(int offsetX, int offsetY, TRectAngle realRegion)
        {
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.realRegion = realRegion;
        }
        public ToolBlobResult resultMergeBlob(ToolBlobResult blobResult)
        {
            //Initialize
            realRegion = new TRectAngle();
            ListBoundingBox = new List<Rectangle>();
            ListBoundingBox_Real = new List<Rectangle>();
            ListArea = new List<int>();
            ListCentroid = new List<PointF>();
            ListCentroid_Real = new List<PointF>();
            ListBlobInfor = new List<UserCvBlob>();


            // Completely Merged ListBlob in MergedBlobResult.ListBlobInfor
            ToolBlobResult MergedBlobResult = (ToolBlobResult)blobResult.Clone();
            MergedBlobResult.ListBlobInfor = MergeOverlappingBlobs(blobResult.ListBlobInfor);


            //Fill Other Informations using ListBlobInfor
            for (int i = 0; i < MergedBlobResult.ListBlobInfor.Count; i++)
            {
                ListBoundingBox.Add(MergedBlobResult.ListBlobInfor[i].BoundingBox);
                TRectAngle rect_real = new TRectAngle(ListBlobInfor[i].BoundingBox.X + realRegion.StartX, ListBlobInfor[i].BoundingBox.Y + realRegion.StartY, ListBlobInfor[i].BoundingBox.Width, ListBlobInfor[i].BoundingBox.Height);
                ListBoundingBox_Real.Add(rect_real.GetSystemRectangle());
                ListArea.Add(MergedBlobResult.ListBlobInfor[i].Area);
                ListCentroid.Add(MergedBlobResult.ListBlobInfor[i].Centroid);
                ListCentroid_Real.Add(new PointF((float)realRegion.StartX + (float)(ListBlobInfor[i].BlobMoments.M10 / ListBlobInfor[i].BlobMoments.M00), (float)realRegion.StartY + (float)(ListBlobInfor[i].BlobMoments.M01 / ListBlobInfor[i].BlobMoments.M00)));
                ListBlobInfor.Add(MergedBlobResult.ListBlobInfor[i]);
            }

            // return MergedBlobResult
            MergedBlobResult.ListBoundingBox = ListBoundingBox;
            MergedBlobResult.ListBoundingBox_Real = ListBoundingBox_Real;
            MergedBlobResult.ListArea = ListArea;
            MergedBlobResult.ListCentroid = ListCentroid;
            MergedBlobResult.ListCentroid_Real = ListCentroid_Real;
            MergedBlobResult.ListBlobInfor = ListBlobInfor;
            MergedBlobResult.nMinArea = MergedBlobResult.ListArea.Min();
            MergedBlobResult.nMaxArea = MergedBlobResult.ListArea.Max();
            MergedBlobResult.nTotalCount = MergedBlobResult.ListBlobInfor.Count;
            return MergedBlobResult;
        }

        


        private List<UserCvBlob> MergeOverlappingBlobs(List<UserCvBlob> blobList)
        {
            List<UserCvBlob> mergedBlobs = new List<UserCvBlob>();
            HashSet<UserCvBlob> visited = new HashSet<UserCvBlob>();

            foreach (var startBlob in blobList)
            {
                if (!visited.Contains(startBlob))
                {
                    List<UserCvBlob> connectedBlobs = DFS(startBlob, blobList, visited);

                    UserCvBlob mergedBlob = MergeBlobs(connectedBlobs);
                    mergedBlobs.Add(mergedBlob);
                }
            }
            //합쳐지지 않은 나머지 Blob도 결과 리스트에 추가
            foreach(var blob in blobList)
            {
                if (!visited.Contains(blob))
                {
                    mergedBlobs.Add(blob);
                }
            }
            return mergedBlobs;
        }
        private List<UserCvBlob> DFS(UserCvBlob startBlob, List<UserCvBlob> blobList, HashSet<UserCvBlob> visited)
        {
            List<UserCvBlob> connectedBlobs = new List<UserCvBlob>();
            Stack<UserCvBlob> stack = new Stack<UserCvBlob>();

            stack.Push(startBlob);
            visited.Add(startBlob);

            while(stack.Count > 0)
            {
                UserCvBlob currentBlob = stack.Pop();
                connectedBlobs.Add(currentBlob);

                foreach (var neighbor in blobList)
                {
                    if(!visited.Contains(neighbor) && currentBlob.BoundingBox.IntersectsWith(neighbor.BoundingBox))
                        {
                            stack.Push(neighbor);
                            visited.Add(neighbor);
                        }
                }
            }
            return connectedBlobs;
        }
        public UserCvBlob MergeBlobs(List<UserCvBlob> userCvBlobs)
        {
            //중심 좌표 평균화
            float mergedX = userCvBlobs.Average(x => x.Centroid.X);
            float mergedY = userCvBlobs.Average (y => y.Centroid.Y);
            //면적을 합친 면적으로 설정
            double mergedArea = userCvBlobs.Sum(x => x.Area);
            // 바운딩 박스를 확장하여 두 블랍을 모두 포함하도록 설정
            int mergedXMin = userCvBlobs[0].BoundingBox.X;
            int mergedYMin = userCvBlobs[0].BoundingBox.Y;
            int mergedXMax = userCvBlobs[0].BoundingBox.X + userCvBlobs[0].BoundingBox.Width;
            int mergedYMax = userCvBlobs[0].BoundingBox.Y + userCvBlobs[0].BoundingBox.Height;
            for(int i = 0; i < userCvBlobs.Count; i++)
            {
                mergedXMin = Math.Min(mergedXMin, userCvBlobs[i].BoundingBox.X);
                mergedYMin = Math.Min(mergedYMin, userCvBlobs[i].BoundingBox.Y);
                mergedXMax = Math.Max(mergedXMax, userCvBlobs[i].BoundingBox.X + userCvBlobs[i].BoundingBox.Width);
                mergedYMax = Math.Max(mergedYMax, userCvBlobs[i].BoundingBox.Y + userCvBlobs[i].BoundingBox.Height);
            }
            // 첫 번째 Blob의 Label 사용
            uint Label = userCvBlobs[0].Label;
            // BoundingBox 생성
            Rectangle BoundingBox = new Rectangle
            {
                X = mergedXMin,
                Y = mergedYMin,
                Width = mergedXMax - mergedXMin,
                Height = mergedYMax - mergedYMin
            };
            //blob moments --> 합친 모멘트는 새로 만들어야 하는 거라서 일단 blob1의 모멘트로 MCKXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            UserCvBlob.Moments Blobmoments = new UserCvBlob.Moments(BoundingBox, userCvBlobs[0].BlobMoments);
            //Centorid 생성
            PointF Centroid = new System.Drawing.PointF(mergedXMin, mergedYMin);
            // new Contours
            List<Point> Points = new List<Point>();
            for (int i = 0; i < userCvBlobs.Count; i++)
            {
                Points.AddRange(userCvBlobs[i].Contours);
            }
            Point[] Contour = Points.Distinct().ToArray();
            int len = userCvBlobs.Sum(x=>x.Contours.Length);
            // NEw MergedBlob
            UserCvBlob mergedBlob = new UserCvBlob(Label, BoundingBox, Blobmoments, Centroid, (int)mergedArea, Contour);
            return mergedBlob;
        }

        public UserCvBlob MergeBlobs(UserCvBlob blob1, UserCvBlob blob2)
        {        // 중심 좌표를 평균화
            float mergedX = (blob1.Centroid.X + blob2.Centroid.X) / 2;
            float mergedY = (blob1.Centroid.Y + blob2.Centroid.Y) / 2;

            // 면적을 합친 면적으로 설정
            double mergedArea = blob1.Area + blob2.Area;

            // 바운딩 박스를 확장하여 두 블랍을 모두 포함하도록 설정
            int mergedXMin = Math.Min(blob1.BoundingBox.X, blob2.BoundingBox.X);
            int mergedYMin = Math.Min(blob1.BoundingBox.Y, blob2.BoundingBox.Y);
            int mergedXMax = Math.Max(blob1.BoundingBox.X + blob1.BoundingBox.Width, blob2.BoundingBox.X + blob2.BoundingBox.Width);
            int mergedYMax = Math.Max(blob1.BoundingBox.Y + blob1.BoundingBox.Height, blob2.BoundingBox.Y + blob2.BoundingBox.Height);

            // 새로운 블랍 생성

            MergeCvBlob mergedBlob = new MergeCvBlob();
            //blob moments
            mergedBlob.Label = blob1.Label;
            mergedBlob.Centroid = new System.Drawing.PointF(mergedX, mergedY);
            mergedBlob.Area = mergedArea;
            //Contours 생성
            int len = blob1.Contours.Length + blob2.Contours.Length;
            Point[] Contours = new Point[len];
            blob1.Contours.CopyTo(Contours, 0);
            blob2.Contours.CopyTo(Contours, blob1.Contours.Length);
            mergedBlob.Contours = Contours;
            //바운딩 박스 생성
            mergedBlob.BoundingBox = new Rectangle
            {
                X = mergedXMin,
                Y = mergedYMin,
                Width = mergedXMax - mergedXMin,
                Height = mergedYMax - mergedYMin
            };
            //blob moments --> 합친 모멘트는 새로 만들어야 하는 거라서 일단 blob1의 모멘트로 MCKXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

            //머지블랍 사용
            mergedBlob.BlobMoments = new MergeCvBlob.Moments(mergedBlob.BoundingBox,blob1.BlobMoments);
            //유저블랍 사용
            UserCvBlob.Moments BlobMoments = new UserCvBlob.Moments(mergedBlob.BoundingBox,blob1.BlobMoments);

            //2023.12.08 MCK 
            UserCvBlob mergedBlob2 = new UserCvBlob(mergedBlob.Label,mergedBlob.BoundingBox, BlobMoments , mergedBlob.Centroid, (int)mergedArea, mergedBlob.Contours);

            return mergedBlob2;
        }

    }
        public class MergeCvBlob : UserCvBlob
        {
            #region Property
            //
            // 요약:
            //     Get the blob label
            public uint Label { get; set; }
            //
            // 요약:
            //     The minimum bounding box of the blob
            public Rectangle BoundingBox { get; set; }
            //
            // 요약:
            //     Get the Blob Moments
            public Moments BlobMoments { get; set; }
            //
            // 요약:
            //     The centroid of the blobF
            public PointF Centroid { get; set; }
            //
            // 요약:
            //     The number of pixels in this blob
            public double Area { get; set; }
            //
            // 요약:
            //     Get the contour that defines the blob
            //
            // 반환 값:
            //     The contour of the blob
            public Point[] Contours { get; set; }
            #endregion
            public struct Moments
            {
                public double W00;
                public double H00;
                //
                // 요약:
                //     Moment 00
                public double M00;
                //
                // 요약:
                //     Moment 10
                public double M10;
                //
                // 요약:
                //     Moment 01
                public double M01;
                //
                // 요약:
                //     Moment 11
                public double M11;
                //
                // 요약:
                //     Moment 20
                public double M20;
                //
                // 요약:
                //     Moment 02
                public double M02;
                //
                // 요약:
                //     Central moment 11
                public double U11;
                //
                // 요약:
                //     Central moment 20
                public double U20;
                //
                // 요약:
                //     Central moment 02
                public double U02;
                //
                // 요약:
                //     Normalized central moment 11
                public double N11;
                //
                // 요약:
                //     Normalized central moment 20
                public double N20;
                //
                // 요약:
                //     Normalized central moment 02
                public double N02;
                //
                // 요약:
                //     Hu moment 1
                public double P1;
                //
                // 요약:
                //     Hu moment 2
                public double P2;
                public Moments(Rectangle rect, CvBlob.Moments moments)
                {
                    W00 = rect.Width;
                    H00 = rect.Height;
                    M00 = moments.M00;
                    M10 = moments.M10;
                    M01 = moments.M01;
                    M11 = moments.M11;
                    M20 = moments.M20;
                    M02 = moments.M02;
                    U11 = moments.M11;
                    U20 = moments.M20;
                    U02 = moments.M02;
                    N11 = moments.M11;
                    N20 = moments.M20;
                    N02 = moments.N02;
                    P1 = moments.P1;
                    P2 = moments.P2;
                }
                public Moments(Rectangle rect, UserCvBlob.Moments moments)
            {
                W00 = rect.Width;
                H00 = rect.Height;
                M00 = moments.M00;
                M10 = moments.M10;
                M01 = moments.M01;
                M11 = moments.M11;
                M20 = moments.M20;
                M02 = moments.M02;
                U11 = moments.M11;
                U20 = moments.M20;
                U02 = moments.M02;
                N11 = moments.M11;
                N20 = moments.M20;
                N02 = moments.N02;
                P1 = moments.P1;
                P2 = moments.P2;
            }
        }

        }
        
        //2023.12.08 MCK 여기까지
    }

