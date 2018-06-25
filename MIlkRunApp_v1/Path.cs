using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MilkRunApp_v3
{
    [Serializable]
    public class Path : ICloneable
    {
        [XmlIgnore]
        private double pathLength;
        [XmlIgnore]
        public int numOfStops;
        [XmlIgnore]
        public double timeRequirement;
        [XmlAttribute]
        public List<Station> stationsToVisit;
        [XmlIgnore]
        public List<Demand> demandsToFulfil;
        [XmlIgnore]
        public List<RouteNode> routeNodesToVisit;
        [XmlIgnore]
        public List<Route> routesOnPath;
        [XmlIgnore]
        public int sumKLTs;
        [XmlIgnore]
        public bool feasible;
        [XmlIgnore]
        public Vehicle vehicleApplied;

        public object Clone()
        {
            Path ret = new Path(stationsToVisit, routesOnPath, vehicleApplied, demandsToFulfil);
            return ret;
        }

        public List<Station> cloneStationList()
        {
            List<Station> stList = new List<Station>();
            foreach (Station s in stationsToVisit)
            {
                stList.Add(s);
            }
            return stList;
        }

        public Path(List<Station> stations, List<Route> routes, Vehicle vehicle, List<Demand> demands)
        {
            this.stationsToVisit = stations;
            this.routesOnPath = routes;
            this.vehicleApplied = vehicle;
            demandsToFulfil = new List<Demand>();

            foreach (Station st in stationsToVisit)
            {
                foreach (Demand dem in demands)
                {
                    if ((dem.ifFinished == false && dem.to == st.id) || (dem.ifFinished == true && dem.from == st.id))
                    {
                        demandsToFulfil.Add(dem);
                    }
                }
            }

            sumKLTs = demandsToFulfil.Sum(dem => dem.amount);
            numOfStops = stationsToVisit.Count();
            pathLength = routesOnPath.Sum(rt => rt.length);
            timeCalc();
            feasible = ifFeasible();
        }

        public void timeCalc()
        {
            timeRequirement = pathLength / (vehicleApplied.speed / 3.6)  + (sumKLTs * 10);
        }

        public bool ifFeasible()
        {
            pathLength = routesOnPath.Sum(rt => rt.length);
            timeCalc();
            int cargo = 0;

            foreach (Demand dem in demandsToFulfil)
            {
                if (dem.ifFinished == false)
                {
                    cargo += dem.amount;
                }
            }

            removeDuplicateStations(stationsToVisit);
            removeStationsWithZeroDemand(stationsToVisit);

            for (int i = 1; i < stationsToVisit.Count() - 1; i++)
                if (demandsToFulfil.FindAll(d => (d.from == stationsToVisit[i].id) || (d.to == stationsToVisit[i].id)).Count() != 0)
                {
                    if (demandsToFulfil.Find(d => d.to == stationsToVisit[i].id && d.ifFinished == false) != null &&
                      demandsToFulfil.Find(d => d.from == stationsToVisit[i].id && d.ifFinished == true) != null)
                    {
                        cargo -= demandsToFulfil.Find(d => d.to == stationsToVisit[i].id && d.ifFinished == false).amount;
                        cargo += demandsToFulfil.Find(d => d.from == stationsToVisit[i].id && d.ifFinished == true).amount;
                    }
                    else
                    {
                        feasible = false;
                        return false;
                    }

                    if (cargo > vehicleApplied.capacity || timeRequirement > 3000)
                    {
                        feasible = false;
                        return false;
                    }
                }
                else
                {
                    stationsToVisit.RemoveAt(i);
                }
            feasible = true;
            return true;
        }

        private List<Station> removeDuplicateStations(List<Station> stationList)
        {
            for (int i = 0; i < stationList.Count() - 1; i++)
            {
                if (stationList[i].id == stationList[i + 1].id)
                    stationList.RemoveAt(i + 1);
            }

            return stationList;
        }

        private List<Station> removeStationsWithZeroDemand(List<Station> stationList)
        {
            foreach (Demand dem in demandsToFulfil)
            {
                if (dem.amount == 0)
                {
                    stationList.RemoveAll(st => (st.id == dem.from && dem.from != "DFC") || (st.id == dem.to && dem.to != "DFC"));
                }
            }

            return stationList;
        }
    }
}
