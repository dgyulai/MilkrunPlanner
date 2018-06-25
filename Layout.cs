using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.VisualBasic.PowerPacks;
using System.Drawing;
using System.Windows.Forms;

namespace MilkRunApp_v3
{
    [Serializable]
    public class RouteNode
    {
        [XmlAttribute]
        public string id;
        [XmlAttribute]
        public string x_coord;
        [XmlAttribute]
        public string y_coord;
        [XmlIgnore]
        public int label;
        [XmlIgnore]
        public bool labeltype; //Igaz, ha a címke állandóvá válik a Dijkstra során
        [XmlIgnore]
        public RichTextBox display;
        [XmlIgnore]
        public RouteNode parentNode;
        [XmlIgnore]
        public RouteNode nextNode;
        [XmlIgnore]
        public int depthInTree;
        [XmlIgnore]
        public RouteNode previousNode;
        [XmlIgnore]
        public TextBox boschNode_1;
        [XmlIgnore]
        public TextBox boschNode_2;

        public override string ToString()
        {
            return id;
        }

        public void drawRouteNode(int x, int y, List<OvalShape> drawedRoutenodes, Panel parent)
        {
            ShapeContainer canvas = new ShapeContainer();
            OvalShape oval = new OvalShape();

            canvas.Parent = parent;
            oval.Parent = canvas;
            oval.FillStyle = FillStyle.Solid;
            oval.FillColor = Color.LightSlateGray;

            oval.Size = new System.Drawing.Size(9, 9);
            oval.Location = new System.Drawing.Point(x - 2, y - 2);

            oval.Click += new EventHandler(oval_Click);

            drawedRoutenodes.Add(oval);
        }

        void oval_Click(object sender, EventArgs e)
        {
            display.Text = "ID: " + id + "\n" +
                    "X coordinate: " + x_coord + "\n" +
                    "Y coordinate: " + y_coord;
        }

        void boschPlanner(object sender, EventArgs e)
        {
                boschNode_2.Text = "ST_" + id;
        }
    }

    [Serializable]
    public class Station
    {
        [XmlAttribute]
        public string id;
        [XmlIgnore]
        public Route route;
        [XmlAttribute]
        public string route_id;
        [XmlAttribute]
        public string type;
        [XmlAttribute]
        public string workcell;
        [XmlIgnore]
        public string x_coord;
        [XmlIgnore]
        public string y_coord;
        [XmlIgnore]
        public RichTextBox display;
        [XmlIgnore]
        public RouteNode closestNode;
        [XmlIgnore]
        public RouteNode prevNode;
        [XmlIgnore]
        public RouteNode nextNode;


        public void CalculateXY(RouteNode node_1, RouteNode node_2)
        {
            this.x_coord = ((int.Parse(node_1.x_coord) + int.Parse(node_2.x_coord)) / 2).ToString();
            this.y_coord = ((int.Parse(node_1.y_coord) + int.Parse(node_2.y_coord)) / 2).ToString();
        }

        public override string ToString()
        {
            return id;
        }

        public void drawStation(int x, int y, string type, List<OvalShape> drawedStations, Panel parent)
        {
            ShapeContainer canvas = new ShapeContainer();
            OvalShape oval = new OvalShape();

            canvas.Parent = parent;
            oval.Parent = canvas;
            oval.FillStyle = FillStyle.Solid;

            switch (type)
            {
                case "warehouse":
                    oval.FillColor = Color.Black;
                    break;
                case "loading":
                    oval.FillColor = Color.LimeGreen;
                    break;
                case "unloading":
                    oval.FillColor = Color.LightSalmon;
                    break;
                case "uni":
                    oval.FillColor = Color.LimeGreen;
                    break;
                default:
                    oval.FillColor = Color.DarkSlateBlue;
                    break;
            }

            oval.Size = new System.Drawing.Size(12, 12);
            oval.Location = new System.Drawing.Point(x, y);

            oval.Click += new EventHandler(clickStation);

            drawedStations.Add(oval);
        }

        public void parseRouteId()
        {
            route_id = route.id;
        }

        private void clickStation(object sender, EventArgs e)
        {
            display.Text = "ID: " + id + "\n" +
                    "Route: " + route.id + "\n" +
                    "Type: " + type + "\n" +
                    "Workcell info: " + workcell;
        }

        public void calculateClosestNode()
        {
            if (route.direction == "uni")
            {
                closestNode = route.node_1;
                nextNode = route.node_2;
            }
            else if (route.direction == "1_2")
            {
                closestNode = route.node_1;
                nextNode = route.node_2;
            }
            else if (route.direction == "2_1")
            {
                closestNode = route.node_2;
                nextNode = route.node_1;
            }
        }
    }

    [Serializable]
    public class Route
    {
        [XmlAttribute]
        public string id;
        [XmlIgnore]
        public RouteNode node_1;
        [XmlIgnore]
        public RouteNode node_2;
        [XmlAttribute]
        public string node_1_id;
        [XmlAttribute]
        public string node_2_id;
        [XmlAttribute]
        public string direction;
        [XmlIgnore]
        public RichTextBox display;
        [XmlIgnore]
        public double length;
        [XmlIgnore]
        public TextBox boschStation_1;
        [XmlIgnore]
        public TextBox boschStation_2;
        [XmlIgnore]
        public double midX;
        [XmlIgnore]
        public double midY;

        public override string ToString()
        {
            return id;
        }

        public void drawRoute(int fromX, int fromY, int toX, int toY, List<LineShape> drawedRoutes, Panel parent)
        {
            ShapeContainer canvas = new ShapeContainer();
            LineShape line = new LineShape();

            canvas.Parent = parent;
            line.Parent = canvas;

            switch (direction)
            {
                case "uni":
                    line.BorderColor = System.Drawing.Color.Orange;
                    break;
                case "1_2":
                    line.BorderColor = System.Drawing.Color.Blue;
                    break;
                case "2_1":
                    line.BorderColor = System.Drawing.Color.Blue;
                    break;
                default:
                    line.BorderColor = System.Drawing.Color.Green;
                    break;
            }

            line.BorderWidth = 3;
            line.StartPoint = new System.Drawing.Point(fromX, fromY);
            line.EndPoint = new System.Drawing.Point(toX, toY);

            line.Click += new EventHandler(routeClick);

            drawedRoutes.Add(line);
        }

        void boschPlanner2(object sender, EventArgs e)
        {
            boschStation_1.Text = "";
            boschStation_1.Text = id;
        }

        private void routeClick(object sender, EventArgs e)
        {
            display.Text = "ID: " + id + "\n" +
                            "Node 1: " + node_1 + "\n" +
                            "Node 2: " + node_2 + "\n" +
                            "Direction: " + direction;
        }

        public void calculateLength()
        {
            this.length = Math.Sqrt(Math.Pow((int.Parse(node_2.x_coord) - int.Parse(node_1.x_coord)), 2) + Math.Pow((int.Parse(node_2.y_coord) - int.Parse(node_1.y_coord)), 2));
        }

        public void parseNodeNames()
        {
            node_1_id = node_1.id;
            node_2_id = node_2.id;
        }

        public void calculateMidPoint()
        {
            midX = (int.Parse(node_1.x_coord) + int.Parse(node_2.x_coord)) / 2;
            midY = (int.Parse(node_1.y_coord) + int.Parse(node_2.y_coord)) / 2;
        }
    }
}
