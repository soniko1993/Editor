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
        public List<string> shapeType;
        public List<double> rotateAngle;
        public List<MyColor> borderBrush;
        public List<MyColor> fillBrush;
        public List<double> thickness;
        public List<Rect> position;
        public List<double> height;
        public List<double> width;
        public List<List<Point>> polylinePoints;
    }
    class GraphicElements
    {
        private Canvas WorkSpace;
        public GraphicElements(Canvas workSpace)
        {
            WorkSpace = workSpace;
        }

        //Создание прямоугольника
        public Rectangle CreateRectangle(Rect rect, SolidColorBrush BorderBrush, double thickness)
        {
            TransformGroup transformGroup = new();
            Rectangle rectangle = new()
            {
                Height = rect.Height,
                Width = rect.Width,
                Stroke = BorderBrush,
                Fill = Brushes.Transparent,
                StrokeThickness = thickness,
                RenderTransform = transformGroup
            };
            Reposition(rectangle, rect.X, rect.Y);
            DrawFigure(rectangle);

            return rectangle;
        }

        //Создание прямоугольника с вращением
        public Rectangle CreateRectangle(Rect rect, SolidColorBrush BorderBrush, double thickness, double angleRot)
        {
            TransformGroup transformGroup = new();
            Rectangle rectangle = new()
            {
                Height = rect.Height,
                Width = rect.Width,
                Stroke = BorderBrush,
                Fill = Brushes.Transparent,
                StrokeThickness = thickness,
                RenderTransform = transformGroup
            };
            Reposition(rectangle, rect.X, rect.Y);
            Rotate(rectangle, angleRot);
            DrawFigure(rectangle);
            return rectangle;
        }

        //Создание линии с изломами (первая точка)
        public Polyline AddLine(Point mp, SolidColorBrush BorderBrush, double thickness)
        {
            TransformGroup transformGroup = new();
            Polyline polyline = new()
            {
                Stroke = BorderBrush,
                StrokeThickness = thickness,
                RenderTransform = transformGroup
            };
            PointCollection myPointCollection = new PointCollection
            {
                mp
            };
            polyline.Points = myPointCollection;
            DrawFigure(polyline);
            return polyline;
        }

        //Добавление точек в линию с изломами
        public Polyline AddPointToLine(Shape shape, Point mousePosition)
        {
            Polyline polyline = (Polyline)shape;
            polyline.Points.Add(mousePosition);
            return polyline;
        }

        //Встав
        public Polyline InsertPointIntoLine(Shape shape, Point mousePosition)
        {
            Polyline polyline = (Polyline)shape;
            for (int i = 0; i < polyline.Points.Count - 1; i++)
            {
                if (CheckIntersection(polyline.Points[i], polyline.Points[i + 1], mousePosition, 0.01, false))
                {
                    polyline.Points.Insert(i + 1, mousePosition);
                    i = polyline.Points.Count;
                }
            }
            return polyline;
        }

        //Заливка примитивов
        public static void Fill(Shape shape, Color color)
        {
            if (shape.GetType() == typeof(Rectangle))
            {
                SolidColorBrush mySolidColorBrush = new SolidColorBrush(color);
                shape.Fill = mySolidColorBrush;
            }
        }

        //Заливка границ примитивов
        public static void FillBorder(Shape shape, Color color, double thickness)
        {
            SolidColorBrush mySolidColorBrush = new(color);
            shape.Stroke = mySolidColorBrush;
            shape.StrokeThickness = thickness;
        }


        //Получит сдвиг относительно начала координат
        public static Point GetOffset(Shape shape)
        {
            Point pt = new(0, 0);
            TransformGroup myTransformGroup = shape.RenderTransform as TransformGroup;
            foreach (Transform t in myTransformGroup.Children)
                if (t is TranslateTransform transform)
                {
                    pt.X += transform.X;
                    pt.Y += transform.Y;
                }

            return pt;
        }

        //получить центр фигуры
        public static Point GetCenter(Rect rect)
        {
            return new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        }
            

        //Вращение примитивов
        public static void Rotate(Shape shape, double angle)
        {
            if (shape is Rectangle)
            {
                TransformGroup myTransformGroup = shape.RenderTransform as TransformGroup;
                Rect rect= GetPosition(shape);
                Point Center = GetCenter(rect);
                myTransformGroup.Children.Add(new RotateTransform(angle, Center.X, Center.Y));
            }               
        }

        //Intersection of Line (x1,x2) with Point x
        public static bool CheckIntersection(Point x1, Point x2, Point x, double GAP, bool isrect)
        {
            double dx1 = x2.X - x1.X;
            double dy1 = x2.Y - x1.Y;

            double dx = x.X - x1.X;
            double dy = x.Y - x1.Y;
            double s = dx / dx1 - dy / dy1;
            if (isrect==true)
            {
                if ((s >= -1 && s <= 1) || (x1.X == x2.X && Math.Abs(dx) < GAP) || (x1.Y == x2.Y && Math.Abs(dy) < GAP))               
                    return true;              
                else
                    return false;
            }
            else
            {
                if (s >= -0.1 && s <= 0.1)              
                   return true;               
                else
                    return false;
            }         
        }

        //Расстояние между двумя точкмаи
        private double GetDistance(Point x1, Point x2)
        {
            return Math.Sqrt(Math.Pow(x1.X - x2.X, 2) + Math.Pow(x1.Y - x2.Y, 2));
        }

        //перемещение точек ищлома
        public void MovePolylineNode(Shape shape, Point currentMousePosition, Point mousePosition)
        {
            Polyline polyline = (Polyline)shape;
            Point currentPoint;
            for (int i=0; i< polyline.Points.Count; i++)
            {
                currentPoint = polyline.Points[i];
                double dist = GetDistance(mousePosition, currentPoint);
                if (dist > 0 && dist < shape.StrokeThickness)
                {
                    polyline.Points[i] = currentMousePosition;
                    i = polyline.Points.Count;
                }                   
            }
        }

        //Объединение точек излома на линии
        public void UnionPolylinePoint(Shape shape)
        {
            Polyline polyline = (Polyline)shape;
            Point point1, point2;
            for (int i = 0; i < polyline.Points.Count-1; i++)
            {
                point1 = polyline.Points[i];
                point2 = polyline.Points[i + 1];
                double dist = GetDistance(point2, point1);
                if (dist >= 0 && dist <= 20)
                {
                    polyline.Points.RemoveAt(i);
                    i = polyline.Points.Count;
                }
            }
        }
    

        //Изменение размеров примитивов
        public static void Resize(Shape shape, Rect rect)
        {
            if (shape.GetType() == typeof(Rectangle))
            {
                double angle = GetRotationAngle(shape);
                shape.Height = rect.Height;
                shape.Width = rect.Width;
                TransformGroup newTransformGroup = new();
                Point Center = GetCenter(rect);
                newTransformGroup.Children.Add(new TranslateTransform(rect.X, rect.Y));
                newTransformGroup.Children.Add(new RotateTransform(angle, Center.X, Center.Y));
                shape.RenderTransform = newTransformGroup;
            }
            
        }

        //Добавление примитивов в Canvas
        private void DrawFigure(Shape shape)
        {         
            Panel.SetZIndex(shape, 0);
            WorkSpace.Children.Add(shape); 
        }

        //Добавление примитивов из Canvas
        public void DeleteFigure(Shape figure)
        {
            WorkSpace.Children.Remove(figure);
        }

        //Сдвиг примитивов
        public static void Reposition(Shape shape, double leftShift, double topShift)
        {
            TranslateTransform translateTransform = new(leftShift, topShift);
            if (shape.RenderTransform is TransformGroup myTransformGroup)
            {              
                if (shape.GetType() == typeof(Polyline))
                {
                    for (int i = 0; i < ((Polyline)shape).Points.Count; i++)
                    {
                        ((Polyline)shape).Points[i] = translateTransform.Transform(((Polyline)shape).Points[i]);
                    }
                }
                else if(shape.GetType() == typeof(Rectangle))
                {
                    myTransformGroup.Children.Add(translateTransform);
                    shape.RenderTransform = myTransformGroup;
                }
           }            
        }
        //Получить координаты примитива
        public static Rect GetPosition(Shape shape)
        {
            Rect rect;
            double offsetX = 0, offsetY = 0;                
            if (shape.RenderTransform is TransformGroup myTransformGroup)
            {
                foreach (Transform t in myTransformGroup.Children)
                    if (t is TranslateTransform transltaeTransform)
                    {
                        offsetX += transltaeTransform.X;
                        offsetY += transltaeTransform.Y;
                    }
            }
            Point myShapePosition = new(offsetX, offsetY);
            Size mySize = new(shape.Width, shape.Height);
            rect = new Rect(myShapePosition, mySize);
            return rect;
        }

        //Получить угол поворота примитива
        public static double GetRotationAngle(Shape shape)
        {
            double angle = 0;
            if (shape.RenderTransform is TransformGroup transformGroup)
            {
                foreach (Transform transform in transformGroup.Children)
                    if (transform is RotateTransform)
                    {
                        RotateTransform rotateTransform = transform as RotateTransform;
                        angle += rotateTransform.Angle;
                    }
            }
            else if (shape.RenderTransform is RotateTransform)
            {
                angle = ((RotateTransform)shape.RenderTransform).Angle;
            }
            return angle;
        }

        //Объявление переменных структуры WorkSpaceObject
        private static WorkSpaceObjects CreateWorkSpaceObject()
        {
            WorkSpaceObjects workSpaceObject = new();
            workSpaceObject.borderBrush = new List<MyColor>();
            workSpaceObject.height = new List<double>();
            workSpaceObject.width = new List<double>();
            workSpaceObject.rotateAngle = new List<double>();
            workSpaceObject.fillBrush = new List<MyColor>();
            workSpaceObject.position = new List<Rect>();
            workSpaceObject.polylinePoints = new List<List<Point>>();
            workSpaceObject.thickness = new List<double>();
            workSpaceObject.shapeType = new List<string>();
            return workSpaceObject;
        }
        //Преобразование цвета в формат MyColor
        private static MyColor SetMyColor(Color color)
        {
            MyColor myColor = new MyColor
            {
                r = color.R,
                g = color.G,
                b = color.B,
                a = color.A
            };
            return myColor;
        }
        //Заполнение структуры WorkSpaceObject
        private static void AddValuesToWorkSpaceObject(WorkSpaceObjects workSpaceObject, Shape currentShape)
        {
            string figureTypeString;
            SolidColorBrush brush;
            MyColor myColor;
            workSpaceObject.height.Add(currentShape.Height);
            workSpaceObject.width.Add(currentShape.Width);
            workSpaceObject.thickness.Add(currentShape.StrokeThickness);
            brush = currentShape.Stroke as SolidColorBrush;
            myColor = SetMyColor(brush.Color);
            workSpaceObject.borderBrush.Add(myColor);

            if (currentShape.GetType() == typeof(Rectangle))
            {
                figureTypeString = typeof(Rectangle).ToString();
                workSpaceObject.shapeType.Add(figureTypeString);              
                workSpaceObject.polylinePoints.Add(null);
                double angle = GetRotationAngle(currentShape);
                workSpaceObject.rotateAngle.Add(angle);

                workSpaceObject.position.Add(GetPosition(currentShape));

                brush = currentShape.Fill as SolidColorBrush;
                myColor = SetMyColor(brush.Color);
                workSpaceObject.fillBrush.Add(myColor);
            }
            else if (currentShape.GetType() == typeof(Polyline))
            {
                figureTypeString = typeof(Polyline).ToString();
                workSpaceObject.shapeType.Add(figureTypeString);
                workSpaceObject.rotateAngle.Add(0);
                List<Point> polylinePoints = new();
                for (int j = 0; j < ((Polyline)currentShape).Points.Count; j++)
                {
                    polylinePoints.Add(((Polyline)currentShape).Points[j]);
                }
                workSpaceObject.polylinePoints.Add(polylinePoints);
                workSpaceObject.position.Add(new Rect(0, 0, 0, 0));
                myColor = SetMyColor(Colors.Transparent);
                workSpaceObject.fillBrush.Add(myColor);
            }

        }

        //Сохранение рабочей области в файл
        public void SaveToFile(string path)
        {
            WorkSpaceObjects workSpaceObject = CreateWorkSpaceObject();
            try
            {
                for(int i=0; i<WorkSpace.Children.Count; i++)
                {
                    AddValuesToWorkSpaceObject(workSpaceObject, WorkSpace.Children[i] as Shape);    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
            FileStream fs = new(path, FileMode.Create);
            try
            {
                BinaryFormatter formatter = new();
                //Новый стандарт для защиты от уязвимости рекомендует сериализацию только  в xml и json. 
#pragma warning disable SYSLIB0011
                formatter.Serialize(fs, workSpaceObject);
                fs.Close();
#pragma warning restore SYSLIB0011
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //Преобразование цвета в формат Color
        private Color SetColor(MyColor myColor)
        {
            Color color = new()
            {
                R = (byte)myColor.r,
                G = (byte)myColor.g,
                B = (byte)myColor.b,
                A = (byte)myColor.a
            };
            return color;
        }



        //Добавление примитивов в рабочую область из WorkSpaceObjects
        private void DrawFromFile(WorkSpaceObjects workSpaceObject)
        {
            for (int i = 0; i < workSpaceObject.shapeType.Count; i++)
            {
                Color color;
                if (workSpaceObject.shapeType[i].CompareTo(typeof(Rectangle).ToString()) == 0)
                {
                    color = SetColor(workSpaceObject.borderBrush[i]);

                    Rectangle rect = CreateRectangle(workSpaceObject.position[i], new SolidColorBrush(color), workSpaceObject.thickness[i]);
                    Rotate(rect, workSpaceObject.rotateAngle[i]);
                    color = SetColor(workSpaceObject.fillBrush[i]);
                    Fill(rect, color);
                }
                else if (workSpaceObject.shapeType[i].CompareTo(typeof(Polyline).ToString()) == 0)
                {
                    color = SetColor(workSpaceObject.borderBrush[i]);
                    Polyline polyline = AddLine(workSpaceObject.polylinePoints[i][0], new SolidColorBrush(color), workSpaceObject.thickness[i]);
                    for (int j = 1; j < workSpaceObject.polylinePoints[i].Count; j++)
                        AddPointToLine(polyline, workSpaceObject.polylinePoints[i][j]);
                }
            }
            WorkSpace.InvalidateMeasure();
        }

        //Загрузка примитивов из файла
        public void LoadFromFile(string path)
        {         
            BinaryFormatter formatter = new();
            WorkSpaceObjects workSpaceObject = CreateWorkSpaceObject();
            try
            {
                FileStream fs = new FileStream(path, FileMode.Open);
#pragma warning disable SYSLIB0011
                workSpaceObject = (WorkSpaceObjects)formatter.Deserialize(fs);
                fs.Close();
#pragma warning restore SYSLIB0011
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            //Добавление примитивов на рабочую область
            try
            {
                DrawFromFile(workSpaceObject);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
