using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MilkRunApp_v3
{
    class LocalSearch
    {
        public Path path1;
        public Path path2;
        public bool isBetter;
        public double sumRequiredTime;
        Path newPath1;
        Path newPath2;
        List<Route> routes;
        List<RouteNode> routenodes;
        List<Route> routeList = new List<Route>();
        List<RouteNode> routeNodeList = new List<RouteNode>();
        List<Station> stations = new List<Station>();
        List<Demand> demands = new List<Demand>();

        public LocalSearch(Path path1, Path path2, List<Route> routes, List<RouteNode> routenodes, List<Station> stations, List<Demand> demands)
        {
            this.path1 = path1;
            this.path2 = path2;
            this.routes = routes;
            this.routenodes = routenodes;
            this.stations = stations;
            this.demands = demands;
        }

        public List<Path>InterInsert()
        {
            List<Path> newPaths = new List<Path>();
            isBetter = false;

            for (int i = 1; i < path1.stationsToVisit.Count(); i++) // for i = 1 not to avoid the warehouse
                for (int j = 1; j < path2.stationsToVisit.Count(); j++)
                {
                    {
                        if (!isBetter)
                        {
                            newPath1 = new Path(new List<Station>(), new List<Route>(), new Vehicle(), new List<Demand>());
                            newPath2 = new Path(new List<Station>(), new List<Route>(), new Vehicle(), new List<Demand>());

                            List<Station> stList1 = new List<Station>();
                            stList1 = path1.cloneStationList();
                            List<Station> stList2 = new List<Station>();
                            stList2 = path2.cloneStationList();

                            List<Route> rtList1 = new List<Route>();
                            List<Route> rtList2 = new List<Route>();

                            Station whatToInsert = new Station();
                            whatToInsert = stList1[i];

                            stList2.Insert(j, whatToInsert);
                            stList1.RemoveAt(i);

                            stList1.Insert(0, stations[0]);
                            stList2.Insert(0, stations[0]);

                            stList1 = removeDuplicateStations(stList1);
                            stList2 = removeDuplicateStations(stList2);

                            int nextNodeIndex1 = 1;

                            for (int k = 0; k < stList1.Count() - 1; k++)
                            {
                                int fromIndex = stations.FindIndex(s => s.id == stList1[k].id);
                                int toIndex = stations.FindIndex(s => s.id == stList1[k + 1].id);


                                if (fromIndex != toIndex)
                                {
                                    double len1 = Form1.stationToStation[fromIndex, toIndex][nextNodeIndex1].Sum(s => s.length);

                                    if (len1 != 0)
                                    {
                                        rtList1.AddRange(Form1.stationToStation[fromIndex, toIndex][nextNodeIndex1]);

                                        if (rtList1[rtList1.Count() - 1].node_1.id == stList1[k + 1].route.node_1.id ||
                                            rtList1[rtList1.Count() - 1].node_2.id == stList1[k + 1].route.node_1.id)
                                        {
                                            nextNodeIndex1 = 1;
                                        }
                                        else
                                        {
                                            nextNodeIndex1 = 0;
                                        }

                                        rtList1.Add(stList1[k + 1].route);
                                        rtList1 = removeDuplicateRoutes(rtList1);
                                    }
                                    else
                                    {
                                        rtList1.Add(stList1[k + 1].route);

                                        if (rtList1[rtList1.Count() - 1].node_1.id == stList1[k].route.node_1.id ||
                                            rtList1[rtList1.Count() - 1].node_2.id == stList1[k].route.node_1.id)
                                        {
                                            nextNodeIndex1 = 1;
                                        }
                                        else
                                        {
                                            nextNodeIndex1 = 0;
                                        }
                                    }


                                    rtList1 = removeDuplicateRoutes(rtList1);
                                    newPath1.routesOnPath.AddRange(rtList1);
                                }
                            }

                            int nextNodeIndex2 = 1;

                            for (int l = 0; l < stList2.Count() - 1; l++)
                            {
                                int fromIndex = stations.FindIndex(s => s.id == stList2[l].id);
                                int toIndex = stations.FindIndex(s => s.id == stList2[l + 1].id);


                                if (fromIndex != toIndex)
                                {
                                    double len1 = Form1.stationToStation[fromIndex, toIndex][nextNodeIndex2].Sum(s => s.length);

                                    if (len1 != 0)
                                    {
                                        rtList2.AddRange(Form1.stationToStation[fromIndex, toIndex][nextNodeIndex2]);

                                        if (rtList2[rtList2.Count() - 1].node_1.id == stList2[l + 1].route.node_1.id ||
                                            rtList2[rtList2.Count() - 1].node_2.id == stList2[l + 1].route.node_1.id)
                                        {
                                            nextNodeIndex2 = 1;
                                        }
                                        else
                                        {
                                            nextNodeIndex2 = 0;
                                        }

                                        rtList2.Add(stList2[l + 1].route);
                                        rtList2 = removeDuplicateRoutes(rtList2);
                                    }
                                    else
                                    {
                                        rtList2.Add(stList2[l + 1].route);

                                        if (rtList2[rtList2.Count() - 1].node_1.id == stList2[l].route.node_2.id ||
                                            rtList2[rtList2.Count() - 1].node_2.id == stList2[l].route.node_2.id)
                                        {
                                            nextNodeIndex2 = 1;
                                        }
                                        else
                                        {
                                            nextNodeIndex2 = 0;
                                        }
                                    }


                                    rtList2 = removeDuplicateRoutes(rtList2);
                                    newPath2.routesOnPath.AddRange(rtList2);
                                }
                            }
                            Path p1 = new Path(stList1, rtList1, path1.vehicleApplied, demands);
                            Path p2 = new Path(stList2, rtList2, path1.vehicleApplied, demands);

                            p1.ifFeasible();
                            p2.ifFeasible();

                            if ((p1.timeRequirement + p2.timeRequirement < path1.timeRequirement + path2.timeRequirement) && p1.feasible && p2.feasible)
                            {
                                //MessageBox.Show(Math.Round((path1.timeRequirement + path2.timeRequirement),0).ToString() + " -------> " + Math.Round((p1.timeRequirement + p2.timeRequirement),0).ToString());
                                newPaths.Add(p1);
                                newPaths.Add(p2);
                                isBetter = true;
                                sumRequiredTime = Math.Round((path1.timeRequirement + path2.timeRequirement));
                                return newPaths;
                            }
                        }
                    }
                }
            return null;
        }

        public int nextNodeIndex { get; set; }

        public List<Station> removeDuplicateStations(List<Station> stationList)
        {
            for (int i = 0; i < stationList.Count() - 1; i++)
            {
                if (stationList[i].id == stationList[i + 1].id)
                    stationList.RemoveAt(i + 1);
            }

            return stationList;
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
    }
}
