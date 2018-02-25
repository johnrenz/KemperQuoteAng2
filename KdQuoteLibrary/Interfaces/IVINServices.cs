using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KdQuoteLibrary.Interfaces
{
    public interface IVINServices
    {
        string GetMakeByYear(string year);
        string GetModelByYearMake(string year, string makeno);
        string GetWebModelByYearMake(string year, string makeno);
        string GetTrimByYearMakeModel(string year, string makeno, string model);
        string GetVehicleYearMakeModel(string vin);
        string GetVehicleYearMakeWebModel(string vin);
        string GetVehicleByYearMakeModel(string year, string makeno, string model, string webmodel);
    }
}
