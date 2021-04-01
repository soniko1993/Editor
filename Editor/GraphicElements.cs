using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Editor
{
    [Serializable]
    struct MyColor
    {
        public int r, g, b, a;
    }
    [Serializable]
    struct WorkSpaceObjects
    {
        public List<string> ShapeType;
        public List<double> rotateAngle;
        public List<MyColor> BorderBrush;
        public List<MyColor> FillBrush;
        public List<double> Thickness;
        public List<Rect> Position;
        public List<double> Height;
        public List<double> Width;
        public List<List<Point>> PolylinePoints;
    }
    class GraphicElements
    {
        private Canvas WorkSpace;
        public GraphicElements(Canvas workSpace)
        {
            WorkSpace = workSpace;
        }

        public Rectangle AddRectangle(Rect rect, SolidColorBrush BorderBrush, double thickness)
        {
            TransformGroup tg = new TransformGroup();
            Rectangle rectangle = new Rectangle();
            rectangle.Height = rect.Height;
            rectangle.Width = rect.Width;
            rectangle.Stroke = BorderBrush;
            rectangle.Fill = Brushes.Transparent;
            rectangle.StrokeThickness = thickness;
            rectangle.RenderTransform = tg;
            SetPosition(rectangle, rect.X, rect.Y);
            DrawFigure(rectangle);
 
            return rectangle;
        }
        public Rectangle AddRectangle(Rect rect, SolidColorBrush BorderBrush, double thickness, double angleRot)
        {
            TransformGroup tg = new TransformGroup();
            Rectangle rectangle = new Rectangle();
            rectangle.Height = rect.Height;
            rectangle.Width = rect.Width;
            rectangle.Stroke = BorderBrush;
            rectangle.Fill = Brushes.Transparent;
            rectangle.StrokeThickness = thickness;
            rectangle.RenderTransform = tg;
            SetPosition(rectangle, rect.X, rect.Y);
            Rotate(rectangle, angleRot);        
            DrawFigure(rectangle);

           
           
            return rectangle;
        }
        public Polyline AddLine(Point mp, SolidColorBrush BorderBrush, double thickness)
        {
            TransformGroup tg = new TransformGroup();
            Polyline polyline = new Polyline(); 
            polyline.Stroke = BorderBrush;
            polyline.StrokeThickness = thickness;
            polyline.RenderTransform = tg;
            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(mp);
            polyline.Points = myPointCollection;
            DrawFigure(polyline);
            return polyline;
        }

        public Polyline AddPointToLine(Shape shape, Point mp)
        {
            Polyline polyline = (Polyline)shape;
            polyline.Points.Add(mp);
            return polyline;
        }
        public Polyline InsertPointIntoLine(Shape shape, Point mp)
        {
            Polyline polyline = (Polyline)shape;
            for (int i = 0; i < polyline.Points.Count-1; i++)
            {
                if (CheckIntersection(polyline.Points[i], polyline.Points[i + 1], mp, 0.01,false))
                {
                    polyline.Points.Insert(i+1, mp);
                    i = polyline.Points.Count;
                }
            }
            return polyline;
        }
        public void Fill(Shape shape, Color color)
        {
            if (shape.GetType() == typeof(Rectangle))
            {
                SolidColorBrush mySolidColorBrush = new SolidColorBrush(color);
                shape.Fill = mySolidColorBrush;
            }
        }

        public void FillBorder(Shape shape, Color color, double thickness)
        {
            SolidColorBrush mySolidColorBrush = new SolidColorBrush(color);
            shape.Stroke = mySolidColorBrush;
            shape.StrokeThickness = thickness;
        }

        public Point GetOffset(Shape shape)
        {
            Point pt=new Point(0,0);
            TransformGroup myTransformGroup = shape.RenderTransform as TransformGroup;
            foreach (Transform t in myTransformGroup.Children)
                if (t is TranslateTransform transform)
                {
                    pt.X += transform.X;
                    pt.Y += transform.Y;
                }

            return pt;
        }
        public void Rotate(Shape shape, double angle)
        {
            if (shape is Rectangle)
            {
                TransformGroup myTransformGroup = shape.RenderTransform as TransformGroup;
                Rect r= GetPosition(shape);
                Point Center = new Point(r.X + r.Width / 2, r.Y + r.Height / 2);
                //foreach (Transform t in myTransformGroup.Children)
                //    if (t is TranslateTransform)
                //    {
                //        offset.X += ((TranslateTransform)t).X;
                //        offset.Y += ((TranslateTransform)t).Y;
                //    }
                // myTransformGroup.Children.Add(new RotateTransform(angle, offset.X + shape.Width / 2, offset.Y + shape.Height / 2));
                myTransformGroup.Children.Add(new RotateTransform(angle, Center.X, Center.Y));
            }               
        }

        //Intersection of Line (x1,x2) with Point x
        public bool CheckIntersection(Point x1, Point x2, Point x, double GAP, bool isrect)
        {
            double dx1 = x2.X - x1.X;
            double dy1 = x2.Y - x1.Y;

            double dx = x.X - x1.X;
            double dy = x.Y - x1.Y;
            double s = dx / dx1 - dy / dy1;
            if (isrect==true)
            {
                if ((s >= -1 && s <= 1) || (x1.X == x2.X && Math.Abs(dx) < GAP) || (x1.Y == x2.Y && Math.Abs(dy) < GAP))
                {
                    return true;
                }
                else
                    return false;
            }
            else
            {
                if (s >= -0.1 && s <= 0.1)
                {
                    return true;
                }
                else
                    return false;
            }

            
        }
        public void MovePolylineNode(Shape shape, Point mp, Point mousePosition)
        {
            Polyline polyline = (Polyline)shape;
            Point tp;
            for (int i=0; i< polyline.Points.Count; i++)
            {
                tp = polyline.Points[i];
                double dist = Math.Sqrt(Math.Pow(mousePosition.X - tp.X, 2) + Math.Pow(mousePosition.Y - tp.Y, 2));
                if (dist > 0 && dist < shape.StrokeThickness)
                {
                    polyline.Points[i] = mp;
                    i = polyline.Points.Count;
                }                   
            }
        }

        public void UnionPoilylinePoint(Shape shape, Point mp)
        {
            Polyline polyline = (Polyline)shape;
            Point tp, tp1;
            for (int i = 0; i < polyline.Points.Count-1; i++)
            {
                tp = polyline.Points[i];
                tp1 = polyline.Points[i + 1];
                double dist = Math.Sqrt(Math.Pow(tp1.X - tp.X, 2) + Math.Pow(tp1.Y - tp.Y, 2));
                if (dist >= 0 && dist <= 20)
                {
                    polyline.Points.RemoveAt(i);
                    i = polyline.Points.Count;
                }
            }
        }
    
        public void Resize(Shape shape, Rect rect)
        {         
            shape.Height = rect.Height;
            shape.Width = rect.Width;
            TransformGroup myTransformGroup = shape.RenderTransform as TransformGroup;
            TransformGroup newTransformGroup = new TransformGroup();
            newTransformGroup.Children.Add(new TranslateTransform(rect.X, rect.Y));
            for (int i = 0; i < myTransformGroup.Children.Count; i++)
                if (myTransformGroup.Children[i] is not TranslateTransform)
                {
                    newTransformGroup.Children.Add(myTransformGroup.Children[i]);
                }

            shape.RenderTransform = newTransformGroup;
           // shape.RenderTransform = new TranslateTransform(rect.X, rect.Y);

        }
        private void DrawFigure(Shape shape)
        {         
            Panel.SetZIndex(shape, 0);
            WorkSpace.Children.Add(shape); 
        }
        public void DeleteFigure(Shape figure)
        {
            WorkSpace.Children.Remove(figure);
        }

        public void SetPosition(Shape shape, double left, double top)
        {
            TransformGroup myTransformGroup = shape.RenderTransform as TransformGroup;
            TranslateTransform tt = new TranslateTransform(left, top);
            if (myTransformGroup != null)
            {              
                if (shape.GetType() == typeof(Polyline))
                {
                    for (int i = 0; i < ((Polyline)shape).Points.Count; i++)
                    {
                        ((Polyline)shape).Points[i] = tt.Transform(((Polyline)shape).Points[i]);
                    }
                }
                else
                {
                    myTransformGroup.Children.Add(tt);
                    shape.RenderTransform = myTransformGroup;
                }
           }            
        }
        public Rect GetPosition(Shape shape)
        {
            Rect r=new Rect(0,0,0,0);
            TransformGroup myTransformGroup = shape.RenderTransform as TransformGroup;
            double offsetX = 0, offsetY = 0;
            if(myTransformGroup!=null)
            {
                foreach (Transform t in myTransformGroup.Children)
                    if (t is TranslateTransform)
                    {
                        offsetX += ((TranslateTransform)t).X;
                        offsetY += ((TranslateTransform)t).Y;
                    }
            }
            Point myShapePosition = new Point(offsetX, offsetY);

            Size mySize = new(shape.Width, shape.Height);
            r = new Rect(myShapePosition, mySize);
            return r;
        }

        public double GetRotationAngle(Shape shape)
        {
            double angle = 0;
            if (shape.RenderTransform is TransformGroup)
            {
                TransformGroup tg = (TransformGroup)shape.RenderTransform;
                foreach (Transform tr in tg.Children)
                    if (tr is RotateTransform)
                    {
                        RotateTransform r = tr as RotateTransform;
                        angle += r.Angle;
                    }
            }
            else if (shape.RenderTransform is RotateTransform)
            {
                angle = ((RotateTransform)shape.RenderTransform).Angle;
            }
            return angle;
        }

        public void SaveToFile(string path)
        {
            WorkSpaceObjects wso = new WorkSpaceObjects();
            wso.BorderBrush = new List<MyColor>();
            wso.Height = new List<double>();
            wso.Width = new List<double>();
            wso.rotateAngle = new List<double>();
            wso.FillBrush = new List<MyColor>();
            wso.Position = new List<Rect>();
            wso.PolylinePoints = new List<List<Point>>();
            wso.Thickness = new List<double>();
            wso.ShapeType = new List<string>();
            try
            {
                for(int i=0; i< WorkSpace.Children.Count;i++)
                {
                    Shape currentShape = (Shape)WorkSpace.Children[i];
                    string t;
                    SolidColorBrush b;
                    Color color;
                    MyColor myColor;
                    if (currentShape.GetType()==typeof(Rectangle))
                    {
                        t = typeof(Rectangle).ToString();
                        wso.ShapeType.Add(t);
                        wso.Height.Add(currentShape.Height);
                        wso.Width.Add(currentShape.Width);
                        wso.PolylinePoints.Add(null);
                        wso.Thickness.Add(currentShape.StrokeThickness);

                        double angle = GetRotationAngle(currentShape);

                        wso.rotateAngle.Add(GetRotationAngle(currentShape));
                        Rotate(currentShape, -angle);
                        wso.Position.Add(GetPosition(currentShape));
                        Rotate(currentShape, angle);
                        b = currentShape.Stroke as SolidColorBrush;
                        color = b.Color;
                        myColor = new MyColor();
                        myColor.r = color.R; myColor.g = color.G; myColor.b = color.B; myColor.a = color.A;
                        wso.BorderBrush.Add(myColor);
                        
                        b = currentShape.Fill as SolidColorBrush;
                        color = b.Color;
                        myColor = new MyColor();
                        myColor.r = color.R; myColor.g = color.G; myColor.b = color.B; myColor.a = color.A;
                        wso.FillBrush.Add(myColor);
                    }
                    if(currentShape.GetType() == typeof(Polyline))
                    {
                        t = typeof(Polyline).ToString();
                        wso.ShapeType.Add(t);
                        wso.Height.Add(currentShape.Height);
                        wso.Width.Add(currentShape.Width);
                        wso.rotateAngle.Add(0);                       
                        List<Point> pp = new List<Point>();
                        for (int j = 0; j < ((Polyline)currentShape).Points.Count; j++)
                        {
                            pp.Add(((Polyline)currentShape).Points[j]);
                        }
                        wso.PolylinePoints.Add(pp);
                        wso.Thickness.Add(currentShape.StrokeThickness);
                        wso.Position.Add(new Rect(0,0,0,0));
                        b = currentShape.Stroke as SolidColorBrush;

                        color = b.Color;
                        myColor = new MyColor();
                        myColor.r = color.R; myColor.g = color.G; myColor.b = color.B; myColor.a = color.A;
                        wso.BorderBrush.Add(myColor);

                        color = Colors.Transparent;              
                        myColor = new MyColor();
                        myColor.r = color.R; myColor.g = color.G; myColor.b = color.B; myColor.a = color.A;
                        wso.FillBrush.Add(myColor);
                    }                
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
            FileStream fs = new FileStream(path, FileMode.Create);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                //Новый стандарт для защиты от уязвимости рекомендует сериализацию только  в xml и json. 
#pragma warning disable SYSLIB0011
                formatter.Serialize(fs, wso);
                fs.Close();
#pragma warning restore SYSLIB0011
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void LoadFromFile(string path)
        {         
            BinaryFormatter formatter = new BinaryFormatter();
            WorkSpaceObjects wso = new WorkSpaceObjects();
            wso.BorderBrush = new List<MyColor>();
            wso.Height = new List<double>();
            wso.Width = new List<double>();
            wso.FillBrush = new List<MyColor>();
            wso.Position = new List<Rect>();
            wso.PolylinePoints = new List<List<Point>>();
            wso.Thickness = new List<double>();
            wso.ShapeType = new List<string>();
            try
            {
                FileStream fs = new FileStream(path, FileMode.Open);
#pragma warning disable SYSLIB0011
                wso = (WorkSpaceObjects)formatter.Deserialize(fs);
                fs.Close();
#pragma warning restore SYSLIB0011
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            try
            {
                for (int i = 0; i < wso.ShapeType.Count; i++)
                {
                    Color color=new Color();
                    if (wso.ShapeType[i].CompareTo(typeof(Rectangle).ToString())==0)
                    {
                        color = new Color();
                        color.R = (byte)wso.BorderBrush[i].r; color.G = (byte)wso.BorderBrush[i].g; color.B = (byte)wso.BorderBrush[i].b; color.A = (byte)wso.BorderBrush[i].a;
                        Rectangle rect = AddRectangle(wso.Position[i], new SolidColorBrush(color), wso.Thickness[i]);
                        Rotate(rect, wso.rotateAngle[i]);
                        color = new Color();
                        color.R = (byte)wso.FillBrush[i].r; color.G = (byte)wso.FillBrush[i].g; color.B = (byte)wso.FillBrush[i].b; color.A = (byte)wso.FillBrush[i].a;
                        Fill(rect, color);                      
                    }
                    
                    if (wso.ShapeType[i].CompareTo(typeof(Polyline).ToString()) == 0)
                    {
                        color = new Color();
                        color.R = (byte)wso.BorderBrush[i].r; color.G = (byte)wso.BorderBrush[i].g; color.B = (byte)wso.BorderBrush[i].b; color.A = (byte)wso.BorderBrush[i].a;
                        Polyline polyline = AddLine(wso.PolylinePoints[i][0], new SolidColorBrush(color), wso.Thickness[i]);
                        for (int j = 1; j < wso.PolylinePoints[i].Count; j++)
                            AddPointToLine(polyline, wso.PolylinePoints[i][j]);
                    }
                }
                WorkSpace.InvalidateMeasure();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }



        }
    }
}
