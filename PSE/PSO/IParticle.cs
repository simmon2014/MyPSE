using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSE.PSO
{
 
    public interface IParticle : IComparable
    {
        double Fitness { get; }
        IParticle pBest { get; set; }

        void Evaluate(IFitnessFunction function);

    }
}
