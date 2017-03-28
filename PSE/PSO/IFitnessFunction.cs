using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSE.PSO
{
    public interface IFitnessFunction
    {
        double Evaluate(IParticle particle);
    }
}
