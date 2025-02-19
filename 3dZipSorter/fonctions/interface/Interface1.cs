using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace _3dZipSorter.fonctions
{
    public interface IFonction
    {
        void Executer(string dossierSource, string dossierDestination, Dictionary<string, string> fileExtensions, Action<string> log, params string[] operations);
    }
}
