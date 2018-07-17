// <copyright file=WorkflowEditor.xaml.cs
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
// <date> 11/2/2016 12:25:59 PM</date>

using HciLab.KoBeLU.InterfacesAndDataModel;
using HciLab.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Runtime.InteropServices.WindowsRuntime;
using KoBeLUAdmin.Backend;
using KoBeLUAdmin.Backend.AssembleyZones;
using KoBeLUAdmin.Backend.Boxes;
using KoBeLUAdmin.Backend.ObjectDetection;
using KoBeLUAdmin.ContentProviders;
using KoBeLUAdmin.Model.Process;
using KoBeLUAdmin.Network;
using KoBeLUAdmin.Scene;

namespace KoBeLUAdmin.GUI.Dialog
{
    /// <summary>
    /// Interaktionslogik für WorkflowEditor.xaml
    /// </summary>
    public partial class WorkflowEditor : Window
    {

        // indicates if a workflow was loaded when opening this editor
        private bool wasWorkflowLoaded = false;


        public WorkflowEditor()
        {
            InitializeComponent();
            init();

            StateManager.Instance.SetNewState(this, HciLab.KoBeLU.InterfacesAndDataModel.AllEnums.State.EDIT);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            SceneManager.Instance.CurrentScene = new Scene.Scene();

            if (wasWorkflowLoaded)
            {
                StateManager.Instance.SetNewState(this, HciLab.KoBeLU.InterfacesAndDataModel.AllEnums.State.WORKFLOW_LOADED);
            }
            else
            {
                if (!m_WorkflowListview.HasItems)
                {
                    StateManager.Instance.SetNewState(this, HciLab.KoBeLU.InterfacesAndDataModel.AllEnums.State.IDLE);
                }
            }
        }


        private void init()
        {
            // setup the boxes

            listBoxBoxes.DataContext = BoxManager.Instance.CurrentLayout;


            // setup the assemblyzones
            listBoxAssemblyZones.DataContext = AssemblyZoneManager.Instance.CurrentLayout;


            // setup the objects
            m_listBoxObjectZones.DataContext = ObjectDetectionManager.Instance.CurrentLayout;

            // setup the networktables
            listBoxNetworkTables.DataContext = CommunicationManager.Instance.ServerInfo;

            // init the new Workflow
            if (WorkflowManager.Instance.LoadedWorkflow != null)
            {
                wasWorkflowLoaded = true;
                Workflow obj = new Workflow();
                UtilitiesCopy.DeepClone<Workflow>(ref obj, WorkflowManager.Instance.LoadedWorkflow);
                EditWorkflowManager.Instance.CurrentWorkflow = obj;
            }
            else
            {
                EditWorkflowManager.Instance.CurrentWorkflow = new Workflow();
            }

            m_WorkflowListview.DataContext = EditWorkflowManager.Instance.CurrentWorkflow;
            EditWorkflowManager.Instance.CurrentWorkflow.BoxLayout = BoxManager.Instance.CurrentLayout;
            //AssemblyZoneManager.Instance.CurrentLayout = new AssemblyZoneLayout(); // Assemblyzones are different -.-
            EditWorkflowManager.Instance.CurrentWorkflow.AssemblyZoneLayout = AssemblyZoneManager.Instance.CurrentLayout;
            EditWorkflowManager.Instance.CurrentWorkflow.ObjectZoneLayout = ObjectDetectionManager.Instance.CurrentLayout;

            Style itemContainerStyle = new Style(typeof(ListBoxItem));
            itemContainerStyle.Setters.Add(new Setter(ListBoxItem.AllowDropProperty, true));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(onListViewMouseLeftButtonDown)));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.DragEnterEvent, new DragEventHandler(onListViewDragEnter)));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.DragLeaveEvent, new DragEventHandler(onListViewDragLeave)));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.DragOverEvent, new DragEventHandler(onListViewDragOver)));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.DropEvent, new DragEventHandler(onListViewDrop)));
            m_WorkflowListview.ItemContainerStyle = itemContainerStyle;

            m_ComboboxAdaptivityLevel.ItemsSource = AdaptivityLevel.AdaptivityLevels;
            m_ComboboxAdaptivityLevel.SelectedValue = SettingsManager.Instance.Settings.AdaptivityLevelId;

            //get name, description, category and creator
            NameBox.Text = EditWorkflowManager.Instance.CurrentWorkflow.Name;
            DescriptionBox.Text = EditWorkflowManager.Instance.CurrentWorkflow.Description;
            CategoryBox.Text = EditWorkflowManager.Instance.CurrentWorkflow.Category;
            CreatorBox.Text = EditWorkflowManager.Instance.CurrentWorkflow.Creator;
            //load Image from workflow file
            if (EditWorkflowManager.Instance.CurrentWorkflow.Thumbnail != null)
            {
                System.Windows.Media.Imaging.WriteableBitmap bmpPx =
                    new System.Windows.Media.Imaging.WriteableBitmap(
                    64,
                    64,
                     299.9993896484375, 299.9993896484375, System.Windows.Media.PixelFormats.Bgra32, null);

                int stride = 64 * (System.Windows.Media.PixelFormats.Bgra32.BitsPerPixel) / 8;

                bmpPx.WritePixels(
                    new Int32Rect(0, 0, 64, 64),
                    EditWorkflowManager.Instance.CurrentWorkflow.Thumbnail, stride, 0);

                ThumbnailImage.Source = bmpPx;
            }

            AdminView.Instance.refreshDataContext();
            AdminView.Instance.refreshWorkflowUI();

        }

        private void onListViewDragLeave(object sender, DragEventArgs e)
        {
            ListBoxItem overItem = sender as ListBoxItem;
            overItem.Opacity = 1;
        }

        private void onListViewDragEnter(object sender, DragEventArgs e)
        {
            ListBoxItem overItem = sender as ListBoxItem;
            overItem.Opacity = 0.7;
        }

        private void onListViewDragOver(object sender, DragEventArgs e)
        {
            ListBoxItem overItem = sender as ListBoxItem;
            //overItem.IsSelected = true;
        }

        private void onListViewDrop(object sender, DragEventArgs e)
        {
            ListBoxItem overItem = sender as ListBoxItem;
            var workingSteps = EditWorkflowManager.Instance.CurrentWorkflow.WorkingSteps;

            WorkingStep droppedData = e.Data.GetData(typeof(WorkingStep)) as WorkingStep;
            WorkingStep target = ((ListBoxItem)(sender)).DataContext as WorkingStep;

            int removedIdx = m_WorkflowListview.Items.IndexOf(droppedData);
            int targetIdx = m_WorkflowListview.Items.IndexOf(target);

            if (targetIdx != removedIdx)
            {
                workingSteps.Move(removedIdx, targetIdx);

                //Set step numbers
                int idx = 1;
                foreach (WorkingStep workingStep in workingSteps)
                {
                    workingStep.StepNumber = idx;
                    idx++;
                }
            }

            overItem.IsSelected = true;
            overItem.Opacity = 1.0;
        }

        private void onListViewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mojo = Mouse.DirectlyOver;
            if (sender is ListBoxItem && !(mojo is Rectangle)) //workaround to allow clicking of checkboxes
            {
                ListBoxItem draggedItem = sender as ListBoxItem;
                DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
                draggedItem.IsSelected = true;
            }
            //e.Handled = true;
        }

        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            Object o = m_WorkflowListview.SelectedItem;
            if (o != null)
            {
                if (o is WorkingStep)
                    EditWorkflowManager.Instance.CurrentWorkflow.WorkingSteps.Remove(o as WorkingStep);
            }
        }

        private void createBoxButton_Click(object sender, RoutedEventArgs e)
        {
            Object o = listBoxBoxes.SelectedItem;

            if (o != null)
            {
                if (o is Box)
                {
                    Box b = (Box)o;

                    AdaptiveScene easyScene = new AdaptiveScene(EditWorkflowManager.Instance.getBoxAutoScene(b), AdaptivityLevel.AdaptivityLevels.First());
                    AdaptiveScene mediumScene = new AdaptiveScene(EditWorkflowManager.Instance.getBoxAutoScene(b), AdaptivityLevel.AdaptivityLevels.ElementAt(1));

                    var adaptiveScenes = new List<AdaptiveScene> { easyScene, mediumScene };

                    EditWorkflowManager.Instance.createStep(HciLab.KoBeLU.InterfacesAndDataModel.AllEnums.PBD_Mode.BOX_WITHDRAWEL, adaptiveScenes, "Box-" + b.Id, b.TriggerMessage);
                }
            }
        }

        private void createAssemblyButton_Click(object sender, RoutedEventArgs e)
        {
            Object o = listBoxAssemblyZones.SelectedItem;

            if (o != null)
            {
                if (o is AssemblyZone)
                {
                    AssemblyZone z = (AssemblyZone)o;

                    AdaptiveScene easyScene = new AdaptiveScene(EditWorkflowManager.Instance.getAssemblyZoneAutoScene(z), AdaptivityLevel.AdaptivityLevels.First());
                    AdaptiveScene mediumScene = new AdaptiveScene(EditWorkflowManager.Instance.getAssemblyZoneAutoScene(z), AdaptivityLevel.AdaptivityLevels.ElementAt(1));

                    var adaptiveScenes = new List<AdaptiveScene> { easyScene, mediumScene };

                    EditWorkflowManager.Instance.createStep(HciLab.KoBeLU.InterfacesAndDataModel.AllEnums.PBD_Mode.ASSEMBLY_DONE, adaptiveScenes, "Zone-" + z.Id, z.TriggerMessage);
                }
            }
        }

        private void saveLayout_Click(object sender, RoutedEventArgs e)
        {
            EditWorkflowManager.Instance.CurrentWorkflow.Name = NameBox.Text;
            EditWorkflowManager.Instance.CurrentWorkflow.Description = DescriptionBox.Text;
            EditWorkflowManager.Instance.CurrentWorkflow.Category = CategoryBox.Text;
            EditWorkflowManager.Instance.CurrentWorkflow.Creator = CreatorBox.Text;

            EditWorkflowManager.Instance.SaveCurrentWorkflow(true);
        }


        private void cancelLayout_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void applyLayout_Click(object sender, RoutedEventArgs e)
        {
            EditWorkflowManager.Instance.CurrentWorkflow.Name = NameBox.Text;
            EditWorkflowManager.Instance.CurrentWorkflow.Description = DescriptionBox.Text;
            EditWorkflowManager.Instance.CurrentWorkflow.Category = CategoryBox.Text;
            EditWorkflowManager.Instance.CurrentWorkflow.Creator = CreatorBox.Text;

            Workflow obj = new Workflow();
            UtilitiesCopy.DeepClone<Workflow>(ref obj, EditWorkflowManager.Instance.CurrentWorkflow);
            WorkflowManager.Instance.LoadedWorkflow = obj;

            AdminView.Instance.refreshDataContext();
            AdminView.Instance.refreshWorkflowUI();

            if (m_WorkflowListview.HasItems)
            {
                StateManager.Instance.SetNewState(WorkflowManager.Instance, AllEnums.State.WORKFLOW_LOADED);
            }
        }

        private void m_ButtonCreateEndCondition_Click(object sender, RoutedEventArgs e)
        {
            EditWorkflowManager.Instance.createEndScene("Endcondition,", "end");
        }


        private void m_WorkflowListview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // update the SceneManager here
            Object o = m_WorkflowListview.SelectedItem;
            if (o != null)
            {
                if (o is WorkingStep)
                {
                    m_FailStatesListView.IsEnabled = true;
                    WorkingStep step = (WorkingStep)o;
                    //SceneManager.Instance.CurrentScene = step.AdaptiveScenes.FirstOrDefault().Scene;
                    if (m_ComboboxAdaptivityLevel.SelectedValue != null)
                    {
                        SceneManager.Instance.CurrentScene = step.getAdaptiveScene((int)m_ComboboxAdaptivityLevel.SelectedValue).Scene;
                        m_FailStatesListView.ItemsSource = step.FailStates;
                    }
                }
            }
            else
            {
                m_FailStatesListView.IsEnabled = false;
                m_FailStatesListView.ItemsSource = null;
            }
        }

        private void MenuItem_EditScene(object sender, RoutedEventArgs e)
        {
            // update the EditWorkflowManager here
            Object o = m_WorkflowListview.SelectedItem;

            if (o != null)
            {
                if (o is WorkingStep)
                {
                    WorkingStep step = (WorkingStep)o;
                    Scene.Scene scene = new Scene.Scene();
                    if (m_ComboboxAdaptivityLevel.SelectedValue != null)
                    {
                        scene = step.getAdaptiveScene((int)m_ComboboxAdaptivityLevel.SelectedValue).Scene;
                        foreach (SceneItem itemIter in scene.Items)
                        {
                            itemIter.Touchy = true;
                        }

                    }
                    SceneManager.Instance.CurrentScene = scene;
                    SceneEditorDialog dlg = new SceneEditorDialog(scene);
                    dlg.Show();
                }
            }
        }

        private void m_ComboboxAdaptivityLevel_Selected(object sender, RoutedEventArgs e)
        {
            // update the SceneManager here
            Object o = m_WorkflowListview.SelectedItem;
            if (o != null)
            {
                if (o is WorkingStep)
                {
                    WorkingStep step = (WorkingStep)o;
                    //SceneManager.Instance.CurrentScene = step.AdaptiveScenes.FirstOrDefault().Scene;
                    SceneManager.Instance.CurrentScene = step.getAdaptiveScene((int)m_ComboboxAdaptivityLevel.SelectedValue).Scene;
                }
            }
        }

        private void ThumbnailButton_click(object sender, RoutedEventArgs e)
        {
            if(ThumbnailImage.Source != null)
            {
                ThumbnailImage.Source = null;
                ThumbnailButton.Content = "Add Thumbnail";
                EditWorkflowManager.Instance.CurrentWorkflow.Thumbnail = null;
                return;
            }
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Title = "Select Workflow Thumbnail...",
                Filter = "Image Files|*.bmp;*.jpg;*.png"
            };
            // Process open file dialog box results
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Open document
                string filename = dlg.FileName;
                System.Windows.Media.Imaging.BitmapImage bmp = new System.Windows.Media.Imaging.BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(filename, UriKind.RelativeOrAbsolute);
                bmp.DecodePixelWidth = 64;
                bmp.DecodePixelHeight = 64;
                bmp.EndInit();
    
                int stride = bmp.PixelWidth * (bmp.Format.BitsPerPixel) / 8;
                EditWorkflowManager.Instance.CurrentWorkflow.Thumbnail = new byte[stride * bmp.PixelHeight];

                bmp.CopyPixels(EditWorkflowManager.Instance.CurrentWorkflow.Thumbnail, stride, 0);

                System.Windows.Media.Imaging.WriteableBitmap bmpPx = 
                    new System.Windows.Media.Imaging.WriteableBitmap(
                    bmp.PixelWidth,
                    bmp.PixelHeight,
                    bmp.DpiX, bmp.DpiY,
                    bmp.Format, null);

                bmpPx.WritePixels(
                    new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight),
                    EditWorkflowManager.Instance.CurrentWorkflow.Thumbnail, stride, 0);

                ThumbnailImage.Source = bmpPx;
                ThumbnailButton.Content = "Remove Thumbnail";
            }
        }

        private void MenuItemCreateErrorTrigger_Cick(object sender, RoutedEventArgs e)
        {
            Object o = listBoxAssemblyZones.SelectedItem;
            if (m_WorkflowListview.SelectedIndex >= 0 && o is AssemblyZone)
            {
                WorkingStep selectedWorkingStep = (WorkingStep)m_WorkflowListview.SelectedItem;
                AssemblyZone selectedAssemblyZone = (AssemblyZone)o;
                //Create new Failstate
                selectedWorkingStep.CreateFailState(selectedAssemblyZone.TriggerMessage);
            }
        }

        private void MenuItemAddErrorTrigger_Cick(object sender, RoutedEventArgs e)
        {
            Object o = listBoxAssemblyZones.SelectedItem;
            if (m_WorkflowListview.SelectedIndex >= 0 && m_FailStatesListView.SelectedIndex >= 0 && o is AssemblyZone)
            {
                WorkingStep selectedWorkingStep = (WorkingStep)m_WorkflowListview.SelectedItem;
                WorkflowFailState selectedFailState = (WorkflowFailState)m_FailStatesListView.SelectedItem;
                AssemblyZone selectedAssemblyZone = (AssemblyZone)o;
                //Create new Failstate
                selectedFailState.AddCondition(selectedAssemblyZone.TriggerMessage);
            }
        }

        private void MenuItem_EditErrorScene(object sender, RoutedEventArgs e)
        {
            Object o = m_FailStatesListView.SelectedItem;
            if (o != null)
            {
                if (o is WorkflowFailState)
                {
                    WorkflowFailState fState = (WorkflowFailState)o;
                    Scene.Scene scene = fState.Scene;
                    foreach (SceneItem itemIter in scene.Items)
                    {
                        itemIter.Touchy = true;
                    }

                    SceneManager.Instance.CurrentScene = scene;
                    SceneEditorDialog dlg = new SceneEditorDialog(scene);
                    dlg.Show();
                }
            }

        }

        private void MenuItemDeleteErrorState_Click(object sender, RoutedEventArgs e)
        {
            Object o = m_WorkflowListview.SelectedItem;
            int idxFailState = m_FailStatesListView.SelectedIndex;

            if (o != null && idxFailState >= 0)
            {
                if (o is WorkingStep)
                {
                    WorkingStep step = (WorkingStep)o;
                    step.FailStates.RemoveAt(idxFailState);
                }
            }

        }

        private void m_FailstatesListview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Object o = m_FailStatesListView.SelectedItem;
            if (o != null)
            {
                if (o is WorkflowFailState)
                {
                    WorkflowFailState fState = (WorkflowFailState)o;
                    SceneManager.Instance.CurrentScene = fState.Scene;
                }
            }
        }

        private void MenuItemClearErrorTriggers_Click(object sender, RoutedEventArgs e)
        {
            Object o = m_FailStatesListView.SelectedItem;
            if (o != null)
            {
                if (o is WorkflowFailState)
                {
                    WorkflowFailState fState = (WorkflowFailState)o;
                    fState.ClearConditions();
                }
            }
        }

        private void MenuItemSetTrigger_Cick(object sender, RoutedEventArgs e)
        {
            Object o = listBoxAssemblyZones.SelectedItem;
            if (m_WorkflowListview.SelectedIndex >= 0 && o is AssemblyZone)
            {
                WorkingStep selectedWorkingStep = (WorkingStep)m_WorkflowListview.SelectedItem;
                AssemblyZone selectedAssemblyZone = (AssemblyZone)o;
                //Create new Failstate
                selectedWorkingStep.EndConditionObjectName = selectedAssemblyZone.TriggerMessage;
            }

        }

        private void MenuItemEditFailState_Click(object sender, RoutedEventArgs e)
        {
            Object o = m_FailStatesListView.SelectedItem;
            if (o != null)
            {
                if (o is WorkflowFailState)
                {
                    WorkflowFailState fs = (WorkflowFailState)o;
                    EditFailStateDialog dlg = new EditFailStateDialog(fs);
                    dlg.ShowDialog(); // blocking
                    if (dlg.wasOkay())
                    {
                        WorkflowFailState editedFailState = dlg.EditedFailState;
                        //
                    }
                }
            }
        }

        private void m_WorkflowListview_GotFocus(object sender, RoutedEventArgs e)
        {
            //TODO Refactor to be DRY
            // update the SceneManager here
            Object o = m_WorkflowListview.SelectedItem;
            if (o != null)
            {
                if (o is WorkingStep)
                {
                    m_FailStatesListView.IsEnabled = true;
                    WorkingStep step = (WorkingStep)o;
                    //SceneManager.Instance.CurrentScene = step.AdaptiveScenes.FirstOrDefault().Scene;
                    if (m_ComboboxAdaptivityLevel.SelectedValue != null)
                    {
                        SceneManager.Instance.CurrentScene = step.getAdaptiveScene((int)m_ComboboxAdaptivityLevel.SelectedValue).Scene;
                        m_FailStatesListView.ItemsSource = step.FailStates;
                    }
                }
            }
            else
            {
                m_FailStatesListView.IsEnabled = false;
                m_FailStatesListView.ItemsSource = null;
            }
        }

        private void m_FailstatesListview_GotFocus(object sender, SelectionChangedEventArgs e)
        {
            //TODO Refactor to be DRY
            Object o = m_FailStatesListView.SelectedItem;
            if (o != null)
            {
                if (o is WorkflowFailState)
                {
                    WorkflowFailState fState = (WorkflowFailState)o;
                    SceneManager.Instance.CurrentScene = fState.Scene;
                }
            }
        }

        private void createFromTableButton_Click(object sender, RoutedEventArgs e)
        {
            Object o = listBoxNetworkTables.SelectedItem;

            if (o != null)
            {
                if (o is TableInfo)
                {
                    TableInfo tableInfo = (TableInfo)o;
                    Scene.Scene autoScene;
                    autoScene = new Scene.Scene();

                    AdaptiveScene aScene = new AdaptiveScene(autoScene, AdaptivityLevel.AdaptivityLevels.FirstOrDefault());

                    WorkingStep step = new WorkingStep();
                    step.AdaptiveScenes.Add(aScene);
                    step.EndConditionObjectName = "net" + tableInfo.Id;

                    EditWorkflowManager.Instance.createStep(HciLab.KoBeLU.InterfacesAndDataModel.AllEnums.PBD_Mode.NETWORK_TABLE_DONE, aScene, "NetTable-" + tableInfo.Id, step.EndConditionObjectName);
                }
            }
        }

        private void MenuItemEditWorkingStep_Click(object sender, RoutedEventArgs e)
        {
            Object o = m_WorkflowListview.SelectedItem;

            if (o != null)
            {
                if (o is WorkingStep)
                {
                    WorkingStep step = (WorkingStep)o;
                    EditWorkingStepDialog dlg = new EditWorkingStepDialog(step);
                    dlg.ShowDialog(); // blocking
                    if (dlg.wasOkay())
                    {
                        step.Name = dlg.EditedName;
                        step.EndConditionObjectName = dlg.EditedEndCondition;
                        step.TimeOut = dlg.EditedTimeOut;
                        step.ExpectedDuration = dlg.EditedExpectedDuration;
                        step.IsManualStep = dlg.IsManualStep;
                        step.IsQSStep = dlg.IsQSStep;
                        //
                    }
                }
            }

        }

        private void NetworkTableMenuItemSetTrigger_Cick(object sender, RoutedEventArgs e)
        {
            Object o = listBoxNetworkTables.SelectedItem;
            if (m_WorkflowListview.SelectedIndex >= 0 && o is TableInfo)
            {
                WorkingStep selectedWorkingStep = (WorkingStep)m_WorkflowListview.SelectedItem;
                TableInfo selectedNetworkTable = (TableInfo)o;
                //Create new Failstate
                selectedWorkingStep.EndConditionObjectName = "net" + selectedNetworkTable.Id;
            }

        }

        private void MenuItemCopyScene_Click(object sender, RoutedEventArgs e)
        {
            Object o = m_WorkflowListview.SelectedItem;

            if (o != null)
            {
                if (o is WorkingStep)
                {
                    WorkingStep step = (WorkingStep)o;
                    Scene.Scene scene = new Scene.Scene();
                    if (m_ComboboxAdaptivityLevel.SelectedValue != null)
                    {
                        scene = step.getAdaptiveScene((int)m_ComboboxAdaptivityLevel.SelectedValue).Scene;
                    }
                    scene.CopyToClipboard();
                }
            }
        }

        private void ContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            FrameworkElement fe = e.Source as FrameworkElement;
            ContextMenu cm = fe.ContextMenu;
            foreach (MenuItem mi in cm.Items)
            {
                if ((String)mi.Header == "Paste Scene" && m_ComboboxAdaptivityLevel.SelectedValue != null)
                {
                    mi.IsEnabled = false;
                    if (Clipboard.ContainsData(typeof(Scene.Scene).FullName)) mi.IsEnabled = true;
                }
            }
        }

        private void MenuItemPasteScene_Click(object sender, RoutedEventArgs e)
        {
            Object o = m_WorkflowListview.SelectedItem;

            if (o != null)
            {
                if (o is WorkingStep)
                {
                    WorkingStep step = (WorkingStep)o;
                    Scene.Scene scene = Scene.Scene.GetFromClipboard();
                    if (m_ComboboxAdaptivityLevel.SelectedValue != null && scene is Scene.Scene)
                    {
                        step.getAdaptiveScene((int)m_ComboboxAdaptivityLevel.SelectedValue).Scene = scene;
                        SceneManager.Instance.CurrentScene = scene;
                    }
                }
            }

        }

        private void MenuItemCopyErrorScene_Click(object sender, RoutedEventArgs e)
        {
            Object o = m_FailStatesListView.SelectedItem;
            if (o != null)
            {
                if (o is WorkflowFailState)
                {
                    WorkflowFailState fState = (WorkflowFailState)o;
                    Scene.Scene scene = fState.Scene;

                    scene.CopyToClipboard();
                }
            }
        }

        private void MenuItemPasteErrorScene_Click(object sender, RoutedEventArgs e)
        {
            Object o = m_FailStatesListView.SelectedItem;
            if (o != null)
            {
                if (o is WorkflowFailState)
                {
                    WorkflowFailState fState = (WorkflowFailState)o;
                    Scene.Scene scene = Scene.Scene.GetFromClipboard();

                    fState.Scene = scene;

                    SceneManager.Instance.CurrentScene = scene;
                }
            }
        }

        private void createFromObjectButton_Click(object sender, RoutedEventArgs e)
        {
            Object o = m_listBoxObjectZones.SelectedItem;

            if (o != null)
            {
                if (o is ObjectDetectionZone)
                {
                    ObjectDetectionZone ob = (ObjectDetectionZone)o;

                    AdaptiveScene easyScene = new AdaptiveScene(EditWorkflowManager.Instance.getObjectAutoScene(ob), AdaptivityLevel.AdaptivityLevels.First());
                    AdaptiveScene mediumScene = new AdaptiveScene(EditWorkflowManager.Instance.getObjectAutoScene(ob), AdaptivityLevel.AdaptivityLevels.ElementAt(1));

                    var adaptiveScenes = new List<AdaptiveScene> { easyScene, mediumScene };

                    EditWorkflowManager.Instance.createStep(HciLab.KoBeLU.InterfacesAndDataModel.AllEnums.PBD_Mode.ASSEMBLY_DONE, adaptiveScenes, "Object-" + ob.Id, ob.TriggerMessage);
                }
            }
        }
    }

}
