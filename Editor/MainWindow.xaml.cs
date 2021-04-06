using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {      
        Point mousePosition;//местоположение мыши
        GraphicElements graphicElements;
        Shape currentFigure;//выбранный примитив

        
        //Тип нажатой кнопки
        private enum ButtonType
        {
           NoneB, rectB, lineB, fillB, deleteB, rotateB, moveB, scaleB, FillBorderB
        };
        ButtonType selectedAction=0;    
        int previousMouseState = 0; // 0 - nothing; 1- press; 2-up; Предыдущее состояние мыши
        int ClickCount = 0; // количество кликов
        double thickness;   //толщина границ примитивов
        HitType hitType;       //Местоположение границы при изменении размера
        bool polylinestate = false; //Статус добавление точек в линию с изломами
        public MainWindow()
        {
            InitializeComponent();
            thickness = Convert.ToDouble(FigureThickness.Text);
            graphicElements = new GraphicElements(WorkSpace);

        }        

        //Определение нажатой кнопки на панели инструментов
       void ButtonChecker(ButtonType buttonType)
        {
            selectedAction = buttonType;
            switch (buttonType)
            {
                case ButtonType.rectB: if (CreateRectangleButton.IsChecked == false) selectedAction = ButtonType.NoneB; break;
                case ButtonType.lineB: if (CreateLineButton.IsChecked == false) selectedAction = ButtonType.NoneB; break;
                case ButtonType.fillB: if (FillButton.IsChecked == false) selectedAction = ButtonType.NoneB; break;
                case ButtonType.deleteB:if (DeleteButton.IsChecked == false) selectedAction = ButtonType.NoneB; break;
                case ButtonType.rotateB:if (RotateButton.IsChecked == false) selectedAction = ButtonType.NoneB; break;
                case ButtonType.moveB: if (MoveButton.IsChecked == false) selectedAction = ButtonType.NoneB; break;
                case ButtonType.scaleB: if (ScaleButton.IsChecked == false) selectedAction = ButtonType.NoneB; break;
                case ButtonType.FillBorderB: if (FillBorderButton.IsChecked == false) selectedAction = ButtonType.NoneB; break;
            }
            if(buttonType!= ButtonType.rectB)
                CreateRectangleButton.IsChecked = false;
            if (buttonType != ButtonType.fillB)
                FillButton.IsChecked = false;
            if (buttonType != ButtonType.deleteB)
                DeleteButton.IsChecked = false;
            if (buttonType != ButtonType.lineB)
                CreateLineButton.IsChecked = false;
            if (buttonType != ButtonType.moveB)
                MoveButton.IsChecked = false;
            if (buttonType != ButtonType.rotateB)
                RotateButton.IsChecked = false;
            if (buttonType != ButtonType.scaleB)
                ScaleButton.IsChecked = false;
            if (buttonType != ButtonType.FillBorderB)
                FillBorderButton.IsChecked = false;
            ClickCount = 0;
            
        }
        private void CreateRectangleButton_Click(object sender, RoutedEventArgs e)
        {
            polylinestate = false;
            selectedAction = ButtonType.rectB;
            ButtonChecker(selectedAction);
        }

        private void CreateLineButton_Click(object sender, RoutedEventArgs e)
        {
            polylinestate = false;
            selectedAction = ButtonType.lineB;
            ButtonChecker(selectedAction);

        }

        private void FillButton_Click(object sender, RoutedEventArgs e)
        {
            polylinestate = false;
            selectedAction = ButtonType.fillB;
            ButtonChecker(selectedAction);

        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            polylinestate = false;
            selectedAction = ButtonType.deleteB;
            ButtonChecker(selectedAction);

        }
        private void RotateButton_Click(object sender, RoutedEventArgs e)
        {
            polylinestate = false;
            selectedAction = ButtonType.rotateB;
            ButtonChecker(selectedAction);
        }

        private void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            polylinestate = false;
            selectedAction = ButtonType.moveB;
            ButtonChecker(selectedAction);

        }
        private void ScaleButton_Click(object sender, RoutedEventArgs e)
        {
            polylinestate = false;
            selectedAction = ButtonType.scaleB;
           ButtonChecker(selectedAction);
        }

        private void FillBorderButton_Click(object sender, RoutedEventArgs e)
        {
            polylinestate = false;
            selectedAction = ButtonType.FillBorderB;
            ButtonChecker(selectedAction);
        }
        private void ColorPickerButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        //Обработка действий при нажатии на ЛКМ в рабочей области
        private void WorkSpace_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            thickness = Convert.ToDouble(FigureThickness.Text);
            mousePosition = Mouse.GetPosition(WorkSpace);
            Color color = ColorPickerButton.SelectedColor.Value;
            SolidColorBrush myBrush = new(color);
            IInputElement inputElement = Mouse.DirectlyOver;//Поиск объектов под мышью
            if (CreateLineButton.IsChecked==false)
            {
                if (inputElement.GetType() != typeof(Canvas))
                    currentFigure = (Shape)inputElement;
            }
            if(selectedAction==ButtonType.NoneB)
            {
                if (inputElement is Polyline && e.ClickCount==2)
                {
                    graphicElements.InsertPointIntoLine((Shape)inputElement, mousePosition);
                }
            }
            else if(selectedAction == ButtonType.rectB)
            {
                currentFigure = graphicElements.CreateRectangle(new Rect(mousePosition.X, mousePosition.Y, mousePosition.X, mousePosition.Y), myBrush, Convert.ToInt16(FigureThickness.Text));
            }
            else if (selectedAction == ButtonType.lineB)
            {
                if (polylinestate == false)
                {
                    if (inputElement is Polyline)
                    {
                        graphicElements.InsertPointIntoLine((Shape)inputElement, mousePosition);
                    }
                    else 
                    {
                        polylinestate = true;
                        currentFigure = graphicElements.AddLine(mousePosition, myBrush, thickness);
                    }
                }
                else
                {
                    if (currentFigure != null)
                    {
                        if (e.ClickCount == 2)
                        {
                            polylinestate = false;
                        }
                        else
                        {
                            graphicElements.AddPointToLine(currentFigure, mousePosition);
                        }
                    }

                }
            }
            else if (selectedAction == ButtonType.fillB)
            {
                if (inputElement is not Canvas) 
                    GraphicElements.Fill(currentFigure, ColorPickerButton.SelectedColor.Value);
            }
            else if (selectedAction == ButtonType.FillBorderB)
            {
                if (inputElement is not Canvas) 
                    GraphicElements.FillBorder(currentFigure, ColorPickerButton.SelectedColor.Value, thickness);
            }
            else if (selectedAction == ButtonType.deleteB)
            {
                if(inputElement is not Canvas)
                    graphicElements.DeleteFigure(currentFigure);
            }
            else if (selectedAction == ButtonType.moveB)
            {
                ClickCount = e.ClickCount;
            }
            else if (selectedAction == ButtonType.scaleB)
            {
                if (inputElement is not Canvas)
                    hitType = BorderCheck(currentFigure, mousePosition);
            }
            previousMouseState = 1;
        }

        //Обработка действий при отжатии ЛКМ в рабочей области
        private void WorkSpace_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {                 
            if (selectedAction == ButtonType.lineB )
            {
                if(polylinestate == false)
                    currentFigure = null;
            }
            else if(selectedAction == ButtonType.NoneB)
            {
                if (currentFigure is Polyline polyline)
                {
                    graphicElements.UnionPolylinePoint(currentFigure);
                    if (polyline.Points.Count <= 1)
                        WorkSpace.Children.Remove(currentFigure);
                }
                currentFigure = null;
            }
            else
                currentFigure = null;

            ClickCount = 0;
            previousMouseState = 3;
        }

        private void PreviewThicknessTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        //Опередление точные координаты границ прямоугольника
        private Point[] GetCornersPosition(Shape shape)
        {
            Point [] corners=new Point[4];
            double angle = GraphicElements.GetRotationAngle(shape);
            Rect rect = GraphicElements.GetPosition(shape);
            Point Center = GraphicElements.GetCenter(rect);
            //Определяем координаты углов прямоугольника без учёта угла поворота
            Point LeftTop = new(rect.X, rect.Y);
            Point LeftBottom = new(rect.X, rect.Y + rect.Height);
            Point RightTop = new(rect.X + rect.Width, LeftTop.Y);
            Point RightBottom = new(rect.X + rect.Width, LeftTop.Y + rect.Height);
            RotateTransform rotateTransform = new(angle, Center.X, Center.Y);
            //Определяем координаты углов прямоугольника с учётом угла поворота
            corners[0] = rotateTransform.Transform(LeftTop);
            corners[1] = rotateTransform.Transform(LeftBottom);
            corners[2] = rotateTransform.Transform(RightTop);
            corners[3] = rotateTransform.Transform(RightBottom);
            return corners;
        }

        //Опередление положениее перемещаемой границы прямоугольника
        HitType BorderCheck(Shape shape, Point currentMousePosition)
        {
            Point[] corners = GetCornersPosition(shape);
            Point LeftTop = corners[0];
            Point LeftBottom = corners[1];
            Point RightTop = corners[2];
            Point RightBottom = corners[3];
            double GAP = thickness * 2;

            //Определяем границу, местоположение мыши на границе
            if (GraphicElements.CheckIntersection(LeftTop,LeftBottom,currentMousePosition, GAP,true))
            {
                // Left edge.
                if (GraphicElements.CheckIntersection(LeftTop,RightTop, currentMousePosition, GAP, true)) return HitType.UpperLeft;
                if (GraphicElements.CheckIntersection(LeftBottom,RightBottom, currentMousePosition, GAP, true)) return HitType.LowerLeft;
                return HitType.Left;
            }
            if (GraphicElements.CheckIntersection(RightTop,RightBottom, currentMousePosition, GAP, true))
            {
                // Right edge.
                if (GraphicElements.CheckIntersection(LeftTop, RightTop, currentMousePosition, GAP, true)) return HitType.UpperRight;
                if (GraphicElements.CheckIntersection(LeftBottom, RightBottom, currentMousePosition, GAP, true)) return HitType.LowerRight;
                return HitType.Right;
            }
            if (GraphicElements.CheckIntersection(LeftTop, RightTop, currentMousePosition, GAP, true)) return HitType.Top;
            if (GraphicElements.CheckIntersection(LeftBottom, RightBottom, currentMousePosition, GAP, true)) return HitType.Bottom;
            return HitType.Body;
        }

        private enum HitType
        {
            None, Body, UpperLeft, UpperRight, LowerRight, LowerLeft, Top,Bottom,Left,Right
        };


        //Определение новых размеров прямоугольника
        private Rect GetNewSize(Shape currentFigure,Point currentMousePosition)
        {
            Rect rect = GraphicElements.GetPosition(currentFigure);
            double new_x = rect.X;
            double new_y = rect.Y;
            double new_width = rect.Width;
            double new_height = rect.Height;
            double offset_x = currentMousePosition.X - mousePosition.X;
            double offset_y = currentMousePosition.Y - mousePosition.Y;

            switch (hitType)
            {
                case HitType.UpperLeft:
                    new_x += offset_x;
                    new_y += offset_y;
                    new_width -= offset_x;
                    new_height -= offset_y;
                    break;
                case HitType.UpperRight:
                    new_y += offset_y;
                    new_width += offset_x;
                    new_height -= offset_y;
                    break;
                case HitType.LowerRight:
                    new_width += offset_x;
                    new_height += offset_y;
                    break;
                case HitType.LowerLeft:
                    new_x += offset_x;
                    new_width -= offset_x;
                    new_height += offset_y;
                    break;
                case HitType.Left:
                    new_x += offset_x;
                    new_width -= offset_x;
                    break;
                case HitType.Right:
                    new_width += offset_x;
                    break;
                case HitType.Bottom:
                    new_height += offset_y;
                    break;
                case HitType.Top:
                    new_y += offset_y;
                    new_height -= offset_y;
                    break;
            }
            return new Rect(new_x, new_y, new_width, new_height);
        }


        //Обработка действий при движении мыши в рабочей области
        private void MouseMoveAction(object sender, MouseEventArgs e)
        {
 
            Point currentMousePosition= Mouse.GetPosition(WorkSpace);
            Rect rect;
            if (selectedAction == ButtonType.NoneB)
            {
                if (previousMouseState == 1)
                {
                    if(currentFigure is Polyline)
                    {
                        graphicElements.MovePolylineNode(currentFigure, currentMousePosition, mousePosition);
                    }
                    else
                    {
                        foreach (Shape shape in WorkSpace.Children)
                        {
                            GraphicElements.Reposition(shape, currentMousePosition.X - mousePosition.X, currentMousePosition.Y - mousePosition.Y);
                        }
                    }
                    mousePosition = currentMousePosition;
                }
            }
            else if (selectedAction == ButtonType.rectB)
            {
                if (previousMouseState == 1)
                {
                    rect = new Rect(mousePosition.X <= currentMousePosition.X ? mousePosition.X : currentMousePosition.X,
                                      mousePosition.Y <= currentMousePosition.Y ? mousePosition.Y : currentMousePosition.Y,
                                     Math.Abs(currentMousePosition.X - mousePosition.X),
                                     Math.Abs(currentMousePosition.Y - mousePosition.Y));
                    GraphicElements.Resize(currentFigure, rect);
                }
            }
            else if (selectedAction == ButtonType.rotateB)
            {
                if (previousMouseState == 1 && currentFigure != null)
                {
                    GraphicElements.Rotate(currentFigure, (mousePosition.X - currentMousePosition.X - mousePosition.Y + currentMousePosition.Y));
                    mousePosition = currentMousePosition;
                }
            }
            else if (selectedAction == ButtonType.moveB)
            {
                if (previousMouseState == 1 && currentFigure != null)
                {

                    GraphicElements.Reposition(currentFigure, currentMousePosition.X - mousePosition.X, currentMousePosition.Y - mousePosition.Y);           
                    mousePosition = currentMousePosition;
                }
            }
            else if (selectedAction == ButtonType.scaleB)
            {
                if (previousMouseState == 1 && currentFigure != null)
                {
                    Rect newRect =GetNewSize(currentFigure, currentMousePosition);
                    if ((newRect.Width > thickness * 2) && (newRect.Height > thickness * 2))
                    {
                        GraphicElements.Resize(currentFigure, newRect);
                    }
                    mousePosition = currentMousePosition;
                }
            }
        }

        private void CreateNewPaperMenuButton(object sender, RoutedEventArgs e)
        {
            previousMouseState = 0;
            ClickCount = 0;
            currentFigure = null;
            polylinestate = false;
            WorkSpace.Children.Clear();
        }

        private void SaveMenuButton(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog SFD = new()
            {
                FileName = "VectorObjects",
                DefaultExt = "vo",
                Filter = "Vector Objects (.txt)|*.vo"
            };
            if (SFD.ShowDialog() == true)
                graphicElements.SaveToFile(SFD.FileName);
        }

        private void LoadMenuButton(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog SFD = new()
            {
                FileName = "VectorObjects",
                DefaultExt = "vo",
                Filter = "Vector Objects (.txt)|*.vo"
            };
            if (SFD.ShowDialog() == true)
            {
                previousMouseState = 0;
                ClickCount = 0;
                currentFigure = null;
                polylinestate = false;
                WorkSpace.Children.Clear();
                graphicElements.LoadFromFile(SFD.FileName);
            }
               
        }
        private void ExitMenuButton(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
