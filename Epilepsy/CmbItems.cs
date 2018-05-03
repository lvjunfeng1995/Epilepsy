using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Epilepsy
{
   public class CmbItems 
    {
        public String items { set; get; }
    }
    public class CmbItem : ObservableCollection<CmbItems>
    {
        public CmbItem()
        {
            this.Add(new CmbItems{items="      0"});
            this.Add(new CmbItems { items = "      1" });
            this.Add(new CmbItems { items = "      2" });

        }
    }
}
