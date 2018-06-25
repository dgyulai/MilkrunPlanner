using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MilkRunApp_v3
{
    class Dijkstra
    {
        public RouteNode from;
        public List<Route> routes { get; set; }
        public List<RouteNode> routenodes { get; set; }
        public List<RouteNode> shortestPath;
        public NodeDistance[,] distancematrix;
        private int from_ind, rn_ind;

        public Dijkstra(List<Route> _routes, List<RouteNode> _routenodes, RouteNode _from)
        {
            this.routes = _routes;
            this.routenodes = _routenodes;
            this.from = _from;
            this.from_ind = routenodes.FindIndex(s => s.id == from.id);

            this.CreateDistanceMatrix();

            foreach (RouteNode rn in routenodes)
            {
                rn.label = 0;
                rn.labeltype = false;
            }
        }

        private NodeDistance[,] CreateDistanceMatrix()
        {
            int n1_x, n1_y, n2_x, n2_y;
            int n1_ind, n2_ind;
            distancematrix = new NodeDistance[routenodes.Count, routenodes.Count];

            for (int i = 0; i < routenodes.Count; i++)
                for (int j = 0; j < routenodes.Count; j++)
                {
                    {
                        distancematrix[i, j] = new NodeDistance();
                        distancematrix[i, j].from = routenodes[i].id;
                        distancematrix[i, j].to = routenodes[j].id;
                        distancematrix[i, j].length = int.MaxValue;
                    }
                }

            for (int i = 0; i < routenodes.Count; i++)
            {
                distancematrix[i, i].length = 0;
            }

            foreach (Route rt in routes)
            {
                n1_x = int.Parse(routenodes[routenodes.FindIndex(s => s.id == rt.node_1.id)].x_coord);
                n1_y = int.Parse(routenodes[routenodes.FindIndex(s => s.id == rt.node_1.id)].y_coord);
                n2_x = int.Parse(routenodes[routenodes.FindIndex(s => s.id == rt.node_2.id)].x_coord);
                n2_y = int.Parse(routenodes[routenodes.FindIndex(s => s.id == rt.node_2.id)].y_coord);

                n1_ind = routenodes.FindIndex(s => s.id == rt.node_1.id);
                n2_ind = routenodes.FindIndex(s => s.id == rt.node_2.id);

                NodeDistance nd = new NodeDistance();
                NodeDistance nd2 = new NodeDistance();

                nd.length = (int)Math.Sqrt((Math.Pow((n1_x - n2_x), 2)) + (Math.Pow((n1_y - n2_y), 2)));
                nd2.length = nd.length;

                switch (rt.direction)
                {
                    case "uni":
                        nd.from = rt.node_1.id;
                        nd.to = rt.node_2.id;
                        distancematrix[n1_ind, n2_ind] = nd;

                        // Ezt a részt át kell majd nézni, hogy hogyan lehetne esetleg szebben megcsinálni!!!!!!!!!!
                        nd2.from = rt.node_2.id;
                        nd2.to = rt.node_1.id;
                        distancematrix[n2_ind, n1_ind] = nd2;
                        break;

                    case "1_2":
                        nd.from = rt.node_1.id;
                        nd.to = rt.node_2.id;
                        distancematrix[n1_ind, n2_ind] = nd;
                        break;

                    case "2_1":
                        nd.from = rt.node_2.id;
                        nd.to = rt.node_1.id;
                        distancematrix[n2_ind, n1_ind] = nd;
                        break;
                    default:
                        break;
                }
            }
            return distancematrix;
        }

        public void CalculateDijkstra()
        {
            Init();

            while (routenodes.Count(s => s.labeltype == true) != routenodes.Count)
            {
                RouteNode u = GetNextVertex();
                foreach (var v in routenodes.Where(rn => rn.labeltype == false))
                {
                    int u_ind = routenodes.FindIndex(s => s.id == u.id);
                    int v_ind = routenodes.FindIndex(s => s.id == v.id);
                    int u_v = distancematrix[u_ind, v_ind].length;

                    //Megnézzük, hogy a kiválasztott következő node szomszédos-e a mostanival
                    if (u_v > 0 && u_v < int.MaxValue)
                    {
                        if (v.label > u.label + u_v)
                        {
                            v.label = u.label + u_v;
                        }
                    }
                }
            }
        }

        private void Init()
        {
            //A kiinduló csúcs 0, a többi csúcsot a kiinduló csúcstól való távolságával címkézünk, illetve (inf)-el, ha nem lehet oda eljutni...
            foreach (RouteNode rn in routenodes)
            {
                if (rn.id != from.id)
                {
                    rn_ind = routenodes.FindIndex(s => s.id == rn.id);
                    rn.labeltype = false;

                    if (distancematrix[from_ind, rn_ind].length != 0 && distancematrix[from_ind, rn_ind].length < int.MaxValue)
                    {
                        rn.label = distancematrix[from_ind, rn_ind].length;
                    }
                    else
                    {
                        rn.label = int.MaxValue;
                    }
                }
                else
                {
                    rn.label = 0;
                    rn.labeltype = true;
                }
            }
        }

        // Kiválasztjuk a legkisebb címkével rendelkező csúcsot, és címkéjét állandóvá tesszük...
        private RouteNode GetNextVertex()
        {
            int min = int.MaxValue;
            RouteNode nextvertex = new RouteNode();

            foreach (var rn in routenodes.Where(rn => rn.labeltype == false && rn.label != 0))
            {
                if (rn.label <= min)
                {
                    min = rn.label;
                    nextvertex = rn;
                }
            }
            nextvertex.labeltype = true;
            return nextvertex;
        }

        public int GetDistance(RouteNode to)
        {
            shortestPath = new List<RouteNode>();
            List<Route> connectingRoutes = new List<Route>();
            List<RouteNode> adjacentNodes = new List<RouteNode>();
            int prevNodeIndex, currNodeIndex, pathLength;
            RouteNode final = to;
            
            pathLength = 0;

            List<NodeDistance> fromList = new List<NodeDistance>();

            foreach (NodeDistance nd in distancematrix)
            {
                if ((nd.to == to.id || nd.from == to.id) && nd.length != 0 && nd.length != int.MaxValue)
                    fromList.Add(nd);
            }

            while (from != to)
            {
                
                // Megkeressük annak a RouteNode-nak a szomszédos node-jait, amelyikben éppen tartózkodunk
                foreach (NodeDistance nd in distancematrix)
                {
                    if (nd.to == to.id && nd.length != 0 && nd.length < int.MaxValue) // Csak a szomszédos nodeok között keresünk
                    {
                        prevNodeIndex = routenodes.FindIndex(s => s.id == nd.from);
                        currNodeIndex = routenodes.FindIndex(s => s.id == to.id);

                        if ((to.label - routenodes[prevNodeIndex].label) == distancematrix[prevNodeIndex , currNodeIndex].length)
                        {
                            shortestPath.Add(routenodes[prevNodeIndex]);
                            pathLength += nd.length;
                            to = routenodes[prevNodeIndex];
                        }
                    }
                }
            }
            shortestPath.Reverse();
            shortestPath.Add(final);
            return pathLength;
        }
    }

    public class NodeDistance
    {
        public string from { get; set; }
        public string to { get; set; }
        public int length { get; set; }
    }
}

