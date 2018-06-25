using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Drawing.Drawing2D;
using System.Collections;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualBasic.PowerPacks;
using System.Threading;
using Excel = Microsoft.Office.Interop.Excel;
using System.Windows.Forms.DataVisualization.Charting;
using System.Runtime.Serialization.Formatters.Binary;
using MoreLinq;
using System.Globalization;



namespace MilkRunApp_v3
{
    /// <summary>
    /// The user interface of the MilkRun application
    /// </summary>
    /// 


    public partial class Form1 : Form
    {
        string layoutPath;
        public string selectedElement = "";
        List<Station> stations = new List<Station>();
        List<Route> routes = new List<Route>();
        List<RouteNode> routenodes = new List<RouteNode>();
        List<Demand> demands = new List<Demand>();
        List<Vehicle> vehicles = new List<Vehicle>();
        List<OvalShape> drawedRoutenodes = new List<OvalShape>();
        List<LineShape> drawedRoutes = new List<LineShape>();
        List<OvalShape> drawedStations = new List<OvalShape>();
        List<Path> initPaths = new List<Path>();
        List<Path> paths = new List<Path>();
        List<Path> improvedPaths = new List<Path>();
        List<Path> bestPlan = new List<Path>();
        RouteNode added_rn;
        List<List<RouteNode>> shortestPathNodes;
        List<List<Station>> shortestPathStations;
        List<List<Route>> shortestPathRoutes;
        Process currentProcess = Process.GetCurrentProcess(); // memory usage
        public static List<Route>[,][] stationToStation;
        int numOfIterations;
        double bestPlanTime = int.MaxValue;
        int debug;
        int debug2;
        List<double> sumTimesOfPlan;
        List<double> bestOfSumTimesOfPlan;
        int backgroundWorkerProgress = 0;
        double totalTimeOfPlan;
        List<double> testImprovePlan;

        /// <summary>
        /// Initializes a new instance of the <see cref="Form1"/> class.
        /// </summary>
        /// 
        public Form1()
        {
            InitializeComponent();

        }

        /// <summary>
        /// Handles the Click event of the exitToolStripMenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// 
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Imports the layout XML file, browsed by the user
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// 
        private void button1_Click(object sender, EventArgs e)
        {
            demands.Clear();
            XmlDocument doc = new XmlDocument();
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "XML Files|*.xml";

            DialogResult dlgResult = ofd.ShowDialog();
            String FileName;
            XmlTextReader tr;


            try
            {
                if (dlgResult == DialogResult.OK)
                {
                    doc.Load(ofd.FileName);
                    FileName = ofd.FileName; ;
                    tr = new XmlTextReader(FileName);

                    while (tr.Read())
                    {
                        Station station = new Station();
                        Route route = new Route();
                        RouteNode routenode = new RouteNode();

                        if (tr.MoveToContent() == XmlNodeType.Element && tr.Name == "Station")
                        {
                            station.id = tr.GetAttribute(0);
                            station.route = routes[routes.FindIndex(s => s.id == tr.GetAttribute(1))];
                            station.type = tr.GetAttribute(2);
                            station.workcell = tr.GetAttribute(3);
                            station.display = richTextBox1;
                            stations.Add(station);
                        }

                        if (tr.MoveToContent() == XmlNodeType.Element && tr.Name == "Route")
                        {
                            route.id = tr.GetAttribute(0);
                            route.node_1 = routenodes[routenodes.FindIndex(s => s.id == tr.GetAttribute(1))];
                            route.node_2 = routenodes[routenodes.FindIndex(s => s.id == tr.GetAttribute(2))];
                            route.direction = tr.GetAttribute(3);
                            route.display = richTextBox1;
                            route.calculateLength();
                            routes.Add(route);
                        }

                        if (tr.MoveToContent() == XmlNodeType.Element && tr.Name == "RouteNode")
                        {
                            routenode.id = tr.GetAttribute(0);
                            routenode.x_coord = tr.GetAttribute(1);
                            routenode.y_coord = tr.GetAttribute(2);
                            routenode.display = richTextBox1;
                            routenodes.Add(routenode);
                        }
                    }

                    foreach (Route rt in routes)
                    {
                        int fx, fy, tx, ty;
                        int fxid, fyid, txid, tyid;

                        fxid = routenodes.FindIndex(s => s.id == rt.node_1.id);
                        fyid = routenodes.FindIndex(s => s.id == rt.node_1.id);
                        txid = routenodes.FindIndex(s => s.id == rt.node_2.id);
                        tyid = routenodes.FindIndex(s => s.id == rt.node_2.id);

                        fx = int.Parse(routenodes[fxid].x_coord);
                        fy = int.Parse(routenodes[fyid].y_coord);
                        tx = int.Parse(routenodes[txid].x_coord);
                        ty = int.Parse(routenodes[tyid].y_coord);

                        rt.drawRoute(fx, fy, tx, ty, drawedRoutes, panel1);
                    }

                    foreach (RouteNode rn in routenodes)
                    {
                        rn.drawRouteNode(int.Parse(rn.x_coord), int.Parse(rn.y_coord), drawedRoutenodes, panel1);
                    }

                    foreach (Station station in stations)
                    {
                        int route_ind;
                        string n1, n2;
                        int f_id, t_id;

                        route_ind = routes.FindIndex(s => s.id == station.route.id);
                        n1 = routes[route_ind].node_1.id;
                        n2 = routes[route_ind].node_2.id;

                        f_id = routenodes.FindIndex(s => s.id == n1);
                        t_id = routenodes.FindIndex(s => s.id == n2);

                        station.CalculateXY(routenodes[f_id], routenodes[t_id]);
                        station.drawStation(int.Parse(station.x_coord), int.Parse(station.y_coord), station.type, drawedStations, panel1);
                        station.calculateClosestNode();
                    }

                    treeView1.BeginUpdate();
                    treeView1.Nodes.Add(doc.DocumentElement.Name);
                    treeView1.Nodes[0].Nodes.Add("Stations");
                    treeView1.Nodes[0].Nodes.Add("Routes");
                    treeView1.Nodes[0].Nodes.Add("Route nodes");

                    foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                    {
                        TreeNode station = new TreeNode(node.Attributes["id"].Value);
                        int index;
                        string newid = node.Name.ToString();

                        switch (node.Name.ToString())
                        {
                            case "Station":
                                index = 0;
                                break;

                            case "Route":
                                index = 1;
                                break;

                            case "RouteNode":
                                index = 2;
                                break;

                            default:
                                index = 0;
                                break;
                        }

                        treeView1.Nodes[0].Nodes[index].Nodes.Add(station);
                        {
                            foreach (XmlNode childnode in node.ChildNodes)
                            {
                                TreeNode n2 = new TreeNode(childnode.Name + " : " + childnode.InnerText);
                                station.Nodes[0].Nodes[1].Nodes.Add(n2);
                            }
                        }
                    }
                    treeView1.EndUpdate();
                    dataGridViewRefresh();
                    button3.Enabled = true;
                    panel1.BringToFront();
                    GetMemoryUsage();
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
            }
        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            int x, y;
            x = e.X;
            y = e.Y;

            label3.Text = "Mouse coordinates: " + "(" + x.ToString() + "; " + y.ToString() + ")";
        }

        private void deleteStation(OvalShape st)
        {
            ShapeContainer canvas;
            // Get the ShapeContainer.
            canvas = st.Parent;
            // If OvalShape is in the same collection, remove it.
            if (canvas.Shapes.Contains(st))
            {
                canvas.Shapes.Remove(st);
            }
        }

        private void deleteRouteNode(OvalShape rn)
        {
            ShapeContainer canvas;
            // Get the ShapeContainer.
            canvas = rn.Parent;
            // If OvalShape2 is in the same collection, remove it.
            if (canvas.Shapes.Contains(rn))
            {
                canvas.Shapes.Remove(rn);
            }
        }

        private void deleteRoute(LineShape rt)
        {
            ShapeContainer canvas;

            canvas = rt.Parent; // Get the ShapeContainer.

            if (canvas.Shapes.Contains(rt)) // If LineShape is in the same collection, remove it.
            {
                canvas.Shapes.Remove(rt);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (routenodes.Count == 0)
                label4.Text = "No valid layout data!";

            int norows = dataGridView1.Rows.Count;
            demands.Clear();

            for (int i = 0; i < norows - 1; i++)
            {
                Demand dem = new Demand();
                dem.orderNo = dataGridView1.Rows[i].Cells[0].Value.ToString();
                dem.from = dataGridView1.Rows[i].Cells[1].Value.ToString();
                dem.to = dataGridView1.Rows[i].Cells[2].Value.ToString();
                dem.item = dataGridView1.Rows[i].Cells[3].Value.ToString();
                dem.amount = int.Parse(dataGridView1.Rows[i].Cells[4].Value.ToString());
                dem.cycleTime = int.Parse(dataGridView1.Rows[i].Cells[5].Value.ToString());

                demands.Add(dem);
            }

            label4.Text = "Demands are successfully updated!";
            dataGridView2.Refresh();

        }

        private void dataGridView1_UserAddedRow(object sender, DataGridViewRowEventArgs e)
        {
            dataGridViewRefresh();
        }

        private void dataGridViewRefresh()
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                List<string> stationNames = new List<string>();

                foreach (Station st in stations)
                {
                    stationNames.Add(st.id);
                }

                DataGridViewComboBoxCell cell = (DataGridViewComboBoxCell)(row.Cells["From"]);
                DataGridViewComboBoxCell cell2 = (DataGridViewComboBoxCell)(row.Cells["To"]);
                cell.DataSource = stationNames;
                cell2.DataSource = stationNames;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            RouteNode fromnode = routenodes[int.Parse(textBox1.Text)];
            RouteNode tonode = routenodes[int.Parse(textBox2.Text)];
            foreach (OvalShape rn in drawedRoutenodes)
            {
                rn.FillColor = Color.LightSlateGray;
            }

            richTextBox2.Text = "";
            Dijkstra dk = new Dijkstra(routes, routenodes, fromnode);
            dk.CalculateDijkstra();


            richTextBox2.Text = dk.GetDistance(tonode).ToString() + '\n';

            foreach (RouteNode rn in dk.shortestPath)
            {
                richTextBox2.Text += rn.id + '\n';
                drawedRoutenodes[routenodes.FindIndex(s => s.id == rn.id)].FillColor = Color.Red;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;";

            DialogResult dlgResult = ofd.ShowDialog();

            if (dlgResult == DialogResult.OK)
            {
                layoutPath = ofd.FileName;
            }

            Image layout = Image.FromFile(layoutPath);
            panel1.BackgroundImage = layout;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            FileStream stream = new FileStream(@"..\..\..\..\..\exp.xml", FileMode.Create);

            XmlSerializer serializer = new XmlSerializer(typeof(List<Route>), new XmlRootAttribute("Layout"));
            XmlSerializer serializer2 = new XmlSerializer(typeof(List<Station>));
            XmlSerializer serializer3 = new XmlSerializer(typeof(List<RouteNode>));

            StringWriter xout1 = new StringWriter();
            StringWriter xout2 = new StringWriter();
            StringWriter xout3 = new StringWriter();

            serializer.Serialize(xout1, routes);
            serializer2.Serialize(xout2, stations);
            serializer3.Serialize(xout3, routenodes);

            XmlDocument x1 = new XmlDocument();
            x1.LoadXml(xout1.ToString());
            XmlDocument x2 = new XmlDocument();
            x2.LoadXml(xout2.ToString());
            XmlDocument x3 = new XmlDocument();
            x3.LoadXml(xout3.ToString());

            foreach (XmlNode node in x2.DocumentElement.ChildNodes)
            {
                XmlNode imported = x1.ImportNode(node, true);
                x1.DocumentElement.AppendChild(imported);
            }

            foreach (XmlNode node in x3.DocumentElement.ChildNodes)
            {
                XmlNode imported = x1.ImportNode(node, true);
                x1.DocumentElement.AppendChild(imported);
            }

            x1.Save(stream);
            stream.Close();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                groupBox1.Text = "Route node";
                label12.Visible = false;
                label8.Text = "ID";
                label9.Text = "X";
                label10.Text = "Y";
                type_combo.Visible = false;
                node_1_combo.Visible = false;
                node_2_combo.Visible = false;
                workcell_box.Visible = false;
                route_combo.Visible = false;
                station_type_combo.Visible = false;
                textBox4.Visible = true;
                textBox5.Visible = true;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                groupBox1.Text = "Route";
                label12.Visible = true;
                label8.Text = "ID";
                label9.Text = "Node 1";
                label10.Text = "Node 2";
                label12.Text = "Type";
                node_1_combo.Visible = true;
                node_2_combo.Visible = true;
                type_combo.Visible = true;
                workcell_box.Visible = false;
                route_combo.Visible = false;
                station_type_combo.Visible = false;
                textBox4.Visible = false;
                textBox5.Visible = false;
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                groupBox1.Text = "Station";
                label12.Visible = true;
                label9.Text = "Route";
                label10.Text = "Type";
                label12.Text = "Workcell";
                type_combo.Visible = false;
                workcell_box.Visible = true;
                route_combo.Visible = true;
                station_type_combo.Visible = true;
                textBox4.Visible = false;
                node_1_combo.Visible = false;
                node_2_combo.Visible = false;
                textBox5.Visible = false;
            }
        }

        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            if (radioButton1.Checked)
            {
                added_rn = new RouteNode();
                added_rn.drawRouteNode(e.X, e.Y, drawedRoutenodes, panel1);
                textBox4.Text = e.X.ToString();
                textBox5.Text = e.Y.ToString();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                added_rn = new RouteNode();
                added_rn.id = textBox3.Text;
                added_rn.x_coord = textBox4.Text;
                added_rn.y_coord = textBox5.Text;

                routenodes.Add(added_rn);
                node_1_combo.Items.Add(added_rn.id);
                node_2_combo.Items.Add(added_rn.id);
            }

            if (radioButton2.Checked)
            {
                Route rt = new Route();
                rt.id = textBox3.Text;
                rt.node_1.id = node_1_combo.Text;
                rt.node_2.id = node_2_combo.Text;
                rt.direction = type_combo.Text;

                routes.Add(rt);
                route_combo.Items.Add(rt.id);

                rt.drawRoute(int.Parse(routenodes[routenodes.FindIndex(s => s.id == rt.node_1.id)].x_coord),
                    int.Parse(routenodes[routenodes.FindIndex(s => s.id == rt.node_1.id)].y_coord),
                    int.Parse(routenodes[routenodes.FindIndex(s => s.id == rt.node_1.id)].x_coord),
                    int.Parse(routenodes[routenodes.FindIndex(s => s.id == rt.node_1.id)].y_coord),
                    drawedRoutes, panel1);
            }

            if (radioButton3.Checked)
            {
                Station st = new Station();
                st.id = textBox3.Text;
                st.route = routes[routes.FindIndex(s => s.id == route_combo.Text)];
                st.type = station_type_combo.Text;

                int route_ind;
                string n1, n2;
                int f_id, t_id;

                route_ind = routes.FindIndex(s => s.id == st.route.id);
                n1 = routes[route_ind].node_1.id;
                n2 = routes[route_ind].node_2.id;

                f_id = routenodes.FindIndex(s => s.id == n1);
                t_id = routenodes.FindIndex(s => s.id == n2);

                st.CalculateXY(routenodes[f_id], routenodes[t_id]);
                st.drawStation(int.Parse(st.x_coord), int.Parse(st.y_coord), st.type, drawedStations, panel1);
                st.calculateClosestNode();

                stations.Add(st);
            }
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            label3.Text = "Mouse coordinates: (" + e.X.ToString() + " ; " + e.Y.ToString() + ")";
        }

        private void button8_Click(object sender, EventArgs e)
        {
            deleteRouteNode(drawedRoutenodes[4]);
            deleteRoute(drawedRoutes[4]);
            deleteRoute(drawedRoutes[5]);
            deleteRoute(drawedRoutes[6]);
            deleteRoute(drawedRoutes[7]);
            deleteStation(drawedStations[2]);
            deleteStation(drawedStations[3]);
            deleteStation(drawedStations[4]);
            deleteStation(drawedStations[5]);
            panel1.Refresh();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView1.SelectedNode.Level != 0)
            {
                int selected = treeView1.SelectedNode.Index;
                string parent = treeView1.SelectedNode.Parent.Text;
                string display;

                switch (parent)
                {
                    case "Stations":
                        display = "ID: " + stations[selected].id.ToString() + "\n" +
                            "Route: " + stations[selected].route.id + "\n" +
                            "Type: " + stations[selected].type.ToString() + "\n" +
                            "Workcell info: " + stations[selected].workcell.ToString();
                        richTextBox1.Text = display;
                        selectedElement = "station";
                        break;

                    case "Routes":
                        display = "ID: " + routes[selected].id.ToString() + "\n" +
                            "Node 1: " + routes[selected].node_1 + "\n" +
                            "Node 2: " + routes[selected].node_2 + "\n" +
                            "Direction: " + routes[selected].direction.ToString();
                        richTextBox1.Text = display;
                        selectedElement = "route";
                        break;

                    case "Route nodes":
                        display = "ID: " + routenodes[selected].id.ToString() + "\n" +
                             "X coordinate: " + routenodes[selected].x_coord.ToString() + "\n" +
                             "Y coordinate: " + routenodes[selected].y_coord.ToString();
                        richTextBox1.Text = display;
                        selectedElement = "routenode";
                        break;

                    default:
                        break;
                }
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Vehicle vh = new Vehicle();
            vh.id = textBox6.Text;
            vh.type = textBox7.Text;
            vh.capacity = int.Parse(textBox8.Text);
            vh.speed = int.Parse(textBox9.Text);

            vehicles.Add(vh);
        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            dataGridViewRefresh();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            FileStream stream = new FileStream(@"..\..\..\..\..\bosch_demand.xml", FileMode.Create);

            XmlSerializer serializer = new XmlSerializer(typeof(List<Demand>), new XmlRootAttribute("Demands"));

            StringWriter xout = new StringWriter();

            serializer.Serialize(xout, demands);

            XmlDocument x1 = new XmlDocument();
            x1.LoadXml(xout.ToString());

            x1.Save(stream);
            stream.Close();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            demands.Clear();
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "XML Files|*.xml";

            DialogResult dlgResult = ofd.ShowDialog();
            XmlTextReader tr = new XmlTextReader(ofd.FileName);

            while (tr.Read())
            {
                Demand demand = new Demand();

                if (tr.MoveToContent() == XmlNodeType.Element && tr.Name == "Demand")
                {
                    demand.orderNo = tr.GetAttribute(0);
                    demand.from = tr.GetAttribute(1);
                    demand.to = tr.GetAttribute(2);
                    demand.item = tr.GetAttribute(3);
                    demand.amount = int.Parse(tr.GetAttribute(4));
                    demand.ifFinished = bool.Parse(tr.GetAttribute(5));
                    demand.cycleTime = int.Parse(tr.GetAttribute(6));

                    demands.Add(demand);
                }
            }

            demands.RemoveAll(d => stations.Find(s => s.id == d.from) == null);
            demands.RemoveAll(d => stations.Find(s => s.id == d.to) == null);
            demands.RemoveAll(d => d.amount == 0);

            var source = new BindingSource();
            source.DataSource = demands;
            dataGridView2.DataSource = source;

            double sumOfAmount = 0;

            foreach (Demand dm in demands)
            {
                sumOfAmount += dm.amount;
            }

            double minVehicles = Math.Ceiling(sumOfAmount / 35);

            label17.Text = "Capacity precalculation: At least " + minVehicles.ToString() + " vehicles are needed with avg. cap.: 35.";        // Ezt később át kell írni a tényleges átlagos kapacitásra!
        }

        private void button13_Click(object sender, EventArgs e)
        {
            FileStream stream = new FileStream(@"..\..\..\..\..\dummy_vehicle.xml", FileMode.Create);

            XmlSerializer serializer = new XmlSerializer(typeof(List<Vehicle>), new XmlRootAttribute("Vehicles"));

            StringWriter xout = new StringWriter();

            serializer.Serialize(xout, vehicles);

            XmlDocument x = new XmlDocument();
            x.LoadXml(xout.ToString());

            x.Save(stream);
            stream.Close();
        }

        private void button14_Click(object sender, EventArgs e)
        {
            vehicles.Clear();
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "XML Files|*.xml";

            DialogResult dlgResult = ofd.ShowDialog();
            XmlTextReader tr = new XmlTextReader(ofd.FileName);

            while (tr.Read())
            {
                Vehicle vehicle = new Vehicle();

                if (tr.MoveToContent() == XmlNodeType.Element && tr.Name == "Vehicle")
                {
                    vehicle.id = tr.GetAttribute(0);
                    vehicle.type = tr.GetAttribute(1);
                    vehicle.capacity = int.Parse(tr.GetAttribute(2));
                    vehicle.speed = int.Parse(tr.GetAttribute(3));

                    vehicles.Add(vehicle);
                }
            }

            textBox6.Text = vehicles[0].id;
            textBox7.Text = vehicles[0].type;
            textBox8.Text = vehicles[0].capacity.ToString();
            textBox9.Text = vehicles[0].speed.ToString();

        }

        private List<RouteNode> listOfClosestNodes()
        {
            List<RouteNode> closestNodes = new List<RouteNode>();

            foreach (Station st in stations)
            {
                closestNodes.Add(st.closestNode);

            }

            return closestNodes;
        }

        // -------------------------------------------------------------------------------------------- 
        //                Finds the cluster stations on the layout
        //--------------------------------------------------------------------------------------------- 
        private void InitClusters()
        {
            detectClusterNodes(routes, routenodes, stations[0]);    // the input is the already found routenodes in a cluster

            foreach (List<RouteNode> clusterPath in shortestPathNodes)
                if (clusterPath.Count() > 1)
                {
                    {
                        for (int i = 1; i < clusterPath.Count() - 1; i++)
                        {
                            Route connectingRoute = new Route();

                            foreach (Route rt in routes)
                            {
                                if ((rt.node_1.id == clusterPath[i].id && rt.node_2.id == clusterPath[i + 1].id) || (rt.node_2.id == clusterPath[i].id && rt.node_1.id == clusterPath[i + 1].id))
                                {
                                    shortestPathRoutes[shortestPathNodes.IndexOf(clusterPath)].Add(rt);

                                    foreach (Station s in stations)
                                        if (s.route.id == rt.id)
                                        {
                                            {
                                                shortestPathStations[shortestPathNodes.IndexOf(clusterPath)].Add(s);
                                            }
                                        }
                                }
                            }
                        }
                    }
                }

            // Display the clusters in the richtextbox
            //foreach (List<Station> stationCluster in shortestPathStations)
            //{
            //    richTextBox3.Text += "To station " + stations[shortestPathStations.IndexOf(stationCluster)].id + " : ";

            //    foreach (Station s in stationCluster)
            //    {
            //        richTextBox3.Text += " --> " + s.id;
            //    }

            //    richTextBox3.Text += '\n';
            //}

            for (int i = 0; i < shortestPathStations.Count(); i++)
            {
                Path p = new Path(shortestPathStations[i], shortestPathRoutes[i], vehicles[0], demands);
                paths.Add(p);
            }

            List<Path> feasiblePaths = new List<Path>();
            //Create feasible paths
            foreach (Path p in paths)
            {
                p.ifFeasible();
                if (p.feasible)
                {
                    feasiblePaths.Add(p);
                }
                else
                {
                    bool feasible = false;
                    int index = paths.IndexOf(p);
                    Station mustVisit = new Station();
                    mustVisit = stations[paths.IndexOf(p)];

                    while (!feasible)
                    {
                        Random r = new Random();
                        int ra = 0;
                        do
                        {
                            ra = r.Next(1, p.stationsToVisit.Count() - 1);
                        }
                        while (p.stationsToVisit[ra].id == mustVisit.id);
                        List<Station> newStations = new List<Station>();
                        p.stationsToVisit.RemoveAt(ra);
                        newStations = p.stationsToVisit;
                        newStations.Insert(0, stations[0]);
                        removeDuplicateStations(newStations);
                        Path newP = stationListToPath(newStations); //new Path(new List<Station>(), new List<Route>(), vehicles[0], new List<Demand>());
                        newP.demandsToFulfil = demands;
                        newP.vehicleApplied = vehicles[0];
                        if (newP.feasible == true)
                        {
                            feasiblePaths.Add(newP);
                            feasible = true;
                        }
                    }
                }
            }

        }

        public List<Station> removeDuplicateStations(List<Station> stationList)
        {
            for (int i = 0; i < stationList.Count() - 1; i++)
            {
                if (stationList[i].id == stationList[i + 1].id)
                    stationList.RemoveAt(i + 1);
            }

            return stationList;
        }

        // -------------------------------------------------------------------------------------------- 
        //                          Simulates the forward and backward Dijkstra's to each nodes
        // -------------------------------------------------------------------------------------------- 
        public void detectClusterNodes(List<Route> routes, List<RouteNode> routenodes, Station depot)
        {

            List<RouteNode> closestNodes = listOfClosestNodes();

            shortestPathNodes = new List<List<RouteNode>>();
            for (int i = 0; i < stations.Count(); i++)
            {
                shortestPathNodes.Add(new List<RouteNode>());
            }

            shortestPathRoutes = new List<List<Route>>();
            for (int i = 0; i < stations.Count(); i++)
            {
                shortestPathRoutes.Add(new List<Route>());
            }

            shortestPathStations = new List<List<Station>>();
            for (int i = 0; i < stations.Count(); i++)
            {
                shortestPathStations.Add(new List<Station>());
            }

            for (int i = 0; i < closestNodes.Count(); i++)
            {
                RouteNode rn = closestNodes[i];

                Dijkstra fromDepot = new Dijkstra(routes, routenodes, routenodes[routenodes.FindIndex(s => s.id == depot.closestNode.id)]);
                fromDepot.CalculateDijkstra();
                fromDepot.GetDistance(rn);
                shortestPathNodes[i] = fromDepot.shortestPath;

                if (fromDepot.shortestPath.Count() > 1)
                    rn.parentNode = fromDepot.shortestPath[fromDepot.shortestPath.Count() - 2];
                else
                    rn.parentNode = fromDepot.shortestPath[0];

                // If we are at a station, forward the next node to the end of station's route, and the previous one to the start of the corridor
                RouteNode endNode = stations[i].nextNode;
                shortestPathNodes[i].Add(endNode);

                rn.nextNode = findNodeToTurn(endNode, rn);

                Dijkstra dj = new Dijkstra(routes, routenodes, endNode);
                dj.CalculateDijkstra();
                dj.GetDistance(rn.nextNode);
                dj.shortestPath.RemoveAt(0);
                shortestPathNodes[i].AddRange(dj.shortestPath);

                Dijkstra dk = new Dijkstra(routes, routenodes, rn.nextNode);
                dk.CalculateDijkstra();
                dk.GetDistance(routenodes[0]);
                dk.shortestPath.RemoveAt(0);
                shortestPathNodes[i].AddRange(dk.shortestPath);
            }

            GetMemoryUsage();
        }

        // -------------------------------------------------------------------------------------------- 
        //                          Greedy search to find the first node to turn in
        // -------------------------------------------------------------------------------------------- 
        public RouteNode findNodeToTurn(RouteNode currentNode, RouteNode previousNode)
        {
            foreach (RouteNode rn in routenodes)
            {
                rn.depthInTree = 0;
            }

            int maxDepth = 1;
            currentNode.depthInTree = 1;
            RouteNode turnNode = new RouteNode();

            Dijkstra dk = new Dijkstra(routes, routenodes, routenodes[0]);
            NodeDistance[,] nd = dk.distancematrix;

            bool ifFounded = false;

            while (ifFounded != true)
            {
                for (int j = 0; j < maxDepth; j++)
                {
                    foreach (RouteNode rn in routenodes)
                        if (rn.depthInTree == maxDepth)
                        {
                            currentNode = rn;
                            int currNodeIndex = routenodes.FindIndex(s => s.id == currentNode.id);
                            int prevNodeIndex = routenodes.FindIndex(s => s.id == previousNode.id);

                            for (int i = 0; i < nd.GetLength(0); i++)
                            {
                                if (nd[currNodeIndex, i].length > 0 && nd[currNodeIndex, i].length < int.MaxValue)
                                    if (i != prevNodeIndex)
                                    {
                                        {
                                            routenodes[i].depthInTree = maxDepth + 1;
                                            routenodes[i].parentNode = currentNode;
                                            previousNode = currentNode;
                                        }
                                    }
                            }
                        }

                    foreach (RouteNode rn in routenodes)
                        if (rn.depthInTree == maxDepth + 1)
                        {
                            Dijkstra dj = new Dijkstra(routes, routenodes, rn);
                            dj.CalculateDijkstra();
                            dj.GetDistance(routenodes[0]);

                            if (dj.shortestPath[1] != rn.parentNode)
                            {
                                ifFounded = true;
                                maxDepth--;
                                turnNode = rn;
                                return turnNode;
                            }
                        }
                    maxDepth++;
                }
            }

            return turnNode;
        }

        public int findRouteNodeIndex(RouteNode routenode)
        {
            return routenodes.FindIndex(s => s.id == routenode.id);
        }

        public void GetMemoryUsage()
        {
            textBox10.Text = (currentProcess.WorkingSet64 / (1024 * 1024)).ToString(); //Calculates the memory usage of the app in Mb, worth to put in another thread (?)
        }

        private void button18_Click(object sender, EventArgs e)
        {
            drawPath(paths[int.Parse(textBox11.Text)]);
            textBox13.Text = Math.Round(((paths[int.Parse(textBox11.Text)].timeRequirement) / 60), 0).ToString();
            paths.ToString();
        }

        public void drawPath(Path path)
        {
            foreach (LineShape r in drawedRoutes)
            {
                r.BorderColor = Color.Orange;
            }

            foreach (Route r in path.routesOnPath)
            {
                int index = routes.FindIndex(s => s.id == r.id);
                drawedRoutes[index].BorderColor = Color.DarkRed;
            }
        }

        //---------------------------------------------------------------------------------------------//
        //                          Create initial paths                                               //         
        // Finds the longest paths to all the unvisited stations, and defines the initial solution for //
        // the local-search algorithm                                                                  //
        //---------------------------------------------------------------------------------------------//
        private void InitPaths()
        {
            List<List<Station>> clusterTrips = new List<List<Station>>();
            List<Station> longestTrip = new List<Station>();
            List<Station> unVisitedStations = new List<Station>();
            List<int> indexesOfInitPaths = new List<int>();
            Path currentPath = new Path(new List<Station>(), new List<Route>(), vehicles[0], demands);

            for (int i = 0; i < stations.Count(); i++)
            {
                if (demands.FindAll(d => (d.from == stations[i].id || d.to == stations[i].id)).Count() != 0)
                {
                    Station st = stations[i];
                    unVisitedStations.Add(st);
                }
            }

            while (unVisitedStations.Count() != 0)
            {
                int maxTripLength = 0;
                foreach (Path p in paths)
                {
                    if (p.stationsToVisit.Count() > maxTripLength)
                    {
                        maxTripLength = p.stationsToVisit.Count();
                        currentPath = p;
                    }
                }

                foreach (Station st in currentPath.stationsToVisit)
                {

                    if (unVisitedStations.Find(s => s.id == st.id) != null)
                    {
                        foreach (Station st2 in currentPath.stationsToVisit)
                        {
                            unVisitedStations.RemoveAll(s => s.id == st2.id);
                        }
                        initPaths.Add(currentPath);
                    }
                }
                paths.Remove(currentPath);
            }

            // Refine paths -> remove duplications
            List<Station> visitedStations = new List<Station>();
            List<Path> initPaths2 = new List<Path>();

            foreach (Path p in initPaths)
            {
                List<Station> tempList = new List<Station>();
                foreach (Station st in p.stationsToVisit)
                {
                    if (visitedStations.Find(s => s.id == st.id) == null && st.type != "warehouse")
                    {
                        tempList.Add(st);
                        visitedStations.Add(st);
                    }   
                }
                tempList.Insert(0, stations[0]);
                tempList.Insert(tempList.Count(), stations[0]);
                Path tempPath = stationListToPath(tempList);
                tempPath.ifFeasible();
                initPaths2.Add(tempPath);
                initPaths = initPaths2;
            }

            richTextBox3.Text = "";
            richTextBox2.Text += "No. of initial solutions: " + initPaths.Count().ToString() + '\n';

            foreach (Path p in initPaths)
            {
                richTextBox3.Text += "Path(" + initPaths.IndexOf(p).ToString() + ")" +
                    '\n' + "Required time: " + p.timeRequirement.ToString() + '\n' + "Visited stations: ";

                foreach (Station s in p.stationsToVisit)
                {
                    richTextBox3.Text += s.id + "-->";
                }
                richTextBox3.Text += '\n';
            }
        }



        private void InitPath()
        {
            chart1.Series[0].Points.Clear();
            initPaths.Clear();
            improvedPaths.Clear();
            paths.Clear();
            InitClusters();
            InitPaths();
            button19.Enabled = true;
        }

        public List<RouteNode> findNextNode(RouteNode currentNode, RouteNode previousNode, RouteNode targetNode)
        {
            RouteNode startNode = currentNode;
            if (currentNode != targetNode)
            {
                foreach (RouteNode rn in routenodes)
                {
                    rn.depthInTree = 0;
                }

                int maxDepth = 1;
                int indexToWatch = 1;
                currentNode.depthInTree = 1;
                RouteNode turnNode = new RouteNode();

                Dijkstra dk = new Dijkstra(routes, routenodes, currentNode);
                NodeDistance[,] nd = dk.distancematrix;

                bool ifFounded = false;
                int escape = 0;

                while (!ifFounded && escape < 500)
                {
                    for (int j = 0; j < maxDepth; j++)
                    {
                        escape++;
                        foreach (RouteNode rn in routenodes)
                            if (rn.depthInTree == maxDepth)
                            {
                                currentNode = rn;
                                int currNodeIndex = routenodes.FindIndex(s => s.id == currentNode.id);
                                int prevNodeIndex = routenodes.FindIndex(s => s.id == previousNode.id);

                                for (int i = 0; i < nd.GetLength(0); i++)
                                {
                                    if (nd[currNodeIndex, i].length > 0 && nd[currNodeIndex, i].length < int.MaxValue)
                                        if (i != prevNodeIndex)
                                        {
                                            {
                                                routenodes[i].previousNode = currentNode;
                                                routenodes[i].depthInTree = maxDepth + 1;
                                                routenodes[i].parentNode = currentNode;
                                                previousNode = currentNode;
                                            }
                                        }
                                }
                            }

                        foreach (RouteNode rn in routenodes)
                        {
                            if (rn.depthInTree == maxDepth + 1)
                            {
                                Dijkstra dj = new Dijkstra(routes, routenodes, rn);
                                dj.CalculateDijkstra();
                                dj.GetDistance(targetNode);

                                if (dj.shortestPath.Count() > 1)
                                {
                                    indexToWatch = 1;
                                }
                                else
                                {
                                    indexToWatch = 0;
                                }

                                if (dj.shortestPath[indexToWatch] != rn.parentNode)
                                {
                                    ifFounded = true;
                                    maxDepth--;
                                    turnNode = rn;
                                    Dijkstra to = new Dijkstra(routes, routenodes, startNode);
                                    to.CalculateDijkstra();
                                    to.GetDistance(turnNode);

                                    List<RouteNode> shortest = new List<RouteNode>();
                                    shortest = to.shortestPath;
                                    shortest.RemoveAt(shortest.Count() - 1);
                                    shortest.AddRange(dj.shortestPath);
                                    shortest.Add(targetNode);


                                    return shortest;
                                }
                            }
                        }
                        maxDepth++;
                    }
                }

                List<RouteNode> shortest2 = new List<RouteNode>();
                shortest2.Add(currentNode);
                return shortest2;

            }
            else
            {
                List<RouteNode> shortest2 = new List<RouteNode>();
                shortest2.Add(currentNode);
                return shortest2;
            }
        }


        private void calculateDistanceBetweenStations()
        {
            backgroundWorkerProgress = 0;

            stationToStation = new List<Route>[stations.Count(), stations.Count][];
            for (int j = 0; j < stationToStation.GetLength(1); j++)
                for (int k = 0; k < stationToStation.GetLength(0); k++)
                {
                    {
                        stationToStation[j, k] = new List<Route>[2];
                        for (int l = 0; l < 2; l++)
                        {
                            stationToStation[j, k][l] = new List<Route>();
                        }
                    }
                }

            foreach (Station stFrom in stations)
            {
                int fromRouteDirection = 0;
                int fromIndex = stations.IndexOf(stFrom);
                debug2 = fromIndex;
                backgroundWorkerProgress++;

                switch (stFrom.route.direction)
                {
                    case "uni":
                        fromRouteDirection = 0;
                        break;
                    case "1_2":
                        fromRouteDirection = 1;
                        break;
                    case "2_1":
                        fromRouteDirection = -1;
                        break;
                }

                foreach (Station stTo in stations)
                {
                    int toRouteDirection = 0;
                    switch (stTo.route.direction)
                    {
                        case "uni":
                            toRouteDirection = 0;
                            break;
                        case "1_2":
                            toRouteDirection = 1;
                            break;
                        case "2_1":
                            toRouteDirection = -1;
                            break;
                    }
                    int toIndex = stations.IndexOf(stTo);
                    debug = toIndex;

                    if (stFrom.id != stTo.id)
                    {
                        if (fromRouteDirection != 1)
                        {
                            List<Route> list1;
                            List<Route> list2;
                            double length1;
                            double length2;

                            if (toRouteDirection != -1)
                            {
                                // Route from node_1 to next station node_1
                                List<RouteNode> toList1 = findNextNode(routenodes.Find(rn => rn.id == stFrom.route.node_1.id),
                                    routenodes.Find(rn => rn.id == stFrom.route.node_2.id), routenodes.Find(rn => rn.id == stTo.route.node_1.id));
                                list1 = parseRouteToRoutenodes(toList1);
                                length1 = list1.Sum(r => r.length);
                            }
                            else
                            {
                                list1 = null;
                                length1 = int.MaxValue;
                            }

                            if (toRouteDirection != 1)
                            {
                                // Route from node_1 to next station node_2
                                List<RouteNode> toList2 = findNextNode(routenodes.Find(rn => rn.id == stFrom.route.node_1.id),
                                    routenodes.Find(rn => rn.id == stFrom.route.node_2.id), routenodes.Find(rn => rn.id == stTo.route.node_2.id));
                                list2 = parseRouteToRoutenodes(toList2);
                                length2 = list2.Sum(r => r.length);
                            }
                            else
                            {
                                list2 = null;
                                length2 = int.MaxValue;
                            }

                            if (length1 < length2)
                            {
                                stationToStation[fromIndex, toIndex][0] = list1;
                            }
                            else
                            {
                                stationToStation[fromIndex, toIndex][0] = list2;
                            }
                        }

                        if (fromRouteDirection != -1)
                        {
                            List<Route> list3;
                            List<Route> list4;
                            double length3;
                            double length4;

                            if (toRouteDirection == 1)
                            {
                                // Route from node_2 to next station node_1
                                List<RouteNode> toList3 = findNextNode(routenodes.Find(rn => rn.id == stFrom.route.node_2.id),
                                    routenodes.Find(rn => rn.id == stFrom.route.node_1.id), routenodes.Find(rn => rn.id == stTo.route.node_1.id));
                                list3 = parseRouteToRoutenodes(toList3);
                                length3 = list3.Sum(r => r.length);
                            }
                            else
                            {
                                list3 = null;
                                length3 = int.MaxValue;
                            }

                            if (toRouteDirection != 1)
                            {
                                List<RouteNode> toList4 = findNextNode(routenodes.Find(rn => rn.id == stFrom.route.node_2.id),
                                    routenodes.Find(rn => rn.id == stFrom.route.node_1.id), routenodes.Find(rn => rn.id == stTo.route.node_2.id));
                                list4 = parseRouteToRoutenodes(toList4);
                                length4 = list4.Sum(r => r.length);
                            }
                            else
                            {
                                list4 = null;
                                length4 = int.MaxValue;
                            }

                            if (length3 < length4)
                            {
                                stationToStation[fromIndex, toIndex][1] = list3;
                            }
                            else
                            {
                                stationToStation[fromIndex, toIndex][1] = list4;
                            }
                        }
                    }
                }
            }

            stationToStation.ToString();
        }

        public List<Route> parseRouteToRoutenodes(List<RouteNode> routeNodeList)
        {
            List<Route> routeList = new List<Route>();

            for (int j = 0; j < routeNodeList.Count() - 1; j++)
            {
                foreach (Route rt in routes)
                {
                    if ((rt.node_1.id == routeNodeList[j].id && rt.node_2.id == routeNodeList[j + 1].id) ||
                        (rt.node_2.id == routeNodeList[j].id && rt.node_1.id == routeNodeList[j + 1].id))
                    {
                        routeList.Add(rt);
                    }
                }
            }

            return routeList;
        }

        private void button20_Click(object sender, EventArgs e)
        {
            button20.Enabled = false;
            backgroundWorker1.RunWorkerAsync();
        }

        private void Optimize()
        {
            int iterations = 0;
            improvedPaths = initPaths;
            totalTimeOfPlan = improvedPaths.Sum(p => p.timeRequirement);
            List<int> planHistory = new List<int>();
            int stepCounter = 0;
            double maxTime = 0;
            testImprovePlan = new List<double>();

            for (int f = 0; f < 5; f++)
            {
                while (totalTimeOfPlan > 3600 && iterations < 10)
                {
                    iterations++;

                    for (int i = 0; i < improvedPaths.Count() - 1; i++)
                        for (int j = 0; j < improvedPaths.Count() - 1; j++)
                        {
                            {
                                LocalSearch test = new LocalSearch(improvedPaths[i], improvedPaths[j], routes, routenodes, stations, demands);
                                List<Path> change = test.InterInsert();

                                if (test.isBetter)
                                {
                                    improvedPaths[i] = change[0];
                                    improvedPaths[j] = change[1];

                                    initPaths = improvedPaths;
                                    totalTimeOfPlan = improvedPaths.Sum(p => p.timeRequirement);
                                }
                                stepCounter++;
                                planHistory.Add(Convert.ToInt32((totalTimeOfPlan)));
                                AllocateRouteToVehicle all = new AllocateRouteToVehicle(improvedPaths, 3600);
                                if (all.Calculate2(iterations) != 0)
                                {
                                    testImprovePlan.Add(all.Calculate2(iterations));
                                }
                                else
                                {
                                    testImprovePlan.Add(testImprovePlan[testImprovePlan.Count() - 1]);
                                }
                            }
                        }
                }
            }

            improvedPaths.RemoveAll(p => p.timeRequirement == 0);
            AllocateRouteToVehicle allocation = new AllocateRouteToVehicle(improvedPaths, 3600);
            List<List<int>> allocated = allocation.Calculate();
            allocation.ToString();

            List<List<Path>> milkrunPlan = new List<List<Path>>();
            for (int i = 0; i < allocated.Count(); i++)
            {
                milkrunPlan.Add(new List<Path>());
            }

            for (int i = 0; i < allocated.Count(); i++)
            {
                for (int j = 0; j < allocated[i].Count(); j++)
                {
                    milkrunPlan[i].Add(improvedPaths[allocated[i][j]]);
                }

            }

            chart1.ChartAreas[0].AxisY.Minimum =  planHistory.Min() - 200;
            for (int z = 0; z < planHistory.Count(); z++)
                {
                    chart1.Series["Series1"].Points.AddY(planHistory[z]);
                }

            //// Set series chart type
            chart1.Series["Series1"].ChartType = SeriesChartType.Line;
            chart1.Series["Series1"].BorderWidth = 3;
            chart1.Series["Series1"].LegendText = "Total time-cost of the plan [sec]";
            chart1.ChartAreas["ChartArea1"].AxisX.IsMarginVisible = false;

            // Export chart values
            string values = "";
            foreach (int z in planHistory)
            {
                values += z.ToString() + ',';
            }

            // Write the string to a file.
            System.IO.StreamWriter file = new System.IO.StreamWriter("c:\\chart.txt");
            file.WriteLine(values);

            file.Close();

            richTextBox3.Text = "";

            double roundTime = 0;
            foreach (List<Path> l in milkrunPlan)
            {
                richTextBox3.Text += "Path(" + milkrunPlan.IndexOf(l).ToString() + "): ";
                foreach (Path p in l)
                {
                    
                    foreach (Station s in p.stationsToVisit)
                    {
                        richTextBox3.Text += s.id + "-->";
                    }
                    richTextBox3.Text += '\n';
                }
                roundTime = l.Sum(t => t.timeRequirement);
                richTextBox3.Text += "Time of the milkrun tour: " + Math.Round(l.Sum(t => t.timeRequirement)).ToString() + " seconds" + '\n' + '\n';
                if (roundTime > maxTime)
                    maxTime = roundTime;
            }
            richTextBox3.Text += "Total time of the plan: " + Math.Round(maxTime).ToString() + " seconds";
        }



        public void drawChart()
        {
            for (int i = 0; i < bestOfSumTimesOfPlan.Count(); i++)
            {
                chart1.Series["Series1"].Points.AddY(bestOfSumTimesOfPlan[i]);
            }

            //// Set series chart type
            chart1.Series["Series1"].ChartType = SeriesChartType.Line;
            //chart1.Series["Series1"].MarkerStyle = MarkerStyle.Square;
            //chart1.Series["Series1"].MarkerSize = 3;
            chart1.Series["Series1"].BorderWidth = 3;
            chart1.Series["Series1"].LegendText = "Total time-cost of the plan [sec]";
            chart1.ChartAreas["ChartArea1"].AxisX.IsMarginVisible = false;
        }


        static public void SerializeToXML(List<Route> routes)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Route>));
            TextWriter textWriter = new StreamWriter(@"C:\route.xml");
            serializer.Serialize(textWriter, routes);
            textWriter.Close();
        }

        public Path stationListToPath(List<Station> stationList)
        {
            List<Station> stList = stationList;
            List<Route> rtList = new List<Route>();
            Path p = new Path(stList, rtList, vehicles[0], demands);

            int nextNodeIndex1 = 1;

            for (int k = 0; k < stList.Count() - 1; k++)
            {
                int fromIndex = stations.FindIndex(s => s.id == stList[k].id);
                int toIndex = stations.FindIndex(s => s.id == stList[k + 1].id);


                if (fromIndex != toIndex)
                {
                    double len1 = Form1.stationToStation[fromIndex, toIndex][nextNodeIndex1].Sum(s => s.length);

                    if (len1 != 0)
                    {
                        rtList.AddRange(Form1.stationToStation[fromIndex, toIndex][nextNodeIndex1]);

                        if (rtList[rtList.Count() - 1].node_1.id == stList[k + 1].route.node_1.id ||
                            rtList[rtList.Count() - 1].node_2.id == stList[k + 1].route.node_1.id)
                        {
                            nextNodeIndex1 = 1;
                        }
                        else
                        {
                            nextNodeIndex1 = 0;
                        }

                        rtList.Add(stList[k + 1].route);
                        rtList = removeDuplicateRoutes(rtList);
                    }
                    else
                    {
                        rtList.Add(stList[k + 1].route);

                        if (rtList[rtList.Count() - 1].node_1.id == stList[k].route.node_1.id ||
                            rtList[rtList.Count() - 1].node_2.id == stList[k].route.node_1.id)
                        {
                            nextNodeIndex1 = 1;
                        }
                        else
                        {
                            nextNodeIndex1 = 0;
                        }
                    }


                    rtList = removeDuplicateRoutes(rtList);
                    p.routesOnPath = rtList;
                }
            }

            return p;
        }


        public List<Route> removeDuplicateRoutes(List<Route> routeList)
        {
            for (int i = 0; i < routeList.Count() - 1; i++)
            {
                if (routeList[i].id == routeList[i + 1].id)
                    routeList.RemoveAt(i + 1);
            }

            return routeList;
        }

        public void export()
        {
            SerializeToXML(routes);
        }

        //public void drawChart()
        //{
        //    for (int i = 0; i < bestOfSumTimesOfPlan.Count(); i++)
        //    {
        //        chart1.Series["Series1"].Points.AddY(bestOfSumTimesOfPlan[i]);
        //    }

        //    //// Set series chart type
        //    chart1.Series["Series1"].ChartType = SeriesChartType.Line;
        //    //chart1.Series["Series1"].MarkerStyle = MarkerStyle.Square;
        //    //chart1.Series["Series1"].MarkerSize = 3;
        //    chart1.Series["Series1"].BorderWidth = 3;
        //    chart1.Series["Series1"].LegendText = "Total time-cost of the plan [sec]";
        //    chart1.ChartAreas["ChartArea1"].AxisX.IsMarginVisible = false;
        //}

        private void button30_Click(object sender, EventArgs e)
        {
            demands.RemoveAll(d => (stations.Find(s => s.id.Substring(0, 6) == "ST_" + d.to.Substring(0, 3)) == null &&
                (stations.Find(s => s.id.Substring(0, 6) == "ST_" + d.from.Substring(0, 3))) == null));
            demands.RemoveAll(d => (d.to == "DFC" &&
                (stations.Find(s => s.id.Substring(0, 6) == "ST_" + d.from.Substring(0, 3))) == null));
            demands.RemoveAll(d => (stations.Find(s => s.id.Substring(0, 6) == "ST_" + d.to.Substring(0, 3)) == null &&
                (d.from == "DFC")));

            foreach (Demand dem in demands)
            {
                if (dem.from == "H109")
                    dem.from = "DFC";
                if (dem.to == "H109")
                    dem.to = "DFC";
            }

            Console.WriteLine(demands.Count().ToString());

            var source = new BindingSource();
            source.DataSource = demands;
            dataGridView2.DataSource = source;

            double sumOfAmount = 0;

            foreach (Demand dm in demands)
            {
                sumOfAmount += dm.amount;
            }

            double minVehicles = Math.Ceiling(sumOfAmount / 35);

            label17.Text = "Capacity precalculation: At least " + minVehicles.ToString() + " vehicles are needed with avg. cap.: 35.";        // Ezt később át kell írni a tényleges átlagos kapacitásra!
        }

        private void button31_Click(object sender, EventArgs e)
        {
            foreach (Station st in stations)
            {
                if (st.id != "ST_DFC")
                {
                    st.id = st.id.Substring(0, 10);
                }
            }

            foreach (Demand dem in demands)
            {
                if (dem.from != "DFC")
                    dem.from = "ST_" + dem.from.Substring(0, 6);
                if (dem.to != "DFC")
                    dem.to = "ST_" + dem.to.Substring(0, 7);
            }

            foreach (Demand dem in demands)
            {
                if (dem.from == "DFC")
                    dem.ifFinished = true;
            }

            var source = new BindingSource();
            source.DataSource = demands;
            dataGridView2.DataSource = source;
        }

        private void button29_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show(routes.Sum(r => r.length).ToString());
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            backgroundWorker1.ReportProgress(backgroundWorkerProgress);
            calculateDistanceBetweenStations();
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;

        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            label25.Text = "Done";
            button17.Enabled = true;
            button20.Enabled = false;
        }

        private void button29_Click_2(object sender, EventArgs e)
        {
            List<Station> visitedStations = new List<Station>();

            foreach (Path p in improvedPaths)
            {
                visitedStations.AddRange(p.stationsToVisit);
            }

            foreach (Station st in stations)
            {
                if (visitedStations.Find(s => s.id == st.id) == null)
                {
                    MessageBox.Show(st.id);
                }
            }
        }

        private void OptimizePlan()
        {
            // Create initial solution

            chart1.Series[0].Points.Clear();
            initPaths.Clear();
            improvedPaths.Clear();
            paths.Clear();

            InitClusters();
            InitPaths();

            // Perform local search
            double totalTimeOfPlan = double.MaxValue;
            int iterations = 0;
            sumTimesOfPlan = new List<double>();

            while (totalTimeOfPlan > 3600 && iterations < 5)
            {
                iterations++;
                bestPlanTime = int.MaxValue;
                List<List<double>> results = new List<List<double>>();
                sumTimesOfPlan.Add(initPaths.Sum(p => p.timeRequirement));
                improvedPaths = initPaths;

                for (int i = 0; i < improvedPaths.Count() - 1; i++)
                    for (int j = 0; j < improvedPaths.Count() - 1; j++)
                    {
                        {
                            LocalSearch test = new LocalSearch(improvedPaths[i], improvedPaths[j], routes, routenodes, stations, demands);
                            List<Path> change = test.InterInsert();
                            if (test.isBetter)
                            {
                                improvedPaths[i] = change[0];
                                improvedPaths[j] = change[1];

                                if (improvedPaths[i].timeRequirement + improvedPaths[j].timeRequirement < 3600)
                                {
                                    double sum = 0;
                                    for (int k = 0; k < improvedPaths.Count - 1; k++)
                                    {
                                        if (k != i && k != j)
                                        {
                                            sum += improvedPaths[k].timeRequirement;
                                        }
                                    }
                                    sumTimesOfPlan.Add(sum);
                                }
                                sumTimesOfPlan.Add(improvedPaths.Sum(p => p.timeRequirement));
                            }
                        }
                    }

                sumTimesOfPlan.Sort();
                sumTimesOfPlan.Reverse();
                sumTimesOfPlan.RemoveAt(0);


                for (int i = sumTimesOfPlan.Count - 1; i > 0; i--)
                {
                    if (sumTimesOfPlan[i - 1] == sumTimesOfPlan[i])
                    {
                        sumTimesOfPlan.RemoveAt(i);
                    }
                }
            }

            if (iterations < numOfIterations)
            {
                numOfIterations = iterations;
                bestOfSumTimesOfPlan = sumTimesOfPlan;
                bestPlan = improvedPaths;
            }
        }

        private void button31_Click_1(object sender, EventArgs e)
        {
            foreach (Path p in bestPlan)
            {
                richTextBox3.Text += "Path(" + bestPlan.IndexOf(p).ToString() + ")" +
                    '\n' + "Required time: " + p.timeRequirement.ToString() + '\n' + "Visited stations: ";

                foreach (Station s in p.stationsToVisit)
                {
                    richTextBox3.Text += s.id + "-->";
                }
                richTextBox3.Text += '\n';
            }

            using (XmlWriter writer = XmlWriter.Create(@"..\..\..\..\..\plan.xml"))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Plan");

                foreach (Path p in bestPlan)
                {
                    if (p.stationsToVisit.Count > 2)
                    {
                        writer.WriteStartElement("Path");
                        foreach (Station s in p.stationsToVisit)
                        {
                            p.demandsToFulfil.ToString();
                            writer.WriteStartElement("Station");
                            writer.WriteAttributeString("id", s.id.ToString());
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            demands.Clear();
            XmlDocument doc = new XmlDocument();
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "XML Files|*.xml";

            DialogResult dlgResult = ofd.ShowDialog();
            String FileName;
            XmlTextReader tr;


            try
            {
                if (dlgResult == DialogResult.OK)
                {
                    doc.Load(ofd.FileName);
                    FileName = ofd.FileName; ;
                    tr = new XmlTextReader(FileName);

                    while (tr.Read())
                    {
                        Station station = new Station();
                        Route route = new Route();
                        RouteNode routenode = new RouteNode();

                        if (tr.MoveToContent() == XmlNodeType.Element && tr.Name == "Station")
                        {
                            station.id = tr.GetAttribute(0);
                            station.route = routes[routes.FindIndex(s => s.id == tr.GetAttribute(1))];
                            station.type = tr.GetAttribute(2);
                            station.workcell = tr.GetAttribute(3);
                            station.display = richTextBox1;
                            stations.Add(station);
                        }

                        if (tr.MoveToContent() == XmlNodeType.Element && tr.Name == "Route")
                        {
                            route.id = tr.GetAttribute(0);
                            route.node_1 = routenodes[routenodes.FindIndex(s => s.id == tr.GetAttribute(1))];
                            route.node_2 = routenodes[routenodes.FindIndex(s => s.id == tr.GetAttribute(2))];
                            route.direction = tr.GetAttribute(3);
                            route.display = richTextBox1;
                            route.calculateLength();
                            routes.Add(route);
                        }

                        if (tr.MoveToContent() == XmlNodeType.Element && tr.Name == "RouteNode")
                        {
                            routenode.id = tr.GetAttribute(0);
                            routenode.x_coord = tr.GetAttribute(1);
                            routenode.y_coord = tr.GetAttribute(2);
                            routenode.display = richTextBox1;
                            routenodes.Add(routenode);
                        }
                    }

                    foreach (Route rt in routes)
                    {
                        int fx, fy, tx, ty;
                        int fxid, fyid, txid, tyid;

                        fxid = routenodes.FindIndex(s => s.id == rt.node_1.id);
                        fyid = routenodes.FindIndex(s => s.id == rt.node_1.id);
                        txid = routenodes.FindIndex(s => s.id == rt.node_2.id);
                        tyid = routenodes.FindIndex(s => s.id == rt.node_2.id);

                        fx = int.Parse(routenodes[fxid].x_coord);
                        fy = int.Parse(routenodes[fyid].y_coord);
                        tx = int.Parse(routenodes[txid].x_coord);
                        ty = int.Parse(routenodes[tyid].y_coord);

                        rt.drawRoute(fx, fy, tx, ty, drawedRoutes, panel1);
                    }

                    foreach (RouteNode rn in routenodes)
                    {
                        rn.drawRouteNode(int.Parse(rn.x_coord), int.Parse(rn.y_coord), drawedRoutenodes, panel1);
                    }

                    foreach (Station station in stations)
                    {
                        int route_ind;
                        string n1, n2;
                        int f_id, t_id;

                        route_ind = routes.FindIndex(s => s.id == station.route.id);
                        n1 = routes[route_ind].node_1.id;
                        n2 = routes[route_ind].node_2.id;

                        f_id = routenodes.FindIndex(s => s.id == n1);
                        t_id = routenodes.FindIndex(s => s.id == n2);

                        station.CalculateXY(routenodes[f_id], routenodes[t_id]);
                        station.drawStation(int.Parse(station.x_coord), int.Parse(station.y_coord), station.type, drawedStations, panel1);
                        station.calculateClosestNode();
                    }

                    treeView1.BeginUpdate();
                    treeView1.Nodes.Add(doc.DocumentElement.Name);
                    treeView1.Nodes[0].Nodes.Add("Stations");
                    treeView1.Nodes[0].Nodes.Add("Routes");
                    treeView1.Nodes[0].Nodes.Add("Route nodes");

                    foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                    {
                        TreeNode station = new TreeNode(node.Attributes["id"].Value);
                        int index;
                        string newid = node.Name.ToString();

                        switch (node.Name.ToString())
                        {
                            case "Station":
                                index = 0;
                                break;

                            case "Route":
                                index = 1;
                                break;

                            case "RouteNode":
                                index = 2;
                                break;

                            default:
                                index = 0;
                                break;
                        }

                        treeView1.Nodes[0].Nodes[index].Nodes.Add(station);
                        {
                            foreach (XmlNode childnode in node.ChildNodes)
                            {
                                TreeNode n2 = new TreeNode(childnode.Name + " : " + childnode.InnerText);
                                station.Nodes[0].Nodes[1].Nodes.Add(n2);
                            }
                        }
                    }
                    treeView1.EndUpdate();
                    dataGridViewRefresh();
                    button3.Enabled = true;
                    panel1.BringToFront();
                    GetMemoryUsage();
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileStream stream = new FileStream(@"..\..\..\..\..\exp.xml", FileMode.Create);

            XmlSerializer serializer = new XmlSerializer(typeof(List<Route>), new XmlRootAttribute("Layout"));
            XmlSerializer serializer2 = new XmlSerializer(typeof(List<Station>));
            XmlSerializer serializer3 = new XmlSerializer(typeof(List<RouteNode>));

            StringWriter xout1 = new StringWriter();
            StringWriter xout2 = new StringWriter();
            StringWriter xout3 = new StringWriter();

            serializer.Serialize(xout1, routes);
            serializer2.Serialize(xout2, stations);
            serializer3.Serialize(xout3, routenodes);

            XmlDocument x1 = new XmlDocument();
            x1.LoadXml(xout1.ToString());
            XmlDocument x2 = new XmlDocument();
            x2.LoadXml(xout2.ToString());
            XmlDocument x3 = new XmlDocument();
            x3.LoadXml(xout3.ToString());

            foreach (XmlNode node in x2.DocumentElement.ChildNodes)
            {
                XmlNode imported = x1.ImportNode(node, true);
                x1.DocumentElement.AppendChild(imported);
            }

            foreach (XmlNode node in x3.DocumentElement.ChildNodes)
            {
                XmlNode imported = x1.ImportNode(node, true);
                x1.DocumentElement.AppendChild(imported);
            }

            x1.Save(stream);
            stream.Close();
        }

        private void loadImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;";

                DialogResult dlgResult = ofd.ShowDialog();

                if (dlgResult == DialogResult.OK)
                {
                    layoutPath = ofd.FileName;
                }

                Image layout = Image.FromFile(layoutPath);
                panel1.BackgroundImage = layout;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
            }

        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            routenodes.Clear();
            routes.Clear();
            stations.Clear();
            drawedRoutenodes.Clear();
            drawedRoutes.Clear();
            drawedStations.Clear();

            Random r = new Random();

            for (int i = 0; i < 200; i++)
            {
                RouteNode rn = new RouteNode();
                rn.id = i.ToString();
                rn.x_coord = r.Next(0, 740).ToString();
                rn.y_coord = r.Next(0, 480).ToString();

                routenodes.Add(rn);
            }

            int index = 0;
            foreach (RouteNode rn in routenodes)
                foreach (RouteNode rn2 in routenodes)
                {
                    {
                        Route rt = new Route();
                        rt.direction = "uni";
                        rt.id = index.ToString();
                        index++;
                        rt.node_1 = rn;
                        rt.node_2 = rn2;
                        rt.calculateLength();
                        routes.Add(rt);
                    }
                }

            for (int i = 0; i < 39500; i++)
            {
                routes.RemoveAt(r.Next(0, routes.Count()));
            }



            foreach (Route rt in routes)
            {
                int fx, fy, tx, ty;
                int fxid, fyid, txid, tyid;

                fxid = routenodes.FindIndex(s => s.id == rt.node_1.id);
                fyid = routenodes.FindIndex(s => s.id == rt.node_1.id);
                txid = routenodes.FindIndex(s => s.id == rt.node_2.id);
                tyid = routenodes.FindIndex(s => s.id == rt.node_2.id);

                fx = int.Parse(routenodes[fxid].x_coord);
                fy = int.Parse(routenodes[fyid].y_coord);
                tx = int.Parse(routenodes[txid].x_coord);
                ty = int.Parse(routenodes[tyid].y_coord);

                rt.drawRoute(fx, fy, tx, ty, drawedRoutes, panel1);
            }
            foreach (RouteNode rn in routenodes)
            {
                rn.drawRouteNode(int.Parse(rn.x_coord), int.Parse(rn.y_coord), drawedRoutenodes, panel1);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 box = new AboutBox1();
            box.ShowDialog();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                InitPath();
                Optimize();
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            InitPath();
        }

        private void button19_Click(object sender, EventArgs e)
        {
            Optimize();

            
        }

        private void button30_Click_1(object sender, EventArgs e)
        {

        }

        private void button11_Click_1(object sender, EventArgs e)
        {
            
        }

        private void importGrAPPAToolStripMenuItem_Click(object sender, EventArgs e)
        {
            demands.Clear();
            XmlDocument doc = new XmlDocument();
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "XML Files|*.xml";

            DialogResult dlgResult = ofd.ShowDialog();
            String FileName;
            XmlTextReader tr;


            try
            {
                if (dlgResult == DialogResult.OK)
                {
                    doc.Load(ofd.FileName);
                    FileName = ofd.FileName; ;
                    tr = new XmlTextReader(FileName);
                    string ro = "";

                    while (tr.Read())
                    {
                        Station station = new Station();
                        Route route = new Route();
                        RouteNode routenode = new RouteNode();

                        if (tr.MoveToContent() == XmlNodeType.Element && tr.Name == "Station")
                        {
                            station.id = tr.GetAttribute(0).TrimStart("Station".ToArray());
                            //station.route = routes[routes.FindIndex(s => s.id == tr.GetAttribute(1))];
                            station.type = tr.GetAttribute(1);
                            station.workcell = tr.GetAttribute(2);
                            station.x_coord = tr.GetAttribute(3);
                            station.y_coord = tr.GetAttribute(4);
                            station.display = richTextBox1;
                            stations.Add(station);
                        }

                        if (tr.MoveToContent() == XmlNodeType.Element && tr.Name == "Route")
                        {
                            route.id = tr.GetAttribute(0).TrimStart("Transport".ToArray());
                            route.node_1 = routenodes[routenodes.FindIndex(s => s.id == tr.GetAttribute(1).TrimStart("Waypoint".ToArray()))];
                            route.node_2 = routenodes[routenodes.FindIndex(s => s.id == tr.GetAttribute(2).TrimStart("Waypoint".ToArray()))];
                            route.direction = tr.GetAttribute(3);
                            route.display = richTextBox1;
                            route.calculateLength();
                            if (route.id != ro)
                            {
                                routes.Add(route);
                                ro = route.id;
                            }
                        }

                        if (tr.MoveToContent() == XmlNodeType.Element && tr.Name == "RouteNode")
                        {
                            routenode.id = tr.GetAttribute(0).TrimStart("Waypoint".ToArray());
                            routenode.x_coord = tr.GetAttribute(1);
                            routenode.y_coord = tr.GetAttribute(2);
                            routenode.display = richTextBox1;
                            routenodes.Add(routenode);
                        }
                    }
                    
                    
                    int minX = Math.Abs(routenodes.Min(r => int.Parse(r.x_coord)));
                    int minY = Math.Abs(routenodes.Min(r => int.Parse(r.y_coord)));

                    foreach (RouteNode rn in routenodes)
                    {
                        rn.x_coord = ((int.Parse(rn.x_coord) + minX) * 20 + 100).ToString();
                        rn.y_coord = ((int.Parse(rn.y_coord) + minY) * -20 + 400).ToString();
                    }

                    foreach (Station st in stations)
                    {
                        double x = (double.Parse(st.x_coord, CultureInfo.InvariantCulture) + minX) * 20 + 100;
                        double y = (double.Parse(st.y_coord, CultureInfo.InvariantCulture) + minY) * -20 + 400;
                        
                        st.x_coord = x.ToString();
                        st.y_coord = y.ToString();;
                    }

                    foreach (Route rt in routes)
                    {
                        int fx, fy, tx, ty;
                        int fxid, fyid, txid, tyid;

                        fxid = routenodes.FindIndex(s => s.id == rt.node_1.id);
                        fyid = routenodes.FindIndex(s => s.id == rt.node_1.id);
                        txid = routenodes.FindIndex(s => s.id == rt.node_2.id);
                        tyid = routenodes.FindIndex(s => s.id == rt.node_2.id);

                        fx = int.Parse(routenodes[fxid].x_coord);
                        fy = int.Parse(routenodes[fyid].y_coord);
                        tx = int.Parse(routenodes[txid].x_coord);
                        ty = int.Parse(routenodes[tyid].y_coord);

                        rt.calculateMidPoint();
                        rt.drawRoute(fx, fy, tx, ty, drawedRoutes, panel1);
                    }

                    foreach (RouteNode rn in routenodes)
                    {
                        rn.drawRouteNode(int.Parse(rn.x_coord), int.Parse(rn.y_coord), drawedRoutenodes, panel1);
                    }

                    foreach (Station st in stations)
                    {
                        double dist = double.MaxValue;

                        foreach(Route rt in routes)
                        {
                            if (MinDistance(int.Parse(st.x_coord), rt.midX, int.Parse(st.y_coord),rt.midY) < dist)
                            {
                                dist = MinDistance(int.Parse(st.x_coord), rt.midX, int.Parse(st.y_coord), rt.midY);
                                st.route = rt;
                            }
                        }

                        st.drawStation(int.Parse(st.x_coord), int.Parse(st.y_coord), st.type, drawedStations, panel1);
                        st.CalculateXY(st.route.node_1, st.route.node_2);
                        
                    }

                    treeView1.BeginUpdate();
                    treeView1.Nodes.Add(doc.DocumentElement.Name);
                    treeView1.Nodes[0].Nodes.Add("Stations");
                    treeView1.Nodes[0].Nodes.Add("Routes");
                    treeView1.Nodes[0].Nodes.Add("Route nodes");

                    foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                    {
                        TreeNode station = new TreeNode(node.Attributes["id"].Value);
                        int index;
                        string newid = node.Name.ToString();

                        switch (node.Name.ToString())
                        {
                            case "Station":
                                index = 0;
                                break;

                            case "Route":
                                index = 1;
                                break;

                            case "RouteNode":
                                index = 2;
                                break;

                            default:
                                index = 0;
                                break;
                        }

                        treeView1.Nodes[0].Nodes[index].Nodes.Add(station);
                        {
                            foreach (XmlNode childnode in node.ChildNodes)
                            {
                                TreeNode n2 = new TreeNode(childnode.Name + " : " + childnode.InnerText);
                                station.Nodes[0].Nodes[1].Nodes.Add(n2);
                            }
                        }
                    }
                    treeView1.EndUpdate();
                    dataGridViewRefresh();
                    button3.Enabled = true;
                    panel1.BringToFront();
                    GetMemoryUsage();
                    tr.Close();
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
            }
        }

        public double MinDistance(int x1, double x2, int y1, double y2)
        {
            double distance = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
            return distance;
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            routes.Clear();
            routenodes.Clear();
            stations.Clear();
            drawedRoutenodes.Clear();
            drawedStations.Clear();
            drawedRoutes.Clear();
            treeView1.Nodes.Clear();
            treeView1.Update();
            panel1.Invalidate();
            panel1.Controls.Clear();
            panel1.Refresh();
        }
    }
}


public class Triplet
{
    public string megallo;
    public string honnan;
    public string hova;
}

