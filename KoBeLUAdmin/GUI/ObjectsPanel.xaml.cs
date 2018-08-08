// <copyright file=ObjectsPanel.xaml.cs
// <copyright>
//  Copyright (c) 2016, University of Stuttgart
//  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the Software),
//  to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
//  and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//  THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
//  DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
//  OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// <license>MIT License</license>
// <main contributors>
//  Markus Funk, Thomas Kosch, Sven Mayer
// </main contributors>
// <co-contributors>
//  Paul Brombosch, Mai El-Komy, Juana Heusler, 
//  Matthias Hoppe, Robert Konrad, Alexander Martin
// </co-contributors>
// <patent information>
//  We are aware that this software implements patterns and ideas,
//  which might be protected by patents in your country.
//  Example patents in Germany are:
//      Patent reference number: DE 103 20 557.8
//      Patent reference number: DE 10 2013 220 107.9
//  Please make sure when using this software not to violate any existing patents in your country.
// </patent information>
// <date> 11/2/2016 12:25:58 PM</date>

using Emgu.CV;
using Emgu.CV.Structure;
using HciLab.KoBeLU.InterfacesAndDataModel;
using HciLab.KoBeLU.InterfacesAndDataModel.Data;
using HciLab.Utilities;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Emgu.CV.Features2D;
using Emgu.CV.Util;
using KoBeLUAdmin.Backend;
using KoBeLUAdmin.Backend.ObjectDetection;
using KoBeLUAdmin.ContentProviders;
using KoBeLUAdmin.Database;
using HciLab.Kinect;

namespace KoBeLUAdmin.GUI
{
    /// <summary>
    /// Interaktionslogik für ObjectsPanel.xaml
    /// </summary>
    public partial class ObjectsPanel : UserControl
    {
        public static readonly int BOX_BORDERWIDTH = 5;
        public static readonly int BOX_MANUALY_INSERT_HEIGHT = 100;
        public static readonly int BOX_MANUALY_INSERT_WIDTH = 100;

        private ObjectDetectionZone m_DraggedObj = null;

        private bool m_DragEnabled = false;

        private AllEnums.Direction m_DragMode = AllEnums.Direction.NONE;

        private const int m_BORDERWIDTHObject = 5;

        private bool m_TakeScreenShotFromZone = false;
        bool m_TakeBackgroundScreenShot = false;

        private ObjectDetectionZone m_SelectedZone = null;

        private long m_ScreenshotTakenTimestamp = 0;

        Image<Bgra, Byte> mColorImage;
        
        public ObjectsPanel()
        {
            InitializeComponent();
            m_TopBar.DataContext = SettingsManager.Instance.Settings;
            m_ButtomBar.DataContext = SettingsManager.Instance.Settings;
            m_ObjectsListView.DataContext = DatabaseManager.Instance;
            m_ListBoxObjects.DataContext = ObjectDetectionManager.Instance.CurrentLayout;
        }

        /// <summary>
        /// Delete object callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        /// 
        private void Menuitem_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (m_ObjectsListView.SelectedIndex != -1)
            {
                TrackableObject obj = (TrackableObject)m_ObjectsListView.SelectedItem;
                DatabaseManager.Instance.deleteTrackableObject(obj);
                DatabaseManager.Instance.listTrackableObject(); // refresh
                m_ObjectsListView.Items.Refresh();
            }
        }

        private void ObjectsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // check if an object is selected
            if (m_ObjectsListView.SelectedItems.Count == 1)
            {
                foreach (var item in m_ObjectsListView.SelectedItems)
                {
                    TrackableObject obj = item as TrackableObject;
                    ObjectDetectionManager.Instance.DisplayObjectEditDialog(obj);
                }
            }
        }

        //Code to be called within the main OnLoaded Method
        public void Boxes_OnLoadedObject(object sender, RoutedEventArgs er)
        {
        }

        //Code to be called within the main ProccessFrame Method
        public void Boxes_ProccessFrameObject()
        {

            if (ObjectDetectionManager.Instance.CurrentLayout != null)
            {
                if (StateManager.Instance.State == AllEnums.State.RECORD)
                {
                    ObjectDetectionManager.Instance.runPBDObjectThereCheck();
                }
            }
        }


        //Code to be called within a certain part of the main ProccessFrame Method
        public void Object_ProccessFrame_Draw(bool hasToUpdateUI, Image<Bgra, Byte> pImage)
        {
            mColorImage = pImage.Convert<Bgra, Byte>();
            // display image with visual feedback
            CvInvoke.cvResetImageROI(pImage);
            if (ObjectDetectionManager.Instance.CurrentLayout != null)
            {
                if (SettingsManager.Instance.Settings.ObjectsVisualFeedbackDisplay && (ObjectDetectionManager.Instance.CurrentLayout.ObjectDetectionZones.Count != 0))
                {
                    //write boxes
                    //MCvFont font = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_SIMPLEX, 0.5, 0.5);
                    foreach (ObjectDetectionZone ob in ObjectDetectionManager.Instance.CurrentLayout.ObjectDetectionZones)
                    {
                        // draw ID
                        pImage.Draw(ob.Id + "", new System.Drawing.Point(ob.X, ob.Y), Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.5, new Bgra(0, 0, 0, 0));
                        // draw Frame
                        if (ob.wasRecentlyTriggered())
                            pImage.Draw(new Rectangle(ob.X, ob.Y, ob.Width, ob.Height), new Bgra(0, 255, 255, 0), 5);
                        else
                            pImage.Draw(new Rectangle(ob.X, ob.Y, ob.Width, ob.Height), new Bgra(255, 255, 255, 0), 5);
                    }
                }

                UtilitiesImage.ToImage(image, pImage);

                if (SettingsManager.Instance.Settings.ObjectsVisualFeedbackProject)
                {
                    SceneManager.Instance.TemporaryObjectsScene.Clear();
                    // add a temporary scene for each box
                    foreach (ObjectDetectionZone ob in ObjectDetectionManager.Instance.CurrentLayout.ObjectDetectionZones)
                    {
                        // false = call from the display loop
                        SceneManager.Instance.TemporaryObjectsScene.Add(ObjectDetectionManager.Instance.createSceneBoxForObjectDetectionZone(ob, false));
                    }
                }
                else
                {
                    SceneManager.Instance.TemporaryObjectsScene.Clear();
                }
            }
        }

        private void initializeDrag(System.Windows.Point p)
        {

            foreach (ObjectDetectionZone o in ObjectDetectionManager.Instance.CurrentLayout.ObjectDetectionZones)
            {
                if (o.IsSelected)
                {
                    m_DragMode = isMouseOnObj(p, o);
                    if (m_DragMode != AllEnums.Direction.NONE)
                    {
                        m_DraggedObj = o;
                        m_DragEnabled = true;
                        return;
                    }
                }
            }
            m_DragEnabled = false;
        }

        private void buttonSaveBoxObjectLayout_Click(object sender, RoutedEventArgs e)
        {
            ObjectDetectionManager.Instance.saveObjectDetectionZoneLayoutToFile();
        }

        private void buttonLoadBoxObjectLayout_Click(object sender, RoutedEventArgs e)
        {

            ObjectDetectionManager.Instance.loadObjectDetectionZoneLayoutFromFile();

            // also update name
            if (ObjectDetectionManager.Instance.CurrentLayout != null)
            {
                this.m_ListBoxObjects.DataContext = ObjectDetectionManager.Instance.CurrentLayout.ObjectDetectionZones;
            }
        }

        public void refreshDataContext()
        {
            this.m_ListBoxObjects.DataContext = ObjectDetectionManager.Instance.CurrentLayout;
        }

        private void image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point p = e.GetPosition(image);
            if (isMouseOnAnyObj(p) == AllEnums.Direction.NONE)
            {
                createANewObj(sender, e);
            }
            else
            {
                initializeDrag(p);
            }
        }

        private void createANewObj(object sender, MouseButtonEventArgs e)
        {
            int width = 0;
            int height = 0;
            double x = e.GetPosition(image).X;
            double y = e.GetPosition(image).Y;
            if (mColorImage != null)
            {
                if (BOX_MANUALY_INSERT_WIDTH + x < mColorImage.Width)
                {
                    width = BOX_MANUALY_INSERT_WIDTH;
                }
                else
                {
                    width = (int)(mColorImage.Width - x);
                }

                if (BOX_MANUALY_INSERT_HEIGHT + y < mColorImage.Height)
                {
                    height = BOX_MANUALY_INSERT_HEIGHT;
                }
                else
                {
                    height = (int)(mColorImage.Height - y);
                }

                ObjectDetectionZone ob = ObjectDetectionManager.Instance.createObjectDetectionZoneFromFactory((int)x, (int)y, width, height);

                ob.X = (int)x;
                ob.Y = (int)y;
                ob.Width = width;
                ob.Height = height;

                Image<Bgra, Byte> croppedImage = mColorImage;
                croppedImage.ROI = new Rectangle((int)x, (int)y, width, height);

                ob.ObjectColorImage = croppedImage;
                ob.MatchPercentageOffset = 80;

                ObjectDetectionManager.Instance.CurrentLayout.ObjectDetectionZones.Add(ob);

            }
        }

        private void image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (m_DragEnabled)
            {
                ObjectDetectionManager.Instance.UpdateCurrentObjectDetectionZone(m_DraggedObj);
                m_DragEnabled = false;
            }
         }


        private void image_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point p = e.GetPosition(image);
            if (m_DragEnabled == false)
            {
                AllEnums.Direction d = isMouseOnAnyObj(p);
                if (d == AllEnums.Direction.NORTH || d == AllEnums.Direction.SOUTH)
                {
                    Cursor = Cursors.SizeNS;
                }
                else if (d == AllEnums.Direction.EAST || d == AllEnums.Direction.WEST)
                {
                    Cursor = Cursors.SizeWE;
                }
                else
                {
                    this.Cursor = Cursors.Arrow;
                }
            }
            else
            {
                newRectSize(p);
            }
        }

        private void newRectSize(System.Windows.Point p)
        {
            if (m_DragEnabled)
            {
                if (m_DragMode == AllEnums.Direction.NORTH) // North
                {
                    if (p.Y > m_DraggedObj.Y && p.Y < m_DraggedObj.Y + m_DraggedObj.Height)
                    {
                        m_DraggedObj.Height = (m_DraggedObj.Y + m_DraggedObj.Height) - ((int)p.Y);
                        m_DraggedObj.Y = (int)p.Y;
                    }

                    else if (p.Y < m_DraggedObj.Y)
                    {
                        m_DraggedObj.Height = m_DraggedObj.Y + m_DraggedObj.Height - ((int)p.Y);
                        m_DraggedObj.Y = (int)p.Y;
                    }
                }
                else if (m_DragMode == AllEnums.Direction.EAST) // East
                {
                    if (p.X > m_DraggedObj.X && p.X < m_DraggedObj.X + m_DraggedObj.Width)
                    {
                        m_DraggedObj.Width = ((int)p.X) - m_DraggedObj.X;
                    }

                    else if (p.X > m_DraggedObj.X + m_DraggedObj.Width)
                    {
                        m_DraggedObj.Width = ((int)p.X) - m_DraggedObj.X;
                    }
                }

                else if (m_DragMode == AllEnums.Direction.SOUTH) // South
                {
                    if (p.Y < m_DraggedObj.Y + m_DraggedObj.Height && p.Y > m_DraggedObj.Y)
                    {
                        m_DraggedObj.Height = ((int)p.Y) - m_DraggedObj.Y;
                    }

                    else if (p.Y > m_DraggedObj.Y + m_DraggedObj.Height)
                    {
                        m_DraggedObj.Height = ((int)p.Y) - m_DraggedObj.Y;
                    }
                }
                else if (m_DragMode == AllEnums.Direction.WEST) // West
                {
                    if (p.X > m_DraggedObj.X && p.X < m_DraggedObj.X + m_DraggedObj.Width)
                    {
                        m_DraggedObj.Width = m_DraggedObj.X + m_DraggedObj.Width - ((int)p.X);
                        m_DraggedObj.X = ((int)p.X);
                    }

                    else if (p.X < m_DraggedObj.X)
                    {
                        m_DraggedObj.Width = m_DraggedObj.X + m_DraggedObj.Width - ((int)p.X);
                        m_DraggedObj.X = ((int)p.X);
                    }
                }
                // update picture after resizing the object
                Image<Bgra, Byte> croppedImage = mColorImage;
                croppedImage.ROI = new Rectangle(m_DraggedObj.X, m_DraggedObj.Y, m_DraggedObj.Width, m_DraggedObj.Height);
                m_DraggedObj.ObjectColorImage = croppedImage;
            }
        }
        
        private AllEnums.Direction isMouseOnAnyObj(System.Windows.Point p)
        {
            foreach (ObjectDetectionZone b in ObjectDetectionManager.Instance.CurrentLayout.ObjectDetectionZones)
            {
                if (b.IsSelected)
                {

                    AllEnums.Direction d = isMouseOnObj(p, b);
                    if (d != AllEnums.Direction.NONE)
                        return d;
                }
            }
            return AllEnums.Direction.NONE;
        }

        private AllEnums.Direction isMouseOnObj(System.Windows.Point pPoint, ObjectDetectionZone pRect)
        {
            if (pPoint.X >= pRect.X && pPoint.X <= (pRect.X + pRect.Width)
                    && pPoint.Y >= pRect.Y - (BOX_BORDERWIDTH)
                    && pPoint.Y <= pRect.Y + (BOX_BORDERWIDTH))
            {
                return AllEnums.Direction.NORTH;
            }
            else if (pPoint.X >= pRect.X && pPoint.X <= (pRect.X + pRect.Width)
                    && pPoint.Y >= pRect.Y + pRect.Height - (BOX_BORDERWIDTH)
                && pPoint.Y <= pRect.Y + (BOX_BORDERWIDTH) + pRect.Height)
            {
                return AllEnums.Direction.SOUTH;
            }
            else if (pPoint.Y >= pRect.Y && pPoint.Y <= (pRect.Y + pRect.Height)
                    && pPoint.X >= pRect.X - (BOX_BORDERWIDTH)
                    && pPoint.X <= pRect.X + (BOX_BORDERWIDTH))
            {
                return AllEnums.Direction.WEST;
            }
            else if (pPoint.Y >= pRect.Y && pPoint.Y <= (pRect.Y + pRect.Height)
                    && pPoint.X >= pRect.X + pRect.Width - (BOX_BORDERWIDTH)
                    && pPoint.X <= pRect.X + (BOX_BORDERWIDTH) + pRect.Width)
            {
                return AllEnums.Direction.EAST;
            }

            return AllEnums.Direction.NONE;
        }

      
        private void MenuItem_EditSelectedBoxObject(object sender, RoutedEventArgs e)
        {
            var selectedItem = m_ListBoxObjects.SelectedItem;
            if (selectedItem is ObjectDetectionZone)
            {
                ObjectDetectionZone ob = (ObjectDetectionZone)selectedItem;

                EditObjectDetectionZoneDialog dlg = new EditObjectDetectionZoneDialog(ob);
                dlg.ShowDialog(); // blocking
                if (dlg.wasOkay())
                {
                    ObjectDetectionZone editedZone = dlg.EditedObjectDetectionZone;
                    ObjectDetectionManager.Instance.UpdateCurrentObjectDetectionZone(editedZone);
                }

            }


        }

        private void MenuItem_DeleteSelectedObjectDetectionZone(object sender, RoutedEventArgs e)
        {
            var selectedItem = m_ListBoxObjects.SelectedItem;
            if (selectedItem is ObjectDetectionZone)
            {
                ObjectDetectionZone z = (ObjectDetectionZone)selectedItem;

                ObjectDetectionManager.Instance.CurrentLayout.ObjectDetectionZones.Remove(z);

            }
        }

        private void buttonObjectScreenShot_Click(object sender, RoutedEventArgs e)
        {
            updateObjectZones();
        }

        private void updateObjectZones()
        {
            var selectedItem = m_ListBoxObjects.SelectedItem;
            if (selectedItem is ObjectDetectionZone)
            {
                ObjectDetectionZone z = (ObjectDetectionZone)selectedItem;

                m_SelectedZone = z;
                m_TakeScreenShotFromZone = true;
                m_ScreenshotTakenTimestamp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                SceneManager.Instance.DisableObjectScenes = true;

            }
        }


        private ObjectDetectionZone GetSelectedObjectZone()
        {
            var selectedItem = m_ListBoxObjects.SelectedItem;
            if (selectedItem is ObjectDetectionZone)
            {
                ObjectDetectionZone z = (ObjectDetectionZone)selectedItem;
                return z;
            }
            return null;
        }

        private void buttonBackgroundScreenShot_Click(object sender, RoutedEventArgs e)
        {
            m_TakeBackgroundScreenShot = true;
        }

        internal void Refresh()
        {
            m_ObjectsListView.Items.Refresh();
        }
    }
}
