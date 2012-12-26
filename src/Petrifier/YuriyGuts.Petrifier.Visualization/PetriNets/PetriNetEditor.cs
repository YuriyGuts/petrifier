using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using YuriyGuts.Petrifier.Core.Helpers;
using YuriyGuts.Petrifier.Core.PetriNets;

namespace YuriyGuts.Petrifier.Visualization.PetriNets
{
    public partial class PetriNetEditor : UserControl, IWidgetLocator
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<PetriNetElementWidget> Widgets { get; private set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<PetriNetElementWidget> SelectedWidgets { get; set; }

        private bool IsInRectangleSelectionMode { get; set; }

        private bool IsInElementDragMode { get; set; }

        private Point RectangleSelectionStart { get; set; }

        private Point RectangleSelectionEnd { get; set; }

        private Type SelectedToolType { get; set; }

        private AutoIncrementIDProvider ElementIDProvider { get; set; }

        public event EventHandler WidgetsChanged;

        public event EventHandler SelectedWidgetsChanged;

        public PetriNetEditor()
        {
            InitializeComponent();
            InitializeTools();

            ElementIDProvider = new AutoIncrementIDProvider();

            Widgets = new List<PetriNetElementWidget>();
            SelectedWidgets = new List<PetriNetElementWidget>();

            UpdateControlAvailability();
        }

        private void InitializeTools()
        {
            btnPlace.Tag = typeof(PlaceWidget);
            btnTransition.Tag = typeof(TransitionWidget);
            btnAndSplit.Tag = typeof(AndSplitWidget);
            btnOrSplit.Tag = typeof(OrSplitWidget);
            btnAndJoin.Tag = typeof(AndJoinWidget);
            btnOrJoin.Tag = typeof(OrJoinWidget);
            btnSubprocess.Tag = typeof(SubprocessWidget);
            SelectedToolType = null;
        }

        public PetriNetElementWidget AddWidget(Type widgetType)
        {
            var newWidget = (PetriNetElementWidget)Activator.CreateInstance(widgetType);
            var newElement = (PetriNetElement)Activator.CreateInstance(newWidget.ElementType);
            newWidget.Element = newElement;

            string elementTypeName = newElement.GetType().Name;
            string elementId;
            do
            {
                elementId = elementTypeName + ElementIDProvider.GetNextID(elementTypeName);
            }
            while (Widgets.Exists(widget => widget.Element != null && widget.Element.ID == elementId));

            newElement.ID = elementId;
            if (newElement is ILabeledElement)
            {
                (newElement as ILabeledElement).Label = newElement.ID;
            }

            newWidget.Location = new Point(30, 30);

            Widgets.Add(newWidget);
            SelectedWidgets.Clear();
            SelectedWidgets.Add(newWidget);
            SelectedToolType = null;

            HandleWidgetsChanged();
            HandleSelectedWidgetsChanged();
            UpdateControlAvailability();

            RedrawCanvas();
            return newWidget;
        }

        public void LoadPetriNet(PetriNet petriNet)
        {
            SelectedWidgets.Clear();
            Widgets.Clear();

            petriNet.UpdateElementRelationships();
            foreach (var element in petriNet.Elements)
            {
                var widget = CreateWidgetForElement(element);
                widget.Location = new Point
                (
                    (int)element.GetGraphicsMetadataItem("graphics.position.@x", 30),
                    (int)element.GetGraphicsMetadataItem("graphics.position.@y", 30)
                );
                Widgets.Add(widget);
            }

            HandleWidgetsChanged();
            HandleSelectedWidgetsChanged();
            UpdateControlAvailability();
            RedrawCanvas();
        }

        public PetriNet ExportToPetriNet()
        {
            var result = new PetriNet();
            foreach (var widget in Widgets)
            {
                if (!(widget is ArcWidget))
                {
                    widget.Element.SetGraphicsMetadataItem("graphics.position.@x", widget.Location.X);
                    widget.Element.SetGraphicsMetadataItem("graphics.position.@y", widget.Location.Y);
                    widget.Element.SetGraphicsMetadataItem("graphics.dimension.@x", widget.Size);
                    widget.Element.SetGraphicsMetadataItem("graphics.dimension.@y", widget.Size);
                }
                result.Elements.Add(widget.Element);
            }
            result.UpdateElementRelationships();
            return result;
        }

        private PetriNetElementWidget CreateWidgetForElement(PetriNetElement element)
        {
            Type elementType = element.GetType();
            if (elementType == typeof(Place))
                return new PlaceWidget(element);
            if (elementType == typeof(Transition))
                return new TransitionWidget(element);
            if (elementType == typeof(AndSplit))
                return new AndSplitWidget(element);
            if (elementType == typeof(AndJoin))
                return new AndJoinWidget(element);
            if (elementType == typeof(OrSplit))
                return new OrSplitWidget(element);
            if (elementType == typeof(OrJoin))
                return new OrJoinWidget(element);
            if (elementType == typeof(Subprocess))
                return new SubprocessWidget(element);
            if (elementType == typeof(Arc))
                return new ArcWidget(element);
            return null;
        }

        public WidgetBase GetSelectedWidgetAtPoint(Point point)
        {
            return GetWidgetZIndexEnumerator(false).FirstOrDefault(widget => GeometryHelper.IsPointInsideRectangle(point, widget.GetBoundingRectangle()) && SelectedWidgets.Contains(widget));
        }

        private Rectangle GetSelectionRectangle()
        {
            return new Rectangle
            (
                new Point
                (
                    Math.Min(RectangleSelectionStart.X, RectangleSelectionEnd.X),
                    Math.Min(RectangleSelectionStart.Y, RectangleSelectionEnd.Y)
                ),
                new Size
                (
                    Math.Abs(RectangleSelectionStart.X - RectangleSelectionEnd.X),
                    Math.Abs(RectangleSelectionStart.Y - RectangleSelectionEnd.Y)
                )
            );
        }

        private void RedrawCanvas()
        {
            imgCanvas.Invalidate();
        }

        private void HandleWidgetsChanged()
        {
            OnWidgetsChanged(EventArgs.Empty);
        }

        private void HandleSelectedWidgetsChanged()
        {
            UpdateControlAvailability();
            focusContainer.Focus();
            OnSelectedWidgetsChanged(EventArgs.Empty);
        }

        private void UpdateControlAvailability()
        {
            btnConnect.Enabled = CanConnectSelectedWidgets();
            btnDisconnect.Enabled = CanDisconnectSelectedWidgets();
            btnInverseConnection.Enabled = CanInvertConnectionBetweenSelectedWidgets();
            btnDelete.Enabled = CanDeleteSelectedWidgets();

            btnPointer.Checked = (SelectedToolType == null);
            foreach (var item in tlbEditorActions.Items)
            {
                if (item is ToolStripButton)
                {
                    var button = item as ToolStripButton;
                    var taggedType = (Type)button.Tag;
                    if (taggedType != null)
                    {
                        button.Checked = taggedType == SelectedToolType;
                    }
                }
            }
        }

        private bool CanConnectSelectedWidgets()
        {
            return (SelectedWidgets.Count == 2 && CanWidgetsBeConnected(SelectedWidgets[0], SelectedWidgets[1]));
        }

        private bool CanDisconnectSelectedWidgets()
        {
            return (SelectedWidgets.Count == 2 && CanWidgetsBeDisconnected(SelectedWidgets[0], SelectedWidgets[1]));
        }

        private bool CanInvertConnectionBetweenSelectedWidgets()
        {
            return (SelectedWidgets.Count == 2 && CanWidgetConnectionBeInverted(SelectedWidgets[0], SelectedWidgets[1]));
        }

        private bool CanDeleteSelectedWidgets()
        {
            return SelectedWidgets.Count > 0;
        }

        private bool CanWidgetsBeConnected(PetriNetElementWidget widget1, PetriNetElementWidget widget2)
        {
            return GetArcBetweenWidgets(widget1, widget2) == null
                && ((widget1.Element != null && widget1.Element is ITokenContainer && widget2.Element != null && widget2.Element is ITokenConsumer)
                    || (widget1.Element != null && widget1.Element is ITokenConsumer && widget2.Element != null && widget2.Element is ITokenContainer));
        }

        private bool CanWidgetsBeDisconnected(PetriNetElementWidget widget1, PetriNetElementWidget widget2)
        {
            return GetArcBetweenWidgets(widget1, widget2) != null;
        }

        private bool CanWidgetConnectionBeInverted(PetriNetElementWidget widget1, PetriNetElementWidget widget2)
        {
            var arcWidget = GetArcBetweenWidgets(widget1, widget2);
            if (arcWidget != null)
            {
                var arc = arcWidget.Element as Arc;
                if (arc != null
                    && ((arc.StartElement == widget1.Element && widget1.Element is ITokenContainer && widget2.Element != null && widget2.Element is ITokenDispatcher)
                        || (arc.StartElement == widget2.Element && widget2.Element is ITokenContainer && widget1.Element != null && widget1.Element is ITokenDispatcher)
                        || (arc.StartElement == widget1.Element && widget1.Element is ITokenConsumer && widget2.Element != null && widget2.Element is ITokenContainer)
                        || (arc.StartElement == widget2.Element && widget2.Element is ITokenConsumer && widget1.Element != null && widget1.Element is ITokenContainer)
                        ))
                {
                    return true;
                }
            }
            return false;
        }

        private ArcWidget GetArcBetweenWidgets(PetriNetElementWidget widget1, PetriNetElementWidget widget2)
        {
            var result = Widgets.FirstOrDefault
                (widget =>
                    widget is ArcWidget
                    && ((((Arc)widget.Element).StartElement == widget1.Element && ((Arc)widget.Element).EndElement == widget2.Element)
                        || (((Arc)widget.Element).StartElement == widget2.Element && ((Arc)widget.Element).EndElement == widget1.Element))
                );
            return result as ArcWidget;
        }

        private void ConnectSelectedWidgets()
        {
            Arc arc = new Arc(SelectedWidgets[0].Element, SelectedWidgets[1].Element);
            do
            {
                arc.ID = "arc" + ElementIDProvider.GetNextID("arc");
            }
            while (Widgets.Exists(widget => widget.Element != null && widget.Element.ID == arc.ID));

            ArcWidget arcWidget = new ArcWidget(arc);
            Widgets.Add(arcWidget);
            HandleWidgetsChanged();

            RedrawCanvas();
            UpdateControlAvailability();
        }

        private void DisconnectSelectedWidgets()
        {
            var arcWidget = GetArcBetweenWidgets(SelectedWidgets[0], SelectedWidgets[1]);
            if (arcWidget != null)
            {
                SelectedWidgets.Remove(arcWidget);
                Widgets.Remove(arcWidget);
                HandleWidgetsChanged();
                HandleSelectedWidgetsChanged();

                RedrawCanvas();
                UpdateControlAvailability();
            }
        }

        private void InvertConnectionForSelectedWidgets()
        {
            var arcWidget = GetArcBetweenWidgets(SelectedWidgets[0], SelectedWidgets[1]);
            if (arcWidget != null)
            {
                Arc arc = (Arc)arcWidget.Element;
                PetriNetElement oldStartElement = arc.StartElement;
                arc.StartElement = arc.EndElement;
                arc.EndElement = oldStartElement;

                RedrawCanvas();
                UpdateControlAvailability();
            }
        }

        private void DeleteSelectedWidgets()
        {
            var arcs = Widgets.Where
                (widget =>
                 widget is ArcWidget
                 && SelectedWidgets.Any
                        (selectedWidget =>
                         selectedWidget.Element == ((Arc)widget.Element).StartElement
                         || selectedWidget.Element == ((Arc)widget.Element).EndElement
                        )
                ).ToList();

            foreach (var arc in arcs)
            {
                Widgets.Remove(arc);
            }

            foreach (var selectedWidget in SelectedWidgets)
            {
                Widgets.Remove(selectedWidget);
            }

            SelectedWidgets.Clear();
            HandleWidgetsChanged();
            HandleSelectedWidgetsChanged();

            RedrawCanvas();
        }

        private void HandleToolSelected(object sender, EventArgs e)
        {
            SelectedToolType = (Type)((ToolStripButton)sender).Tag;
            UpdateControlAvailability();
        }

        private IEnumerable<PetriNetElementWidget> GetWidgetZIndexEnumerator(bool isAscending)
        {
            Func<PetriNetElementWidget, int> zIndexCalculator = (widget => (SelectedWidgets.Contains(widget) ? 1000000 : 0) + Widgets.IndexOf(widget));
            if (isAscending)
            {
                return Widgets.OrderBy(zIndexCalculator);
            }
            return Widgets.OrderByDescending(zIndexCalculator);
        }

        protected virtual void OnWidgetsChanged(EventArgs e)
        {
            var handler = WidgetsChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnSelectedWidgetsChanged(EventArgs e)
        {
            var handler = SelectedWidgetsChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void imgCanvas_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            foreach (var widget in GetWidgetZIndexEnumerator(true))
            {
                widget.Render(new WidgetRenderingContext
                    {
                        Graphics = e.Graphics,
                        TextFont = Font,
                        WidgetLocator = this,
                        IsSelected = (SelectedWidgets.Contains(widget)),
                    });
            }

            if (IsInRectangleSelectionMode)
            {
                using (Pen selectionPen = new Pen(Color.DodgerBlue, 2F))
                {
                    selectionPen.DashStyle = DashStyle.Dot;
                    e.Graphics.DrawRectangle(selectionPen, GetSelectionRectangle());
                }
            }
        }

        private void imgCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (SelectedToolType != null)
                {
                    var newWidget = AddWidget(SelectedToolType);
                    newWidget.Location = e.Location;
                    RedrawCanvas();
                    return;
                }

                var widgetUnderPointer = (PetriNetElementWidget)GetWidgetAtPoint(e.Location);
                if (ModifierKeys.HasFlag(Keys.Control))
                {
                    if (widgetUnderPointer != null)
                    {
                        if (SelectedWidgets.Contains(widgetUnderPointer))
                        {
                            SelectedWidgets.Remove(widgetUnderPointer);
                        }
                        else
                        {
                            SelectedWidgets.Add(widgetUnderPointer);
                        }
                        HandleSelectedWidgetsChanged();
                    }
                }
                else
                {
                    if (widgetUnderPointer == null)
                    {
                        SelectedWidgets.Clear();
                        IsInRectangleSelectionMode = true;
                        RectangleSelectionStart = RectangleSelectionEnd = e.Location;
                        HandleSelectedWidgetsChanged();
                    }
                    if (widgetUnderPointer != null && !SelectedWidgets.Contains(widgetUnderPointer))
                    {
                        SelectedWidgets.Clear();
                        SelectedWidgets.Add(widgetUnderPointer);
                        HandleSelectedWidgetsChanged();
                    }
                }
                RedrawCanvas();
            }
        }

        private void imgCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseButtons.HasFlag(MouseButtons.Left))
            {
                if (IsInRectangleSelectionMode)
                {
                    RectangleSelectionEnd = e.Location;
                }

                if (SelectedWidgets.Count > 0)
                {
                    Cursor = Cursors.SizeAll;

                    var widgetUnderPointer = (PetriNetElementWidget)GetSelectedWidgetAtPoint(e.Location);
                    if (widgetUnderPointer != null)
                    {
                        if (IsInElementDragMode)
                        {
                            int locationDiffX = e.Location.X - widgetUnderPointer.Location.X;
                            int locationDiffY = e.Location.Y - widgetUnderPointer.Location.Y;

                            foreach (var selectedWidget in SelectedWidgets)
                            {
                                selectedWidget.Location = new Point(selectedWidget.Location.X + locationDiffX, selectedWidget.Location.Y + locationDiffY);
                            }
                        }
                        else
                        {
                            IsInElementDragMode = true;
                        }
                    }
                }

                RedrawCanvas();
            }
        }

        private void imgCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Cursor = Cursors.Default;

                RectangleSelectionEnd = e.Location;
                if (IsInRectangleSelectionMode)
                {
                    if (!ModifierKeys.HasFlag(Keys.Control))
                    {
                        SelectedWidgets.Clear();
                        HandleSelectedWidgetsChanged();
                    }

                    Rectangle selectionRectangle = GetSelectionRectangle();
                    foreach (var widget in Widgets)
                    {
                        if (widget is ArcWidget)
                        {
                            continue;
                        }

                        bool selectionChanged = false;
                        if (GeometryHelper.IsPointInsideRectangle(widget.Location, selectionRectangle))
                        {
                            SelectedWidgets.Add(widget);
                            selectionChanged = true;
                        }
                        if (selectionChanged)
                        {
                            HandleSelectedWidgetsChanged();
                        }
                    }
                    RedrawCanvas();
                }
                IsInRectangleSelectionMode = false;
                IsInElementDragMode = false;
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            ConnectSelectedWidgets();
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            DisconnectSelectedWidgets();
        }

        private void btnInverseConnection_Click(object sender, EventArgs e)
        {
            InvertConnectionForSelectedWidgets();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DeleteSelectedWidgets();
        }

        private void focusContainer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && e.Modifiers == Keys.None)
            {
                if (CanDeleteSelectedWidgets())
                {
                    DeleteSelectedWidgets();
                }
            }
            if (e.KeyCode == Keys.Add && e.Modifiers == Keys.Control)
            {
                if (CanConnectSelectedWidgets())
                {
                    ConnectSelectedWidgets();
                }
            }
            if (e.KeyCode == Keys.Subtract && e.Modifiers == Keys.Control)
            {
                if (CanDisconnectSelectedWidgets())
                {
                    DisconnectSelectedWidgets();
                }
            }
            if (e.KeyCode == Keys.Multiply && e.Modifiers == Keys.Control)
            {
                if (CanInvertConnectionBetweenSelectedWidgets())
                {
                    InvertConnectionForSelectedWidgets();
                }
            }
        }

        #region IWidgetLocator Members

        public WidgetBase FindWidget(string id)
        {
            return Widgets.FirstOrDefault(widget => widget.Element != null && widget.Element.ID == id);
        }

        public WidgetBase GetWidgetAtPoint(Point point)
        {
            return GetWidgetZIndexEnumerator(false).FirstOrDefault(widget => GeometryHelper.IsPointInsideRectangle(point, widget.GetBoundingRectangle()));
        }

        #endregion

        private void PetriNetEditor_Resize(object sender, EventArgs e)
        {
            imgCanvas.Width = Width * 2;
            imgCanvas.Height = Height * 2;
        }
    }
}
