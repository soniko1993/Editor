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
         // Pressed button on toolbar, 0 = all uncheked
        Point mousePosition;
        Point mousePositionLocal;
        GraphicElements graphicElements;
        Shape currentFigure;

        private enum ButtonType
        {
           NoneB, rectB, lineB, fillB, deleteB, rotateB, moveB, scaleB, FillBorderB
        };
        ButtonType selectedAction=0;
        int previousMouseState = 0; // 0 - nothing; 1- press; 2-up;
        int ClickCount = 0;
        double thickness;
        HitType HT;
        bool polylinestate = false;
        public MainWindow()
        {
            InitializeComponent();
            thickness = Convert.ToDouble(FigureThickness.Text);
            graphicElements = new GraphicElements(WorkSpace);


        }        
       void ButtonChecker(ButtonType bt)
        {
            selectedAction = bt;
            switch (bt)
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
            if(bt!= ButtonType.rectB)
                CreateRectangleButton.IsChecked = false;
            if (bt != ButtonType.fillB)
                FillButton.IsChecked = false;
            if (bt != ButtonType.deleteB)
                DeleteButton.IsChecked = false;
            if (bt != ButtonType.lineB)
                CreateLineButton.IsChecked = false;
            if (bt != ButtonType.moveB)
                MoveButton.IsChecked = false;
            if (bt != ButtonType.rotateB)
                RotateButton.IsChecked = false;
            if (bt != ButtonType.scaleB)
                ScaleButton.IsChecked = false;
            if (bt != ButtonType.FillBorderB)
                FillBorderButton.IsChecked = false;
            ClickCount = 0;
            
        }
        private void CreateRectangleButton_Click(object sender, RoutedEventArgs e)
        {        
                selectedAction = ButtonType.rectB;
                ButtonChecker(selectedAction);
        }

        private void CreateLineButton_Click(object sender, RoutedEventArgs e)
        {

                selectedAction = ButtonType.lineB;
                ButtonChecker(selectedAction);

        }

        private void FillButton_Click(object sender, RoutedEventArgs e)
        {
                selectedAction = ButtonType.fillB;
                ButtonChecker(selectedAction);

        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
                selectedAction = ButtonType.deleteB;
                ButtonChecker(selectedAction);

        }
        private void RotateButton_Click(object sender, RoutedEventArgs e)
        {
                selectedAction = ButtonType.rotateB;
                ButtonChecker(selectedAction);
        }

        private void MoveButton_Click(object sender, RoutedEventArgs e)
        {

                selectedAction = ButtonType.moveB;
                ButtonChecker(selectedAction);

        }
        private void ScaleButton_Click(object sender, RoutedEventArgs e)
        {
                selectedAction = ButtonType.scaleB;
                ButtonChecker(selectedAction);
        }

        private void FillBorderButton_Click(object sender, RoutedEventArgs e)
        {
                selectedAction = ButtonType.FillBorderB;
                ButtonChecker(selectedAction);

        }
        private void ColorPickerButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void WorkSpace_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            thickness = Convert.ToDouble(FigureThickness.Text);
            mousePosition = Mouse.GetPosition(WorkSpace);
            Color color = ColorPickerButton.SelectedColor.Value;
            SolidColorBrush myBrush = new SolidColorBrush(color);
            IInputElement II = Mouse.DirectlyOver;
            if (CreateLineButton.IsChecked==false)
            {
                if (II.GetType() != typeof(Canvas))
                    currentFigure = (Shape)II;
            }

            switch (selectedAction)
            {
                case 0:  break;
                case ButtonType.rectB: 
                    currentFigure=graphicElements.AddRectangle(new Rect(mousePosition.X , mousePosition.Y, mousePosition.X, mousePosition.Y),myBrush, Convert.ToInt16(FigureThickness.Text)); 
                        break;
                case ButtonType.lineB:
                    if (polylinestate == false)
                    {
                        if (II.GetType() == typeof(Polyline))                     
                        {
                            graphicElements.InsertPointIntoLine((Shape)II, mousePosition);
                        }
                        else
                        {
                            polylinestate = true;
                            currentFigure = graphicElements.AddLine(mousePosition, myBrush, thickness);
                        }                         
                    }
                    else
                    {
                        if(currentFigure!=null)
                        {
                            if (e.ClickCount == 2)
                            {
                                graphicElements.AddPointToLine(currentFigure, mousePosition);
                                polylinestate = false;
                            }
                            else
                            {
                                graphicElements.AddPointToLine(currentFigure, mousePosition);
                            }
                        }

                    }                                    
                    break;
                case ButtonType.fillB: if (II.GetType() != typeof(Canvas)) graphicElements.Fill(currentFigure, ColorPickerButton.SelectedColor.Value); break;
                case ButtonType.FillBorderB: if (II.GetType() != typeof(Canvas)) graphicElements.FillBorder(currentFigure, ColorPickerButton.SelectedColor.Value,thickness); break;
                case ButtonType.deleteB: if(II.GetType() != typeof(Canvas)) graphicElements.DeleteFigure(currentFigure); break;
                case ButtonType.rotateB: mousePositionLocal = e.GetPosition(currentFigure); break;
                case ButtonType.moveB: ClickCount=e.ClickCount; mousePositionLocal = e.GetPosition(currentFigure); break;
                case ButtonType.scaleB: mousePositionLocal = e.GetPosition(currentFigure);
                    if (II.GetType() != typeof(Canvas))
                        HT = BorderCheck(currentFigure, mousePosition);
                            break;
            }
            previousMouseState = 1;
        }
        
        private void WorkSpace_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {       
            Point mp = Mouse.GetPosition(WorkSpace);
            Color color = ColorPickerButton.SelectedColor.Value;
            SolidColorBrush myBrush = new SolidColorBrush(color);
            switch (selectedAction)
            {
                case ButtonType.NoneB: currentFigure = null; break;
                case ButtonType.rectB: currentFigure = null; break;
                case ButtonType.lineB: if(polylinestate == false) currentFigure = null; break;
                case ButtonType.fillB: currentFigure = null; break;
                case ButtonType.deleteB: currentFigure = null; break;
                case ButtonType.rotateB: currentFigure = null; break;
                case ButtonType.moveB: 
                    if(currentFigure!=null)
                    {
                        if (ClickCount == 2 && currentFigure.GetType() == typeof(Polyline))
                        {
                            graphicElements.UnionPoilylinePoint(currentFigure, mp);
                            if(((Polyline)currentFigure).Points.Count<=1)
                                WorkSpace.Children.Remove(currentFigure);
                        }
                        currentFigure = null;
                    }
                    break;
                case ButtonType.scaleB: currentFigure = null; break;
            }
            ClickCount = 0;
            previousMouseState = 3;
        }

        private void PreviewThicknessTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }


        HitType BorderCheck(Shape shape, Point mp)
        {
            double angle = graphicElements.GetRotationAngle(shape);

            Rect r = graphicElements.GetPosition(shape);
            Point offset = graphicElements.GetOffset(shape);
            Point MP = mp;
            Point Center =new Point(r.X + r.Width / 2, r.Y + r.Height / 2);

            Point LT = new Point(r.X, r.Y);
            Point LB = new Point(r.X, r.Y + r.Height);
            Point RT = new Point(r.X + r.Width, LT.Y);
            Point RB = new Point(r.X + r.Width, LT.Y + r.Height);

            RotateTransform rt = new RotateTransform(angle, Center.X, Center.Y);
         
            LT = rt.Transform(LT);
            LB = rt.Transform(LB);
            RT = rt.Transform(RT);
            RB = rt.Transform(RB);
            double GAP = thickness * 2;
            if (graphicElements.CheckIntersection(LT,LB,MP, GAP,true))
            {
                // Left edge.
                if (graphicElements.CheckIntersection(LT,RT, MP, GAP, true)) return HitType.UL;
                if (graphicElements.CheckIntersection(LB,RB, MP, GAP, true)) return HitType.LL;
                return HitType.L;
            }
            if (graphicElements.CheckIntersection(RT,RB, MP, GAP, true))
            {
                // Right edge.
                if (graphicElements.CheckIntersection(LT, RT, MP, GAP, true)) return HitType.UR;
                if (graphicElements.CheckIntersection(LB, RB, MP, GAP, true)) return HitType.LR;
                return HitType.R;
            }
            if (graphicElements.CheckIntersection(LT, RT, MP, GAP, true)) return HitType.T;
            if (graphicElements.CheckIntersection(LB, RB, MP, GAP, true)) return HitType.B;
            return HitType.Body;
        }

        private enum HitType
        {
            None, Body, UL, UR, LR, LL, T,B,L,R,
        };
        private void MouseMoveAction(object sender, MouseEventArgs e)
        {
 
            Point mp = Mouse.GetPosition(WorkSpace);
            Color color = ColorPickerButton.SelectedColor.Value;
            SolidColorBrush myBrush = new SolidColorBrush(color);
            switch (selectedAction)
            {
                case ButtonType.NoneB: if (previousMouseState == 1)
                    {
                        foreach (Shape ui in WorkSpace.Children)
                        {
                            Point rectPos = e.GetPosition(currentFigure);
                            graphicElements.SetPosition(ui, mp.X - mousePosition.X, mp.Y - mousePosition.Y);
                        }
                        mousePosition = mp;
                    }
                    break;
                case ButtonType.rectB:
                    if (previousMouseState == 1)
                    {
                        Rect r = new Rect(mousePosition.X <= mp.X ? mousePosition.X : mp.X,
                                         mousePosition.Y <= mp.Y ? mousePosition.Y : mp.Y,
                                        Math.Abs(mp.X - mousePosition.X),
                                        Math.Abs(mp.Y - mousePosition.Y));
                        graphicElements.Resize(currentFigure, r);
                    }
                    break;
                case ButtonType.lineB: break;
                case ButtonType.fillB: break;
                case ButtonType.deleteB: break;
                case ButtonType.rotateB:
                    if (previousMouseState == 1 && currentFigure != null)
                    {
                        graphicElements.Rotate(currentFigure, (mousePosition.X - mp.X - mousePosition.Y + mp.Y));
                        mousePosition = mp;
                    }

                    break;
                case ButtonType.moveB:
                    if (previousMouseState == 1 && currentFigure != null)
                    {
                        if (ClickCount == 2 && currentFigure.GetType() == typeof(Polyline))
                        {
                            graphicElements.MovePolylineNode(currentFigure, mp, mousePosition);
                        }
                        else
                        {
                            graphicElements.SetPosition(currentFigure, mp.X - mousePosition.X, mp.Y - mousePosition.Y);
                        }
                        mousePosition = mp;
                    }
                    break;                                   
                case ButtonType.scaleB:
                    if (previousMouseState == 1 && currentFigure != null)
                    {                    
                        Rect r = graphicElements.GetPosition(currentFigure);
                        mp = Mouse.GetPosition(WorkSpace);
                        double angle = graphicElements.GetRotationAngle(currentFigure);
                        Point Center = new Point(r.X + r.Width / 2, r.Y + r.Height / 2);
                        double new_x = r.X;
                        double new_y = r.Y;
                        double new_width = r.Width;
                        double new_height = r.Height;
                        double offset_x = mp.X - mousePosition.X;
                        double offset_y = mp.Y - mousePosition.Y;     

                        switch (HT)
                       {
                            case HitType.UL:
                                new_x += offset_x;
                                new_y += offset_y;
                                new_width -= offset_x;
                                new_height -= offset_y;
                                break;
                            case HitType.UR:
                                new_y += offset_y;
                                new_width += offset_x;
                                new_height -= offset_y;
                                break;
                            case HitType.LR:
                                new_width += offset_x;
                                new_height += offset_y;
                                break;
                            case HitType.LL:
                                new_x += offset_x;
                                new_width -= offset_x;
                                new_height += offset_y;
                                break;
                            case HitType.L:
                                new_x += offset_x;
                                new_width -= offset_x;
                                break;
                            case HitType.R:
                                new_width += offset_x;
                                break;
                            case HitType.B:
                                new_height += offset_y;
                                break;
                            case HitType.T:
                                new_y += offset_y;
                                new_height -= offset_y;
                                break;
                        }
                        if ((new_width > thickness*2) && (new_height > thickness*2))
                        {
                            r = new Rect(new_x, new_y, new_width, new_height);
                            graphicElements.Resize(currentFigure, r);
                        }                    
                    }
                    mousePosition = mp;
                    break;
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
            Microsoft.Win32.SaveFileDialog SFD = new Microsoft.Win32.SaveFileDialog();
            SFD.FileName = "VectorObjects";
            SFD.DefaultExt = "vo";
            SFD.Filter = "Vector Objects (.txt)|*.vo";
            if (SFD.ShowDialog() == true)
                graphicElements.SaveToFile(SFD.FileName);
        }

        private void LoadMenuButton(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.OpenFileDialog SFD= new Microsoft.Win32.OpenFileDialog();
            SFD.FileName = "VectorObjects";
            SFD.DefaultExt = "vo";
            SFD.Filter = "Vector Objects (.txt)|*.vo";
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
