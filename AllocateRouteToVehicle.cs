using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MilkRunApp_v3
{
    // The algorithm below solves the classic bin packing problem

    class AllocateRouteToVehicle
    {
        private List<Path> inputPaths;
        private List<double> virtualVehicles;
        private int timeLimit;
        private List<List<int>> allocation;
        public double sumTimeOfPlan;

        public AllocateRouteToVehicle(List<Path> pathList, int timeLimit)
        {
            this.inputPaths = pathList.OrderBy(p => p.timeRequirement).Reverse().ToList();
            this.timeLimit = timeLimit;
            this.virtualVehicles = new List<double>();
            this.allocation = new List<List<int>>();
        }

        public List<List<int>> Calculate()
        {
            int numOfVehicles = 0;

            // Prepare vehicles to worst case: generate as many vehicles as many paths we have
            for (int i = 0; i < inputPaths.Count(); i++)
            {
                virtualVehicles.Add(timeLimit);
                allocation.Add(new List<int>());
            }

            for (int i = 0; i < inputPaths.Count(); i++)
            {
                int index = virtualVehicles.FindIndex(v => v > inputPaths[i].timeRequirement);
                virtualVehicles[index] -= inputPaths[i].timeRequirement;
                allocation[index].Add(i);
            }

            numOfVehicles = virtualVehicles.Count(v => v < timeLimit);
            allocation.RemoveAll(l => l.Count() == 0);
            return allocation;
        }

        public double Calculate2(int maxNumOfVehicles)
        {
            double prevValue = 0;
            
            for (int i = 0; i < maxNumOfVehicles; i++)
            {
                virtualVehicles.Add(timeLimit);
                allocation.Add(new List<int>());
            }

            for (int i = 0; i < inputPaths.Count(); i++)
            {
                int index = virtualVehicles.FindIndex(v => v > inputPaths[i].timeRequirement);
                if (index >= 0)
                {
                    virtualVehicles[index] -= inputPaths[i].timeRequirement;
                    allocation[index].Add(i);
                }
                else
                {
                    prevValue = inputPaths.Where(p => inputPaths.IndexOf(p) >= i).Select(p => p.timeRequirement).Sum();
                    return prevValue;
                }
            }

            allocation.RemoveAll(l => l.Count() == 0);
            return prevValue;
        }
    }
}
