using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static _3dZipSorter.Database.DatabaseManager;

namespace _3dZipSorter.fonctions
{
    public interface IFonction
    {
        void Executer(string dossierSource, string dossierDestination, List<ArchiveSortingRule> fileExtensions, Action<string> log, params string[] operations);
    }
}
