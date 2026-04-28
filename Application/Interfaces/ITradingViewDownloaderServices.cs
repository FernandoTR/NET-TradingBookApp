using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces;

public interface ITradingViewDownloaderServices
{
    void DescargarImagenes(List<(int Id, string Url)> dataset);
}
